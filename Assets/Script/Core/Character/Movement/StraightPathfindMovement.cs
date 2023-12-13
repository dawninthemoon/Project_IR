// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Tilemaps;

// public class StraightPathfindMovement : MovementBase
// {
//     private struct PathfindNode
//     {
//         public Vector3Int cell;
//         public Vector3Int parentCell;
//     }
//     private Dictionary<Vector3Int,PathfindNode> _checkedMap = new Dictionary<Vector3Int,PathfindNode>();
//     private List<Vector3Int> _pathList = new List<Vector3Int>();
//     private Vector2     _originalPosition = Vector2.zero;
//     private Vector2     _nextCellPosition = Vector2.zero;
//     private float       _elapsedTime = 0f;
//     private float       _destTime = 0f;
//     private int _nextCellPoint = 0;
//     private bool _arrivedNextCell = true;
//     public override void Initialize()
//     {
//         _arrivedNextCell = true;
//         _checkedMap.Clear();
//         _pathList.Clear();
//     }
//     public override bool Progress(float deltaTime)
//     {
//         if(isMoving == false)
//             return false;
//         if(_pathList.Count == 0)
//         {
//             isMoving = false;
//             return false;
//         }
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
//         Pathfind(originPos,targetPos);
//         _destTime = destTime;
//         _nextCellPoint = 0;
//         isMoving = true;
//     }
//     private void SetNextMovement(Vector2 originPos, Vector2 nextPos)
//     {
//         _originalPosition = originPos;
//         _nextCellPosition = nextPos;
//         _elapsedTime = 0f;
//     }
//     private void Pathfind(Vector2 startPosition, Vector2 endPosition)
//     {
//         Vector3Int start = WorldToCell(startPosition);
//         Vector3Int end = WorldToCell(endPosition);
//         Vector3Int parent = start;
//         Vector3Int check = start;
//         _checkedMap.Clear();
//         _pathList.Clear();
//         int count = 0;
//         float prevDist = float.MaxValue;
//         if(IsMoveableCell(end) == false)
//         {
//             DebugUtil.assert(false,"destination is on wall");
//             return;
//         }
//         while(true)
//         {
//             DebugUtil.assert(++count <= 100,"infinity loop");
//             int nearestTarget = -1;
//             float nearestDistance = float.MaxValue;
//             for(int i = 0 ; i < 4; ++i)
//             {
//                 Vector3Int target = check + _directions[i];
//                 if(_checkedMap.ContainsKey(target) == true)
//                     continue;
//                 if(IsMoveableCell(target) == false)
//                     continue;
//                 PathfindNode node;
//                 node.cell = target;
//                 node.parentCell = parent;
//                 _checkedMap.Add(target,node);
//                 float distance = Vector3Int.Distance(target,end);
//                 if(nearestDistance > distance)
//                 {
//                     nearestTarget = i;
//                     nearestDistance = distance;
//                 }
//             }
//             if(nearestTarget == -1)
//                 break;
            
//             if(nearestDistance > prevDist)
//                 break;
            
//             prevDist = nearestDistance;
//             Vector3Int near = check + _directions[nearestTarget];
//             _pathList.Add(near);
//             parent = check;
//             check = near;
//             if(check == end)
//                 break;
//         }
//     }
//     private bool UpdateMarchTarget()
//     {
//         if(_arrivedNextCell == false)
//             return false;
        
//         _arrivedNextCell = false;
        
//         if(_nextCellPoint >= _pathList.Count)
//             return true;
//         Vector3Int nextCell = _pathList[_nextCellPoint++];
        
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
//     private Vector3Int WorldToCell(Vector2 world)
//     {
//         return SceneMaster.Instance().GetMainGrid().WorldToCell(world);
//     }
//     private bool IsMoveableCell(Vector3Int cellPoint)
//     {
//         return SceneMaster.Instance().GetWallTilemap().GetTile(cellPoint) == null;
//     }
// }
