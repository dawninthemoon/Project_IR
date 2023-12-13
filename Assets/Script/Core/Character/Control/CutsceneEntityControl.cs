// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class CutsceneEntityControl : EntityControlBase
// {
//     private StraightPathfindMovement _testMovement;
//     public override void Initialize()
//     {
//         _testMovement = SetMovement<StraightPathfindMovement>();
//     }
//     public override bool Progress(float deltaTime)
//     {
//         if(IsValid() == false)
//         {
//             DebugUtil.assert(false, "cutscene control is not valid");
//             return false;
//         }
//         _testMovement.UpdatePosition(_targetTransform.localPosition);
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

// }
