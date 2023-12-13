using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ManagerBase : MessageHub<ObjectBase>, IProgress
{
    public static ManagerBase _managerInstance;

    private List<ObjectBase> _addedObject = new List<ObjectBase>();
    public override void RegisterReceiver(ObjectBase receiver)
    {
        base.RegisterReceiver(receiver);
        _addedObject.Add(receiver);
    }
    public void SendMessageQuick(Message msg)
    {
        MasterManager.instance.HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void SendMessageQuick(ushort title, int target, Object data)
    {
        var msg = MessagePack(title,target,data);
        MasterManager.instance.HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    protected override void Awake()
    {
        _managerInstance = this;

        base.Awake();
        
        assign();
        //Initialize();
    }
    protected virtual void Start()
    {
        initialize();
    }
    public virtual void assign()
    {
        _unknownMessageProcess = (msg)=>
        {
            SendMessageEx(msg);
        };
        AddAction(MessageTitles.system_registerRequest,(msg)=>
        {
            ObjectBase receiver = (ObjectBase)msg.sender;
            if(receiver == null)
            {
                DebugUtil.assert(false,"receiver is null Tlqkf");
                return;
            }
            RegisterReceiver(receiver);
        });
        
        AddAction(MessageTitles.system_deregisterRequest,(msg)=>
        {
            DeregisteReceiver(((ObjectBase)msg.sender).GetUniqueID());
        });
    }
    public virtual void initialize(){}
    public virtual void progress(float deltaTime)
    {
        if(_addedObject.Count != 0)
        {
            foreach(var receiver in _addedObject)
            {
                receiver.firstUpdate();
            }
            _addedObject.Clear();
        }
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
            {
                continue;
            }
            receiver.progress(deltaTime);
        }
    }
    public virtual void afterProgress(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
                continue;
            receiver.afterProgress(deltaTime);
        }
    }
    public virtual void fixedProgress(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
                continue;
            receiver.fixedProgress(deltaTime);
        }
    }
    public virtual void release(bool disposeFromMaster)
    {
        if(disposeFromMaster == false)
            DeregisterRequest();
#if UNITY_EDITOR
        Debug_ClearQueue();
#endif
    }
    public void deregisterAll()
    {
        foreach(var receiver in _receivers.Values)
        {
            if(receiver == null || !receiver.gameObject.activeInHierarchy || !receiver.enabled)
            {
                continue;
            }

            receiver.deactive();
            receiver.DeregisterRequest();
        }
    }
    public void RegisterRequest()
    {
        var msg = MessagePack(MessageTitles.system_registerRequest,0,null);
        MasterManager.instance.HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void DeregisterRequest()
    {
        var msg = MessagePack(MessageTitles.system_deregisterRequest,0,GetUniqueID());
        MasterManager.instance.ReceiveMessage(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public override void dispose(bool disposeFromMaster)
    {
        base.dispose(disposeFromMaster);
        release(disposeFromMaster);
    }
    protected virtual void OnDestroy()
    {
        dispose(false);
    }
}
