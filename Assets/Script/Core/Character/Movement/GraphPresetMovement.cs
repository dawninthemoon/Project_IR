using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPresetMovement : MovementBase
{
    private MovementGraphPresetData _presetData = null;
    private GameEntityBase _targetEntity;

    public override MovementType getMovementType(){return MovementType.GraphPreset;}

    public override void initialize(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        _currentDirection = Vector3.right;
    }

    public override void updateFirst(GameEntityBase targetEntity)
    {
        setGraphPresetData(targetEntity.getCurrentMovementGraphPreset());
    }

    public void setGraphPresetData(MovementGraphPresetData preset)
    {
         if(preset == null || preset.isValid() == false)
        {
            DebugUtil.assert(false, "무브먼트 그래프 프리셋 데이터가 존재하지 않습니다. 이게 머징 : {0}", preset == null);
            return;
        }

        _presetData = preset;
    }

    public override bool progress(float deltaTime, Vector3 direction)
    {
        if(_presetData == null || _presetData.isValid() == false || _targetEntity == null)
        {
            DebugUtil.assert(false, "무브먼트 그래프 프리셋 데이터가 존재하지 않습니다. 이게 머징");
            return false;
        }

        _currentDirection = Vector3.right;

        if(direction == Vector3.zero)
            return false;

        _currentDirection = (Quaternion.FromToRotation(Vector3.right,direction) * Quaternion.Euler(0f,0f,_presetData.getDirectionAngle())) * Vector3.right;
        movementOfFrame += _currentDirection * _presetData.getMoveValuePerFrameFromTime(_targetEntity.getMoveValuePerFrameFromTimeDesc());
        return true;
    }

    public override void release()
    {
        _presetData = null;
    }

}