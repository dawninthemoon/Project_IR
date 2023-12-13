

public enum DanmakuVariableEventType
{
    Add = 0,
    Set,
    Mul,
    Div,
    Count
};

public enum DanmakuVariableType
{
    Velocity = 0,
    Acceleration,
    Friction,
    Angle,
    AngularAccel,
    LifeTime,
    Count
};

public enum DanmakuEventType
{
    VariableEvent,
    ProjectileEvent,
    LoopEvent,
    WaitEvent,
    Count,
}

[System.Serializable]
public abstract class DanmakuEventBase
{
    public abstract DanmakuEventType getEventType();
}

[System.Serializable]
public class DanmakuVariableEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.VariableEvent;

    public DanmakuVariableType _type;
    public DanmakuVariableEventType[] _eventType;

    public int _eventCount;

    public FloatEx[] _value;
};

[System.Serializable]
public class DanmakuProjectileEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.ProjectileEvent;
    
    public string _projectileName;
    public DirectionType _directionType;
    public ActionFrameEvent_Projectile.ShotInfoUseType _shotInfoUseType;

    public ActionFrameEvent_Projectile.PredictionType _predictionType = ActionFrameEvent_Projectile.PredictionType.Path;
    public UnityEngine.Vector3[]           _pathPredictionArray = null;
    public SetTargetType                   _setTargetType = SetTargetType.SetTargetType_Self;
    public int                             _predictionAccuracy = 0;
    public float                           _startTerm = 0f;
    
};

[System.Serializable]
public class DanmakuLoopEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.LoopEvent;

    public int _loopCount;
    public float _term;

    public DanmakuEventBase[] _events;
    public int _eventCount;
};

[System.Serializable]
public class DanmakuWaitEventData : DanmakuEventBase
{
    public override DanmakuEventType getEventType()=>DanmakuEventType.WaitEvent;

    public float _waitTime;
}

[System.Serializable]
public class DanmakuGraphBaseData
{
    public string _name;

    public DanmakuEventBase[] _danamkuEventList;
    public int _danamkuEventCount;
}