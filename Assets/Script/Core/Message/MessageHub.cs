using System.Collections.Generic;
using System;

public abstract class MessageHub<T> : MessageReceiver where T : MessageReceiver
{
    protected Dictionary<int, T>    _receivers = new Dictionary<int, T>();
    protected Action<Message>       _unknownMessageProcess = (msg)=>{};
    public virtual void RegisterReceiver(T receiver)
    {
        _receivers.Add(receiver.GetUniqueID(), receiver);
    }
    public virtual void DeregisteReceiver(int target)
    {
        if(_receivers.ContainsKey(target))
        {
            _receivers.Remove(target);
        }
            
    }
    public virtual void DeregisteReceiver(MessageReceiver target)
    {
        if(_receivers.ContainsKey(target.GetUniqueID()))
            _receivers.Remove(target.GetUniqueID());
    }
    public void CallReceiveMessageProcessing()
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.ReceiveMessageProcessing();
        }
    }
    public void SendMessageProcessing()
    {
        foreach(var receiver in _receivers.Values)
        {
            SendMessageProcessing(receiver);
        }
    }
    public virtual void SendMessageProcessing(T receiver)
    {
        Message msg = receiver.DequeueSendMessage();
        while(msg != null)
        {
            if(msg.target <= _boradcastNumber + 1)
            {
                HandleBroadcastMessage(msg);
            }
            else
            {
                HandleMessage(msg);
            }
            msg = receiver.DequeueSendMessage();
        }
    }
    public virtual void HandleBroadcastMessage(Message msg)
    {
        foreach(var item in _receivers.Values)
        {
            if(msg.target == _boradcastWithoutSenderNumber && msg.sender != null)
            {
                if(item.GetUniqueID() == ((MessageReceiver)msg.sender).GetUniqueID())
                    continue;
            }
            var send = MessagePack(msg);
            item.ReceiveMessage(send);
        }
        MessagePool.ReturnMessage(msg);
    }
    public virtual void HandleMessage(Message msg)
    {
        if(IsInReceivers(msg.target))
        {
            _receivers[msg.target].ReceiveMessage(msg);
        }
        else if(msg.target == 0 || GetUniqueID() == msg.target)
        {
            ReceiveMessage(msg);
        }
        else
        {
            _unknownMessageProcess(msg);
        }
    }
    public T GetReciever(int number)
    {
        return _receivers[number];
    }
    public bool IsInReceivers(int number)
    {
        return _receivers.ContainsKey(number);
    }
    public override void dispose(bool disposeFromMaster)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.dispose(true);
        }
        _receivers.Clear();
        base.dispose(disposeFromMaster);
    }
#if UNITY_EDITOR
    public Dictionary<int, T> Debug_GetReceivers()
    {
        return _receivers;
    }
#endif
}
