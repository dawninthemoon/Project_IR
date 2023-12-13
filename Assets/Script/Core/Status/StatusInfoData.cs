
public class StatusInfoData
{
    public string _statusInfoName;
    public StatusDataFloat[] _statusData;
    public StatusGraphicInterfaceData[] _graphicInterfaceData;

    public StatusInfoData(string name, StatusDataFloat[] statusArray, StatusGraphicInterfaceData[] graphicInterfaceArray)
    {
        _statusInfoName = name;
        _statusData = statusArray;
        _graphicInterfaceData = graphicInterfaceArray;
    }

}

public class StatusDataFloat
{
    public string _statusName;
    public StatusType _statusType;

    public float _maxValue;
    public float _minValue;
    public float _initialValue;

    public StatusDataFloat(){}
    public StatusDataFloat(StatusType type, string name, float min, float max, float init)
    {
        _statusType = type;
        _statusName = name;

        _maxValue = max;
        _minValue = min;
        _initialValue = init;
    }

    public bool isStatusValid()
    {
        return _statusType != StatusType.Count && _maxValue >= _minValue;
    }

    public void initStat(ref float value)
    {
        value = _initialValue;
    }

    public void variStat(ref float value, float additionalMax, float factor )
    {
        value = MathEx.clampf(value + factor, _minValue, _maxValue + additionalMax);
    }

    public void setStat(ref float value, float additionalMax, float factor )
    {
        value = MathEx.clampf(factor,_minValue,_maxValue + additionalMax);
    }


    public bool isMax(float value)
    {
        return value >= _maxValue;
    }

    public bool isMin(float value)
    {
        return value <= _minValue;
    }

    public StatusType getStatusType() {return _statusType;}
    public string getName() {return _statusName;}

    public float getInitValue() {return _initialValue;}
    public float getMaxValue() {return _maxValue;}
    public float getMinValue() {return _minValue;}

}

public class StatusGraphicInterfaceData
{
    public UnityEngine.Color    _interfaceColor;
    public string               _targetStatus;
    public float                _horizontalGap;
}


public enum StatusVariType
{
    Fixed,
    List
};


public enum StatusType
{
    HP,
    Stamina,
    Blood,
    Battle,
    HitCount,
    GuardCount,
    Custom,
    Count,
}