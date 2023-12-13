using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MovementTrackPointData
{
    public MathEx.EaseType _easeType = MathEx.EaseType.Linear;
    public Vector2     _point = Vector2.zero;
    public Vector2     _bezierPoint = Vector2.zero;

    public float       _speedToNextPoint = 1f;
    public float       _waitSecond = 0f;

    public bool        _isLinearPath;

    public Vector2 getInverseBezierPoint()
    {
        return _point - (_bezierPoint - _point);
    }

    public Vector2 convertInverseBezierPointToBezierPoint(Vector2 bezierPointInv)
    {
        return _point - (bezierPointInv - _point);
    }
}

[System.Serializable]
public class MovementTrackData
{
    public string       _name;
    public List<MovementTrackPointData> _trackPointData = new List<MovementTrackPointData>();
    public float[]      _pointLengthArray;
    public float        _trackTotalLength;

    public bool         _startBlend = true;
    public bool         _endBlend = true;

    public void calculateTrackLength()
    {
        _pointLengthArray = new float[_trackPointData.Count - 1];
        _trackTotalLength = 0f;

        for(int index = 0; index < _pointLengthArray.Length; ++index)
        {
            MovementTrackPointData p0 = _trackPointData[index];
            MovementTrackPointData p1 = _trackPointData[index + 1];
            _pointLengthArray[index] = MathEx.approximateBezierCurveLength(p0._point,p0._bezierPoint,p1._bezierPoint,p1._point);

            _trackTotalLength += _pointLengthArray[index];
        }
    }
}

public class MovementTrackProcessor
{
    public MovementTrackData _trackData = null;
    public int      _currentPointIndex = 0;
    public float    _trackPathProcessRate = 0f;
    public float    _waitSecondTime = 0f;
    public bool     _isLinearPath = false;
    public bool     _isEnd = false;
    public MovementTrackProcessor()
    {
    }
    public void initialize(MovementTrackData trackData)
    {
        _trackData = trackData;
        if(_trackData == null || _trackData._trackPointData.Count == 0)
        {
            DebugUtil.assert(false, "Track이 비정상임 [TrackName: {0}]", _trackData == null ? "Null" : _trackData._name);
            return;
        }
        _currentPointIndex = 0;
        _trackPathProcessRate = 0f;
        _waitSecondTime = _trackData._trackPointData[0]._waitSecond;
        _isLinearPath = _trackData._trackPointData[0]._isLinearPath;
        _isEnd = false;
    }

    public void clear()
    {
        _trackData = null;
        _currentPointIndex = 0;
        _trackPathProcessRate = 0f;
        _waitSecondTime = 0f;
        _isLinearPath = false;
        _isEnd = false;
    }

    public bool getCurrentTrackStartPosition(out Vector2 outPosition)
    {
        outPosition = Vector2.zero;
        if(_trackData == null)
            return false;

        outPosition = _trackData._trackPointData[0]._point;
        return true;
    }
    
    public bool processTrack(float deltaTime, out Vector2 resultPoint)
    {
        resultPoint = Vector2.zero;
        if(deltaTime <= 0f)
            return false;

        if(_trackData == null || _trackData._trackPointData.Count == 0)
            return false;
            
        if(_isEnd)
        {
            resultPoint = _trackData._trackPointData[_trackData._trackPointData.Count - 1]._point;
            return true;
        }
        while(true)
        {
            if(_isEnd)
                break;
            MovementTrackPointData point0 = _trackData._trackPointData[_currentPointIndex];
            MovementTrackPointData point1 = _trackData._trackPointData[_currentPointIndex + 1];
            if(_waitSecondTime > 0f)
            {
                _waitSecondTime -= deltaTime;
                if(_waitSecondTime <= 0f)
                {
                    deltaTime += _waitSecondTime;
                    _waitSecondTime = 0f;
                }
                else
                {
                    resultPoint = point0._point;
                    return true;
                }
            }
            float trackLength = _trackData._pointLengthArray[_currentPointIndex];
            if(trackLength == 0f)
            {
                _isEnd = true;
                DebugUtil.assert(false, "트랙 패스 길이가 0입니다. 뭔가 잘못됨 통보 바람 [Name: {0}]", _trackData._name);
            }
            float speed = MathEx.lerpf(point0._speedToNextPoint,point1._speedToNextPoint, _trackPathProcessRate);
            float finalSpeed = (trackLength / speed);
            float resultRate = deltaTime / finalSpeed;
            _trackPathProcessRate += resultRate;
            if(_trackPathProcessRate >= 1f)
            {
                _trackPathProcessRate -= 1f;
                _currentPointIndex += 1;
                if(_currentPointIndex == _trackData._trackPointData.Count - 1)
                {
                    _isEnd = true;
                    _trackPathProcessRate = 1f;
                }
                else if(_trackPathProcessRate != 0f)
                {
                    _trackPathProcessRate *= finalSpeed;
                    deltaTime = _trackPathProcessRate;
                    _trackPathProcessRate = 0f;
                    _waitSecondTime = _trackData._trackPointData[_currentPointIndex]._waitSecond;
                    _isLinearPath = _trackData._trackPointData[_currentPointIndex]._isLinearPath;
                    continue;
                }
            }

            float finalRate = _trackPathProcessRate;
            finalRate = MathEx.getEaseFormula(point0._easeType,0f,1f,finalRate);

            if(_isLinearPath)
            {
                resultPoint = Vector2.Lerp(point0._point, point1._point, finalRate);
            }
            else
            {
                resultPoint = MathEx.getPointOnBezierCurve(point0._point,point0._bezierPoint,point1.getInverseBezierPoint(),point1._point, finalRate);
            }
            break;
        }
        return true;
    }

    public bool isEndBlend()
    {
        if(_trackData == null)
            return false;

        return _trackData._endBlend;
    }

    public bool isEnd() {return _isEnd;}
}