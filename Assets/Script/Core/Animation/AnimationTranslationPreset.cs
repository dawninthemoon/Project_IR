using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationTranslationPreset", menuName = "Scriptable Object/Animation Translation Preset", order = int.MaxValue)]
public class AnimationTranslationPreset : ScriptableObject
{
    [SerializeField] private List<AnimationTranslationPresetData > _presetData = new List<AnimationTranslationPresetData >();
    private Dictionary<string, AnimationTranslationPresetData > _presetCache = new Dictionary<string, AnimationTranslationPresetData >();
    private bool _isCacheConstructed = false;


    public AnimationTranslationPresetData getPresetData(string targetName)
    {
        AnimationTranslationPresetData target = null;
        if (_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach (AnimationTranslationPresetData item in _presetData)
            {
                if (item.getName() == targetName)
                {
                    target = item;
                    break;
                }
            }
        }

        DebugUtil.assert(target != null, "해당 애니메이션 트렌슬레이션 프리셋 데이터가 존재하지 않습니다. 이름을 잘못 쓰지 않았나요? : {0}", targetName);
        return target;
    }


    private void constructPresetCache()
    {
        if (_isCacheConstructed == true)
            return;

        foreach (AnimationTranslationPresetData item in _presetData)
        {
            _presetCache.Add(item.getName(), item);
        }

        _isCacheConstructed = true;
    }

}


[System.Serializable]
public class AnimationTranslationPresetData
{
    [SerializeField] private string _name;
    [SerializeField] private AnimationCurve _xCurve;
    [SerializeField] private AnimationCurve _yCurve;

    private float evaulate(float normalizedTime, AnimationCurve curve)
    {
        return curve.Evaluate(normalizedTimeToReal(normalizedTime, curve));
    }

    public Vector2 evaulate(float normalizedTime)
    {
        float x = evaulate(normalizedTime, _xCurve);
        float y = evaulate(normalizedTime, _yCurve);

        return new Vector2(x, y);
    }

    public string getName() { return _name; }

    public bool isValid()
    {
        return _xCurve != null && _xCurve.length > 1 && _yCurve != null && _yCurve.length > 1;
    }

    public Vector2 getTranslationValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc)
    {
        float x = getTranslationValuePerFrameFromTime(desc, _xCurve);
        float y = getTranslationValuePerFrameFromTime(desc, _yCurve);

        return new Vector2(x, y);
    }

    private float getTranslationValuePerFrameFromTime(MoveValuePerFrameFromTimeDesc desc, AnimationCurve curve)
    {
        return getTranslationValuePerFrameFromTime(desc.prevNormalizedTime, desc.currentNormalizedTime, desc.loopCount, curve);
    }

    public float getTranslationValuePerFrameFromTime(float prevNormalizedTime, float currentNormalizedTime, int loopCount, AnimationCurve curve)
    {
        float value = 0f;

        if (loopCount > 0)
            value += evaulate(currentNormalizedTime, curve) + evaulate(1f - prevNormalizedTime, curve);
        else
            value += evaulate(currentNormalizedTime, curve) - evaulate(prevNormalizedTime, curve);

        if (loopCount - 1 > 0)
            value += getTotalTranslation(curve) * (float)(loopCount - 1);

        return value;
    }

    private float getTotalTranslation(AnimationCurve curve)
    {
        if (isValid() == false)
        {
            DebugUtil.assert(false, "start and end must exist");
            return 0f;
        }

        return curve.keys[curve.length - 1].value - curve.keys[0].value;
    }

    private float normalizedTimeToReal(float normalizedTime, AnimationCurve curve)
    {
        if (curve.length <= 1)
        {
            DebugUtil.assert(false, "start and end must exist");
            return 0f;
        }

        normalizedTime = MathEx.clamp01f(normalizedTime);

        float start = curve.keys[0].time;
        float end = curve.keys[curve.length - 1].time;

        return MathEx.lerpf(start, end, normalizedTime);
    }
};