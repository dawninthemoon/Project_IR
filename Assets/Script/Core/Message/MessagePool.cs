using System.Collections.Generic;


public static class MessagePool
{
    private static Queue<Message>   _freeQueue = new Queue<Message>();
    private static int              _messageCount = 0;
    
    public static Message CreateNewItem()
    {
        ++_messageCount;
        return new Message();
    }

    public static Message GetMessage()
    {
        if(_freeQueue.Count == 0)
            return CreateNewItem();

        return _freeQueue.Dequeue();
    }

    public static void ReturnMessage(Message msg)
    {
        if (msg.data != null && msg.data.GetType() == typeof(MessageData))
        {
            var messageData = (MessageData) msg.data;
            messageData.isUsing = false;
        }
        _freeQueue.Enqueue(msg);
    }

    public static int GetMessageCountAll() {return _messageCount;}
}


