using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DialogueConfig {
    public DialogueCommandType  _type;
    public string[]             _variables;
}

public class DialogueEditor : EditorWindow
{
    private static DialogueEditor   _window;
    private List<DialogueConfig>    _dialogueConfigList;
    private Dictionary<DialogueCommandType, IDialogueCommand> _commandInstanceDictionary;

    [MenuItem("Tools/DialogueEditor", priority = 0)]
    public static void ShowWindow()
    {
        _window = (DialogueEditor)EditorWindow.GetWindow(typeof(DialogueEditor));
        _window.Show();
    }

    private void Awake()
    {
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

        _dialogueConfigList = new List<DialogueConfig>();

        _dialogueConfigList.Add(new DialogueConfig(){
            _type = DialogueCommandType.Dialogue, _variables = null
        });
        _dialogueConfigList.Add(new DialogueConfig(){
            _type = DialogueCommandType.Dialogue, _variables = null
        });
        _dialogueConfigList.Add(new DialogueConfig(){
            _type = DialogueCommandType.None, _variables = null
        });
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
            if(GUILayout.Button("Add Point"))
            {
                //addStagePoint();
            }

            //GUI.enabled = GUI.enabled ? _pointSelectedIndex >= 0 && _pointSelectedIndex < _editingStagePointList.Count - 1 : false;
            if(GUILayout.Button("Insert Point Next"))
            {
                //insertNextStagePoint(_pointSelectedIndex);
            }
        GUILayout.EndHorizontal();

        OnScrollViewGUI();
    }

    private void OnScrollViewGUI() 
    {
        GUILayout.BeginScrollView(Vector2.zero, "box", GUILayout.Height(200f));
            for(int i = 0; i < _dialogueConfigList.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                    _dialogueConfigList[i]._type = (DialogueCommandType)EditorGUILayout.EnumPopup("CommandType", _dialogueConfigList[i]._type);
                    
                    Color currentColor = GUI.color;
                    GUI.color = new Color(1f,0.2f,0.2f);
                    if (GUILayout.Button("Delete", GUILayout.Width(100f))) {

                    }
                    GUI.color = currentColor;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (_commandInstanceDictionary.TryGetValue(_dialogueConfigList[i]._type, out IDialogueCommand current)) {
                    current.Draw();
                }

                GUILayout.EndHorizontal();
            }

        GUILayout.EndScrollView();
    }
}