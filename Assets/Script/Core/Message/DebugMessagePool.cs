using System.Collections.Generic;


public static class DebugMessagePool
{
    private static Queue<DebugMessage>  _freeQueue = new Queue<DebugMessage>();
    private static int                  _messageCount = 0;
    public static DebugMessage CreateNewItem()
    {
        ++_messageCount;
        return new DebugMessage();
    }
    public static DebugMessage GetMessage()
    {
        if(_freeQueue.Count == 0)
            return CreateNewItem();
        return _freeQueue.Dequeue();
    }
    public static void ReturnMessage(DebugMessage msg)
    {
        _freeQueue.Enqueue(msg);
    }
    public static int GetMessageCountAll() {return _messageCount;}
}
