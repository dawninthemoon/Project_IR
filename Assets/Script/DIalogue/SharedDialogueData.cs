using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SharedVariables
{
    public Dictionary<string, string> GlobalVariables
    {
        get;
        private set;
    } = new Dictionary<string, string>();

    public DialogueCharacterData CharacterData
    {
        get;
        set;
    }
}

public class SharedDialogueData 
{
    public class SharedInputData
    {
        public KeyCode NextProgress { get; set; }
        public KeyCode FastForward { get; set; }

        public bool CanGoToNext
        {
            get 
            { 
                return IsNextKeyDown || Input.GetKey(FastForward);
            }
        }

        public bool IsNextKeyDown
        {
            get 
            { 
                return Input.GetKeyDown(NextProgress);
            }
        }
    }

    
    public Dictionary<string, Image> ActiveSCGDictionary
    {
        get;
        private set;
    } = new Dictionary<string, Image>();

    public DialogueUIData UIData 
    {
        get;
        private set;
    }
    public SharedInputData InputData
    {
        get;
        set;
    }

    public SharedDialogueData(DialogueUIData uiData)
    {
        UIData = uiData;
        InputData = new SharedInputData();
    }
}