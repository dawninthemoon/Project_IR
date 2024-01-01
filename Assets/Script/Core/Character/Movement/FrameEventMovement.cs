using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameEventMovement : MovementBase
{
    private GameEntityBase _targetEntity;

    public enum FrameEventMovementValueType
    {
        Speed = 0,
        Velocity,
        MaxVelocity,
        Friction,
        Count,
    };

    private readonly float _gravity = -9.0f;

    private float[] _movementValues = new float[(int)FrameEventMovementValueType.Count];
    private Vector3 _currentVelocity = Vector3.zero;
    private GroundController _controller;
    private float _gravityAccumulate = 0f;

    public override MovementType getMovementType(){return MovementType.FrameEvent;}

    public override void initialize(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
        _currentDirection = Vector3.zero;

        if(_controller == null)
            _controller = targetEntity.getGroundController();
        
        int numMovementValue = (int)FrameEventMovementValueType.Count;
        for(int i = 0; i < numMovementValue; ++i)
        {
            _movementValues[i] = 0f;
        }

        _currentVelocity = Vector3.zero;
        _gravityAccumulate = 0f;
    }

    public override void updateFirst(GameEntityBase targetEntity)
    {
        
    }

    public override bool progress(float deltaTime, Vector3 direction)
    {
        if(_targetEntity == null)
        {
            DebugUtil.assert(false, "타겟 엔티티가 없는데 무브먼트 업데이트가 돌고 있습니다. 통보 요망");
            return false;
        }

        float resultSpeed = _movementValues[0] + (_movementValues[0] >= 0 ? -_movementValues[3] : _movementValues[3]);
        _currentVelocity += (direction * _movementValues[0]) * deltaTime;

        Vector3 velocityDirection = _currentVelocity.normalized;
        _currentVelocity -= _currentVelocity.normalized * _movementValues[3] * deltaTime;

        if(Vector3.Angle(_currentVelocity.normalized, velocityDirection) > 100f)
            _currentVelocity = Vector3.zero;
        else if(_currentVelocity.sqrMagnitude > _movementValues[2] * _movementValues[2])
            _currentVelocity = _currentVelocity.normalized * _movementValues[2];

        _currentDirection = _currentVelocity.normalized;
        Vector3 moveDelta = _currentVelocity * deltaTime;

        _gravityAccumulate += _gravity * deltaTime;
        moveDelta.y = (_currentVelocity + (Vector3.up * _gravityAccumulate)).y * deltaTime;

        movementOfFrame = _controller.Progress(_targetEntity.transform.position, moveDelta, Vector2.zero, false);
        bool onGround = _controller.collisions.below;
        if(onGround || onGround == false && _controller.collisions.above && _gravityAccumulate > 0f)
            _gravityAccumulate = 0f;

        _targetEntity.getActionGraph().setActionConditionData_Bool(ConditionNodeUpdateType.Action_OnGround, onGround);
        _targetEntity.getActionGraph().setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsFalling, !onGround && moveDelta.y < 0f);

        return true;
    }

    public override void release()
    {
        
    }

    public void setMovementValue(float value, int valueType)
    {
        if(valueType == 1)
        {
            if(MathEx.equals(_targetEntity.getDirection().sqrMagnitude, 0f, float.Epsilon) == false)
            {
                float targetVelocity = value > _movementValues[2] ? _movementValues[2] : value;
                _currentVelocity = _targetEntity.getDirection() * targetVelocity;
            }
            return;
        }

        _movementValues[valueType] = value;
    }

    public void StartJump(float jumpPower) 
    {
        _gravityAccumulate = jumpPower;
    }
}