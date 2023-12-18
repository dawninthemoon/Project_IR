using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueCommandData 
{
    public DialogueCommandType  _type;
    public string[]             _parameters;

    public DialogueCommandData(DialogueCommandType type, int parameterCount) {
        _type = type;
        _parameters = new string[parameterCount];
    }

    public DialogueCommandData(DialogueCommandType type, string[] parameters) {
        _type = type;
        _parameters = parameters;
    }

    public DialogueCommandData Clone() 
    {
        DialogueCommandType type = _type;
        string[] parameters = _parameters.Clone() as string[];
        DialogueCommandData cloneObject = new DialogueCommandData(type, parameters);
        return cloneObject;
    }
}

public class DialogueData : ScriptableObject 
{
    public string                       _name;
    public List<DialogueCommandData>    _commandsDataList 
    {
        get;
        private set;
    }

    public void Initialize(List<DialogueCommandData> commandsDataList)
    {
        _commandsDataList = commandsDataList;
    }
}