using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySensor : MonoBehaviour
{
    public GameObject _targetObject = null;

    private System.Action<GameObject> _whenTargetChanged = (obj)=>{};


    public void AddChangedAction(System.Action<GameObject> targetAction)
    {
        _whenTargetChanged += targetAction;
    }

    public void DeleteChangedAction(System.Action<GameObject> targetAction)
    {
        _whenTargetChanged -= targetAction;
    }

    public GameObject GetTargetObject()
    {
        if(_targetObject != null && _targetObject.activeInHierarchy == false)
            _targetObject = null;

        return _targetObject;
    }

    public void OnTriggerStay2D(Collider2D coll)
    {
        if(transform.parent != null && coll.gameObject == transform.parent.gameObject)
            return;

        if(GetTargetObject() != null)
        {
            float targetDist = Vector2.Distance(transform.position, _targetObject.transform.position);
            float newTargetDist = Vector2.Distance(transform.position, coll.transform.position);

            if(targetDist > newTargetDist)
            {
                _targetObject = coll.gameObject;
                _whenTargetChanged.Invoke(_targetObject);
            }
        }
        else
        {
            _targetObject = coll.gameObject;
            _whenTargetChanged.Invoke(_targetObject);
        }
    }

    public void OnTriggerExit2D(Collider2D coll)
    {
        if(_targetObject == coll.gameObject)
        {
            _targetObject = null;
            _whenTargetChanged.Invoke(_targetObject);
        }
    }
}
