using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

public class DialogueTestScript : MonoBehaviour
{
    [SerializeField, Header("Temp Option")] private Canvas _dialogueCanvas;
    [SerializeField, Header("Temp Option")] private TMP_Text _dialogueNameText;
    [SerializeField, Header("Temp Option")] private TMP_Text _dialogueText;

    [SerializeField] private DialogueData _data;
    private DialogueExecuter _executer;
    private bool _isDialogueEnd;

    private void Awake()
    {
        SharedDialogueData.SharedUIData uiData = new SharedDialogueData.SharedUIData() 
        {
            DialogueCanvas = _dialogueCanvas,
            DialogueNameText = _dialogueNameText,
            DialogueText = _dialogueText,
        };
        SharedDialogueData sharedData = new SharedDialogueData(uiData);
        SharedVariables sharedVariables = new SharedVariables();
        _executer = new DialogueExecuter(sharedData);
    }

    private void Start()
    {
        StartDialogue().Forget();
    }

    private async UniTaskVoid StartDialogue()
    {
        _isDialogueEnd = false;

        await _executer.ExecuteDialogue(_data);

        _isDialogueEnd = true;
    }
}
