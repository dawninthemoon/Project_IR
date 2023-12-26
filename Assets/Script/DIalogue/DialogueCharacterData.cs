using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class DialogueCharacterInfo 
{
    public string       _characterID;
    public string       _characterName_Kor;
    public string       _nameColor;
    public List<string> _stateNameList;
    public List<string> _scgPathList;
    public Dictionary<string, string> SCGPathByStateName
    {
        get;
        private set;
    }

    public DialogueCharacterInfo()
    {
        _stateNameList = new List<string>();
        _scgPathList = new List<string>();
    }

    public void MakeDictionary()
    {
        SCGPathByStateName = _stateNameList.Zip(_scgPathList, (k, v) => new {k, v}).ToDictionary(x => x.v, x => x.v);
    }

    public string FindSCGPath(string stateName)
    {
        string result = null;
        for (int i = 0; i < _stateNameList.Count; ++i)
        {
            if (_stateNameList[i].Equals(stateName))
            {
                result = _scgPathList[i];
            }
        }
        return result;
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

    public DialogueCharacterInfo Find(string id)
    {
        DialogueCharacterInfo result = null;
        for (int i = 0; i < _dialogueCharacterList.Count; ++i)
        {
            if (_dialogueCharacterList[i]._characterID.Equals(id))
            {
                result = _dialogueCharacterList[i];
                break;
            }
        }
        return result;
    }

    public string FindSCGPath(string characterID, string stateName)
    {
        DialogueCharacterInfo characterInfo = Find(characterID);
        return _defaultPath + characterInfo.FindSCGPath(stateName);
    }
}
