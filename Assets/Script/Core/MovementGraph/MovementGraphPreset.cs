using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MovementGraphPreset", menuName = "Scriptable Object/Movement Graph Preset", order = int.MaxValue)]
public class MovementGraphPreset : ScriptableObject
{
    [SerializeField]private List<MovementGraphPresetData> _presetData = new List<MovementGraphPresetData>();
    private Dictionary<string, MovementGraphPresetData> _presetCache = new Dictionary<string, MovementGraphPresetData>();
    private bool _isCacheConstructed = false;

    // private void Awake()
    // {
    //     constructPresetCache();
    // }

    public MovementGraphPresetData getPresetData(string targetName)
    {
        MovementGraphPresetData target = null;
        if(_isCacheConstructed)
        {
            target = _presetCache.ContainsKey(targetName) == true ? _presetCache[targetName] : null;
        }
        else
        {
            foreach(MovementGraphPresetData item in _presetData)
            {
                if(item.getName() == targetName)
                {
                    target = item;
                    break;
                }
            }
        }

        DebugUtil.assert(target != null,"target presetData is not exists : {0}",targetName);
        return target;
    }


    private void constructPresetCache()
    {
        if(_isCacheConstructed == true)
            return;

        foreach(MovementGraphPresetData item in _presetData)
        {
            _presetCache.Add(item.getName(), item);
        }

        _isCacheConstructed = true;
    }

}
