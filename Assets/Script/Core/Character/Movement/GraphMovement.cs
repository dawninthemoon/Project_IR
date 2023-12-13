using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphMovement : MovementBase
{
    private MovementGraph _movementGraph = null;

    private GameEntityBase _targetEntity;


    public override MovementType getMovementType(){return MovementType.RootMotion;}

    public override void initialize(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        _currentDirection = Vector3.right;

        
    }

    public override void updateFirst(GameEntityBase targetEntity)
    {
        setGraphData(targetEntity.getCurrentMovementGraph());
    }

    public void setGraphData(MovementGraph graph)
    {
         if(graph == null || graph.isValid() == false)
        {
            DebugUtil.assert(false, "해당 무브먼트 그래프가 유효하지 않습니다. 이건 뭐지 : {0}", graph == null);
            return;
        }

        _movementGraph = graph;

    }

    public override bool progress(float deltaTime, Vector3 direction)
    {
        if(_movementGraph == null || _movementGraph.isValid() == false || _targetEntity == null)
        {
            DebugUtil.assert(false, "해당 무브먼트 그래프가 유효하지 않습니다. 이게 머징");
            return false;
        }

        movementOfFrame += (Quaternion.FromToRotation(Vector3.right,direction) * _movementGraph.getMoveValuePerFrameFromTime(_targetEntity.getMoveValuePerFrameFromTimeDesc()));
        return true;
    }

    public override void release()
    {
        _movementGraph = null;
    }

}