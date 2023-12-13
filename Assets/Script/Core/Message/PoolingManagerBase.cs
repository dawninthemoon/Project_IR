using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManagerBase<T> : ManagerBase where T : ObjectBase
{
    private Queue<T> _objectPool = new Queue<T>();

    public override void DeregisteReceiver(int target)
    {
        ObjectBase obj = _receivers[target];
        if((obj is T) == true)
        {
            obj.gameObject.SetActive(false);
            _objectPool.Enqueue(obj as T);
        }
        
        base.DeregisteReceiver(target);
    }

    protected T dequeuePoolEntity()
    {
        if(_objectPool.Count == 0)
            createPoolItem(1);

        T obj = _objectPool.Dequeue();
        obj.gameObject.SetActive(true);

        return obj;
    }

    private void createPoolItem(int count)
    {
        for(int i = 0; i < count; ++i)
        {
            GameObject projectileObject = new GameObject();
            T entity = projectileObject.AddComponent<T>();

            projectileObject.gameObject.SetActive(false);

            _objectPool.Enqueue(entity);
        }
    }
}
