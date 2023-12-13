
public struct WeightGroupData
{
    public string _groupKey;
    public int _weightCount;
    public WeightData[] _weights;

    public bool isValid()
    {
        return _weights != null && _weightCount != 0 && _groupKey != "";
    }
}

public struct WeightData
{
    public string _key;
    public float _weight;
}