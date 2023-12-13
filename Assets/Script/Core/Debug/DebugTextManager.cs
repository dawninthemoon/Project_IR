using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTextManager : MonoBehaviour
{
    class DebugItem
    {
        public TextMesh _textMesh;
        public float _timer = 0f;
        public string _itemKey;
    }

    public GameObject debugTextPrefab;
    public float padding = -0.3f;
    public float stayTime = 0.3f;
    private Dictionary<string, DebugItem> _debugTextList = new Dictionary<string, DebugItem>();

    private Stack<string> _deleteTarget = new Stack<string>();

    public void updatePosition(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void Update()
    {
        float deltaTime = GlobalTimer.Instance().isUpdateProcessing() ? GlobalTimer.Instance().getSclaedDeltaTime() : 0f;

        foreach(var item in _debugTextList.Values)
        {
            item._timer -= deltaTime;
            if(item._timer <= 0f)
                _deleteTarget.Push(item._itemKey);
        }

        if(_deleteTarget.Count != 0)
        {
            while(_deleteTarget.Count != 0)
            {
                string targetName = _deleteTarget.Pop();
                Destroy(_debugTextList[targetName]._textMesh.gameObject);
                _debugTextList.Remove(targetName);
            }

            updateDebugTextPosition();
        }
    }

    public void updateDebugTextPosition()
    {
        int index = 0;
        foreach(var item in _debugTextList.Values)
        {
            item._textMesh.transform.localPosition = new Vector3(0f,index * padding, 0f);
            index++;
        }
    }

    public void updateDebugText(string targetName, string text)
    {
        updateDebugText(targetName,text,stayTime);
    }

    public void updateDebugText(string targetName, string text, float time)
    {
        if(_debugTextList.ContainsKey(targetName) == false)
        {
            TextMesh debugText = Instantiate(debugTextPrefab).GetComponent<TextMesh>();
            debugText.transform.SetParent(this.transform);
            debugText.transform.localPosition = new Vector3(0f,_debugTextList.Count * padding, 0f);

            DebugItem item = new DebugItem();
            item._textMesh = debugText;
            item._timer = time;
            item._itemKey = targetName;

            _debugTextList.Add(targetName,item);
        }

        _debugTextList[targetName]._textMesh.text = text;
        _debugTextList[targetName]._timer = time;
    }
}
