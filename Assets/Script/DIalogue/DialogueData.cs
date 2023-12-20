using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueCommandData 
{
    public DialogueCommandType  _type;
    public string[]             _parameters;
    public bool                 _executeWithNextCommand;

    public DialogueCommandData(DialogueCommandType type, int parameterCount) {
        _type = type;
        _parameters = new string[parameterCount];
        _executeWithNextCommand = false;
    }

    public DialogueCommandData(DialogueCommandType type, string[] parameters, bool executeWithNext) {
        _type = type;
        _parameters = parameters;
        _executeWithNextCommand = executeWithNext;
    }

    public DialogueCommandData Clone() 
    {
        DialogueCommandType type = _type;
        string[] parameters = _parameters.Clone() as string[];
        DialogueCommandData cloneObject = new DialogueCommandData(type, parameters, _executeWithNextCommand);
        return cloneObject;
    }
}

[System.Serializable]
public class DialogueData : ScriptableObject 
{
    public string                       _name;
    public List<DialogueCommandData>    _commandsDataList = new List<DialogueCommandData>();
}