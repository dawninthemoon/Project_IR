using UnityEngine;

[System.Serializable]
public class AnimationRotationPresetData
{
    [SerializeField]public string _name;
    [SerializeField]public AnimationCurve _rotationCurve;

    public float evaulate(float normalizedTime)
    {
        return _rotationCurve.Evaluate(normalizedTimeToReal(normalizedTime));
    }

    public string getName() {return _name;}

    public bool isValid()
    {
        return _rotationCurve != null && _rotationCurve.length > 1;
    }

    public float getRotateValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc)
    {
        return getRotateValuePerFrameFromTime(desc.prevNormalizedTime,desc.currentNormalizedTime,desc.loopCount);
    }

    public float getRotateValuePerFrameFromTime(float prevNormalizedTime, float currentNormalizedTime, int loopCount)
    {
        float rotateValue = 0f;

        if(loopCount > 0)
            rotateValue += evaulate(currentNormalizedTime) + evaulate(1f - prevNormalizedTime);
        else
            rotateValue += evaulate(currentNormalizedTime) - evaulate(prevNormalizedTime);

        if(loopCount - 1 > 0)
            rotateValue += getTotalRotation() * (float)(loopCount - 1);
        
        return rotateValue;
    }

    public float getTotalRotation()
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"AnimationRotationPresetData의 Start와 End는 무조건 존재해야 합니다. : {0}",_name);
            return 0f;
        }

        return _rotationCurve.keys[_rotationCurve.length - 1].value - _rotationCurve.keys[0].value;
    }

    private float normalizedTimeToReal(float normalizedTime)
    {
        if(_rotationCurve.length <= 1)
        {
            DebugUtil.assert(false,"AnimationRotationPresetData의 Start와 End는 무조건 존재해야 합니다. : {0}",_name);
            return 0f;
        }

        normalizedTime = MathEx.clamp01f(normalizedTime);

        float start = _rotationCurve.keys[0].time;
        float end = _rotationCurve.keys[_rotationCurve.length - 1].time;

        return MathEx.lerpf(start,end,normalizedTime);
    }
};