using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmakuGraph
{
    public class DanmakuPlayItem
    {
        public DanmakuGraphBaseData _data;
        public float[] _danmakuVariables = new float[(int)DanmakuVariableType.Count];
        
        public List<DanmakuLoopPlayItem> _currentLoopProcess = new List<DanmakuLoopPlayItem>();
        public SimplePool<DanmakuLoopPlayItem> _loopEventPool = new SimplePool<DanmakuLoopPlayItem>();

        public SearchIdentifier _forceSearchIdentifier = SearchIdentifier.Count;
        public Vector3 _forcePosition = Vector3.zero;
        public Vector3 _offsetPosition = Vector3.zero;

        public bool _isEnd = false;
        public bool _eventEnd = false;
        public bool _useFlip = false;
        public int _currentWaitIndex = -1;
        public float _currentWaitTime = 0f;

        public void initialize(DanmakuGraphBaseData data)
        {
            _data = data;

            for(int i = 0; i < _danmakuVariables.Length; ++i)
                _danmakuVariables[i] = 0f;
            
            _isEnd = false;
            _eventEnd = false;

            _currentWaitIndex = -1;
            _currentWaitTime = 0f;

            for(int i = 0; i < _currentLoopProcess.Count; ++i)
                _loopEventPool.enqueue(_currentLoopProcess[i]);
            _currentLoopProcess.Clear();
        }

        public ProjectileGraphShotInfoData createShotInfo()
        {
            ProjectileGraphShotInfoData shotInfoData;
            shotInfoData._acceleration = _danmakuVariables[(int)DanmakuVariableType.Acceleration];
            shotInfoData._defaultAngle = _danmakuVariables[(int)DanmakuVariableType.Angle];
            shotInfoData._angularAcceleration = _danmakuVariables[(int)DanmakuVariableType.AngularAccel];
            shotInfoData._friction = _danmakuVariables[(int)DanmakuVariableType.Friction];
            shotInfoData._lifeTime = _danmakuVariables[(int)DanmakuVariableType.LifeTime];
            shotInfoData._deafaultVelocity = _danmakuVariables[(int)DanmakuVariableType.Velocity];
            shotInfoData._useRandomAngle = false;
            shotInfoData._randomAngle = Vector2.zero;

            return shotInfoData;
        }

        public void variDanmakuVariable(DanmakuVariableType variableType, DanmakuVariableEventType eventType, float value)
        {
            ref float variable = ref _danmakuVariables[(int)variableType];

            switch(eventType)
            {
                case DanmakuVariableEventType.Add:
                    variable += value;
                break;
                case DanmakuVariableEventType.Div:
                    variable /= value;
                break;
                case DanmakuVariableEventType.Mul:
                    variable *= value;
                break;
                case DanmakuVariableEventType.Set:
                    variable = value;
                break;
                case DanmakuVariableEventType.Count:
                    DebugUtil.assert(false,"잘못된 이벤트 타입입니다. 이게 말이 되나 : {0}", eventType);
                break;
            }
        }

        public bool isEnd()
        {
            return _isEnd;
        }
    }

    public class DanmakuLoopPlayItem
    {
        public DanmakuLoopEventData _loopData;
        public float _term = 0f;
        public int _loopCount = 0;

        public DanmakuLoopPlayItem() {}
        public void initialize(DanmakuLoopEventData eventData)
        {
            _loopData = eventData;
            _term = 0;
            _loopCount = _loopData._loopCount;
        }
    }

    private ObjectBase _ownerEntity;

    private List<DanmakuPlayItem> _currentPlayList = new List<DanmakuPlayItem>();
    private SimplePool<DanmakuPlayItem> _danmakuPlayItemPool = new SimplePool<DanmakuPlayItem>();

    public void initialize(ObjectBase entity)
    {
        _ownerEntity = entity;

        for(int i = 0; i < _currentPlayList.Count; ++i)
        {
            _danmakuPlayItemPool.enqueue(_currentPlayList[i]);
        }
        _currentPlayList.Clear();
    }

    public void process(float deltaTime)
    {
        for(int i = 0; i < _currentPlayList.Count;)
        {
            processDanmaku(deltaTime,_currentPlayList[i]);
            if(_currentPlayList[i].isEnd() == true)
            {
                _danmakuPlayItemPool.enqueue(_currentPlayList[i]);
                _currentPlayList.RemoveAt(i);
            }
            else
                ++i;
        }
    }

    public void release()
    {

    }


    private void processDanmaku(float deltaTime, DanmakuPlayItem playItem)
    {
        for(int i = 0; i < playItem._currentLoopProcess.Count;)
        {
            if(processDanamkuLoopEvent(deltaTime, playItem._currentLoopProcess[i], playItem) == true)
            {
                playItem._loopEventPool.enqueue(playItem._currentLoopProcess[i]);
                playItem._currentLoopProcess.RemoveAt(i);
            }

            return;
        }

        playItem._isEnd = playItem._eventEnd && playItem._currentLoopProcess.Count == 0;

        if(playItem._eventEnd)
            return;
         
        if(playItem._currentWaitTime > 0f)
        {
            playItem._currentWaitTime -= deltaTime;
            if(playItem._currentWaitTime > 0f)
                return;
        }

        for(int i = playItem._currentWaitIndex + 1; i < playItem._data._danamkuEventCount; ++i)
        {
            processDanmakuEvent(playItem._data._danamkuEventList[i],playItem);
            if(playItem._data._danamkuEventList[i].getEventType() == DanmakuEventType.WaitEvent ||
                playItem._data._danamkuEventList[i].getEventType() == DanmakuEventType.LoopEvent)
            {
                playItem._currentWaitIndex = i;
                return;
            }
        }

        playItem._currentWaitIndex = playItem._data._danamkuEventCount;
        playItem._eventEnd = true;
    }

    private bool processDanamkuLoopEvent(float deltaTime, DanmakuLoopPlayItem loopPlayItem, DanmakuPlayItem playItem)
    {
        loopPlayItem._term -= deltaTime;
        while(loopPlayItem._term <= 0f)
        {
            for(int eventIndex = 0; eventIndex < loopPlayItem._loopData._eventCount; ++eventIndex )
            {
                processDanmakuEvent(loopPlayItem._loopData._events[eventIndex], playItem);
            }

            if(--loopPlayItem._loopCount <= 0)
                return true;

            loopPlayItem._term = loopPlayItem._loopData._term;
        }

        return false;
    }

    private void addDanmakuLoopEvent(DanmakuLoopEventData loopEventData, DanmakuPlayItem playItem)
    {
        DanmakuLoopPlayItem loopPlayItem = playItem._loopEventPool.dequeue();
        loopPlayItem.initialize(loopEventData);

        playItem._currentLoopProcess.Add(loopPlayItem);
    }

    private void processDanmakuEvent(DanmakuEventBase eventTarget, DanmakuPlayItem playItem)
    {
        switch(eventTarget.getEventType())
        {
            case DanmakuEventType.VariableEvent:
            {
                DanmakuVariableEventData eventData = (DanmakuVariableEventData)eventTarget;
                for(int i = 0; i < eventData._eventCount; ++i)
                {
                    playItem.variDanmakuVariable(eventData._type,eventData._eventType[i], eventData._value[i].getValue());
                }
            }
            break;
            case DanmakuEventType.ProjectileEvent:
            {
                DanmakuProjectileEventData eventData = (DanmakuProjectileEventData)eventTarget;

                ProjectileGraphBaseData baseData = ProjectileManager._instance.getProjectileGraphData(eventData._projectileName);
                if(baseData == null)
                    return;

                ProjectileGraphShotInfoData shotInfo = baseData._defaultProjectileShotInfoData;
                
                switch(eventData._shotInfoUseType)
                {
                    case ActionFrameEvent_Projectile.ShotInfoUseType.UseDefault:
                    break;
                    case ActionFrameEvent_Projectile.ShotInfoUseType.Overlap:
                    shotInfo = playItem.createShotInfo();
                    break;
                    case ActionFrameEvent_Projectile.ShotInfoUseType.Add:
                    shotInfo += playItem.createShotInfo();
                    break;
                }
                
                GameEntityBase gameEntityOwner = _ownerEntity == null ? null : (_ownerEntity is GameEntityBase ? _ownerEntity as GameEntityBase : null);

                Vector3 direction = Vector3.right;
                if(eventData._directionType != DirectionType.Count && _ownerEntity != null && gameEntityOwner != null)
                    direction = gameEntityOwner.getDirectionFromType(eventData._directionType);
                
                ObjectBase targetEntity = null;
                if(gameEntityOwner != null)
                    targetEntity = (_ownerEntity as GameEntityBase).getCurrentTargetEntity();

                float angle = MathEx.directionToAngle(direction);

                shotInfo._defaultAngle += angle;
                Vector3 spawnPosition = _ownerEntity == null ? playItem._forcePosition : ActionFrameEvent_Projectile.getSpawnPosition(eventData._setTargetType,_ownerEntity,targetEntity);

                Vector3 offset = playItem._offsetPosition;
                if(playItem._useFlip && gameEntityOwner != null && gameEntityOwner.getFlipState().xFlip)
                    offset.y *= -1f;

                Quaternion rotateByQuat = Quaternion.Euler(0f,0f,angle);
                offset = rotateByQuat * offset;

                spawnPosition += offset;

                if(eventData._startTerm != 0f && gameEntityOwner != null)
                {
                    if(eventData._pathPredictionArray != null)
                    {
                        switch(eventData._predictionType)
                        {
                            case ActionFrameEvent_Projectile.PredictionType.Path:
                            {
                                ActionFrameEvent_Projectile.predictionPath(eventData._predictionAccuracy,eventData._pathPredictionArray,ref shotInfo);

                                EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
                                requestData.clearRequestData();
                                requestData._updateType = EffectUpdateType.NoneScaledDeltaTime;
                                requestData._effectType = EffectType.TrailEffect;
                                requestData._lifeTime = eventData._startTerm;
                                requestData._parentTransform = _ownerEntity.transform;
                                requestData._trailWidth = ProjectileManager._instance.getProjectileGraphData(eventData._projectileName)._collisionRadius * 2f;
                                requestData._trailMaterial = ResourceContainerEx.Instance().GetMaterial("Material/Material_TrailBase");
                                requestData._trailPositionData = eventData._pathPredictionArray;

                                _ownerEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);
                            }
                            break;
                            case ActionFrameEvent_Projectile.PredictionType.StartPosition:
                            {
                                EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
                                requestData.clearRequestData();
                                requestData._effectPath = "Resources/Sprites/Effect/ProjectilePredictionPosition";
                                requestData._startFrame = 0f;
                                requestData._endFrame = -1f;
                                requestData._framePerSecond = 1.0f / eventData._startTerm;
                                requestData._position = spawnPosition;
                                requestData._usePhysics = false;
                                requestData._rotation = Quaternion.identity;
                                requestData._effectType = EffectType.SpriteEffect;
                                requestData._updateType = EffectUpdateType.NoneScaledDeltaTime;
                                requestData._castShadow = false;

                                float radius = ProjectileManager._instance.getProjectileGraphData(eventData._projectileName)._collisionRadius * 2f;
                                requestData._scale = new Vector3(radius,radius,1f);

                                _ownerEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);
                            }
                            break;
                        }
                    }

                    ProjectileManager._instance.spawnProjectileDelayed(eventData._projectileName, eventData._startTerm,_ownerEntity,targetEntity,eventData._setTargetType,ref shotInfo,_ownerEntity,_ownerEntity == null ? playItem._forceSearchIdentifier : _ownerEntity._searchIdentifier);
                }
                else
                {
                    ProjectileManager._instance.spawnProjectile(eventData._projectileName,ref shotInfo,spawnPosition,_ownerEntity,_ownerEntity == null ? playItem._forceSearchIdentifier : _ownerEntity._searchIdentifier);
                }
            }
            break;
            case DanmakuEventType.LoopEvent:
            {
                DanmakuLoopEventData eventData = (DanmakuLoopEventData)eventTarget;
                addDanmakuLoopEvent(eventData, playItem);
            }
            break;
            case DanmakuEventType.WaitEvent:
            {
                DanmakuWaitEventData eventData = (DanmakuWaitEventData)eventTarget;
                playItem._currentWaitTime = eventData._waitTime;
            }
            break;
        }
    }

    public void addDanmakuGraph(string graphPath, Vector3 forcePosition, Vector3 offset, bool useFlip, SearchIdentifier searchIdentifier)
    {
        DanmakuGraphBaseData graphData = ResourceContainerEx.Instance().GetDanmakuGraph(graphPath);
        if(graphData == null)
            return;

        DanmakuPlayItem playItem = _danmakuPlayItemPool.dequeue();
        playItem.initialize(graphData);
        playItem._forcePosition = forcePosition;
        playItem._forceSearchIdentifier = searchIdentifier;
        playItem._offsetPosition = offset;
        playItem._useFlip = useFlip;

        _currentPlayList.Add(playItem);
    }

}