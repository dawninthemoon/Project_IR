using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MiniStageData", menuName = "Scriptable Object/Mini Stage Data", order = 3)]
public class MiniStageData : StageData
{
    public MiniStageData()
    {
        _isMiniStage = true;
    }

    public SearchIdentifier         _targetSearchIdentifier = SearchIdentifier.Enemy;

    public float                    _triggerWidth = 1f;
    public float                    _triggerHeight = 2f;
    public Vector3                  _triggerOffset = Vector3.zero;
}