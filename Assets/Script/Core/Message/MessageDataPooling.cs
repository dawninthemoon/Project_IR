using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MessageData
{
    public bool isUsing = false;
}
public class MessageDataPool 
{
    public List<MessageData>    _poolList = new List<MessageData>();
    public int                  _count = 0;
    public MessageData Get<T>() where T : MessageData, new()
    {
        if(_count == _poolList.Count)
            _count = 0;
        if(_poolList[_count].isUsing == true)
        {
            _poolList.Add(new T());
            _count = 0;
            return _poolList[_poolList.Count - 1];
        }
        else
        {
            _poolList[_count].isUsing = true;
            return _poolList[_count++];
        }
    }
}
public static class MessageDataPooling
{
    private static Dictionary<System.Type, MessageDataPool> _messageDataDic = new Dictionary<System.Type, MessageDataPool>();
    static MessageDataPooling()
    {
        RegisterMessageData<IntData>(10);
        RegisterMessageData<FloatData>(10);
        RegisterMessageData<BoolData>(10);
        RegisterMessageData<StringData>(10);
        RegisterMessageData<ActionData>();
        RegisterMessageData<Vector3Data>(5);
        RegisterMessageData<QuaternionData>(5);
    }
    public static void RegisterMessageData<T>(int initNum = 3) where T : MessageData,new()
    {
        System.Type type = typeof(T);
        if(_messageDataDic.ContainsKey(type) == false)
        {
            _messageDataDic.Add(type, new MessageDataPool());
            for(int i =0; i<initNum;i++)
            {
                _messageDataDic[type]._poolList.Add(new T());
            }
        }
    }
    public static T GetMessageData<T>() where T : MessageData, new()
    {
        System.Type type = typeof(T);
        if(_messageDataDic.ContainsKey(type) == false)
        {
            RegisterMessageData<T>();
        }
        return _messageDataDic[type].Get<T>() as T;
    }
    public static T CastData<T>(object data) where T : MessageData
    {
        T realData = (T)data;
        realData.isUsing = false;
        return realData;
    }

    public static void ReturnData(MessageData data)
    {
        data.isUsing = false;
    }
}
#region PrimaryMessageData

public class IntData : MessageData
{
    public int value;
}
public class FloatData : MessageData
{
    public float value;
}
public class BoolData : MessageData
{
    public bool value;
}
public class StringData : MessageData
{
    public string value;
}
public class ActionData : MessageData
{
    public Action value;
}
public class Vector3Data : MessageData
{
    public Vector3 value;
}
public class QuaternionData : MessageData
{
    public Quaternion value;
}
public class ColorData : MessageData
{
    public Color value;
}


#endregion
