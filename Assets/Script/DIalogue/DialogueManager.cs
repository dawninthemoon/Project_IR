using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DialogueManager : Singleton<DialogueManager>
{
    private DialogueExecuter _executer;
    private SharedDialogueData _sharedData;
    public bool IsDialogueEnd
    {
        get;
        private set;
    }

    public DialogueManager()
    {
        InitializeDialogueManager();
    }

    private void InitializeDialogueManager()
    {
        DialogueUIData uiData = GameObject.FindObjectOfType<DialogueUIData>();
        if (uiData == null)
            return;

        _sharedData = new SharedDialogueData(uiData);
        _sharedData.InputData.NextProgress = KeyCode.Space;
        _sharedData.InputData.FastForward = KeyCode.LeftControl;

        SharedVariables sharedVariables = new SharedVariables();
        sharedVariables.CharacterData 
            = ResourceContainerEx.Instance().GetScriptableObject("DialogueData/CharacterData/DialogueCharacterData") as DialogueCharacterData;
        _executer = new DialogueExecuter(_sharedData, sharedVariables);
    }

    public void StartDialogue(string dialogueKey)
    {
        DialogueData dialogueData = ResourceContainerEx.Instance().GetScriptableObject("DialogueData/" + dialogueKey) as DialogueData;
        StartDialogue(dialogueData).Forget();
    }

    private async UniTaskVoid StartDialogue(DialogueData dialogueData)
    {
        IsDialogueEnd = false;
        _sharedData.UIData.OnDialogueStart();

        await _executer.ExecuteDialogue(dialogueData);

        IsDialogueEnd = true;
        _sharedData.UIData.OnDialogueEnd();
    }
}
