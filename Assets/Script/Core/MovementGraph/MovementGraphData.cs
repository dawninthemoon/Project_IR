[System.Serializable]
public struct MovementGraphData
{
    public MovementGraphData(UnityEngine.Vector3 total, UnityEngine.Vector3 move, UnityEngine.Vector2Int pixelPosition, int index = -1)
    {
        _totalMoveFactor = total;
        _moveFactor = move;
        _pixelPosition = pixelPosition;
        _index = index;
    }

    public UnityEngine.Vector3 _totalMoveFactor;
    public UnityEngine.Vector3 _moveFactor;
    public UnityEngine.Vector2Int _pixelPosition;
    public int _index;

    public bool isValid()
    {
        return _index >= 0;
    }

    public UnityEngine.Vector3 lerp(MovementGraphData data, float time)
    {
        DebugUtil.assert(data.isValid(),"data invalid");
        return MathEx.lerpV3WithoutZ(_moveFactor, data._moveFactor, time);
    }

    public UnityEngine.Vector3 lerpTotal(MovementGraphData data, float time)
    {
        DebugUtil.assert(data.isValid(),"data invalid");
        return MathEx.lerpV3WithoutZ(_totalMoveFactor,data._totalMoveFactor,time);
    }
}