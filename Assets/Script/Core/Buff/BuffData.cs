

public class BuffData
{
    public string              _buffName;
    public int                 _buffKey;
    public string              _targetStatusName;
    public BuffUpdateType      _buffUpdateType;
    public BuffApplyType       _buffApplyType;

    public bool                _allowOverlap = false;

    public float               _buffVaryStatFactor;
    public float               _buffCustomValue0;
    public float               _buffCustomValue1;
    public string[]            _buffCustomValue2 = null;

    public string              _buffCustomStatusName;

    public string              _particleEffect = "";
    public string              _timelineEffect = "";
    public string              _animationEffect = "";

    public int                 _audioID = -1;
    public int[]               _audioParameter = null;

    public BuffData()
    {
        _buffName = null;
        _buffKey = -1;
        _targetStatusName = null;
        _buffUpdateType = BuffUpdateType.Count;
        _buffApplyType = BuffApplyType.Count;
        _allowOverlap = false;

        _buffVaryStatFactor = 0f;
        _buffCustomValue0 = 0f;
        _buffCustomValue1 = 0f;
        _buffCustomValue2 = null;

        _particleEffect = "";
        _timelineEffect = "";
        _animationEffect = "";
        _audioID = -1;
        _audioParameter = null;
    }

    public void copy(BuffData target)
    {
        _buffKey = target._buffKey;
        _targetStatusName = target._targetStatusName;
        _buffUpdateType = target._buffUpdateType;
        _buffApplyType = target._buffApplyType;
        _allowOverlap = target._allowOverlap;
        _buffVaryStatFactor = target._buffVaryStatFactor;
        _buffCustomValue0 = target._buffCustomValue0;
        _buffCustomValue1 = target._buffCustomValue1;
        _buffCustomValue2 = target._buffCustomValue2;
        _particleEffect = target._particleEffect;
        _timelineEffect = target._timelineEffect;
        _animationEffect = target._animationEffect;
        _audioID = target._audioID;
        _audioParameter = target._audioParameter;
    }


    public bool isBuffValid()
    {
        return _buffKey >= 0 && _buffUpdateType != BuffUpdateType.Count && _buffApplyType != BuffApplyType.Count;
    }
}

public struct BuffSet
{
    public int[] _buffKeys;
}

public enum BuffApplyType
{
    Direct = 0,
    DirectDelta,
    Additional,
    DirectSet,
    Empty,
    Count
}

public enum BuffUpdateType
{
    OneShot = 0,
    Time,
    StatSection,
    Continuous,
    DelayedContinuous,
    ButtonHit,
    GreaterThenSet,
    Count,
};