// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ChaserEntityControlEx : EntityControlBase
// {
//     private RandomMovement _randomMovement;
//     private StraightPathfindMovement _pathfindMovement;
//     private Transform _chaseTarget;
//     private bool _isChaseTarget = false;
//     private float _identifyRange = 0f;
//     private float _chaseRange = 0f;
//     public override void Initialize()
//     {
//         _pathfindMovement = SetMovement<StraightPathfindMovement>();
//         _randomMovement = SetMovement<RandomMovement>();
//         _randomMovement.SetMovement(1f);
//         _randomMovement.SetRandomUpdateInterval(5f);
//     }
//     public override bool Progress(float deltaTime)
//     {
//         if(IsValid() == false)
//         {
//             DebugUtil.assert(false, "chaser control is invalid");
//             return false;
//         }
//         if(_isChaseTarget == true)
//         {
//             if(CanChase() == true)
//             {
//                 UpdatePath(_chaseTarget.position,0.5f);
//             }
//             else
//             {
//                 SetMovement<RandomMovement>();
//                 _isChaseTarget = false;
//             }
//         }
//         else
//         {
//             if(TargetInRange() == true)
//             {
//                 SetMovement<StraightPathfindMovement>();
//                 _isChaseTarget = true;
//             }
//         }
//         _currentMovement.UpdatePosition(_targetTransform.localPosition);
//         if(_currentMovement.Progress(deltaTime) == true)
//         {
//             _currentMovement.SetFrameToLocalTransform(_targetTransform);
//             return true;
//         }
//         return false;
//     }
//     public override void Release()
//     {
//     }
//     public bool TargetInRange()
//     {
//         float dist = Vector3.Distance(_targetTransform.position,_chaseTarget.position);
//         return dist <= _identifyRange;
//     }
//     public bool CanChase()
//     {
//         float dist = Vector3.Distance(_targetTransform.position,_chaseTarget.position);
//         return dist <= _chaseRange;
//     }
//     public void SetChaseTarget(Transform target, float identifyRange, float chaseRange)
//     {
//         _chaseTarget = target;
//         _identifyRange = identifyRange;
//         _chaseRange = chaseRange;
//         DebugUtil.assert(identifyRange < chaseRange, "identifyRange >= chaseRange");
//     }
//     public void UpdatePath(Vector2 targetPosition, float destTime)
//     {
//         _pathfindMovement.SetMovement(_targetTransform.position,targetPosition,destTime);
//     }
// }
