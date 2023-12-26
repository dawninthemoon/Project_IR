using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using RieslingUtils;

public class DialogueLogger
{
    private ScrollRect _logWindowScrollRect;
    private TMP_Text _logText;
    private StringBuilder _sb;
    private static readonly string DialogueSpliter = ": ";
    private static readonly string NewLine = "\n";

    public DialogueLogger(ScrollRect logWindowScrollRect)
    {
        _sb = new StringBuilder();
        _logWindowScrollRect = logWindowScrollRect;
        _logText = _logWindowScrollRect.GetComponentInChildren<TMP_Text>();
    }

    public void Reset()
    {
        _sb.Clear();
    }

    public void AddDialogue(string name, string text)
    {
        _sb.Append(name);
        _sb.Append(DialogueSpliter);
        _sb.Append(text);
        _sb.Append(NewLine);

        _logText.text = _sb.ToString();
    }

    public void Toggle()
    {
        _logWindowScrollRect.verticalNormalizedPosition = 1f;
        _logWindowScrollRect.gameObject.SetActive(!_logWindowScrollRect.gameObject.activeSelf);
    }
}
