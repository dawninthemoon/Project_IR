using UnityEngine;
using System.IO;
using System.Text;



public struct MoveValuePerFrameFromTimeDesc
{
    public float prevNormalizedTime;
    public float currentNormalizedTime;
    public int loopCount;
}


[System.Serializable]
public class MovementGraph : ScriptableObject
{
    [SerializeField] private string _graphName;
    [SerializeField] private MovementGraphData[] _movementGraphDataArray = null;
    [SerializeField] private int _arraySize = -1;
    [SerializeField] private  Vector3 _totalMoveFactor;


    public bool isValid()
    {
        return _arraySize > 0 && _movementGraphDataArray != null;
    }

    public MovementGraphData[] getDataArray() {return _movementGraphDataArray;}
    public int getArraySize() {return _arraySize;}
    public Vector3 getTotalMoveFactor() {return _totalMoveFactor;}

    public void setData(MovementGraphData[] movementGraphArray, string graphName)
    {
        DebugUtil.assert(movementGraphArray != null, "data is null");

        _graphName = graphName;

        _movementGraphDataArray = movementGraphArray;
        _arraySize = _movementGraphDataArray.Length;

        _totalMoveFactor = _movementGraphDataArray[_arraySize - 1]._totalMoveFactor;
    }

    public Vector3 getMoveValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc)
    {
        return getMoveValuePerFrameFromTime(desc.prevNormalizedTime,desc.currentNormalizedTime,desc.loopCount);
    }

    public Vector3 getMoveValuePerFrameFromTime(float prevNormalizedTime, float currentNormalizedTime, int loopCount)
    {
        Vector3 moveValue = Vector3.zero;

        if(loopCount > 0)
            moveValue += getMoveValueTotalFromTime(currentNormalizedTime) + getMoveValueTotalFromTime(1f - prevNormalizedTime);
        else
            moveValue += getMoveValueTotalFromTime(currentNormalizedTime) - getMoveValueTotalFromTime(prevNormalizedTime);

        if(loopCount - 1 > 0)
            moveValue += _totalMoveFactor * (float)(loopCount - 1);
        
        return moveValue;
    }

    public Vector3 getMoveValueFromTime(float normalizedTime)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"movement graph is not valid");
            return Vector3.zero;
        }

        int index = 0;
        float processValue = 0f;

        getIndexFromTime(normalizedTime,out index, out processValue);

        if(index == _arraySize - 1)
            return _movementGraphDataArray[index]._moveFactor;
        else
            return MathEx.lerpV3WithoutZ(_movementGraphDataArray[index]._moveFactor,_movementGraphDataArray[index+1]._moveFactor,processValue);
    }

    public Vector3 getMoveValueTotalFromTime(float normalizedTime)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"movement graph is not valid");
            return Vector3.zero;
        }

        int index = 0;
        float processValue = 0f;

        getIndexFromTime(normalizedTime,out index, out processValue);

        if(index == _arraySize - 1)
            return _movementGraphDataArray[index]._totalMoveFactor;
        else
            return MathEx.lerpV3WithoutZ(_movementGraphDataArray[index]._totalMoveFactor,_movementGraphDataArray[index+1]._totalMoveFactor,processValue);
    }


    private void getIndexFromTime(float normalizedTime, out int index, out float processValue)
    {
        if(normalizedTime < 0f)
        {
            DebugUtil.assert(false,"wrong time value");
            index = 0;
            processValue = 0f;
            return;
        }

        processValue = ( (float)(_arraySize - 1) ) * normalizedTime; 
        index = (int)processValue;
        index = MathEx.clampi(index, 0, _arraySize - 1);

        processValue = processValue - (float)index;
    }
    
    public void serialize(BinaryWriter writer)
    {
        writer.Write(_graphName);
        writer.Write(_arraySize);
        writer.Write(_totalMoveFactor.x);
        writer.Write(_totalMoveFactor.y);

        for(int i = 0; i < _arraySize; ++i)
        {
            writer.Write(_movementGraphDataArray[i]._index);
            writer.Write(_movementGraphDataArray[i]._moveFactor.x);
            writer.Write(_movementGraphDataArray[i]._moveFactor.y);
            writer.Write(_movementGraphDataArray[i]._pixelPosition.x);
            writer.Write(_movementGraphDataArray[i]._pixelPosition.y);
            writer.Write(_movementGraphDataArray[i]._totalMoveFactor.x);
            writer.Write(_movementGraphDataArray[i]._totalMoveFactor.x);
        }
    }

    public void deserialize(BinaryReader reader)
    {
        _graphName = reader.ReadString();
        _arraySize = reader.ReadInt32();
        _totalMoveFactor.x = reader.ReadSingle();
        _totalMoveFactor.y = reader.ReadSingle();

        for(int i = 0; i < _arraySize; ++i)
        {
            _movementGraphDataArray[i]._index = reader.ReadInt32();
            _movementGraphDataArray[i]._moveFactor.x = reader.ReadSingle();
            _movementGraphDataArray[i]._moveFactor.y = reader.ReadSingle();
            _movementGraphDataArray[i]._pixelPosition.x = reader.ReadInt32();
            _movementGraphDataArray[i]._pixelPosition.y = reader.ReadInt32();
            _movementGraphDataArray[i]._totalMoveFactor.x = reader.ReadSingle();
            _movementGraphDataArray[i]._totalMoveFactor.x = reader.ReadSingle();
        }
    }
}
