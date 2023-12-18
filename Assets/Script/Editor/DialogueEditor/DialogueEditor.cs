using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DialogueEditor : EditorWindow
{
    private static DialogueEditor   _window;
    private ReorderableList         _list;

    [MenuItem("Tools/DialogueEditor", priority = 0)]
    public static void ShowWindow()
    {
        _window = (DialogueEditor)EditorWindow.GetWindow(typeof(DialogueEditor));
    }

    private void Awake()
    {

    }

    private void OnGUI()
    {
        
    }
}
