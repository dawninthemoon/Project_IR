// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class StepPathfindMovement : MovementBase
// {
//     private Vector2     _originalPosition = Vector2.zero;
//     private Vector2     _nextCellPosition = Vector2.zero;
//     private Vector2     _targetPosition = Vector2.zero;
    
//     private float       _elapsedTime = 0f;
//     private float       _destTime = 0f;
//     private bool        _arrivedNextCell = true;
//     public override void Initialize()
//     {
//         _arrivedNextCell = true;
//     }
//     public override bool Progress(float deltaTime)
//     {
//         if(isMoving == false)
//             return false;
//         isMoving = UpdateMarchTarget() == false;
//         _arrivedNextCell = _elapsedTime >= _destTime;
//         float percentage = _elapsedTime / _destTime;
//         movementOfFrame = (_arrivedNextCell == false ? Vector2.Lerp(_originalPosition,_nextCellPosition, percentage) : _nextCellPosition);
//         _elapsedTime += deltaTime;
//         return true;
//     }
//     public override void Release()
//     {
        
//     }

//     public void SetMovement(Vector2 originPos, Vector2 targetPos, float destTime)
//     {
//         UpdatePosition(originPos);
//         _targetPosition = WorldToCellCenter(targetPos);
//         _destTime = destTime;
//         isMoving = true;
//     }
//     private void SetNextMovement(Vector2 originPos, Vector2 NextPos)
//     {
//         _originalPosition = originPos;
//         _nextCellPosition = NextPos;
//         _elapsedTime = 0f;
//     }
//     private bool UpdateMarchTarget()
//     {
//         if(_arrivedNextCell == false)
//             return false;
        
//         _arrivedNextCell = false;
//         Vector3Int currentCell = GetCurrentCell();
//         Vector3Int targetCell = GetTargetCell();
//         if(currentCell == targetCell)
//             return true;
//         Vector3Int nextCell;
//         if(SelectNextCell(GetTargetDirection(),out nextCell) == false)
//         {
//             DebugUtil.assert(false,"blocked every direction");
//             return false;
//         }
//         SetNextMovement(_currentPosition,CellToWorld(nextCell));
//         return false;
//     }
//     private Vector2 WorldToCellCenter(Vector2 world)
//     {
//         Vector3Int cell = SceneMaster.Instance().GetMainGrid().WorldToCell(world);
//         return SceneMaster.Instance().GetMainGrid().GetCellCenterWorld(cell);
//     }
//     private Vector2 CellToWorld(Vector3Int cell)
//     {
//         return SceneMaster.Instance().GetMainGrid().GetCellCenterWorld(cell);
//     }
//     private Vector3Int GetCurrentCell()
//     {
//         return SceneMaster.Instance().GetMainGrid().WorldToCell(_currentPosition);
//     }
//     private Vector3Int GetTargetCell()
//     {
//         return SceneMaster.Instance().GetMainGrid().WorldToCell(_targetPosition);
//     }
//     private int GetTargetDirection()
//     {
//         Vector3Int currentCell = GetCurrentCell();
//         Vector3Int targetCell = GetTargetCell();
//         if(currentCell.x < targetCell.x)
//             return 2;
//         if(currentCell.y < targetCell.y)
//             return 1;
//         if(currentCell.x > targetCell.x)
//             return 0;
//         if(currentCell.y > targetCell.y)
//             return 3;
//         DebugUtil.assert(false,"wtf");
//         return -1;
//     }
//     private bool SelectNextCell(int targetDirection, out Vector3Int nextCell)
//     {
//         Vector3Int point = GetCurrentCell();
//         nextCell = point;
//         for(int i = 0; i < 4; ++i)
//         {
//             int nextDirection = targetDirection + i;
//             nextDirection = nextDirection >= 4 ? nextDirection - 4 : nextDirection;
//             nextCell = point + _directions[nextDirection];
//             if(IsMoveableCell(nextCell) == true)
//             {
//                 return true;
//             }
//         }
//         return false;
//     }
//     private bool IsMoveableCell(Vector3Int cellPoint)
//     {
//         return SceneMaster.Instance().GetWallTilemap().GetTile(cellPoint) == null;
//     }
// }
