// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class RandomMovement : MovementBase
// {
//     private Vector2     _originalPosition = Vector2.zero;
//     private Vector2     _targetPosition = Vector2.zero;
//     private float       _elapsedTime = 0f;
//     private float       _destTime = 0f;
//     private float       _randomUpdateInterval = 3f;
//     private float       _randomUpdateTime = 0f;
//     public override void Initialize()
//     {
//         _randomUpdateTime = 0;
//         isMoving = false;
//     }
//     public override bool Progress(float deltaTime)
//     {
//         _randomUpdateTime += deltaTime;
//         if(_randomUpdateTime >= _randomUpdateInterval)
//         {
//             Vector3Int cell = SceneMaster.Instance().GetMainGrid().WorldToCell(_currentPosition) + _directions[Random.Range(0,4)];
//             SetNextMovement(SceneMaster.Instance().GetMainGrid().GetCellCenterWorld(cell),_currentPosition);
//         }
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
//     public void SetRandomUpdateInterval(float interval)
//     {
//         _randomUpdateInterval = interval;
//     }
//     public void SetMovement(float destTime)
//     {
//         _destTime = destTime;
//     }
//     public void SetNextMovement(Vector2 nextPos, Vector2 originPos)
//     {
//         _originalPosition = originPos;
//         _targetPosition = nextPos;
//         _elapsedTime = 0f;
//         _randomUpdateTime = 0f;
//         isMoving = true;
//     }
// }
