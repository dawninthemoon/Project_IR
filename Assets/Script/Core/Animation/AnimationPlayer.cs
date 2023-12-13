using System.Collections.Generic;
using UnityEngine;


public class AnimationPlayDataInfo
{
    public AnimationPlayDataInfo(){}

    //따로 때내어야 함
    public ActionFrameEventBase[]       _frameEventData = null;
    public ActionFrameEventBase[]       _timeEventData = null;

    public MultiSelectAnimationData[]   _multiSelectAnimationData = null;

    public AnimationTranslationPresetData _translationPresetData = null;
    public AnimationRotationPresetData  _rotationPresetData = null;
    public AnimationScalePresetData     _scalePresetData = null;
    public AnimationCustomPresetData    _customPresetData = null;
    public AnimationCustomPreset        _customPreset = null;

    public int                          _multiSelectAnimationDataCount = 0;

    public string                       _path = "";
    public float                        _framePerSec = -1f;
    public float                        _actionTime = -1f;
    public float                        _startFrame = -1f;
    public float                        _endFrame = -1f;

    public float                        _duration = -1f;

    public int                          _animationLoopCount = 0;
    public int                          _frameEventDataCount = -1;
    public int                          _timeEventDataCount = -1;
    public int                          _angleBaseAnimationSpriteCount = -1;

    public bool                         _isLoop = false;
    public bool                         _hasMovementGraph = false;
    public bool                         _isAngleBaseAnimation = false;
    public bool                         _multiSelectConditionUpdateOnce = false;


    public FlipState                    _flipState;

#if UNITY_EDITOR
    public int                          _lineNumber;
#endif
}

public class MultiSelectAnimationData
{
    public string                           _path = "";
    public ActionGraphConditionCompareData  _actionConditionData = null;
}

public struct FlipState
{
    public bool xFlip;
    public bool yFlip;
}

public struct FrameEventProcessDescription
{
    public ActionFrameEventBase     _targetFrameEvent;
    public ObjectBase               _executeObject;
    public float                    _endTime;
    public bool                     _isTimeBase;

    public void processFrameEvent()
    {
        if(_executeObject is GameEntityBase && _targetFrameEvent.checkCondition(_executeObject as GameEntityBase) == false)
            return;

        _targetFrameEvent.onExecute(_executeObject);
    }

    public void exitFrameEvent()
    {
        _targetFrameEvent.onExit(_executeObject);
    }
}

public class AnimationPlayer
{
    private AnimationTimeProcessor _animationTimeProcessor;
    private AnimationPlayDataInfo _currentAnimationPlayData;

    private string _currentAnimationName;
    private Sprite[] _currentAnimationSprites;
    private MovementGraph _currentMovementGraph;

    private bool _multiSelectAnimationUpdated = false;

    private int _currentAnimationFrameEventIndex;
    private int _currentFrameEventIndex;
    private int _currentTimeEventIndex;

    private List<FrameEventProcessDescription> _frameEventProcessList = new List<FrameEventProcessDescription>();

    public AnimationPlayer()
    {
        _animationTimeProcessor = new AnimationTimeProcessor();
    }

    public bool isValid()
    {
        return true;//_currentAnimationPlayData != null;
    }

    public void initialize()
    {
        _animationTimeProcessor.initialize();
    }

    public bool progress(float deltaTime, ObjectBase targetEntity)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"잘못된 플레이 데이터 입니다. 통보 요망");
            return false;
        }

        bool isEnd = _animationTimeProcessor.updateTime(deltaTime);
        if(isEnd == false && _animationTimeProcessor.isLoopedThisFrame())
        {
            _currentFrameEventIndex = 0;
            _currentAnimationFrameEventIndex = 0;
        }
            
        if(targetEntity != null)
        {
            processFrameEventContinue();
            processFrameEvent(_currentAnimationPlayData, targetEntity);
        }
        

        return _animationTimeProcessor.isEnd();
    } 

    public void Release()
    {
        
    }

    public void processMultiSelectAnimation(ActionGraph actionGraph)
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._multiSelectAnimationDataCount == 0)
            return;

        if(_currentAnimationPlayData._multiSelectConditionUpdateOnce && _multiSelectAnimationUpdated)
            return;

        _multiSelectAnimationUpdated = true;
        for(int i = 0; i < _currentAnimationPlayData._multiSelectAnimationDataCount; ++i)
        {
            if(actionGraph.processActionCondition(_currentAnimationPlayData._multiSelectAnimationData[i]._actionConditionData) == true)
            {
                _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(_currentAnimationPlayData._multiSelectAnimationData[i]._path);
                return;
            }
        }

        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(_currentAnimationPlayData._path);
    }
    

    public void processFrameEventContinue()
    {
        for(int i = 0; i < _frameEventProcessList.Count;)
        {
            _frameEventProcessList[i].processFrameEvent();

            if(_frameEventProcessList[i]._endTime <= _animationTimeProcessor.getAnimationTotalPlayTime() || 
               MathEx.equals(_frameEventProcessList[i]._endTime, _animationTimeProcessor.getAnimationTotalPlayTime(), float.Epsilon))
            {
                _frameEventProcessList[i].exitFrameEvent();
                _frameEventProcessList.RemoveAt(i);
            }
            else
                ++i;
        }
    }
    
    public void processFrameEvent(AnimationPlayDataInfo playData, ObjectBase targetEntity)
    {
        if(playData == null)
            return;

        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        if(playData._customPresetData != null && playData._customPresetData._effectFrameEvent != null && targetEntity is GameEntityBase)
        {
            for(int i = _currentAnimationFrameEventIndex; i < playData._customPresetData._effectFrameEvent.Length; ++i)
            {
                string effectEvent = playData._customPresetData._effectFrameEvent[i];
                if(MathEx.equals((float)i, currentFrame,float.Epsilon) == true || (float)i < currentFrame)
                {
                    EffectInfoManager.Instance().requestEffect(effectEvent,targetEntity as GameEntityBase, null,CommonMaterial.Empty);
                    _currentAnimationFrameEventIndex++;
                }
                else
                {
                    break;
                }
            }
        }

        for(int i = _currentFrameEventIndex; i < playData._frameEventDataCount; ++i)
        {
            if (playData._frameEventData == null || playData._frameEventData.Length == 0)
                break;

            ActionFrameEventBase frameEvent = playData._frameEventData[i];
            if(MathEx.equals(frameEvent._startFrame, currentFrame,float.Epsilon) == true || frameEvent._startFrame < currentFrame)
            {
                if(targetEntity is GameEntityBase && frameEvent.checkCondition(targetEntity as GameEntityBase) == false)
                {
                    _currentFrameEventIndex++;
                    continue;
                }
                
                frameEvent.initialize();
                if(frameEvent.onExecute(targetEntity) == true && frameEvent._endFrame > frameEvent._startFrame)
                {
                    FrameEventProcessDescription desc;
                    desc._executeObject = targetEntity;
                    desc._endTime = _animationTimeProcessor.frameToTime(frameEvent._endFrame);
                    desc._targetFrameEvent = frameEvent;
                    desc._isTimeBase = false;

                    _frameEventProcessList.Add(desc);
                }
                else
                {
                    frameEvent.onExit(targetEntity);
                }

                _currentFrameEventIndex++;
            }
            else
            {
                break;
            }
        }

        
        

        float currentTotalTime = _animationTimeProcessor.getAnimationTotalPlayTime();
        for(int i = _currentTimeEventIndex; i < playData._timeEventDataCount; ++i)
        {
            ActionFrameEventBase frameEvent = playData._timeEventData[i];
            if(MathEx.equals(frameEvent._startFrame, currentTotalTime,float.Epsilon) == true || frameEvent._startFrame < currentTotalTime)
            {
                if(targetEntity is GameEntityBase && frameEvent.checkCondition(targetEntity as GameEntityBase) == false)
                {
                    _currentTimeEventIndex++;
                    continue;
                }
                
                frameEvent.initialize();
                if(frameEvent.onExecute(targetEntity) == true && frameEvent._endFrame > frameEvent._startFrame)
                {
                    FrameEventProcessDescription desc;
                    desc._executeObject = targetEntity;
                    desc._endTime = frameEvent._endFrame;
                    desc._targetFrameEvent = frameEvent;
                    desc._isTimeBase = true;

                    _frameEventProcessList.Add(desc);
                }
                else
                {
                    frameEvent.onExit(targetEntity);
                }

                _currentTimeEventIndex++;
            }
            else
            {
                break;
            }
        }
    }

    private void setCurrentFrameEventIndex(AnimationPlayDataInfo playData)
    {
        float currentFrame = _animationTimeProcessor.getCurrentFrame();
        for(int i = 0; i < playData._frameEventDataCount; ++i)
        {
            _currentFrameEventIndex = i;
            if(playData._frameEventData[i]._startFrame >= currentFrame)
                break;
        }

        float currentTime = _animationTimeProcessor.getAnimationTotalPlayTime();
        for(int i = 0; i < playData._timeEventDataCount; ++i)
        {
            _currentTimeEventIndex = i;
            if(playData._timeEventData[i]._startFrame >= currentTime)
                break;
        }

        _currentAnimationFrameEventIndex = 0;
    }

    public void changeAnimation(AnimationPlayDataInfo playData)
    {
        _currentAnimationPlayData = playData;
        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(playData._path);
        if(playData._hasMovementGraph == true)
        {
            _currentMovementGraph = ResourceContainerEx.Instance().getMovementgraph(playData._path);
            DebugUtil.assert(_currentMovementGraph != null, "무브먼트 그래프가 존재하지 않는데 사용하려 합니다. : {0}", playData._path);        
        }

        DebugUtil.assert(_currentAnimationSprites != null, "애니메이션 스프라이트 배열이 null입니다. 해당 경로에 스프라이트가 존재 하나요? : {0}",playData._path);
        _multiSelectAnimationUpdated = false;

        _currentAnimationName = playData._path;

        float startFrame = playData._startFrame;
        startFrame = startFrame == -1f ? 0f : startFrame;

        float endFrame = playData._endFrame;
        endFrame = endFrame == -1f ? (float)_currentAnimationSprites.Length : endFrame;

        float framePerSecond = playData._framePerSec;

        if(framePerSecond == -1f && playData._actionTime != -1f)
            framePerSecond = ((float)(endFrame - startFrame) / playData._actionTime) * (playData._animationLoopCount > 0 ? (float)playData._animationLoopCount : 1.0f);

        if(playData._isAngleBaseAnimation)
        {
            endFrame = playData._angleBaseAnimationSpriteCount;
            endFrame = endFrame == -1f ? 1f : endFrame;
        }
        
        if(framePerSecond == -1f)
        {
            DebugUtil.assert(false, "애니메이션 FPS가 -1 입니다. 코드버그인듯? 통보 요망");
            framePerSecond = 1f;
        }

        _animationTimeProcessor.initialize();
        _animationTimeProcessor.setFrame(startFrame,endFrame, framePerSecond);
        _animationTimeProcessor.setLoop(playData._isLoop);
        _animationTimeProcessor.setLoopCount(playData._animationLoopCount);
        _animationTimeProcessor.setActionDuration(playData._duration);
        _animationTimeProcessor.setFrameToTime(startFrame);
        _animationTimeProcessor.setAnimationSpeed(1f);

        if(playData._customPresetData != null)
        {
            DebugUtil.assert(playData._customPresetData.getTotalDuration() > 0,"CustomPreset의 TotalDuration이 0이거나 음수 입니다. 데이터를 잘못 만든듯? : [Path : {0}]",playData._path);
            _animationTimeProcessor.setCustomPresetData(playData._customPresetData);
        }

        for(int i = 0; i < _frameEventProcessList.Count; ++i)
        {
            _frameEventProcessList[i].exitFrameEvent();
        }
        _frameEventProcessList.Clear();

        setCurrentFrameEventIndex(playData);
    }

    public void changeAnimationByCustomPreset(string path)
    {
        ScriptableObject[] scriptableObjects = ResourceContainerEx.Instance().GetScriptableObjects(path);
        if(scriptableObjects == null)
            return;

        if(scriptableObjects == null || (scriptableObjects[0] is AnimationCustomPreset) == false)
            return;

        AnimationCustomPreset animationCustomPreset = (scriptableObjects[0] as AnimationCustomPreset);

        changeAnimationByCustomPreset(path, animationCustomPreset);
    }

    public void changeAnimationByCustomPreset(string path, AnimationCustomPreset customPreset)
    {
        _currentAnimationPlayData = null;
        _currentAnimationSprites = ResourceContainerEx.Instance().GetSpriteAll(path);
        DebugUtil.assert(_currentAnimationSprites != null, "애니메이션 스프라이트 배열이 null입니다. 해당 경로에 스프라이트가 존재 하나요? : {0}",path);

        _currentAnimationName = path;

        _animationTimeProcessor.initialize();
        //_animationTimeProcessor.setActionDuration(customPreset._animationCustomPresetData.getTotalDuration());
        _animationTimeProcessor.setAnimationSpeed(1f);

        _animationTimeProcessor.setCustomPresetData(customPreset._animationCustomPresetData);

        for(int i = 0; i < _frameEventProcessList.Count; ++i)
        {
            _frameEventProcessList[i].exitFrameEvent();
        }
        _frameEventProcessList.Clear();
        _currentFrameEventIndex = -1;
        _currentAnimationFrameEventIndex = 0;
        _currentTimeEventIndex = 0;
        
    }

    public int angleToSectorNumberByAngleBaseSpriteCount(float angleDegree)
    {
        angleDegree = MathEx.clamp360Degree(angleDegree);
        float baseAngle = 360f / (float)_currentAnimationSprites.Length;
        if(angleDegree < baseAngle * 0.5f || angleDegree >= 360f - baseAngle * 0.5f)
            return 0;

        return (int)((angleDegree + baseAngle * 0.5f) / baseAngle);
    }

    public int angleToSectorNumberByCount(float angleDegree, int count)
    {
        angleDegree = MathEx.clamp360Degree(angleDegree);
        float baseAngle = 360f / (float)count;
        if(angleDegree < baseAngle * 0.5f || angleDegree > 360f - baseAngle * 0.5f)
            return 0;

        return (int)((angleDegree + baseAngle * 0.5f) / baseAngle);
    }

    public bool isEnd() {return _animationTimeProcessor.isEnd();}

    public string getCurrentAnimationName() {return _currentAnimationName;}

    public void setAnimationSpeed(float speed) {_animationTimeProcessor.setAnimationSpeed(speed);}

    public float getCurrentFrame() {return _animationTimeProcessor.getCurrentFrame();}
    public float getCurrentAnimationTime() {return _animationTimeProcessor.getCurrentAnimationTime();}
    public float getCurrentAnimationDuration() {return _animationTimeProcessor.getAnimationDuration();}

    public int getCurrentIndex() {return _animationTimeProcessor.getCurrentIndex();}
    public int getEndIndex() {return _animationTimeProcessor.getEndIndex();}

    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc() {return _animationTimeProcessor.getMoveValuePerFrameFromTimeDesc();}
    public AnimationTimeProcessor getTimeProcessor(){return _animationTimeProcessor;}
    public MovementGraph getCurrentMovementGraph() {return _currentMovementGraph;}

    public bool getCurrentAnimationTranslation(out Vector3 outTranslation)
    {
        if (_currentAnimationPlayData == null || _currentAnimationPlayData._translationPresetData == null)
        {
            outTranslation = Vector3.zero;
            return false;
        }

        Vector2 currentTranslation = _currentAnimationPlayData._translationPresetData.evaulate(_animationTimeProcessor.getCurrentAnimationNormalizedTime());
        outTranslation = new Vector3(currentTranslation.x, currentTranslation.y, 0f);
        return true;
    }

    public Vector3 getAnimationTranslationPerFrame()
    {
        if (_currentAnimationPlayData == null || _currentAnimationPlayData._translationPresetData == null)
            return Vector3.one;

        Vector2 translation = _currentAnimationPlayData._translationPresetData.getTranslationValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc());
        return new Vector3(translation.x, translation.y, 1f);
    }

    public bool getCurrentAnimationScale(out Vector3 outScale)
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._scalePresetData == null)
        {
            outScale = Vector3.one;
            return false;
        }
        
        Vector2 currentScale = _currentAnimationPlayData._scalePresetData.evaulate(_animationTimeProcessor.getCurrentAnimationNormalizedTime());
        outScale = new Vector3(currentScale.x,currentScale.y,1f);
        return true;
    }

    public Vector3 getAnimationScalePerFrame()
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._scalePresetData == null)
            return Vector3.one;
        

        Vector2 scale = _currentAnimationPlayData._scalePresetData.getScaleValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc());
        return new Vector3(scale.x,scale.y,1f);
    }


    public Quaternion getCurrentAnimationRotation()
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._rotationPresetData == null)
            return Quaternion.identity;

        return Quaternion.Euler(0f,0f,_currentAnimationPlayData._rotationPresetData.evaulate(_animationTimeProcessor.getCurrentAnimationNormalizedTime()));
    }

    public Quaternion getAnimationRotationPerFrame()
    {
        if(_currentAnimationPlayData == null || _currentAnimationPlayData._rotationPresetData == null)
            return Quaternion.identity;
        
        return Quaternion.Euler(0f,0f,_currentAnimationPlayData._rotationPresetData.getRotateValuePerFrameFromTime(getMoveValuePerFrameFromTimeDesc()));
    }

    public FlipState getCurrentFlipState() 
    {
        if (_currentAnimationPlayData == null)
            return new FlipState { xFlip = false, yFlip = false };
        return _currentAnimationPlayData._flipState;
    }

    public Sprite getCurrentSprite(float currentAngleDegree = 0f)
    {
        if(_currentAnimationSprites.Length <= _animationTimeProcessor.getCurrentIndex())
        {
            DebugUtil.assert(false, "스프라이트 Out Of Index 입니다. AnimationPreset이 잘못되어 있지는 않나요? End Frame을 확인해 주세요. [Length: {0}] [Index: {1}] [Loop: {2}] [Path: {3}]",_currentAnimationSprites.Length, _animationTimeProcessor.getCurrentIndex(), _animationTimeProcessor.isLoop(), _currentAnimationPlayData._path);
            return null;
        }

        if(_currentAnimationPlayData != null && _currentAnimationPlayData._isAngleBaseAnimation)
        {
            int index = angleToSectorNumberByAngleBaseSpriteCount(currentAngleDegree) + _animationTimeProcessor.getCurrentIndex();
            if(_currentAnimationSprites.Length <= index)
            {
                DebugUtil.assert(false, "잘못된 인덱스 입니다. 통보 요망 : [index : {0}] [angle : {1}] [timerProcessorIndex : {2}] [spriteCount : {3}]", index, currentAngleDegree,angleToSectorNumberByAngleBaseSpriteCount(currentAngleDegree), _animationTimeProcessor.getCurrentIndex() );
                return null;
            }

            return _currentAnimationSprites[angleToSectorNumberByAngleBaseSpriteCount(currentAngleDegree) + _animationTimeProcessor.getCurrentIndex()];
        }

        return _currentAnimationSprites[_animationTimeProcessor.getCurrentIndex()];
    }
}
