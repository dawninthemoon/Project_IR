using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Reflection;

public class DialogueExecuter
{
    private Dictionary<DialogueCommandType, IDialogueCommand> _commandInstanceDictionary;
    private SharedDialogueData _sharedData;
    private SharedVariables _sharedVariables;

    public DialogueExecuter(SharedDialogueData sharedData, SharedVariables sharedVariables) 
    {
        _sharedData = sharedData;
        _sharedVariables = sharedVariables;

        string interfaceName = "IDialogueCommand";
        var dialogueTypes = typeof(DialogueCommand).GetNestedTypes();
        _commandInstanceDictionary 
            = dialogueTypes
                .Where(type => type.GetInterface(interfaceName) != null)
                .Where(type => type.GetCustomAttribute(typeof(DialogueAttribute)) != null)
                .ToDictionary(
                    type => (type.GetCustomAttribute(typeof(DialogueAttribute)) as DialogueAttribute).CommandType,
                    type => System.Activator.CreateInstance(type) as IDialogueCommand
                );
    }

    public async UniTask ExecuteDialogue(DialogueData dialogueData)
    {
        _sharedData.UIData.Logger.Reset();
        await ExecuteCommands(dialogueData);
    }

    private async UniTask ExecuteCommands(DialogueData dialogueData)
    {
        int commandsCount = dialogueData._commandsDataList.Count;
        for (int i = 0; i < commandsCount; ++i) {
            DialogueCommandData current = dialogueData._commandsDataList[i];

            if (_commandInstanceDictionary.TryGetValue(current._type, out IDialogueCommand instance)) 
            {
                UniTask currentTask = instance.Execute(current._parameters, _sharedData, _sharedVariables);
                if (!current._executeWithNextCommand)
                    await currentTask;
            }
        }
    }
}
