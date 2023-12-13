public class DebugMessage
{
    public ushort       title;
    public int          target;

    public bool         data;
    public int          senderNumber;
    public string       senderName;
    public UnityEngine.GameObject gameObject;

    public int          count;

    public void Set(ushort title, int target, bool data, int senderNumber, string senderName, int count)
    {
        this.title = title;
        this.target = target;
        this.data = data;
        this.senderNumber = senderNumber;
        this.senderName = senderName;
        this.count = count;
    }
}
