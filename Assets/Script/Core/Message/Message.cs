using System;


public class Message
{
    public ushort       title;
    public int          target;

    public Object       data;
    public Object       sender;

    public void Set(ushort title, int target, Object data, Object sender)
    {
        this.title = title;
        this.target = target;
        this.data = data;
        this.sender = sender;
    }
}
