using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using RieslingUtils;

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

            GUI.color = new Color(0f, 0.843f, 1f);
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
            OnBoxGUI();
            OnScrollViewGUI();
        }
    }

    private void OnBoxGUI() {
        Color defaultColor = GUI.color;
        GUI.backgroundColor = new Color(0.337f, 1f, 0.761f);
        GUILayout.BeginVertical("box");
            _selectedDialogueData._name = EditorGUILayout.TextField("Name", _selectedDialogueData._name);

            if (GUILayout.Button("Add New Command"))
            {
                _selectedDialogueData._commandsDataList.Add(CreateNewCommand(DialogueCommandType.None));
                EditorUtility.SetDirty(_selectedDialogueData);
            }
        GUILayout.EndVertical();
        GUI.backgroundColor = defaultColor;
    }

    private void OnScrollViewGUI() 
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, "box", GUILayout.ExpandHeight(true));
            Color defaultColor = GUI.color;
            List<DialogueCommandData> commandsDataList = _selectedDialogueData._commandsDataList;
            for(int i = 0; i < commandsDataList.Count; ++i)
            {
                var type = commandsDataList[i]._type;
                if (_commandInstanceDictionary[type]._attr.Color != null) 
                    GUI.backgroundColor = _commandInstanceDictionary[type]._attr.Color.ToColor();
                GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    commandsDataList[i]._type = (DialogueCommandType)EditorGUILayout.EnumPopup($"{i + 1}. CommandType", commandsDataList[i]._type);
                    if (EditorGUI.EndChangeCheck())
                    {
                        commandsDataList[i] = CreateNewCommand(commandsDataList[i]._type);
                    }

                    GUI.color = new Color(0, 0.871f, 0.616f);
                    if (GUILayout.Button("Copy", GUILayout.Width(80f))) 
                    {
                        commandsDataList.Add(commandsDataList[i].Clone());
                    }
                    if (GUILayout.Button("Copy To Next", GUILayout.Width(120f))) 
                    {
                        commandsDataList.Insert(i + 1, commandsDataList[i].Clone());
                    }
                    GUI.color = new Color(1f,0.2f,0.2f);
                    if (GUILayout.Button("Delete", GUILayout.Width(100f))) 
                    {
                        commandsDataList.RemoveAt(i);
                    }
                    GUI.color = defaultColor;
                GUILayout.EndHorizontal();

                if ((i < commandsDataList.Count) && _commandInstanceDictionary.TryGetValue(commandsDataList[i]._type, out EditorDialogueConfig current)) 
                {
                    GUILayout.Space(10f);
                    current._instance.Draw(commandsDataList[i]._parameters);
                    GUILayout.BeginHorizontal();
                        GUI.contentColor = Color.red;
                        EditorGUILayout.LabelField("다음 커맨드와 동시에 실행", GUILayout.Width(150f));
                        commandsDataList[i]._executeWithNextCommand = EditorGUILayout.Toggle(commandsDataList[i]._executeWithNextCommand);
                        GUI.contentColor = defaultColor;
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10f);
                }

                GUILayout.EndVertical();
                GUI.backgroundColor = defaultColor;
            }
        GUILayout.EndScrollView();
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

        EditorUtility.SetDirty(_selectedDialogueData);

        if (!AssetDatabase.Contains(_selectedDialogueData))
        {
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
        AssetDatabase.SaveAssets();
    }
}