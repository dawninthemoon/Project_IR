using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UniqueIDBase : MonoBehaviour
{
    static private Dictionary<string, int>  _uniqueIDCache = new Dictionary<string, int>();
    private int                             _uniqueID;
    [SerializeField] private int            _shareID;

    protected virtual void Awake()
    {
        setUniqueID();
    }
    public int GetUniqueID() {return _uniqueID;}
    public int GetShareID() {return _shareID;}
    public void CacheUniqueID(string key)
    {
        if(_uniqueIDCache.ContainsKey(key) == true)
        {
            DebugUtil.assert(false,"unique id cache aleardy exists, key : {0}",key);
            return;
        }
            
        
        _uniqueIDCache.Add(key, _uniqueID);
    }
    public static int QueryUniqueID(string key)
    {
        if(_uniqueIDCache.ContainsKey(key) == false)
        {
            DebugUtil.assert(_uniqueIDCache.ContainsKey(key) == true,"unique id cache does not exists, key : {0}",key);
            return -1;
        }
        return _uniqueIDCache[key];
    }
    public static void ClearUniqueIDCache()
    {
        _uniqueIDCache.Clear();
    }
    public void setUniqueID()
    {
        _uniqueID = gameObject.GetInstanceID();
    }

}