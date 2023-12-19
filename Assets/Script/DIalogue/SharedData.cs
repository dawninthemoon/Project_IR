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
}

public class SharedDialogueData 
{
    public class SharedUIData
    {
        public Canvas DialogueCanvas
        {
            get;
            set;
        }
        public TMP_Text DialogueNameText
        {
            get;
            set;
        }
        public TMP_Text DialogueText
        {
            get;
            set;
        }
    }

    public Dictionary<string, Image> ActiveSCGDictionary
    {
        get;
        private set;
    } = new Dictionary<string, Image>();

    public SharedUIData UIData 
    {
        get;
        private set;
    }

    public SharedDialogueData(SharedUIData uiData)
    {
        UIData = uiData;
    }
}