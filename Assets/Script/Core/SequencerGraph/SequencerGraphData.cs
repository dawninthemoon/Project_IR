using UnityEngine;
using System.Xml;
using System.Collections.Generic;

public enum SequencerGraphEventType
{
    SpawnCharacter,
    WaitSecond,
    SetCameraTarget,
    SetCameraPosition,
    SetAudioListner,
    SetCrossHair,
    SetHPSphere,
    WaitTargetDead,
    TeleportTargetTo,
    ApplyPostProcessProfile,
    SaveEventExecuteIndex,
    CallAIEvent,
    WaitSignal,
    SetCameraZoom,
    ZoomEffect,
    FadeIn,
    FadeOut,
    ForceQuit,
    BlockInput,
    BlockAI,
    SetAction,
    PlayAnimation,
    AIMove,
    QTEFence,
    DeadFence,
    SetHideUI,
    ShakeEffect,
    SetTimeScale,
    NextStage,
    ToastMessage,
    Task,
    LetterBoxShow,
    LetterBoxHide,
    TalkBalloon,
    Dialogue,
    CameraTrack,
    TaskFence,

    Count,
}

public abstract class SequencerGraphEventBase
{
    public abstract SequencerGraphEventType getSequencerGraphEventType();
    public abstract void Initialize(SequencerGraphProcessor processor);
    public abstract bool Execute(SequencerGraphProcessor processor, float deltaTime);
    public virtual void Exit(SequencerGraphProcessor processor) {}
    public abstract void loadXml(XmlNode node);
}

public class SequencerGraphEvent_TaskFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TaskFence;

    public string _taskName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return processor.isTaskEnd(_taskName);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "TaskName")
                _taskName = attrValue;
        }
    }
}

public class SequencerGraphEvent_WaitSignal : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitSignal;

    public string _targetSignal = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return processor.checkSignal(_targetSignal);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Signal")
                _targetSignal = attrValue;
        }
    }
}

public class SequencerGraphEvent_CameraTrack : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.CameraTrack;

    private string _trackName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.startCameraTrack(_trackName);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "TrackName")
                _trackName = attributes[i].Value;
        }

        if(_trackName == "")
            DebugUtil.assert(false,"Track Name이 존재하지 않습니다. 이거 필수임");
    }
}

public class SequencerGraphEvent_Dialogue : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.Dialogue;

    private string _key = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        bool dialogueKeyExists = DialogueManager.Instance().StartDialogue(_key);
        if (!dialogueKeyExists)
        {
            DebugUtil.assert(false, "존재하지 않는 Dialogue Key 입니다. 데이터를 확인 해 주세요. [Key: {0}]", _key);
            return true;
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for (int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name.Equals("DialogueKey"))
                _key = attributes[i].Value;
        }

        if(string.IsNullOrEmpty(_key))
            DebugUtil.assert(false, "Dialogue Key가 존재하지 않습니다. 이거 필수임");
    }
}

public class SequencerGraphEvent_TalkBalloon : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TalkBalloon;

    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";

    private string _key = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        SubtitleData_SimpleTalk simpleTalkData = SubtitleManager.Instance().getSimpleTalkData(_key);
        if(simpleTalkData == null)
        {
            DebugUtil.assert(false,"존재하지 않는 SimpleTalk Key 입니다. 데이터를 확인 해 주세요. [Key: {0}]",_key);
            return true;
        }

        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            TalkBalloonManager.Instance().activeTalkBalloon(uniqueEntity.transform,new UnityEngine.Vector3(0f, uniqueEntity.getHeadUpOffset(),0f),simpleTalkData._text,simpleTalkData._time);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                TalkBalloonManager.Instance().activeTalkBalloon(item.transform,new UnityEngine.Vector3(0f, item.getHeadUpOffset(),0f),simpleTalkData._text,simpleTalkData._time);
            }
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "SimpleTalkKey")
                _key = attributes[i].Value;
            else if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
        }

        if(_key == "")
            DebugUtil.assert(false,"SimpleTalk Key가 존재하지 않습니다. 이거 필수임");
    }
}

public class SequencerGraphEvent_LetterBoxHide : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.LetterBoxHide;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        LetterBox._instance.Hide();
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
}

public class SequencerGraphEvent_LetterBoxShow : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.LetterBoxShow;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        LetterBox._instance.Show();
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
}

public class SequencerGraphEvent_Task : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.Task;

    public SequencerGraphEventBase[] _eventList = null;
    public SequencerGraphProcessor.TaskProcessType taskProcessType;
    public string _taskName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
        foreach (var item in _eventList)
        {
            item.Initialize(processor);
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(taskProcessType == SequencerGraphProcessor.TaskProcessType.Count)
            return true;
            
        processor.addTask(_eventList,taskProcessType, _taskName);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;

        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "ProcessType")
                taskProcessType = (SequencerGraphProcessor.TaskProcessType)System.Enum.Parse(typeof(SequencerGraphProcessor.TaskProcessType), attributes[i].Value);
            else if(attributes[i].Name == "Name")
                _taskName = attributes[i].Value;
        }

        if(taskProcessType == SequencerGraphProcessor.TaskProcessType.Count)
        {
            DebugUtil.assert(false, "TaskProcessType은 필수입니다");
            return;
        }

        List<SequencerGraphEventBase> eventList = new List<SequencerGraphEventBase>();
        for(int index = 0; index < node.ChildNodes.Count; ++index)
        {
            eventList.Add(SequencrGraphLoader.readEventData(node.ChildNodes[index]));
        }
        _eventList = eventList.ToArray();
    }
}


public class SequencerGraphEvent_ToastMessage : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ToastMessage;

    public string _text = "";
    public float _time = 1f;
    public Color _color = Color.white;


    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ToastMessage._instance.ShowToastMessage(_text,_time,_color);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Text")
                _text = attributes[i].Value;
            else if(attributes[i].Name == "Time")
                _time = float.Parse(attributes[i].Value);
            else if(attributes[i].Name == "Color")
                _color = XMLScriptConverter.valueToLinearColor(attributes[i].Value);
            
        }
    }
}

public class SequencerGraphEvent_NextStage : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.NextStage;

    public string _stageDataPath = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        MasterManager.instance._stageProcessor.requestStartStage(_stageDataPath, "");
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
                _stageDataPath = attributes[i].Value;
        }
    }
}

public class SequencerGraphEvent_SetTimeScale : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetTimeScale;

    public float _targetTimeScale = 0f;
    public float _timeScalingTime = 0f;
    public float _timeScaleBlendTime = 0f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        Vector3Data data = MessageDataPooling.GetMessageData<Vector3Data>();
        data.value = new UnityEngine.Vector3(_targetTimeScale, _timeScalingTime, _timeScaleBlendTime);

        Message msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.entity_setTimeScale,0,data,null);
        MasterManager.instance.HandleMessage(msg);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _targetTimeScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Time")
                _timeScalingTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "BlendTime")
                _timeScaleBlendTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_ShakeEffect : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ShakeEffect;

    public float _shakeTime = 0f;
    public float _shakeScale = 0f;
    public float _shakeSpeed = 1f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setShake(_shakeScale, _shakeSpeed, _shakeTime);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Scale")
                _shakeScale = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Time")
                _shakeTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Speed")
                _shakeSpeed = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}


public class SequencerGraphEvent_SetHideUI : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetHideUI;

    public bool _value;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        HPSphereUIManager.Instance().setActive(_value == false);
        CrossHairUI._instance.setActive(_value == false);
        ScreenIndicatorUI._instance.setActive(_value == false);
        ScreenDirector._instance.setActiveMainHud(_value == false);

        processor.getUniqueEntity("Player")?.setGraphicInterfaceActive(_value == false);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Hide")
                _value = bool.Parse(attrValue);
        }
    }
}

public class SequencerGraphEvent_QTEFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.QTEFence;

    private string _keyName = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return ActionKeyInputManager.Instance().keyCheck(_keyName);
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "KeyName")
                _keyName = attrValue;
        }
    }
}

public class SequencerGraphEvent_DeadFence : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.DeadFence;

    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        bool success = true;
        if(_uniqueKey != "")
            success = processor.getUniqueEntity(_uniqueKey) == null;
        
        if(success && _uniqueGroupKey != "")
        {
            var list = processor.getUniqueGroup(_uniqueGroupKey);
            if(list != null)
                success = list.Count == 0;
        }
        
        return success;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
        }
    }
}


public class SequencerGraphEvent_CallAIEvent : SequencerGraphEventBase
{
    enum SequencerCallAIEventTargetType
    {
        UniqueTarget,
        Range,
    };

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.CallAIEvent;

    private string _customAiEventName = "";
    private string _uniqueKey = "";

    private SequencerCallAIEventTargetType _eventTargetType = SequencerCallAIEventTargetType.UniqueTarget;

    private List<CharacterEntityBase> _rangeSearchEntityList = new List<CharacterEntityBase>();
    private SearchIdentifier _searchIdentifier = SearchIdentifier.Count;

    private float _range = 0f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ObjectBase executeTargetEntity = processor.getUniqueEntity(_uniqueKey);
        if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
            return true;

        switch(_eventTargetType)
        {
            case SequencerCallAIEventTargetType.UniqueTarget:
            {
                (executeTargetEntity as GameEntityBase).executeCustomAIEvent(_customAiEventName);
            }
            break;
            case SequencerCallAIEventTargetType.Range:
            {
                SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
                _rangeSearchEntityList.Clear();
                sceneCharacterManager.targetSearchRange(executeTargetEntity.transform.position,_range,_searchIdentifier,ref _rangeSearchEntityList);

                foreach(var item in _rangeSearchEntityList)
                {
                    if(item == null || item is GameEntityBase == false)
                        continue;
                        
                    (item as GameEntityBase).executeCustomAIEvent(_customAiEventName);
                }
            }
            break;
        }

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "EventName")
            {
                _customAiEventName = attributes[i].Value;
            }
            else if(attrName == "UniqueKey")
            {
                _uniqueKey = attributes[i].Value;
            }
            else if(attrName == "EventTargetType")
            {
                _eventTargetType = (SequencerCallAIEventTargetType)System.Enum.Parse(typeof(SequencerCallAIEventTargetType), attrValue);
            }
            else if(attrName == "Range")
            {
                _range = float.Parse(attrValue);
            }
            else if(attrName == "SearchIdentifier")
            {
                _searchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), attrValue);
            }
        }
    }
}

public class SequencerGraphEvent_FadeIn : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.FadeIn;

    private float _lambda = -1f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFadeIn(_lambda);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Lambda")
                _lambda = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_AIMove : SequencerGraphEventBase
{
    private enum AnimationState
    {
        Start,
        Loop,
        End
    };

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.AIMove;

    private string _uniqueKey;

    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private Vector3 _endAnimationStartPosition;

    private AnimationState _currentAnimationState = AnimationState.Start;

    private string _startAction = "";
    private string _loopAction = "";
    private string _endAction = "";

    private string _markerName = "";

    private int _startActionIndex = -1;
    private int _loopActionIndex = -1;
    private int _endActionIndex = -1;

    private float _totalAnimationPlayTime = 0f;

    private float _totalLoopAnimationPlayTime = 0f;

    private float _startAnimationPlayTime = 0f;
    private float _endAnimationPlayTime = 0f;
    private float _loopAnimationPlayTime = 0f;


    private float _processTimer = 0f;
    private bool _loopActionOnly = false;


    private float _startActionDistance = 0f;
    private float _loopActionDistance = 0f;
    private float _endActionDistance = 0f;

    private float _totalLoopActionDistance = 0f;

    private bool _firstUpdate = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return;
        }

        if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
                _endPosition = item._position;
        }

        _totalAnimationPlayTime = 0f;
        _totalLoopAnimationPlayTime = 0f;

        _startActionDistance = 0f;
        _endActionDistance = 0f;
        _loopActionDistance = 0f;

        if(_startAction != "")
        {
            _startActionIndex = uniqueEntity.getActionIndex(_startAction);
            MovementGraphPresetData presetData = uniqueEntity.getMovementGraphPresetDataFromActionIndex(_startActionIndex);
            if(presetData == null)
            {
                DebugUtil.assert(false,"해당 액션에 MovementGraphPreset 설정이 안되어 있습니다. 확인 필요 [Action: {0}]", _startAction);
                return;
            }

            _startActionDistance = presetData.getTotalMovement();
            _startAnimationPlayTime = uniqueEntity.getAnimationPlayTimeFromActionIndex(_startActionIndex);
            _totalAnimationPlayTime += _startAnimationPlayTime;
        }
        if(_endAction != "")
        {
            _endActionIndex = uniqueEntity.getActionIndex(_endAction);
            MovementGraphPresetData presetData = uniqueEntity.getMovementGraphPresetDataFromActionIndex(_endActionIndex);
            if(presetData == null)
            {
                DebugUtil.assert(false,"해당 액션에 MovementGraphPreset 설정이 안되어 있습니다. 확인 필요 [Action: {0}]", _endAction);
                return;
            }

            _endActionDistance = presetData.getTotalMovement();
            _endAnimationPlayTime = uniqueEntity.getAnimationPlayTimeFromActionIndex(_endActionIndex);
            _totalAnimationPlayTime += _endAnimationPlayTime;
        }
        
        if(_startAction == "" && _endAction == "")
            _loopActionOnly = true;

        {
            _loopActionIndex = uniqueEntity.getActionIndex(_loopAction);
            MovementGraphPresetData presetData = uniqueEntity.getMovementGraphPresetDataFromActionIndex(_loopActionIndex);
            if(presetData == null)
            {
                DebugUtil.assert(false,"해당 액션에 MovementGraphPreset 설정이 안되어 있습니다. 확인 필요 [Action: {0}]", _loopAction);
                return;
            }

            _loopActionDistance = presetData.getTotalMovement();
            _loopAnimationPlayTime = uniqueEntity.getAnimationPlayTimeFromActionIndex(_loopActionIndex);
        }

        _firstUpdate = true;
        _processTimer = 0f;
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_loopAction == "")
            return true;

        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        if(_firstUpdate)
        {
            float startEndDistance = _startActionDistance + _endActionDistance;
            _startPosition = uniqueEntity.transform.position;

            float moveDistance = Vector3.Distance(_startPosition, _endPosition);
            if(_loopActionOnly || moveDistance < startEndDistance)
            {
                _currentAnimationState = AnimationState.Loop;

                float rate = moveDistance * (1f / _loopActionDistance);
                _totalLoopActionDistance = _loopActionDistance * rate;
                _totalLoopAnimationPlayTime = _loopAnimationPlayTime * rate;

                _totalAnimationPlayTime = 0f;

                _loopActionOnly = true;
            }
            else
            {
                _currentAnimationState = AnimationState.Start;
                float rate = (moveDistance - startEndDistance) * (1f / _loopActionDistance);
                _totalLoopActionDistance = _loopActionDistance * rate;
                _totalLoopAnimationPlayTime = _loopAnimationPlayTime * rate;
            }

            _totalAnimationPlayTime += _totalLoopAnimationPlayTime;

            Vector3 direction = (_endPosition - _startPosition).normalized;
            uniqueEntity.blockAI(true);
            uniqueEntity.setDirection(direction);
            uniqueEntity.setAiDirection(direction);
            uniqueEntity.setDirectionType(DirectionType.AI);

            if(_loopActionOnly)
                uniqueEntity.setAction(_loopActionIndex);
            else
                uniqueEntity.setAction(_startActionIndex);
            
            _endAnimationStartPosition = _startPosition + direction * (_startActionDistance + _totalLoopActionDistance);

            _firstUpdate = false;
            return false;
        }

        _processTimer += deltaTime;
        
        switch(_currentAnimationState)
        {
            case AnimationState.Start:
            {
                if(_processTimer >= _startAnimationPlayTime)
                {
                    _processTimer = _startAnimationPlayTime;
                    uniqueEntity.setAction(_loopActionIndex);
                    _currentAnimationState = AnimationState.Loop;
                }
            }
            break;
            case AnimationState.Loop:
            {
                if(_loopActionOnly == false && _processTimer >= _startAnimationPlayTime + _totalLoopAnimationPlayTime)
                {
                    _processTimer = _startAnimationPlayTime + _totalLoopAnimationPlayTime;
                    uniqueEntity.setAction(_endActionIndex);
                    uniqueEntity.transform.position = _endAnimationStartPosition;
                    _currentAnimationState = AnimationState.End;
                }
            }
            break;
        }

        GizmoHelper.instance.drawCircle(_startPosition,0.2f,16,Color.green);
        GizmoHelper.instance.drawCircle(_endPosition,0.2f,16,Color.green);
        GizmoHelper.instance.drawLine(_startPosition,_endPosition,Color.green);
        
        if(_processTimer >= _totalAnimationPlayTime)
        {
            uniqueEntity.transform.position = _endPosition;
            uniqueEntity.blockAI(false);
            uniqueEntity.setDefaultAction();
            return true;
        }

        return false;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "StartAction")
                _startAction = attrValue;
            else if(attrName == "LoopAction")
                _loopAction = attrValue;
            else if(attrName == "EndAction")
                _endAction = attrValue;
            else if(attrName == "EndPosition")
                _endPosition = XMLScriptConverter.valueToVector3(attrValue);
            else if(attrName == "EndPositionMarker")
                _markerName = attrValue;
        }

        DebugUtil.assert(_loopAction != "", "Loop Action은 필수입니다. [Line: {0}]", XMLScriptConverter.getLineNumberFromXMLNode(node));
    }
}

public class SequencerGraphEvent_PlayAnimation : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.PlayAnimation;

    private string _animationPath;
    private string _uniqueKey;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        uniqueEntity.blockAI(true);
        uniqueEntity.setDummyAction();
        uniqueEntity.changeAnimationByPath(_animationPath);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "Path")
                _animationPath = attrValue;
        }
    }
}


public class SequencerGraphEvent_SetAction : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetAction;

    private string _actionName;
    private string _uniqueKey = "";
    private string _uniqueGroupKey = "";

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_uniqueKey != "")
        {
            GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
            if(uniqueEntity == null)
            {
                DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
                return true;
            }

            uniqueEntity.setAction(_actionName);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
            {
                DebugUtil.assert(false,"대상 Unique Group이 존재하지 않습니다 : {0}",_uniqueGroupKey);
                return true;
            }

            foreach(var item in uniqueGroup)
            {
                item.setAction(_actionName);
            }
        }
        

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attrValue;
            else if(attrName == "Action")
                _actionName = attrValue;
        }
    }
}

public class SequencerGraphEvent_BlockAI : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.BlockAI;

    public string _uniqueKey = "";
    public string _uniqueGroupKey = "";
    public bool _value = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_uniqueKey != "")
        {
            ObjectBase executeTargetEntity = processor.getUniqueEntity(_uniqueKey);
            if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
                return true;

            (executeTargetEntity as GameEntityBase).blockAI(_value);
        }

        if(_uniqueGroupKey != "")
        {
            var uniqueGroup = processor.getUniqueGroup(_uniqueGroupKey);
            if(uniqueGroup == null)
                return true;

            foreach(var item in uniqueGroup)
            {
                item.blockAI(_value);
            }
        }
        

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "Enable")
                _value = bool.Parse(attrValue);
            else if(attrName == "UniqueKey")
                _uniqueKey = attributes[i].Value;
            else if(attrName == "UniqueGroupKey")
                _uniqueGroupKey = attributes[i].Value;
        }
    }
}

public class SequencerGraphEvent_BlockInput : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.BlockInput;

    private bool _value = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase playerEntity = processor.getUniqueEntity("Player");
        if(playerEntity == null)
        {
            DebugUtil.assert(false,"플레이어가 존재하지 않습니다.");
            return true;
        }

        playerEntity.blockInput(_value);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Enable")
                _value = bool.Parse(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_ForceQuit : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ForceQuit;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return true;
    }

    public override void loadXml(XmlNode node)
    {

    }
}

public class SequencerGraphEvent_FadeOut : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.FadeOut;

    private float _lambda = -1f;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScreenDirector._instance.ScreenFadeOut(_lambda);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Lambda")
                _lambda = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_ZoomEffect : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ZoomEffect;

    private FloatEx _zoom = new FloatEx();

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().Zoom(_zoom);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Factor")
                _zoom.loadFromXML(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_SetCameraZoom : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraZoom;

    private FloatEx _zoom = new FloatEx();
    private FloatEx _zoomSpeed = new FloatEx();
    private bool _force = false;

    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_zoom.getValue() <= 0f)
            CameraControlEx.Instance().setDefaultZoomSize();
        else
        {
            if(_force)
                CameraControlEx.Instance().setZoomSizeForce(_zoom);
            else 
                CameraControlEx.Instance().setZoomSize(_zoom,_zoomSpeed);
        }
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Size")
                _zoom.loadFromXML(attributes[i].Value);
            else if(attributes[i].Name == "Speed")
                _zoomSpeed.loadFromXML(attributes[i].Value);
            else if(attributes[i].Name == "Force")
                _force = bool.Parse(attributes[i].Value);
        }
    }
}

public class SequencerGraphEvent_SpawnCharacter : SequencerGraphEventBase
{
    private string                      _characterKey;

    private CharacterInfoData           _characterInfoData;
    private SpawnCharacterOptionDesc    _spawnDesc = SpawnCharacterOptionDesc.defaultValue;

    private string                      _uniqueEntityKey = "";
    private string                      _markerName = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SpawnCharacter;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        _characterInfoData = CharacterInfoManager.Instance().GetCharacterInfoData(_characterKey);
        if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
                _spawnDesc._position = item._position;
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
        CharacterEntityBase createdCharacter = sceneCharacterManager.createCharacterFromPool(_characterInfoData,_spawnDesc);

        if(createdCharacter == null)
            return true;

        if(_uniqueEntityKey != "")
            processor.addUniqueEntity(_uniqueEntityKey, createdCharacter);
        
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "CharacterKey")
            {
                _characterKey = attrValue;
            }
            else if(attrName == "Position")
            {
                _spawnDesc._position = XMLScriptConverter.valueToVector3(attrValue);
            }
            else if(attrName == "PositionMarker")
            {
                _markerName = attrValue;
            }
            else if(attrName == "SearchIdentifier")
            {
                _spawnDesc._searchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), attrValue);
            }
            else if(attrName == "UniqueKey")
            {
                _uniqueEntityKey = attrValue;
            }

        }
    }
}

public class SequencerGraphEvent_WaitSecond : SequencerGraphEventBase
{
    private float   _waitTime = 0f;
    private float   _timer = 0f;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitSecond;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        _timer = 0f;
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        _timer += deltaTime;
        return _waitTime <= _timer;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Time")
                _waitTime = XMLScriptConverter.valueToFloatExtend(attrValue);
        }
    }
}

public class SequencerGraphEvent_SetHPSphere : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetHPSphere;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase uniqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(uniqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        HPSphereUIManager.Instance().release();
        HPSphereUIManager.Instance().initialize(uniqueEntity);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
}

public class SequencerGraphEvent_SetCrossHair : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCrossHair;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        CrossHairUI._instance.setTarget(unqueEntity);
        CrossHairUI._instance.setActive(true);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
}

public class SequencerGraphEvent_WaitTargetDead : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.WaitTargetDead;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
            return true;
        
        return unqueEntity.isDead();
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
}

public class SequencerGraphEvent_SaveEventExecuteIndex : SequencerGraphEventBase
{
    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SaveEventExecuteIndex;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        return true;
    }

    public override void loadXml(XmlNode node)
    {
    }
}

public class SequencerGraphEvent_ApplyPostProcessProfile : SequencerGraphEventBase
{
    private string _path = "";
    private float _blendTime = 0f;
    private PostProcessProfileApplyType _applyType = PostProcessProfileApplyType.BaseBlend;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.ApplyPostProcessProfile;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        ScriptableObject profile = ResourceContainerEx.Instance().GetScriptableObject(_path);
        if(profile == null || (profile is PostProcessProfile) == false)
            return true;

        switch(_applyType)
        {
            case PostProcessProfileApplyType.BaseBlend:
                CameraControlEx.Instance().getPostProcessProfileControl().addBaseBlendProfile(profile as PostProcessProfile,_blendTime);
            break;
            case PostProcessProfileApplyType.Additional:
                CameraControlEx.Instance().getPostProcessProfileControl().setAdditionalEffectProfile(profile as PostProcessProfile,_blendTime);
            break;
        }
        
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Path")
                _path = attrValue;
            else if(attrName == "BlendTime")
                _blendTime = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "ApplyType")
                _applyType = (PostProcessProfileApplyType)System.Enum.Parse(typeof(PostProcessProfileApplyType), attrValue);
        }
    }
}

public class SequencerGraphEvent_TeleportTargetTo : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private string _markerName = "";
    private Vector3 _targetPosition;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.TeleportTargetTo;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
                _targetPosition = item._position;
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }
        
        unqueEntity.updatePosition(_targetPosition);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "Position")
                _targetPosition = XMLScriptConverter.valueToVector3(attrValue);
            else if(attrName == "PositionMarker")
                _markerName = attrValue;
        }
    }
}

public class SequencerGraphEvent_SetAudioListner : SequencerGraphEventBase
{
    private string _uniqueKey = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetAudioListner;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        GameEntityBase unqueEntity = processor.getUniqueEntity(_uniqueKey);
        if(unqueEntity == null)
        {
            DebugUtil.assert(false,"대상 Unique Entity가 존재하지 않습니다 : {0}",_uniqueKey);
            return true;
        }

        FMODAudioManager.Instance().setListener(unqueEntity.transform);
        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
        }
    }
}

public class SequencerGraphEvent_SetCameraTarget : SequencerGraphEventBase
{
    private string _uniqueKey = "";
    private CameraModeType _cameraMode = CameraModeType.Count;

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraTarget;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        if(_cameraMode != CameraModeType.Count)
            CameraControlEx.Instance().setCameraMode(_cameraMode);
        CameraControlEx.Instance().setCameraTarget(processor.getUniqueEntity(_uniqueKey).getSpriteRendererTransform());

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "UniqueKey")
                _uniqueKey = attrValue;
            else if(attrName == "CameraMode")
                _cameraMode = (CameraModeType)System.Enum.Parse(typeof(CameraModeType), attrValue);
        }
    }
}

public class SequencerGraphEvent_SetCameraPosition : SequencerGraphEventBase
{
    private Vector3 _cameraTargetPosition = Vector3.zero;
    private string _markerName = "";

    public override SequencerGraphEventType getSequencerGraphEventType() => SequencerGraphEventType.SetCameraPosition;
    
    public override void Initialize(SequencerGraphProcessor processor)
    {
        if(_markerName != "")
        {
            MarkerItem item = processor.getMarker(_markerName);
            if(item != null)
                _cameraTargetPosition = item._position;
        }
    }

    public override bool Execute(SequencerGraphProcessor processor,float deltaTime)
    {
        CameraControlEx.Instance().setCameraMode(CameraModeType.PositionMode);
        CameraControlEx.Instance().setCameraTargetPosition(_cameraTargetPosition);

        return true;
    }

    public override void loadXml(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "TargetPosition")
                _cameraTargetPosition = XMLScriptConverter.valueToVector3(attrValue);
            else if(attrName == "TargetPositionMarker")
                _markerName = attrValue;
        }
    }
}

public enum SequencerGraphPhaseType
{
    Initialize = 0,
    Update,
    End,
    Count,
}

public class SequencerGraphPhaseData
{
    public SequencerGraphEventBase[]    _sequencerGraphEventList;
    public int                          _sequencerGraphEventCount;
}

public class SequencerGraphBaseData
{
    public string                       _sequencerName;

    public SequencerGraphPhaseData[]    _sequencerGraphPhase = new SequencerGraphPhaseData[(int)SequencerGraphPhaseType.Count];
    
}
