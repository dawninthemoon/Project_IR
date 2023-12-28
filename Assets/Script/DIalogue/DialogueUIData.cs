using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUIData : MonoBehaviour
{
    [SerializeField] private Canvas     _dialogueCanvas;
    [SerializeField] private TMP_Text   _speakerName;
    [SerializeField] private TMP_Text   _dialogueText;
    [SerializeField] private ScrollRect _logWindow;

    public Canvas DialogueCanvas
    {
        get { return _dialogueCanvas; }
    }
    public TMP_Text SpeakerName
    {
        get { return _speakerName; }
    }
    public TMP_Text DialogueText
    {
        get { return _dialogueText; }
    }
    public DialogueLogger Logger
    {
        get;
        set;
    }

    private void Awake()
    {
        Logger = new DialogueLogger(_logWindow);
    }

    public void OnDialogueStart()
    {
        SpeakerName.transform.parent.gameObject.SetActive(true);
        DialogueText.transform.parent.gameObject.SetActive(true);
        Logger.SetActive(false);
    }

    public void OnDialogueEnd()
    {
        SpeakerName.transform.parent.gameObject.SetActive(false);
        DialogueText.transform.parent.gameObject.SetActive(false);
        Logger.SetActive(true);
    }
}
