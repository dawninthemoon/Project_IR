using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkBalloonManager : Singleton<TalkBalloonManager>
{
    private Queue<TalkBalloon> _talkBalloonPool = new Queue<TalkBalloon>();
    private List<TalkBalloon> _currentlyActivateItem = new List<TalkBalloon>();

    private Dictionary<Transform, TalkBalloon> _attachMap = new Dictionary<Transform, TalkBalloon>();

    public void updateTalkBalloonManager(float deltaTime)
    {
        for(int index = 0; index < _currentlyActivateItem.Count; ++index)
        {
            if(_currentlyActivateItem[index].updateTalkBalloon(deltaTime) || _currentlyActivateItem[index].gameObject.activeInHierarchy == false)
            {
                if(_attachMap.ContainsKey(_currentlyActivateItem[index].transform.parent))
                    _attachMap.Remove(_currentlyActivateItem[index].transform.parent);

                _currentlyActivateItem[index].setActive(false);
                _currentlyActivateItem[index].transform.SetParent(null);

                _talkBalloonPool.Enqueue(_currentlyActivateItem[index]);
                _currentlyActivateItem.RemoveAt(index);

                --index;
            }
        }

    }

    public void activeTalkBalloon(Transform attachTransform, Vector3 localOffset, string text, float time)
    {
        if(_talkBalloonPool.Count == 0)
        {
            GameObject talkBalloonObject = GameObject.Instantiate(ResourceContainerEx.Instance().GetPrefab("Prefab/TalkBalloon"));
            TalkBalloon talkBalloonComponent = talkBalloonObject.GetComponent<TalkBalloon>();
            _talkBalloonPool.Enqueue(talkBalloonComponent);
        }

        TalkBalloon talkBalloon = _talkBalloonPool.Dequeue();
        talkBalloon.transform.SetParent(attachTransform);
        talkBalloon.transform.localPosition = localOffset;

        talkBalloon.setText(text, time);
        talkBalloon.setActive(true);

        _currentlyActivateItem.Add(talkBalloon);

        if(attachTransform == null)
            return;

        if(_attachMap.ContainsKey(attachTransform))
        {
            _attachMap[attachTransform].setActive(false);
            _attachMap[attachTransform] = talkBalloon;
        }
        else
        {
            _attachMap.Add(attachTransform,talkBalloon);
        }
    }

    public void deactiveTalkBalloon(Transform parentTransform)
    {
        if(_attachMap.ContainsKey(parentTransform) == false)
            return;

        _attachMap[parentTransform].setActive(false);

    }

    
}
