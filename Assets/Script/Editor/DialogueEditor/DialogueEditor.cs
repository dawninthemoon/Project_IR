using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using RieslingUtils;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class DialogueEditor : EditorWindow
{
    private class DialogueCommandConfig 
    {
        public DialogueCommandType  _type;
        public string[]             _parameters;

        public DialogueCommandConfig(DialogueCommandType type, int parameterCount) {
            _type = type;
            _parameters = new string[parameterCount];
        }

        public DialogueCommandConfig(DialogueCommandType type, string[] parameters) {
            _type = type;
            _parameters = parameters;
        }

        public DialogueCommandConfig Clone() 
        {
            DialogueCommandType type = _type;
            string[] parameters = _parameters.Clone() as string[];
            DialogueCommandConfig cloneObject = new DialogueCommandConfig(type, parameters);
            return cloneObject;
        }
    }

    private class EditorDialogueConfig 
    {
        public IDialogueCommand     _instance;
        public DialogueAttribute    _attr;

        public EditorDialogueConfig(IDialogueCommand instnace, DialogueAttribute attr) {
            _instance = instnace;
            _attr = attr;
        }
    }

    private static DialogueEditor _window;
    private List<DialogueCommandConfig> _dialogueConfigList;
    private Dictionary<DialogueCommandType, EditorDialogueConfig> _commandInstanceDictionary;

    private Vector2 _scrollPosition;

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
                    type => {
                        var instance = System.Activator.CreateInstance(type) as IDialogueCommand;
                        var attr = type.GetCustomAttribute(typeof(DialogueAttribute)) as DialogueAttribute;
                        return new EditorDialogueConfig(instance, attr);
                    }
                );

        _dialogueConfigList = new List<DialogueCommandConfig>();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
            if(GUILayout.Button("Add Point"))
            {
                AddNewCommand(DialogueCommandType.None);
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
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box", GUILayout.ExpandHeight(true));
        Color defaultColor = GUI.color;
            for(int i = 0; i < _dialogueConfigList.Count; ++i)
            {
                var type = _dialogueConfigList[i]._type;
                //if (_commandInstanceDictionary[type]._attr.Color != null) 
                //    GUI.contentColor = _commandInstanceDictionary[type]._attr.Color.ToColor();

                GUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    _dialogueConfigList[i]._type = (DialogueCommandType)EditorGUILayout.EnumPopup($"{i + 1}. CommandType", _dialogueConfigList[i]._type);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _dialogueConfigList[i] = CreateNewCommand(_dialogueConfigList[i]._type);
                    }

                    GUI.color = new Color(0, 0.871f, 0.616f);
                    if (GUILayout.Button("Copy", GUILayout.Width(80f))) {
                        _dialogueConfigList.Add(_dialogueConfigList[i].Clone());
                    }
                    if (GUILayout.Button("Copy To Next", GUILayout.Width(120f))) {
                        _dialogueConfigList.Insert(i + 1, _dialogueConfigList[i].Clone());
                    }
                    GUI.color = new Color(1f,0.2f,0.2f);
                    if (GUILayout.Button("Delete", GUILayout.Width(100f))) {
                        _dialogueConfigList.RemoveAt(i--);
                    }
                    GUI.color = defaultColor;
                GUILayout.EndHorizontal();

                if (_commandInstanceDictionary.TryGetValue(_dialogueConfigList[i]._type, out EditorDialogueConfig current)) {
                    GUILayout.Space(10f);
                    current._instance.Draw(_dialogueConfigList[i]._parameters);
                    GUILayout.Space(10f);
                }

                GUI.contentColor = defaultColor;
            }

        GUILayout.EndScrollView();
    }

    private void AddNewCommand(DialogueCommandType type)
    {
        _dialogueConfigList.Add(CreateNewCommand(type));
    }

    private DialogueCommandConfig CreateNewCommand(DialogueCommandType type) 
    {
        int parameterCount = _commandInstanceDictionary[type]._attr.ParameterCount;
        DialogueCommandConfig newConfig = new DialogueCommandConfig(type, parameterCount);
        return newConfig;
    }
}