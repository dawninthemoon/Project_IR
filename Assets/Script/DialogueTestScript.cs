using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTestScript : MonoBehaviour
{
    private void Start()
    {
        DialogueManager.Instance().StartDialogue("DialogueTest");
    }
}
