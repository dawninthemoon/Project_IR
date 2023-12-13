using UnityEngine;

[System.Serializable]
public class AnimationScalePresetData
{
    [SerializeField]private string _name;
    [SerializeField]private AnimationCurve _xScaleCurve;
    [SerializeField]private AnimationCurve _yScaleCurve;

    private float evaulate(float normalizedTime, AnimationCurve curve)
    {
        return curve.Evaluate(normalizedTimeToReal(normalizedTime, curve));
    }

    public Vector2 evaulate(float normalizedTime)
    {
        float x = evaulate(normalizedTime, _xScaleCurve);
        float y = evaulate(normalizedTime, _yScaleCurve);

        return new Vector2(x,y);
    }

    public string getName() {return _name;}

    public bool isValid()
    {
        return _xScaleCurve != null && _xScaleCurve.length > 1 && _yScaleCurve != null && _yScaleCurve.length > 1;
    }

    public Vector2 getScaleValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc)
    {
        float x = getScaleValuePerFrameFromTime(desc,_xScaleCurve);
        float y = getScaleValuePerFrameFromTime(desc,_xScaleCurve);

        return new Vector2(x,y);
    } 

    private float getScaleValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc, AnimationCurve curve)
    {
        return getScaleValuePerFrameFromTime(desc.prevNormalizedTime,desc.currentNormalizedTime,desc.loopCount, curve);
    }

    public float getScaleValuePerFrameFromTime(float prevNormalizedTime, float currentNormalizedTime, int loopCount, AnimationCurve curve)
    {
        float rotateValue = 0f;

        if(loopCount > 0)
            rotateValue += evaulate(currentNormalizedTime, curve) + evaulate(1f - prevNormalizedTime, curve);
        else
            rotateValue += evaulate(currentNormalizedTime, curve) - evaulate(prevNormalizedTime, curve);

        if(loopCount - 1 > 0)
            rotateValue += getTotalScale(curve) * (float)(loopCount - 1);
        
        return rotateValue;
    }

    private float getTotalScale(AnimationCurve curve)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"start and end must exist");
            return 0f;
        }

        return curve.keys[curve.length - 1].value - curve.keys[0].value;
    }

    private float normalizedTimeToReal(float normalizedTime, AnimationCurve curve)
    {
        if(curve.length <= 1)
        {
            DebugUtil.assert(false,"start and end must exist");
            return 0f;
        }

        normalizedTime = MathEx.clamp01f(normalizedTime);

        float start = curve.keys[0].time;
        float end = curve.keys[curve.length - 1].time;

        return MathEx.lerpf(start,end,normalizedTime);
    }
};