using System.Collections.Generic;
using UnityEngine;

public class MovementControl
{
    private Dictionary<System.Type, MovementBase> _movementCache = new Dictionary<System.Type, MovementBase>();
    private MovementBase _currentMovement;

    public void initialize()
    {
        _currentMovement = null;
    }
    public bool progress(float deltaTime, Vector3 direction)
    {
        _currentMovement?.progress(deltaTime, direction);

        return true;
    }
    public void release()
    {
        _currentMovement?.release();
    }

    public bool isValid()
    {
        return _currentMovement != null;
    }
    public bool isMoving() 
    {
        return _currentMovement.isMoving();
    }

    public void setMoveScale(float moveScale){_currentMovement?.setMoveScale(moveScale);}

    public void addFrameToWorld(ObjectBase targetObject)
    {
        if (_currentMovement == null)
            targetObject.updatePosition(targetObject.transform.position);
        else
            _currentMovement?.AddFrameToWorldTransform(targetObject);
    }

    public Vector3 getMoveDirection()
    {
        if(_currentMovement == null)
            return Vector3.zero;
        return _currentMovement.getCurrentDirection();
    }

    public MovementBase getCurrentMovement(){return _currentMovement;}
    public MovementBase.MovementType getCurrentMovementType(){return _currentMovement.getMovementType();}

    public MovementBase changeMovement(GameEntityBase targetEntity,MovementBase.MovementType movementType)
    {
        if(_currentMovement != null && _currentMovement.getMovementType() == movementType)
        {
            _currentMovement.updateFirst(targetEntity);
            return _currentMovement;
        }

        switch(movementType)
        {
        case MovementBase.MovementType.Empty:
            setMovementEmpty();
            break;
        case MovementBase.MovementType.RootMotion:
            return changeMovement<GraphMovement>(targetEntity);
        case MovementBase.MovementType.GraphPreset:
            return changeMovement<GraphPresetMovement>(targetEntity);
        case MovementBase.MovementType.FrameEvent:
            return changeMovement<FrameEventMovement>(targetEntity);
        default:
            DebugUtil.assert(false,"잘못된 무브먼트 타입 입니다. 이게 머징 : {0}",movementType);
            break;
        }

        return null;
    }

    public void setMovementEmpty()
    {
        _currentMovement?.release();
        _currentMovement = null;
    }

    public T changeMovement<T>(GameEntityBase targetEntity) where T : MovementBase, new()
    {
        _currentMovement?.release();
        
        if(_movementCache.ContainsKey(typeof(T)) == false)
            _movementCache.Add(typeof(T), new T());
        T newMovement = _movementCache[typeof(T)] as T;
        _currentMovement = newMovement;

        if(_currentMovement == null)
        {
            DebugUtil.assert(false, "현재 무브먼트가 존재하지 않습니다. 통보 요망");
            return null;
        }

        _currentMovement.initialize(targetEntity);
        _currentMovement.updateFirst(targetEntity);
        return newMovement;
    }
}
