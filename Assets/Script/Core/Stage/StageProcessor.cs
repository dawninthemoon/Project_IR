using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using RieslingUtils;
using System;

public class StageProcessor : Singleton<StageProcessor>
{
    struct StartNextStageRequsetDesc
    {
        public bool _startNextStage;
        public string _stageDataPath;
    }

    public StageData _stageData = null;

    private int _currentPoint = 0;
    private bool _isEnd = false;
    private StartNextStageRequsetDesc _startNextStageRequestDesc = new StartNextStageRequsetDesc();

    private CameraControlEx _targetCameraControl;

    private Vector3 _smoothDampVelocity;

    private Vector3 _offsetPosition = Vector3.zero;
    private Vector3 _cameraPositionBlendStartPosition;
    private float _cameraPositionBlendTimeLeft = 0f;

    private GameEntityBase _playerEntity = null;
    private GameObject _stageTilemapParent = null;
    private GameObject _stageBackgroundObject = null;

    private MiniStageListItem   _miniStageInfo = null;
    private BoundBox            _miniStageTrigger = new BoundBox(0f,0f,Vector3.zero);

    Dictionary<int,List<SequencerGraphProcessor.SpawnedCharacterEntityInfo>> _spawnedCharacterEntityDictionary = new Dictionary<int, List<SequencerGraphProcessor.SpawnedCharacterEntityInfo>>();
    Dictionary<string, CharacterEntityBase> _keepAliveMap = new Dictionary<string, CharacterEntityBase>();

    private Queue<StageProcessor> _miniStagePool = new Queue<StageProcessor>();
    private List<StageProcessor> _miniStageProcessor = new List<StageProcessor>();

    private SequencerGraphProcessManager _sequencerProcessManager = new SequencerGraphProcessManager(null);
    private MovementTrackProcessor _trackProcessor = new MovementTrackProcessor();

    private Vector3         _cameraTrackPositionError = Vector3.zero;
    private float           _cameraTrackPositionErrorReduceTime = 0f;

    public StageProcessor()
    {
        _startNextStageRequestDesc._stageDataPath = "";
        _startNextStageRequestDesc._startNextStage = false;
    }

    public void requestStartStage(string stagePath)
    {
        _startNextStageRequestDesc._startNextStage = true;
        _startNextStageRequestDesc._stageDataPath = stagePath;
    }

    public void setTargetCameraControl(CameraControlEx target)
    {
        _targetCameraControl = target;
    }

    public void startMiniStage(MiniStageListItem data, Vector3 startPosition)
    {
        Vector3 worldPosition = startPosition + data._localStagePosition + data._overrideTriggerOffset;
        _miniStageInfo = data;
        _miniStageTrigger.setData(data._overrideTriggerWidth * 0.5f,data._overrideTriggerHeight * 0.5f,data._overrideTriggerOffset);
        _miniStageTrigger.updateBoundBox(worldPosition);

        startStage(data._data, worldPosition);
    }

    public void startStage(StageData data, Vector3 startPosition)
    {
        _sequencerProcessManager.initialize();
        _trackProcessor.clear();

        _cameraTrackPositionError = Vector3.zero;
        _cameraTrackPositionErrorReduceTime = 0f;

        _stageData = data;
        bool isMiniStage = _stageData._isMiniStage;

        _currentPoint = 0;
        _isEnd = false;

        if(_stageData._stagePointData.Count == 0)
            return;

        _offsetPosition = startPosition - _stageData._stagePointData[0]._stagePoint;

        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            item.Clear();
        }

        if(isMiniStage == false)
        {
            if(_stageData._stagePointData[0]._onEnterSequencerPath == null)
                return;

            CameraControlEx.Instance().clearCamera(_stageData._stagePointData[0]._stagePoint);
            CameraControlEx.Instance().setZoomSizeForce(_stageData._stagePointData[0]._cameraZoomSize);

            ScreenDirector._instance.initialize();
            UIRepeater.Instance().initialize();
        }

        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            StagePointData stagePointData = _stageData._stagePointData[index];
            if(_spawnedCharacterEntityDictionary.ContainsKey(index) == false)
                _spawnedCharacterEntityDictionary.Add(index,new List<SequencerGraphProcessor.SpawnedCharacterEntityInfo>());

            foreach(var characterSpawnData in stagePointData._characterSpawnData)
            {
                if(_keepAliveMap.ContainsKey(characterSpawnData._uniqueKey))
                {
                    CharacterEntityBase characterEntity = _keepAliveMap[characterSpawnData._uniqueKey];
                    characterEntity.updatePosition((stagePointData._stagePoint + _offsetPosition) + characterSpawnData._localPosition);

                    SequencerGraphProcessor.SpawnedCharacterEntityInfo keepEntityInfo = new SequencerGraphProcessor.SpawnedCharacterEntityInfo();
                    keepEntityInfo._keepAlive = characterSpawnData._keepAlive;
                    keepEntityInfo._uniqueKey = characterSpawnData._uniqueKey;
                    keepEntityInfo._characterEntity = characterEntity;

                    _spawnedCharacterEntityDictionary[index].Add(keepEntityInfo);
                    continue;
                }

                CharacterInfoData infoData = CharacterInfoManager.Instance().GetCharacterInfoData(characterSpawnData._characterKey);
                if(infoData == null)
                {
                    DebugUtil.assert(false,"CharacterInfo가 뭔가 잘못됐습니다. 통보 바람 [StageName: {0}]",data._stageName);
                    stopStage(true);
                    return;
                }

                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                SpawnCharacterOptionDesc spawnDesc = new SpawnCharacterOptionDesc();
                spawnDesc._position = (stagePointData._stagePoint + _offsetPosition) + characterSpawnData._localPosition;
                spawnDesc._direction = characterSpawnData._flip ? Vector3.left : Vector3.right;
                spawnDesc._rotation = Quaternion.identity;
                spawnDesc._searchIdentifier = characterSpawnData._searchIdentifier;

                CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(infoData,spawnDesc);
                if(createdCharacter == null)
                {
                    DebugUtil.assert(false,"Character Spawn 실패! [StageName: {0}]",data._stageName);
                    stopStage(true);
                    return;
                }

                bool activeSelf = true;
                switch(characterSpawnData._activeType)
                {
                    case StageSpawnCharacterActiveType.Spawn:
                        activeSelf = true;
                    break;
                    case StageSpawnCharacterActiveType.PointActivated:
                    if(data._isMiniStage == false && index == 0)
                        activeSelf = true;
                    else
                        activeSelf = false;
                    break;
                }

                createdCharacter.setActiveSelf(activeSelf,characterSpawnData._hideWhenDeactive);
                SequencerGraphProcessor.SpawnedCharacterEntityInfo entityInfo = new SequencerGraphProcessor.SpawnedCharacterEntityInfo();
                entityInfo._keepAlive = characterSpawnData._keepAlive;
                entityInfo._uniqueKey = characterSpawnData._uniqueKey;
                entityInfo._characterEntity = createdCharacter;

                _spawnedCharacterEntityDictionary[index].Add(entityInfo);

                if(characterSpawnData._startAction != "")
                {
                    createdCharacter.setAction(characterSpawnData._startAction);
                    createdCharacter.progress(0f);
                }

                createdCharacter.updateFlipState(FlipType.Direction);
            }
        }


        if(isMiniStage == false)
        {
            if(_stageData._stagePointData[0]._onEnterSequencerPath != null && _stageData._stagePointData[0]._onEnterSequencerPath.Length != 0)
            {
                for(int index = 0; index < _stageData._stagePointData[0]._onEnterSequencerPath.Length; ++index)
                {
                    SequencerGraphProcessor processor = _sequencerProcessManager.startSequencerFromStage(_stageData._stagePointData[0]._onEnterSequencerPath[index],_stageData._stagePointData[0],_spawnedCharacterEntityDictionary[0],null,_stageData._markerData,false);

                    // if(_playerEntity != null && playerEntity != null)
                    // {
                    //     DebugUtil.assert(false,"Stage에 Player가 2명 이상 존재합니다. 데이터를 확인해 주세요. [StageName: {0}]",data._stageName);
                    //     stopStage();
                    //     return;
                    // }
                    // else 
                    if (_playerEntity == null)
                    {
                        setPlayEntity(processor?.getUniqueEntity("Player"));
                        UIRepeater.Instance().registerUniqueEntity("Player", getPlayerEntity());
                    }
                }

            }

            foreach(var item in _stageData._miniStageData)
            {
                StageProcessor processor = null;
                if(_miniStagePool.Count == 0)
                    processor = new StageProcessor();
                else
                    processor = _miniStagePool.Dequeue();
            
                processor.startMiniStage(item,startPosition);
                processor.setPlayEntity(_playerEntity);
                _miniStageProcessor.Add(processor);
            }
        }

        if (_stageData._tilemapConfigPath != null) 
        {
            if (_stageTilemapParent == null)
            {
                GameObject tilemapParentPrefab = ResourceContainerEx.Instance().GetPrefab("Prefab/TilemapSettings");
                _stageTilemapParent = GameObject.Instantiate(tilemapParentPrefab);
            }

            Tilemap wallTilemap = _stageTilemapParent.transform.Find("Wall").GetComponent<Tilemap>();
            Tilemap backgroundTilemap = _stageTilemapParent.transform.Find("Background").GetComponent<Tilemap>();
            
            wallTilemap.SetTiles(_stageData._tilemapConfigPath._wall);
            backgroundTilemap.SetTiles(_stageData._tilemapConfigPath._background);

            _stageTilemapParent.transform.position = startPosition;
        }

        if(_stageData._backgroundPrefabPath != null)
        {
            _stageBackgroundObject = GameObject.Instantiate(_stageData._backgroundPrefabPath);

            _stageBackgroundObject.SetActive(true);
            _stageBackgroundObject.transform.position = startPosition;
        }
    }

    public void setPlayEntity(GameEntityBase player)
    {
        _playerEntity = player;
    }

    public void playMiniStage(MiniStageListItem miniStage)
    {


    }

    public void stopStage(bool forceStop)
    {
        _sequencerProcessManager.initialize();
        
        _trackProcessor.clear();

        _cameraTrackPositionError = Vector3.zero;
        _cameraTrackPositionErrorReduceTime = 0f;

        bool isMiniStage = _stageData == null ? false : _stageData._isMiniStage;
        _stageData = null;
        _playerEntity = null;
        _miniStageInfo = null;
        _currentPoint = 0;
        _isEnd = false;
        _offsetPosition = Vector3.zero;
        if(_stageTilemapParent != null)
            GameObject.Destroy(_stageTilemapParent);
        if(_stageBackgroundObject != null)
            GameObject.Destroy(_stageBackgroundObject);

        _keepAliveMap.Clear();
        foreach(var item in _spawnedCharacterEntityDictionary.Values)
        {
            for(int i = 0; i < item.Count;)
            {
                if(forceStop == false && item[i]._keepAlive)
                {
                    _keepAliveMap.Add(item[i]._uniqueKey,item[i]._characterEntity);
                    ++i;
                    continue;
                }

                item[i]._characterEntity.deactive();
                item[i]._characterEntity.DeregisterRequest();

                item.RemoveAt(i);
            }
        }

        if(isMiniStage == false)
        {
            foreach(var item in _miniStageProcessor)
            {
                item.stopStage(forceStop);
                _miniStagePool.Enqueue(item);
            }

            _miniStageProcessor.Clear();

            Message msg = MessagePool.GetMessage();
            msg.Set(MessageTitles.game_stageEnd,MessageReceiver._boradcastNumber,null,null);

            MasterManager.instance.HandleBroadcastMessage(msg);
        }
    }

    public void processStage(float deltaTime)
    {
        if(isValid() == false)
            return;
        
        if(_startNextStageRequestDesc._startNextStage)
        {
            _startNextStageRequestDesc._startNextStage = false;
            stopStage(false);

            StageData stageData = ResourceContainerEx.Instance().GetStageData(_startNextStageRequestDesc._stageDataPath);
            if(stageData == null)
            {
                DebugUtil.assert(false,"대상 스테이지 데이터가 존재하지 않습니다. [Path: {0}]",_startNextStageRequestDesc._stageDataPath);
                return;
            }

            startStage(stageData,Vector3.zero);

            return;
        }

        _sequencerProcessManager?.progress(deltaTime);

        if(_stageData._isMiniStage)
        {
            if(_isEnd == false)
            {
                _isEnd = _miniStageTrigger.intersection(_playerEntity.transform.position);
                if(_isEnd)
                {
                    if(_stageData._stagePointData[0]._onEnterSequencerPath != null && _stageData._stagePointData[0]._onEnterSequencerPath.Length != 0)
                    {
                        for(int index = 0; index < _stageData._stagePointData[0]._onEnterSequencerPath.Length; ++index)
                        {
                            SequencerGraphProcessor processor = _sequencerProcessManager.startSequencerFromStage(_stageData._stagePointData[0]._onEnterSequencerPath[index],_stageData._stagePointData[0],_spawnedCharacterEntityDictionary[0],null,_stageData._markerData,false);
                        }
                    }

                    if(_spawnedCharacterEntityDictionary.ContainsKey(_currentPoint))
                    {
                        for(int index = 0; index < _spawnedCharacterEntityDictionary[_currentPoint].Count; ++index)
                        {
                            if(_stageData._stagePointData[_currentPoint]._characterSpawnData[index]._activeType == StageSpawnCharacterActiveType.PointActivated)
                                _spawnedCharacterEntityDictionary[_currentPoint][index]._characterEntity?.setActiveSelf(true,false);
                        }
                    }
                }
                
            }
            Color color = _isEnd ? Color.green : Color.red;
            GizmoHelper.instance.drawRectangle(_offsetPosition,new Vector3(_miniStageInfo._overrideTriggerWidth * 0.5f, _miniStageInfo._overrideTriggerHeight * 0.5f),color);
            return;
        }

        Vector3 resultPoint;
        float fraction = getLimitedFractionOnLine(_currentPoint, _targetCameraControl.getCameraPosition(), out resultPoint);
        if(_stageData._stagePointData.Count - 1 > _currentPoint && _stageData._stagePointData[_currentPoint]._lerpCameraZoom)
        {
            float currentZoom = _stageData._stagePointData[_currentPoint]._cameraZoomSize;
            currentZoom = math.lerp(currentZoom, _stageData._stagePointData[_currentPoint + 1]._cameraZoomSize, fraction);
            CameraControlEx.Instance().setZoomSize(currentZoom,_stageData._stagePointData[_currentPoint]._cameraZoomSpeed);
        }

        if(_isEnd == false && _currentPoint < _stageData._stagePointData.Count - 1)
        {
            StagePointData stagePoint = _stageData._stagePointData[_currentPoint + 1];
            if(stagePoint._useTriggerBound)
            {
                GizmoHelper.instance.drawRectangle(stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,new Vector3(stagePoint._triggerWidth * 0.5f, stagePoint._triggerHeight * 0.5f),Color.green);
                bool intersectionResult = MathEx.intersectRect(_playerEntity.transform.position,stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,stagePoint._triggerWidth,stagePoint._triggerHeight);

                if(intersectionResult)
                {
                    fraction = 1f;
                    _cameraPositionBlendStartPosition = resultPoint;
                    _cameraPositionBlendTimeLeft = 1f;
                }
            }
        }

        if(_isEnd == false && fraction >= 1f)
        {
            startExitSequencers(_stageData._stagePointData[_currentPoint],_currentPoint,_currentPoint != 0);

            if(getNextPointIndex(ref _currentPoint) == false)
            {
                _isEnd = true;
            }
            else
            {
                startEnterSequencers(_stageData._stagePointData[_currentPoint],_currentPoint,true);
                CameraControlEx.Instance().setZoomSize(_stageData._stagePointData[_currentPoint]._cameraZoomSize,_stageData._stagePointData[_currentPoint]._cameraZoomSpeed);

                if(_spawnedCharacterEntityDictionary.ContainsKey(_currentPoint))
                {
                    for(int index = 0; index < _spawnedCharacterEntityDictionary[_currentPoint].Count; ++index)
                    {
                        if(_stageData._stagePointData[_currentPoint]._characterSpawnData[index]._activeType == StageSpawnCharacterActiveType.PointActivated)
                            _spawnedCharacterEntityDictionary[_currentPoint][index]._characterEntity?.setActiveSelf(true,false);
                    }
                }
            }
        }

        foreach(var item in _miniStageProcessor)
        {
            item.processStage(deltaTime);
        }

        if(_cameraPositionBlendTimeLeft != 0f)
        {
            resultPoint = Vector2.Lerp(_cameraPositionBlendStartPosition, resultPoint, MathEx.easeOutCubic(1f - _cameraPositionBlendTimeLeft) );
            _cameraPositionBlendTimeLeft -= deltaTime;
        }

        Vector2 trackPosition;
        if(_trackProcessor.isEnd() == false && _trackProcessor.processTrack(deltaTime,out trackPosition))
        {
            if(_trackProcessor.isEnd())
            {
                if(_trackProcessor.isEndBlend())
                {
                    _cameraTrackPositionError = (Vector3)trackPosition - resultPoint;
                    _cameraTrackPositionErrorReduceTime = 0f;
                }
                else
                {
                    _cameraTrackPositionErrorReduceTime = 1f;
                }
                
                _trackProcessor.clear();
            }
            else
            {
                resultPoint = trackPosition;
            }
        }

        if(_cameraTrackPositionErrorReduceTime < 1f)
        {
            _cameraTrackPositionErrorReduceTime += deltaTime;
            if(_cameraTrackPositionErrorReduceTime > 1f)
                _cameraTrackPositionErrorReduceTime = 1f;

            resultPoint = resultPoint + _cameraTrackPositionError * (1f - _cameraTrackPositionErrorReduceTime);
        }

        resultPoint.z = -10f;
        _targetCameraControl.setCameraPosition(resultPoint);
        for(int index = 0; index < _stageData._stagePointData.Count; ++index)
        {
            Color targetColor = index < _currentPoint ? Color.green : ( index == _currentPoint ? Color.magenta : Color.red);
            GizmoHelper.instance.drawCircle(_stageData._stagePointData[index]._stagePoint + _offsetPosition, 0.3f, 12, targetColor);
        }
    }

    public MovementTrackData getTrackData(string trackName)
    {
        if(_stageData == null)
            return null;
        foreach(MovementTrackData trackData in _stageData._trackData)
        {
            if(trackData._name == trackName)
                return trackData;
        }
        return null;
    }

    public void startCameraTrack(string trackName)
    {
        MovementTrackData trackData = getTrackData(trackName);
        if(trackData == null)
            return;
            
        startCameraTrack(trackData);
    }

    public void startCameraTrack(MovementTrackData trackData)
    {
        _trackProcessor.initialize(trackData);

        Vector2 trackStartPosition;
        if(_trackProcessor.getCurrentTrackStartPosition(out trackStartPosition) == false)
            return;

        Vector3 resultPosition = trackStartPosition;

        if(trackData._startBlend)
        {
            _cameraTrackPositionError = _targetCameraControl.getCameraPosition() - resultPosition;
            _cameraTrackPositionErrorReduceTime = 0f;
        }
        else
        {
            _cameraTrackPositionErrorReduceTime = 1f;
        }
    }

    public void startEnterSequencers(StagePointData pointData, int pointIndex, bool includePlayer)
    {
        if(pointData == null || pointData._onEnterSequencerPath == null)
            return;

        foreach(var path in pointData._onEnterSequencerPath)
        {
            _sequencerProcessManager.startSequencerFromStage(path,pointData,_spawnedCharacterEntityDictionary[pointIndex],null,_stageData._markerData,includePlayer);
        }
    }

    public void startExitSequencers(StagePointData pointData,int pointIndex,bool includePlayer)
    {
        if(pointData == null || pointData._onExitSequencerPath == null)
            return;

        foreach(var path in pointData._onExitSequencerPath)
        {
            _sequencerProcessManager.startSequencerFromStage(path,pointData,_spawnedCharacterEntityDictionary[pointIndex],null,_stageData._markerData,includePlayer);
        }
    }

    public bool isValid()
    {
        return _stageData != null;
    }

    public GameEntityBase getPlayerEntity()
    {
        return _playerEntity;
    }

    public bool getNextPointIndex(ref int resultIndex)
    {
        resultIndex++;
        if(resultIndex >= _stageData._stagePointData.Count)
        {
            resultIndex = _stageData._stagePointData.Count - 1;
            return false;
        }

        return true;
    }

    public bool getPrevPointIndex(ref int resultIndex)
    {
        resultIndex--;
        if(resultIndex < 0)
        {
            resultIndex = 0;
            return false;
        }

        return true;
    }

    private bool isInTriggerBound(int pointIndex, Vector3 position)
    {
        if(isValid() == false || pointIndex >= _stageData._stagePointData.Count - 1 || pointIndex < 0)
            return false;

        StagePointData stagePoint = _stageData._stagePointData[pointIndex + 1];
        if(stagePoint._useTriggerBound)
        {
            GizmoHelper.instance.drawRectangle(stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,new Vector3(stagePoint._triggerWidth * 0.5f, stagePoint._triggerHeight * 0.5f),Color.green);
            return MathEx.intersectRect(position,stagePoint._stagePoint + _offsetPosition + stagePoint._triggerOffset,stagePoint._triggerWidth,stagePoint._triggerHeight);
        }

        return false;
    }

    public void updatePointIndex(Vector3 position, ref int pointIndex)
    {
        if(isValid() == false)
            return;

        while(true)
        {
            float fraction = getLimitedFractionOnLine(pointIndex, position, out Vector3 resultPosition);
            if(fraction >= 1f)
            {
                if(getNextPointIndex(ref pointIndex) == false)
                    break;

                continue;
            }

            break;
        }
    }

    public bool isInCameraBound(int pointIndex, Vector3 position, out Vector3 resultPosition)
    {
        resultPosition = position;
        if(isValid() == false)
            return true;

        getLimitedFractionOnLine(pointIndex, position, out resultPosition);
        return CameraControlEx.Instance().IsInCameraBound(position, resultPosition, out resultPosition);
    }

    private float getLimitedFractionOnLine(int targetPointIndex, Vector3 targetPosition, out Vector3 resultPoint)
    {
        Vector3 targetPositionWithoutZ = MathEx.deleteZ(targetPosition);
        resultPoint = targetPosition;

        if(_stageData._stagePointData.Count == 0 || targetPointIndex >= _stageData._stagePointData.Count)
            return 0f;

        bool isEndPoint = targetPointIndex == _stageData._stagePointData.Count - 1;
        StagePointData startPoint = _stageData._stagePointData[targetPointIndex];
        StagePointData nextPoint = isEndPoint ? startPoint : _stageData._stagePointData[targetPointIndex + 1];

        Vector3 onLinePosition = targetPositionWithoutZ;
        float resultFraction = 0f;
        float limitedDistance = 0f;
        if(startPoint._lockCameraInBound)
        {
            onLinePosition = startPoint._stagePoint + _offsetPosition;
            limitedDistance = startPoint._maxLimitedDistance;
        }
        else
        {
            onLinePosition = isEndPoint ? (startPoint._stagePoint + _offsetPosition) : MathEx.getPerpendicularPointOnLineSegment((startPoint._stagePoint + _offsetPosition), (nextPoint._stagePoint + _offsetPosition),targetPositionWithoutZ);
            resultFraction = isEndPoint ? 1f : (onLinePosition - (startPoint._stagePoint + _offsetPosition)).magnitude * (1f / ((nextPoint._stagePoint + _offsetPosition) - (startPoint._stagePoint + _offsetPosition)).magnitude);
            limitedDistance = Mathf.Lerp(startPoint._maxLimitedDistance, nextPoint._maxLimitedDistance, resultFraction);
        }

        resultPoint = targetPositionWithoutZ;
        if(MathEx.distancef(targetPositionWithoutZ.y,onLinePosition.y) > limitedDistance)
        {
            if(targetPositionWithoutZ.y > onLinePosition.y)
                resultPoint.y = onLinePosition.y + limitedDistance;
            else
                resultPoint.y = onLinePosition.y - limitedDistance;
        }
        if(MathEx.distancef(targetPositionWithoutZ.x,onLinePosition.x) > limitedDistance)
        {
            if(targetPositionWithoutZ.x > onLinePosition.x)
                resultPoint.x = onLinePosition.x + limitedDistance;
            else
                resultPoint.x = onLinePosition.x - limitedDistance;
        }

        return resultFraction;
    }

}

