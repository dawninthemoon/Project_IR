using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RieslingUtils;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IDialogueCommand {
    UniTask Execute(string[] parameters, SharedData sharedData);
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
    public enum SCGDirection
    {
        Left,
        Right
    }

    [DialogueAttribute(DialogueCommandType.None)]
    public class Command_None : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedData sharedData) 
        {
            await UniTask.Yield();
        }

        public void Draw(string[] parameters) 
        {

        }
    }

    [DialogueAttribute(DialogueCommandType.Dialogue, Color = "green", ParameterCount = 4)]
    public class Command_Dialogue : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedData sharedData) 
        {
            sharedData.UIData.DialogueNameText.text = parameters[0];
            
            var dialogueText = sharedData.UIData.DialogueText;
            dialogueText.text = parameters[1];

            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
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

    [DialogueAttribute(DialogueCommandType.ShowSCG, ParameterCount = 10)]
    public class Command_ShowSCG : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedData sharedData) 
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

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("종류", GUILayout.Width(40f));
                parameters[3] = EditorGUILayout.TextField(parameters[3]);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SCG 방향", GUILayout.Width(100f));
                SCGDirection selectedDir = ExParser.ParseEnumOrDefault<SCGDirection>(parameters[4]);
                parameters[4] = EditorGUILayout.EnumPopup(selectedDir).ToString();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("위치", GUILayout.Width(40f));
                parameters[5] = EditorGUILayout.IntSlider(ExParser.ParseIntOrDefault(parameters[5]), 0, 3).ToString();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("페이드 인", GUILayout.Width(100f));
                parameters[6] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[6])).ToString();
            GUILayout.EndHorizontal();
            if (ExParser.ParseBoolOrDefault(parameters[6]))
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("시간", GUILayout.Width(40f));
                    parameters[7] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[7])).ToString();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("움직이며 등장", GUILayout.Width(100f));
                parameters[8] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[8])).ToString();
            GUILayout.EndHorizontal();
            if (ExParser.ParseBoolOrDefault(parameters[8]))
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("시간", GUILayout.Width(40f));
                    parameters[9] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[9])).ToString();
                GUILayout.EndHorizontal();
            } 

            GUI.color = currentColor;
        }
    }
}