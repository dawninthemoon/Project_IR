using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CollisionDelegate(CollisionSuccessData collisionData);

public struct CollisionSuccessData
{
    public object _requester;
    public object _target;
    public Vector3 _startPoint;
}

public struct CollisionRequestData
{
    public CollisionDelegate _collisionDelegate;
    public System.Action _collisionEndEvent;
    public CollisionInfo _collision;

    public object _requestObject;
}

public struct CollisionObjectData
{
    public CollisionInfo _collisionInfo;
    public object _collisionObject;
}

public class CollisionManager : Singleton<CollisionManager>
{
    private Dictionary<int, List<CollisionObjectData>>          _collisionObjectList = new Dictionary<int, List<CollisionObjectData>>();
    private Stack<CollisionRequestData>        _collisionRequestStack = new Stack<CollisionRequestData>();


    private bool[][] _collisionMatrix;

    private int                                                 _collisionTypeCount = 0;
    private int                                                 _collisionRequestCount = 0;

    public CollisionManager()
    {
        _collisionTypeCount = (int)CollisionType.Count;

        buildCollisionMatrix();
        buildCollisionInfoList();
    }

    private void buildCollisionMatrix()
    {
        int collisionTypeCount = (int)CollisionType.Count;
        _collisionMatrix = new bool[collisionTypeCount][];

        for(int i = 0; i < collisionTypeCount; ++i)
        {
            _collisionMatrix[i] = new bool[collisionTypeCount];

            CollisionType type = (CollisionType)i;
            if(type == CollisionType.Default)
            {
                setCollisionEnable(i,CollisionType.Default,true);
                setCollisionEnable(i,CollisionType.Character,true);
                setCollisionEnable(i,CollisionType.Attack,false);
                setCollisionEnable(i,CollisionType.Projectile,false);
            }
            else if(type == CollisionType.Character)
            {
                setCollisionEnable(i,CollisionType.Default,true);
                setCollisionEnable(i,CollisionType.Character,true);
                setCollisionEnable(i,CollisionType.Attack,false);
                setCollisionEnable(i,CollisionType.Projectile,false);
            }
            else if(type == CollisionType.Projectile)
            {
                setCollisionEnable(i,CollisionType.Default,true);
                setCollisionEnable(i,CollisionType.Character,true);
                setCollisionEnable(i,CollisionType.Attack,false);
                setCollisionEnable(i,CollisionType.Projectile,false);
            }
            else if(type == CollisionType.Attack)
            {
                setCollisionEnable(i,CollisionType.Default,true);
                setCollisionEnable(i,CollisionType.Character,true);
                setCollisionEnable(i,CollisionType.Attack,false);
                setCollisionEnable(i,CollisionType.Projectile,false);
            }
            
        }
    }

    public bool canCollision(CollisionType type, CollisionType target)
    {
        return _collisionMatrix[(int)type][(int)target];
    }

    private void setCollisionEnable(int type, int target, bool value)
    {
        _collisionMatrix[type][target] = value;
    }

    private void setCollisionEnable(int type, CollisionType target, bool value)
    {
        setCollisionEnable(type,(int)target,value);
    }

    private void setCollisionEnable(CollisionType type, CollisionType target, bool value)
    {
        setCollisionEnable((int)type, (int)target, value);
    }

    private void buildCollisionInfoList()
    {
        for(int i = 0; i < _collisionTypeCount; ++i)
        {
            _collisionObjectList.Add(i, new List<CollisionObjectData>());
        }
    }

    public void registerObject(CollisionInfo collisionData, object collisionObject)
    {
        registerObject(new CollisionObjectData{_collisionInfo = collisionData, _collisionObject = collisionObject},collisionData.getCollisionType());
    }

    public void registerObject(CollisionObjectData objectData, CollisionType collisionType)
    {
        _collisionObjectList[(int)collisionType].Add(objectData);
    }
    
    public void deregisterObject(CollisionInfoData collisionInfo, object collisionObject)
    {
        for(int i = 0; i < _collisionObjectList[(int)collisionInfo.getCollisionType()].Count;)
        {
            if(_collisionObjectList[(int)collisionInfo.getCollisionType()][i]._collisionObject == collisionObject)
                deregisterObject(i,collisionInfo.getCollisionType());
            else
                ++i;

        }
    }

    public void deregisterObject(int index, CollisionType collisionType)
    {
        if(index < 0 || index >= _collisionObjectList[(int)collisionType].Count)
            return;

        _collisionObjectList[(int)collisionType].RemoveAt(index);
    }

    public void collisionRequest(CollisionInfo collisionData, object requestObject, CollisionDelegate collisionDelegate, System.Action collisionEndEvent)
    {
        collisionRequest(
            new CollisionRequestData
            {
                _collisionDelegate = collisionDelegate, 
                _collisionEndEvent = collisionEndEvent, 
                _requestObject = requestObject, 
                _collision = collisionData,
            });
    }

    public void collisionRequest(CollisionRequestData request)
    {
        ++_collisionRequestCount;
        _collisionRequestStack.Push(request);
    }

    public void collisionUpdate()
    {
        if(_collisionRequestCount == 0)
            return;

        while(_collisionRequestStack.Count != 0)
        {
            CollisionRequestData request = _collisionRequestStack.Pop();

            for(int i = 0; i < _collisionTypeCount; ++i)
            {
                collisionCheck(i,request);
            }

            request._collisionEndEvent?.Invoke();
        }

        _collisionRequestCount = 0;
    }

    private void collisionCheck(int layer, CollisionRequestData request)
    {
        if(canCollision(request._collision.getCollisionType(), (CollisionType)layer) == false)
            return;

        List<CollisionObjectData> collisionList = _collisionObjectList[layer];
        CollisionInfo collisionInfo = request._collision;
        for(int i = 0; i < collisionList.Count; ++i)
        {
            if(collisionInfo.getUniqueID() == collisionList[i]._collisionInfo.getUniqueID() || request._requestObject == collisionList[i]._collisionObject)
                continue;
            
            if(collisionInfo.collisionCheck(collisionList[i]._collisionInfo) == true)
            {
                CollisionSuccessData data;
                data._requester = request._requestObject;
                data._target = collisionList[i]._collisionObject;
                data._startPoint = request._collision.getCenterPosition();

                request._collisionDelegate?.Invoke(data);
            }
                
        }
    }
}
