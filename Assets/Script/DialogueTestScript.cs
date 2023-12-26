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
    [SerializeField, Header("Temp Option")] private ScrollRect _logWindow;

    [SerializeField] private DialogueData _data;
    private DialogueExecuter _executer;
    private SharedDialogueData _sharedData;
    private bool _isDialogueEnd;

    private void Awake()
    {
        SharedDialogueData.SharedUIData uiData = new SharedDialogueData.SharedUIData() 
        {
            DialogueCanvas = _dialogueCanvas,
            DialogueNameText = _dialogueNameText,
            DialogueText = _dialogueText,
            Logger = new DialogueLogger(_logWindow),
        };
        _sharedData = new SharedDialogueData(uiData);
        _sharedData.InputData.NextProgress = KeyCode.Space;
        _sharedData.InputData.FastForward = KeyCode.LeftControl;

        SharedVariables sharedVariables = new SharedVariables();
        sharedVariables.CharacterData 
            = ResourceContainerEx.Instance().GetScriptableObject("DialogueData/NewDialogueCharacterData") as DialogueCharacterData;
        _executer = new DialogueExecuter(_sharedData, sharedVariables);
    }

    private void Start()
    {
        StartDialogue().Forget();
    }

    private void Update()
    {
        Time.timeScale = Input.GetKey(_sharedData.InputData.FastForward) ? 2f : 1f;
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _sharedData.UIData.Logger.Toggle();
        }
    }

    private async UniTaskVoid StartDialogue()
    {
        _isDialogueEnd = false;

        await _executer.ExecuteDialogue(_data);

        _isDialogueEnd = true;
    }
}
