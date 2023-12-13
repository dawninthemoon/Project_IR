using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRepeater : Singleton<UIRepeater>
{
    private Dictionary<string, GameEntityBase> _targetStatusMap = new Dictionary<string, GameEntityBase>();
    private Dictionary<string, List<UIStatusReceiver>> _registeredReceiverMap = new Dictionary<string, List<UIStatusReceiver>>();

    private List<string> _deleteList = new List<string>();

    public void initialize()
    {
        _targetStatusMap.Clear();
        _deleteList.Clear();
    }

    public void clear()
    {
        _targetStatusMap.Clear();
        _registeredReceiverMap.Clear();

        _deleteList.Clear();
    }

    public void registerUniqueEntity(string uniqueKey, GameEntityBase targetEntity)
    {
        _targetStatusMap[uniqueKey] = targetEntity;
    }

    public void registerUIReceiver(string uniqueKey, UIStatusReceiver receiver)
    {
        if(_registeredReceiverMap.ContainsKey(uniqueKey) == false)
            _registeredReceiverMap.Add(uniqueKey, new List<UIStatusReceiver>());

        _registeredReceiverMap[uniqueKey].Add(receiver);
    }

    public void updateUIRepeater()
    {
        _deleteList.Clear();
        foreach (var item in _targetStatusMap)
        {
            if (item.Value == null || item.Value.isDead())
            {
                _deleteList.Add(item.Key);
                continue;
            }

            if(_registeredReceiverMap.ContainsKey(item.Key) == false)
                continue;

            List<UIStatusReceiver> receiverList = _registeredReceiverMap[item.Key];
            for (int index = 0; index < receiverList.Count; ++index)
            {
                if (receiverList[index] == null)
                {
                    receiverList.RemoveAt(index);
                    --index;
                    continue;
                }

                receiverList[index].updateUI(item.Value.getStatusInfo());
            }
        }

        foreach(var item in _deleteList)
        {
            _targetStatusMap.Remove(item);
        }
    }

}
