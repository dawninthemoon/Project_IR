using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager
{
    private EntityControlBase _mainControl;
    private EntityControlBase _subControl;
    private bool _subControlActivate = false;
    private Dictionary<System.Type, EntityControlBase> _controlCache = new Dictionary<System.Type, EntityControlBase>();
    public void Prgoress(float deltaTime)
    {
        if(_subControlActivate == true)
        {
            _subControlActivate = _subControl.Progress(deltaTime);
        }
        else
        {
            _mainControl?.Progress(deltaTime);
        }
        
    }
    public void Release()
    {
        _mainControl?.Release();
        _subControl?.Release();
        _controlCache.Clear();
    }
    public bool IsMoving() 
    {
        return _subControlActivate ? _subControl.IsMoving() : _mainControl.IsMoving();
    }
    public T SetControl<T>(Transform targetTransform) where T : EntityControlBase, new()
    {
        _mainControl?.Release();
        if(_controlCache.ContainsKey(typeof(T)) == false)
            _controlCache.Add(typeof(T), new T());
        T newControl = _controlCache[typeof(T)] as T;
        _mainControl = newControl;
        _mainControl.SetTargetTransform(targetTransform);
        _mainControl.Initialize();
        
        return newControl;
    }
    public T SetSubControl<T>(Transform targetTransform) where T : EntityControlBase, new()
    {
        _subControl?.Release();
        if(_controlCache.ContainsKey(typeof(T)) == false)
            _controlCache.Add(typeof(T), new T());
        T newControl = _controlCache[typeof(T)] as T;
        _subControl = newControl;
        _subControl.SetTargetTransform(targetTransform);
        _subControl.Initialize();
        
        _subControlActivate = true;
        return newControl;
    }
}
