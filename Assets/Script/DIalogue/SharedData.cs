using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SharedVariables
{
    public Dictionary<string, string> GlobalVariables
    {
        get;
        private set;
    } = new Dictionary<string, string>();
}

public class SharedData 
{
    public class SharedUIData
    {
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

    public SharedUIData UIData 
    {
        get;
        private set;
    }

    public SharedData(SharedUIData uiData)
    {
        UIData = uiData;
    }
}