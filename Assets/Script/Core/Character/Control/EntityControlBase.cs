using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityControlBase : IProgressBase
{
    public abstract void Initialize();
    public abstract bool Progress(float deltaTime);
    public abstract void Release();
    private Dictionary<System.Type, MovementBase> _movementCache = new Dictionary<System.Type, MovementBase>();
    protected MovementBase _currentMovement;
    protected Transform _targetTransform;
    public void SetTargetTransform(Transform target) 
    {
        _targetTransform = target;
    }
    public bool IsValid()
    {
        return _currentMovement != null && _targetTransform != null;
    }
    public bool IsMoving() 
    {
        return _currentMovement.isMoving();
    }
    public T SetMovement<T>() where T : MovementBase, new()
    {
        _currentMovement?.release();
        
        if(_movementCache.ContainsKey(typeof(T)) == false)
            _movementCache.Add(typeof(T), new T());
        T newMovement = _movementCache[typeof(T)] as T;
        _currentMovement = newMovement;
        
        // if(DebugUtil.assert(_currentMovement != null, "movement is null") == true)
        // {
        //     _currentMovement.initialize();
        // }
        return newMovement;
    }
}
