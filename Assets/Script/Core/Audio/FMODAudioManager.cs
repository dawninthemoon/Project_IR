using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FMODAudioManager : Singleton<FMODAudioManager>
{
    private AudioInfoItem                                           _infoItem;

    private Dictionary<int, AudioInfoItem.AudioInfo>                _audioMap;
    private Dictionary<int, Queue<FMODUnity.StudioEventEmitter>>    _cacheMap;

    private Dictionary<int, List<FMODUnity.StudioEventEmitter>>     _activeMap;
    private Dictionary<int, FMOD.Studio.PARAMETER_DESCRIPTION>      _globalCache;

    private GameObject                                              _audioObject;
    private GameObject                                              _listener;

    public void initialize()
    {
        if(_audioObject == null)
        {
            _audioObject = new GameObject("Audio");
        }

        if(_listener == null)
        {
            _listener = new GameObject("Listener");
            _listener.AddComponent<FMODUnity.StudioListener>();
        }

        _infoItem = ResourceContainerEx.Instance().GetScriptableObject("Audio/AudioInfo/AudioInfo") as AudioInfoItem;

        CreateAudioMap();

        _cacheMap = new Dictionary<int, Queue<FMODUnity.StudioEventEmitter>>();
        _activeMap = new Dictionary<int, List<FMODUnity.StudioEventEmitter>>();
        _globalCache = new Dictionary<int, FMOD.Studio.PARAMETER_DESCRIPTION>();

        CreateCachedGlobalParams();
    }

    public void updateAudio()
    {
        foreach(var pair in _activeMap)
        {
            var value = pair.Value;
            for(int i = 0; i < value.Count;)
            {
                if(!value[i].IsPlaying())
                {
                    //_cacheMap[value[i].DataCode].Enqueue(value[i]);
                    ReturnCache(pair.Key,value[i]);
                    value.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }
    }

    public void setListener(Transform target)
    {
        _listener.transform.SetParent(target);
        _listener.transform.localPosition = Vector3.zero;
    }

    public FMODUnity.StudioEventEmitter Play(int id, Vector3 localPosition,Transform parent)
    {
        var emitter = GetCache(id);

        emitter.transform.SetParent(parent);
        emitter.transform.localPosition = localPosition;
        emitter.gameObject.SetActive(true);

        emitter.Play();

        AddActiveMap(id,emitter);
        
        return emitter;
    }

    public void ReturnAllCache()
    {
        foreach(var pair in _activeMap)
        {
            var value = pair.Value;
            for(int i = 0; i < value.Count; ++i)
            {
                value[i].Stop();
                value[i].transform.SetParent(_audioObject.transform);
                ReturnCache(pair.Key,value[i]);

            }
            
            value.Clear();
        }
    }

    public FMODUnity.StudioEventEmitter Play(int id, Vector3 position)
    {
        var emitter = GetCache(id);
        
        emitter.transform.SetParent(null);
        emitter.transform.SetPositionAndRotation(position,Quaternion.identity);
        emitter.gameObject.SetActive(true);

        emitter.Play();
        
        AddActiveMap(id,emitter);

        return emitter;
    }

    private void AddActiveMap(int id, FMODUnity.StudioEventEmitter emitter)
    {
        if(_activeMap.ContainsKey(id))
        {
            _activeMap[id].Add(emitter);
        }
        else
        {
            var list = new List<FMODUnity.StudioEventEmitter>
            {
                emitter
            };
            _activeMap.Add(id,list);
        }
    }

    public void setParam(ref FMODUnity.StudioEventEmitter eventEmitter, int audioID, int[] parameterID, float[] value)
    {
        AudioInfoItem.AudioInfo audioInfo = FindAudioInfo(audioID);
        for(int index = 0; index < parameterID.Length; ++index)
        {
            AudioInfoItem.AudioParameter parameter = audioInfo.FindParameter(parameterID[index]);
            if(parameter == null)
                return;

            float resultValue = Mathf.Clamp(value[index],parameter.min,parameter.max);
            eventEmitter.SetParameter(parameter.name, resultValue);
        }
    }

    public void setParam(ref FMODUnity.StudioEventEmitter eventEmitter, int audioID, int[] parameterID, float value)
    {
        AudioInfoItem.AudioInfo audioInfo = FindAudioInfo(audioID);
        for(int index = 0; index < parameterID.Length; ++index)
        {
            AudioInfoItem.AudioParameter parameter = audioInfo.FindParameter(parameterID[index]);
            if(parameter == null)
                return;

            float resultValue = Mathf.Clamp(value,parameter.min,parameter.max);
            eventEmitter.SetParameter(parameter.name, resultValue);
        }
    }

    public void SetParam(int audioID, int parameterID, float value)
    {
        var n = FindAudioInfo(audioID).FindParameter(parameterID);
        value = Mathf.Clamp(value,n.min,n.max);

        foreach(var list in _activeMap[audioID])
        {
            list.SetParameter(n.name,value);
        }

        foreach(var list in _cacheMap[audioID])
        {
            list.SetParameter(n.name,value);
        }
    }

    public void SetGlobalParam(int id, float value)
    {
        var desc = FindGlobalParamDesc(id);
        var result = FMODUnity.RuntimeManager.StudioSystem.setParameterByID(desc.id, value);
        if(result != FMOD.RESULT.OK)
            Debug.Log("global parameter not found");
        
    }
    
    public float GetGlobalParam(int id)
    {
        var desc = FindGlobalParamDesc(id);
        RESULT result = FMODUnity.RuntimeManager.StudioSystem.getParameterByID(desc.id, out var value);
        if(result != FMOD.RESULT.OK)
            Debug.Log("global parameter not found");
            
        return value;
    }

    private void ReturnCache(int id, FMODUnity.StudioEventEmitter emitter)
    {
        emitter.gameObject.SetActive(false);
        emitter.transform.SetParent(_audioObject.transform);
        _cacheMap[id].Enqueue(emitter);
    }

    private FMODUnity.StudioEventEmitter GetCache(int id)
    {
        if (_cacheMap == null)
        {
            initialize();
        }
        
        if(!_cacheMap.ContainsKey(id) || _cacheMap[id].Count == 0)
        {
            CreateAudioCacheItem(id,1);
        }

        return _cacheMap[id].Dequeue();
    }

    private void CreateAudioCacheItem(int id,int count,bool active = false)
    {
        var audio = FindAudioInfo(id);
        if(audio == null)
            return;
        
        for(int i = 0; i < count; ++i)
        {
            var comp = new GameObject("Audio").AddComponent<FMODUnity.StudioEventEmitter>();
            
            comp.EventReference = audio.eventReference;
            comp.Preload = true;
            comp.gameObject.SetActive(active);
            comp.transform.SetParent(_audioObject.transform);

            if(_cacheMap.ContainsKey(id))
            {
                _cacheMap[id].Enqueue(comp);
            }
            else
            {
                var queue = new Queue<FMODUnity.StudioEventEmitter>();
                queue.Enqueue(comp);
                _cacheMap.Add(id,queue);
            }
            
        }
    }

    private FMOD.Studio.PARAMETER_DESCRIPTION FindGlobalParamDesc(int id)
    {
        if(_globalCache.ContainsKey(id))
        {
            return _globalCache[id];
        }
        else
        {
            Debug.Log("global parameter does not exists");
            return default(FMOD.Studio.PARAMETER_DESCRIPTION);
        }
    }

    private AudioInfoItem.AudioInfo FindAudioInfo(int id)
    {
        if(_audioMap.ContainsKey(id))
        {
            return _audioMap[id];
        }
        else
        {
            Debug.LogError("Audio id not found");
            return null;
        }
    }

    private void CreateCachedGlobalParams()
    {
        var global = _infoItem.FindAudio(0);
        
        foreach(var item in global.parameters)
        {
            var result = FMODUnity.RuntimeManager.StudioSystem.getParameterDescriptionByName(item.name, out var desc);

            if(result != FMOD.RESULT.OK)
            {
                Debug.Log("global Parameter does not exists : " + item.name);
                return;
            }

            _globalCache.Add(item.id,desc);
        }
    }

    private void CreateAudioMap()
    {
        if(_audioMap != null)
        {
            _audioMap.Clear();
        }

        _audioMap = new Dictionary<int, AudioInfoItem.AudioInfo>();

        var data = _infoItem.audioData;

        foreach(var d in data)
        {
            _audioMap.Add(d.id,d);
        }
    }
}