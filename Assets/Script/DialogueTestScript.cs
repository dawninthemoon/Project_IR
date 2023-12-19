using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DialogueTestScript : MonoBehaviour
{
    [SerializeField] private DialogueData _data;
    private DialogueExecuter _executer;
    private bool _isDialogueEnd;

    private void Awake()
    {
        _executer = new DialogueExecuter();
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
