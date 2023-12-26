using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueCharacterInfo 
{
    public string       _characterID;
    public string       _characterName_Kor;
    public string       _nameColor;
    public List<string> _scgPathList;
    public List<string> _stateNameList;   

    public DialogueCharacterInfo()
    {
        _scgPathList = new List<string>();
        _stateNameList = new List<string>();
    }
}

[CreateAssetMenu(menuName = "ScriptableObjects/Dialogue/CharacterData", fileName = "NewDialogueharacterData")]
public class DialogueCharacterData : ScriptableObject
{
    public string                           _defaultPath;
    [SerializeField]
    private List<DialogueCharacterInfo>     _dialogueCharacterList;

    public List<DialogueCharacterInfo>      DialogueCharacterList
    {
        get { return _dialogueCharacterList; }
    }
}
