using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RieslingUtils;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IDialogueCommand {
    UniTask Execute(string[] parameters);
    void Draw(string[] parameters);
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class DialogueAttribute : System.Attribute 
{
    public DialogueCommandType  CommandType { get; }
    public string               Color { get; set; }
    public int                  ParameterCount { get; set; }
    
    public DialogueAttribute(DialogueCommandType type) {
        this.CommandType = type;
    }
}

public enum DialogueCommandType {
    None,
    Dialogue,
    ShowSCG
}

public class DialogueCommand {

    [DialogueAttribute(DialogueCommandType.None)]
    public class Command_None : IDialogueCommand {
        public async UniTask Execute(string[] parameters) 
        {
            await UniTask.Yield();
        }

        public void Draw(string[] parameters) 
        {

        }
    }

    [DialogueAttribute(DialogueCommandType.Dialogue, Color = "green", ParameterCount = 4)]
    public class Command_Dialogue : IDialogueCommand {
        public async UniTask Execute(string[] parameters) 
        {
            await UniTask.Yield();
        }

        public void Draw(string[] parameters) 
        {
            Color currentColor = GUI.color;
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("이름", GUILayout.Width(40f));
                parameters[0] = EditorGUILayout.TextField(parameters[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("대사", GUILayout.Width(40f));
                parameters[1] = EditorGUILayout.TextArea(parameters[1], GUILayout.MinHeight(40f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SCG 강조", GUILayout.Width(100f));
                parameters[2] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[2])).ToString();
            GUILayout.EndHorizontal();

            if (ExParser.ParseBoolOrDefault(parameters[2])) 
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("강조할 SCG(공란일 시 '이름' 항목 따라감)", GUILayout.Width(250f));
                    parameters[3] = EditorGUILayout.TextField(parameters[3]);
                GUILayout.EndHorizontal();
            }
            GUI.color = currentColor;
        }
    }

    [DialogueAttribute(DialogueCommandType.ShowSCG, ParameterCount = 4)]
    public class Command_ShowSCG : IDialogueCommand {
        public async UniTask Execute(string[] parameters) 
        {
            await UniTask.Yield();
        }

        public void Draw(string[] parameters) 
        {
            Color currentColor = GUI.color;

             GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("경로 직접 입력", GUILayout.Width(100f));
                parameters[0] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[0])).ToString();
            GUILayout.EndHorizontal();

            if (ExParser.ParseBoolOrDefault(parameters[0])) 
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("경로", GUILayout.Width(40f));
                    parameters[1] = EditorGUILayout.TextField(parameters[1]);
                GUILayout.EndHorizontal();
            }
            else 
            {
                GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("캐릭터 이름", GUILayout.Width(100f));
                    parameters[2] = EditorGUILayout.TextField(parameters[2]);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("종류", GUILayout.Width(40f));
                parameters[3] = EditorGUILayout.TextField(parameters[3]);
            GUILayout.EndHorizontal();

            GUI.color = currentColor;
        }
    }
}