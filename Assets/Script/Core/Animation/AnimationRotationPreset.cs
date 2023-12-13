using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "AnimationRotationPreset", menuName = "Scriptable Object/Animation Rotation Preset", order = int.MaxValue)]
public class AnimationRotationPreset : ScriptableObject
{
    [SerializeField]private List<AnimationRotationPresetData> _presetData = new List<AnimationRotationPresetData>();
    private Dictionary<string, AnimationRotationPresetData> _presetCache = new Dictionary<string, AnimationRotationPresetData>();
    private bool _isCacheConstructed = false;

    // private void Awake()
    // {
    //     constructPresetCache();
    // }

    public void addPresetData(AnimationRotationPresetData data)
    {
        _presetData.Add(data);
    }

    public AnimationRotationPresetData getPresetData(string targetName)
    {
        AnimationRotationPresetData target = null;
        if(_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach(AnimationRotationPresetData item in _presetData)
            {
                if(item.getName() == targetName)
                {
                    target = item;
                    break;
                }
            }
        }

        DebugUtil.assert(target != null,"해당 애니메이션 로테이션 프리셋 데이터가 존재하지 않습니다. 이름을 잘못 쓰지 않았나요? : {0}",targetName);
        return target;
    }


    private void constructPresetCache()
    {
        if(_isCacheConstructed == true)
            return;

        foreach(AnimationRotationPresetData item in _presetData)
        {
            _presetCache.Add(item.getName(), item);
        }

        _isCacheConstructed = true;
    }

}


#if UNITY_EDITOR

[CustomEditor(typeof(AnimationRotationPreset))]
public class AnimationRotationPresetEditor : Editor
{
    AnimationRotationPreset controll;

	void OnEnable()
    {
        controll = (AnimationRotationPreset)target;
    }

    public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();

        GUILayout.Space(10f);
        if(GUILayout.Button("Add 0 ~ 360 Linear"))
        {
            AnimationRotationPresetData presetData = new AnimationRotationPresetData();
            presetData._name = "NewPreset";
            presetData._rotationCurve = new AnimationCurve();
            presetData._rotationCurve.AddKey(0f,0f);
            presetData._rotationCurve.AddKey(1f,360f);

            controll.addPresetData(presetData);
            EditorUtility.SetDirty(controll);
        }
    }

}


#endif
