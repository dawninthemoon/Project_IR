using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RieslingUtils;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public interface IDialogueCommand {
    UniTask Execute(string[] parameters, SharedDialogueData sharedData);
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
    public enum SCGPivot
    {
        Left,
        Right
    }

    [DialogueAttribute(DialogueCommandType.None)]
    public class Command_None : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedDialogueData sharedData) 
        {
            await UniTask.Yield();
        }

        public void Draw(string[] parameters) 
        {

        }
    }

    [DialogueAttribute(DialogueCommandType.Dialogue, Color = "green", ParameterCount = 4)]
    public class Command_Dialogue : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedDialogueData sharedData) 
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

    [DialogueAttribute(DialogueCommandType.ShowSCG, ParameterCount = 11)]
    public class Command_ShowSCG : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedDialogueData sharedData) 
        {
            string id = parameters[0];
            var scgDictionary = sharedData.ActiveSCGDictionary;

            if (scgDictionary.TryGetValue(id, out Image scgImage))
            {
                // 처리...
            }

            SCGPivot pivot = ExEnum.Parse<SCGPivot>(parameters[5]);
            float scaleX = (pivot == SCGPivot.Left) ? -1f : 1f;

            if (bool.Parse(parameters[1]))
            {
                Image newImage = new GameObject(id).AddComponent<Image>();
                newImage.transform.SetParent(sharedData.UIData.DialogueCanvas.transform);
                newImage.transform.SetSiblingIndex(scgDictionary.Count);
                newImage.rectTransform.sizeDelta = new Vector2(1095f, 1742f);
                newImage.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);

                int positionIndex = ExParser.ParseIntOrDefault(parameters[6]);
                Vector3 position = GetPositionByIndex(pivot, positionIndex);
                newImage.rectTransform.localPosition = position;
                
                Sprite sprite = ResourceContainerEx.Instance().GetSprite(parameters[2]);
                newImage.sprite = sprite;

                scgDictionary.Add(id, newImage);

                float waitTime = 0f;
                if (bool.Parse(parameters[7]))
                {
                    float duration = float.Parse(parameters[8]);
                    waitTime += duration;
                    DoFade(newImage, duration).Forget();
                }

                if (bool.Parse(parameters[9]))
                {
                    float duration = float.Parse(parameters[10]);
                    waitTime += duration;
                    DoMove(newImage.rectTransform, pivot, positionIndex, duration).Forget();
                }

                // waitTime /= timeScale;
                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime));
            }
        }

        private async UniTaskVoid DoFade(Image image, float duration)
        {
            Color transparentColor = image.color;
            transparentColor.a = 0f;
            Color defaultColor = image.color;

            float timeAgo = 0f;
            float timeScale = 1f; // 나중에 GlobalTimer에서 받아옴
            while (timeAgo < duration)
            {
                image.color = Color.Lerp(transparentColor, defaultColor, timeAgo / duration);

                await UniTask.Yield();

                timeAgo += Time.deltaTime * timeScale;
            }
        }

        private async UniTaskVoid DoMove(RectTransform t, SCGPivot pivot, int positionIndex, float duration)
        {
            Vector3 defaultPosition = GetPositionByIndex(pivot, -1);
            Vector3 targetPosition = GetPositionByIndex(pivot, positionIndex);

            float timeAgo = 0f;
            float timeScale = 1f; // 나중에 GlobalTimer에서 받아옴
            while (timeAgo < duration)
            {
                t.localPosition = Vector3.Lerp(defaultPosition, targetPosition, timeAgo / duration);

                await UniTask.Yield();

                timeAgo += Time.deltaTime * timeScale;
            }
        }

        private Vector3 GetPositionByIndex(SCGPivot pivot, int positionIndex)
        {
            float startPos = (pivot == SCGPivot.Left) ? -960f : 960f;
            float xPos =  startPos + (1920f / 4f * (positionIndex + 1));
            return new Vector3(xPos, 0f, 0f);
        }

        public void Draw(string[] parameters) 
        {
            Color currentColor = GUI.color;

            GUILayout.BeginHorizontal();
                GUILayout.Space(20f);
                EditorGUILayout.LabelField("ID", GUILayout.Width(40f));
                parameters[0] = EditorGUILayout.TextField(parameters[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("경로 직접 입력", GUILayout.Width(100f));
                parameters[1] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[1])).ToString();
            GUILayout.EndHorizontal();

            if (ExParser.ParseBoolOrDefault(parameters[1])) 
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("경로", GUILayout.Width(40f));
                    parameters[2] = EditorGUILayout.TextField(parameters[2]);
                GUILayout.EndHorizontal();
            }
            else 
            {
                GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("캐릭터 이름", GUILayout.Width(100f));
                    parameters[3] = EditorGUILayout.TextField(parameters[3]);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("종류", GUILayout.Width(40f));
                parameters[4] = EditorGUILayout.TextField(parameters[4]);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SCG 기준 위치", GUILayout.Width(100f));
                SCGPivot selectedPivot = ExParser.ParseEnumOrDefault<SCGPivot>(parameters[5]);
                parameters[5] = EditorGUILayout.EnumPopup(selectedPivot).ToString();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("위치", GUILayout.Width(40f));
                parameters[6] = EditorGUILayout.IntSlider(ExParser.ParseIntOrDefault(parameters[6]), 0, 3).ToString();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("페이드 인", GUILayout.Width(100f));
                parameters[7] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[7])).ToString();
            GUILayout.EndHorizontal();

            if (ExParser.ParseBoolOrDefault(parameters[7]))
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("시간", GUILayout.Width(40f));
                    parameters[8] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[8])).ToString();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("움직이며 등장", GUILayout.Width(100f));
                parameters[9] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[9])).ToString();
            GUILayout.EndHorizontal();
            if (ExParser.ParseBoolOrDefault(parameters[9]))
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("시간", GUILayout.Width(40f));
                    parameters[10] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[10])).ToString();
                GUILayout.EndHorizontal();
            } 

            GUI.color = currentColor;
        }
    }
}