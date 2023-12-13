using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtteckRayEffect : MonoBehaviour
{
    public LineRenderer _targetLine;
    public LineRenderer _placeholder;

    public void setAttackData(string attackPresetName)
    {
        Debug.Log("?");
        AttackPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AttackPreset") as AttackPreset;
        AttackPresetData presetData = preset.getPresetData(attackPresetName);
        if(presetData == null)
        {
            DebugUtil.assert(false, "failed to load attack preset: {0}",attackPresetName);
            return;
        }

        _targetLine.startWidth = presetData._attackRayRadius * 2f;
        _targetLine.endWidth = presetData._attackRayRadius * 2f;

        _placeholder.startWidth = _targetLine.startWidth;
        _placeholder.endWidth = _targetLine.endWidth;

        _targetLine.SetPosition(1, new Vector3(presetData._attackRadius,0f,0f));
        _placeholder.SetPosition(1, new Vector3(presetData._attackRadius,0f,0f));
    }
}
