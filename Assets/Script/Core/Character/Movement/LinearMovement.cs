// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class LinearMovement : MovementBase
// {
//     private Vector2     _originalPosition = Vector2.zero;
//     private Vector2     _targetPosition = Vector2.zero;
//     private float       _elapsedTime = 0f;
//     private float       _destTime = 0f;
//     public override void Initialize()
//     {
//     }
//     public override bool Progress(float deltaTime)
//     {
//         if(isMoving == false)
//             return false;
//         isMoving = _elapsedTime < _destTime;
//         float percentage = _elapsedTime / _destTime;
//         movementOfFrame = (isMoving == true ? Vector2.Lerp(_originalPosition,_targetPosition, percentage) : _targetPosition);
//         _elapsedTime += deltaTime;
//         return true;
//     }
//     public override void Release()
//     {
//     }

// }
