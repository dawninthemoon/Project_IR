using UnityEngine;

[System.Serializable]
public class MovementGraphPresetData
{
    [SerializeField]private string _name;
    [SerializeField]private AnimationCurve _movementCurve;
    [SerializeField]private float _magnification = 1f;
    [SerializeField]private float _directionAngle = 0f;

    public float evaulate(float normalizedTime)
    {
        return _movementCurve.Evaluate(normalizedTimeToReal(normalizedTime)) * _magnification;
    }

    public string getName() {return _name;}

    public bool isValid()
    {
        return _movementCurve != null && _movementCurve.length > 1;
    }

    public float getMoveValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc)
    {
        return getMoveValuePerFrameFromTime(desc.prevNormalizedTime,desc.currentNormalizedTime,desc.loopCount);
    }

    public float getMoveValuePerFrameFromTime(float prevNormalizedTime, float currentNormalizedTime, int loopCount)
    {
        float moveValue = 0f;

        if(loopCount > 0)
            moveValue += evaulate(currentNormalizedTime) + evaulate(1f - prevNormalizedTime);
        else
            moveValue += evaulate(currentNormalizedTime) - evaulate(prevNormalizedTime);

        if(loopCount - 1 > 0)
            moveValue += getTotalMovement() * (float)(loopCount - 1);
        
        return moveValue;
    }

    public float getDirectionAngle() {return _directionAngle;}

    public float getTotalMovement()
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"start and end must exist");
            return 0f;
        }

        return _movementCurve.keys[_movementCurve.length - 1].value - _movementCurve.keys[0].value;
    }

    private float normalizedTimeToReal(float normalizedTime)
    {
        if(_movementCurve.length <= 1)
        {
            DebugUtil.assert(false,"start and end must exist");
            return 0f;
        }

        normalizedTime = MathEx.clamp01f(normalizedTime);

        float start = _movementCurve.keys[0].time;
        float end = _movementCurve.keys[_movementCurve.length - 1].time;

        return MathEx.lerpf(start,end,normalizedTime);
    }
};