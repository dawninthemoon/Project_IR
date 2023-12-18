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

    private DialogueData _selectedDialogueData;
    private List<DialogueCommandData> _commandsDataList;
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
    }

    private void OnGUI()
    {
        Color defaultColor = GUI.color;
        GUILayout.BeginHorizontal();
            _selectedDialogueData = EditorGUILayout.ObjectField("Dialogue Data", _selectedDialogueData, typeof(DialogueData), false) as DialogueData;
            
            if (_selectedDialogueData)
                _commandsDataList = _selectedDialogueData._commandsDataList;

            GUI.color = new Color(0f, 0.694f, 1f);
            if (GUILayout.Button("Create New", GUILayout.Width(100f)))
            {
                bool createNew = true;
                if (_selectedDialogueData != null)
                {
                    createNew = EditorUtility.DisplayDialog("alert", "이미 편집중인 스테이지가 존재합니다. 새로 생성 하시겠습니까?","네","아니오");
                }

                if (createNew)
                {
                    _selectedDialogueData = ScriptableObject.CreateInstance<DialogueData>();
                    _selectedDialogueData._name = "NewDialogueData";
                    _selectedDialogueData.Initialize(_commandsDataList = new List<DialogueCommandData>());
                }
            }
            GUI.color = new Color(0f, 1f, 0.408f);
            if (GUILayout.Button("Save", GUILayout.Width(100f)))
            {
                SaveCurrentData();
            }
            GUI.color = defaultColor;
        GUILayout.EndHorizontal();

        if (_selectedDialogueData != null)
        {
            GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add New Command"))
                {
                    AddNewCommand(DialogueCommandType.None);
                }
            GUILayout.EndHorizontal();

            OnScrollViewGUI();
        }
    }

    private void OnScrollViewGUI() 
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box", GUILayout.ExpandHeight(true));
        Color defaultColor = GUI.color;
            for(int i = 0; i < _commandsDataList.Count; ++i)
            {
                var type = _commandsDataList[i]._type;
                //if (_commandInstanceDictionary[type]._attr.Color != null) 
                //    GUI.contentColor = _commandInstanceDictionary[type]._attr.Color.ToColor();

                GUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    _commandsDataList[i]._type = (DialogueCommandType)EditorGUILayout.EnumPopup($"{i + 1}. CommandType", _commandsDataList[i]._type);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _commandsDataList[i] = CreateNewCommand(_commandsDataList[i]._type);
                    }

                    GUI.color = new Color(0, 0.871f, 0.616f);
                    if (GUILayout.Button("Copy", GUILayout.Width(80f))) 
                    {
                        _commandsDataList.Add(_commandsDataList[i].Clone());
                    }
                    if (GUILayout.Button("Copy To Next", GUILayout.Width(120f))) 
                    {
                        _commandsDataList.Insert(i + 1, _commandsDataList[i].Clone());
                    }
                    GUI.color = new Color(1f,0.2f,0.2f);
                    if (GUILayout.Button("Delete", GUILayout.Width(100f))) 
                    {
                        _commandsDataList.RemoveAt(i--);
                    }
                    GUI.color = defaultColor;
                GUILayout.EndHorizontal();

                if (_commandInstanceDictionary.TryGetValue(_commandsDataList[i]._type, out EditorDialogueConfig current)) 
                {
                    GUILayout.Space(10f);
                    current._instance.Draw(_commandsDataList[i]._parameters);
                    GUILayout.Space(10f);
                }

                GUI.contentColor = defaultColor;
            }

        GUILayout.EndScrollView();
    }

    private void AddNewCommand(DialogueCommandType type)
    {
        _commandsDataList.Add(CreateNewCommand(type));
    }

    private DialogueCommandData CreateNewCommand(DialogueCommandType type) 
    {
        int parameterCount = _commandInstanceDictionary[type]._attr.ParameterCount;
        DialogueCommandData newConfig = new DialogueCommandData(type, parameterCount);
        return newConfig;
    }

    private void SaveCurrentData() 
    {
        if (_selectedDialogueData == null)
            return;

        string defaultName = _selectedDialogueData._name + ".asset";
        string path = EditorUtility.SaveFilePanel(
            "Save Stage Data",
            "Assets/Resources/DialogueData/",
            defaultName,
            "asset"
        );

        if (string.IsNullOrEmpty(path)) 
            return;

        path = FileUtil.GetProjectRelativePath(path);
        AssetDatabase.CreateAsset(_selectedDialogueData, path);
    }
}