using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RieslingUtils;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Unity.VisualScripting;


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
    ShowSCG,
    RemoveSCG
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
            await UniTask.Yield(PlayerLoopTiming.Update);
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
            
            if (!bool.Parse(parameters[2]))
            {
                string id = string.IsNullOrEmpty(parameters[3]) ? parameters[0] : parameters[3];
                if (sharedData.ActiveSCGDictionary.TryGetValue(id, out Image scgImage))
                {
                    EmphasisSCG(id, sharedData);
                }
            }

            await DoText(parameters[1], 0.05f, sharedData);

            await UniTask.Yield(PlayerLoopTiming.Update);
            await UniTask.WaitUntil(() => sharedData.InputData.CanGoToNext);
        }

        private async UniTask DoText(string text, float delay, SharedDialogueData sharedData)
        {
            int textLength = text.Length;

            sharedData.UIData.DialogueText.text = null;
            for (int currentIndex = 0; currentIndex < textLength; ++currentIndex)
            {
                sharedData.UIData.DialogueText.text += text[currentIndex];

                await UniTask.Yield(PlayerLoopTiming.Update);

                if (sharedData.InputData.IsNextKeyDown)
                    break;
            }

            sharedData.UIData.DialogueText.text = text;
        }

        private void EmphasisSCG(string targetID, SharedDialogueData sharedData)
        {
            foreach (var pair in sharedData.ActiveSCGDictionary)
            {
                Color targetColor = pair.Key.Equals(targetID) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
                pair.Value.color = targetColor;
                if (pair.Key.Equals(targetID))
                {
                    pair.Value.transform.SetSiblingIndex(sharedData.ActiveSCGDictionary.Count - 1);
                }
            }
        }

        public void Draw(string[] parameters) 
        {
            Color currentColor = GUI.color;
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("이름", GUILayout.Width(100f));
                parameters[0] = EditorGUILayout.TextField(parameters[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("대사", GUILayout.Width(100f));
                parameters[1] = EditorGUILayout.TextArea(parameters[1], GUILayout.MinHeight(40f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SCG 강조 안함", GUILayout.Width(100f));
                parameters[2] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[2])).ToString();
            GUILayout.EndHorizontal();

            if (!ExParser.ParseBoolOrDefault(parameters[2])) 
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("강조할 SCG ID(공란일 시 '이름' 항목 따라감)", GUILayout.Width(250f));
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
            float timeScale = Time.timeScale;
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
                    waitTime = Mathf.Max(waitTime, duration);
                    DoFade(newImage, duration, timeScale).Forget();
                }

                if (bool.Parse(parameters[9]))
                {
                    float duration = float.Parse(parameters[10]);
                    waitTime = Mathf.Max(waitTime, duration);
                    DoMove(newImage.rectTransform, pivot, positionIndex, duration, timeScale).Forget();
                }

                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime / timeScale));
            }
        }

        private async UniTaskVoid DoFade(Image image, float duration, float timeScale)
        {
            Color transparentColor = image.color;
            transparentColor.a = 0f;
            Color defaultColor = image.color;

            float timeAgo = 0f;
            while (timeAgo < duration)
            {
                image.color = Color.Lerp(transparentColor, defaultColor, timeAgo / duration);

                await UniTask.Yield(PlayerLoopTiming.Update);

                timeAgo += Time.deltaTime * timeScale;
            }
        }

        private async UniTaskVoid DoMove(RectTransform t, SCGPivot pivot, int positionIndex, float duration, float timeScale)
        {
            Vector3 defaultPosition = GetPositionByIndex(pivot, -1);
            Vector3 targetPosition = GetPositionByIndex(pivot, positionIndex);

            float timeAgo = 0f;
            while (timeAgo < duration)
            {
                t.localPosition = Vector3.Lerp(defaultPosition, targetPosition, timeAgo / duration);

                await UniTask.Yield(PlayerLoopTiming.Update);

                timeAgo += Time.deltaTime * timeScale;
            }
        }

        private Vector3 GetPositionByIndex(SCGPivot pivot, int positionIndex)
        {
            float sign = (pivot == SCGPivot.Left) ? 1f : -1f;
            float startPos = -960f * sign;
            float xPos =  startPos + (1920f / 4f * (positionIndex + 1)) * sign;
            return new Vector3(xPos, 0f, 0f);
        }

        public void Draw(string[] parameters) 
        {
            Color currentColor = GUI.color;

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ID", GUILayout.Width(100f));
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
                    EditorGUILayout.LabelField("경로", GUILayout.Width(100f));
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
                EditorGUILayout.LabelField("종류", GUILayout.Width(100f));
                parameters[4] = EditorGUILayout.TextField(parameters[4]);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SCG 기준 위치", GUILayout.Width(100f));
                SCGPivot selectedPivot = ExParser.ParseEnumOrDefault<SCGPivot>(parameters[5]);
                parameters[5] = EditorGUILayout.EnumPopup(selectedPivot).ToString();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("위치", GUILayout.Width(100f));
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
                    EditorGUILayout.LabelField("시간", GUILayout.Width(100f));
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
                    EditorGUILayout.LabelField("시간", GUILayout.Width(100f));
                    parameters[10] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[10])).ToString();
                GUILayout.EndHorizontal();
            }

            GUI.color = currentColor;
        }
    }

    [DialogueAttribute(DialogueCommandType.RemoveSCG, ParameterCount = 6)]
    public class Command_RemoveSCG : IDialogueCommand {
        public async UniTask Execute(string[] parameters, SharedDialogueData sharedData) 
        {
            float timeScale = Time.timeScale;
            string id = parameters[0];
            if (sharedData.ActiveSCGDictionary.TryGetValue(id, out Image scgImage))
            {
                float waitTime = 0f;

                if (bool.Parse(parameters[1]))
                {
                    float duration = float.Parse(parameters[2]);
                    waitTime = Mathf.Max(waitTime, duration);
                    DoFadeOut(scgImage, duration, timeScale).Forget();
                }
                if (bool.Parse(parameters[3]))
                {
                    SCGPivot pivot = ExEnum.Parse<SCGPivot>(parameters[4]);
                    float duration = float.Parse(parameters[5]);
                    waitTime = Mathf.Max(waitTime, duration);
                    DoMove(scgImage.rectTransform, pivot, duration, timeScale).Forget();
                }

                await UniTask.Delay(System.TimeSpan.FromSeconds(waitTime / timeScale));

                scgImage.color = new Color(1f, 1f, 1f, 0f);
                sharedData.ActiveSCGDictionary.Remove(id);
                // 나중에 수정
                GameObject.Destroy(scgImage.gameObject);
            }
        }

        private async UniTaskVoid DoFadeOut(Image image, float duration, float timeScale)
        {
            Color defaultColor = image.color;
            Color transparentColor = image.color;
            transparentColor.a = 0f;

            float timeAgo = 0f;
            while (timeAgo < duration)
            {
                image.color = Color.Lerp(defaultColor, transparentColor, timeAgo / duration);

                await UniTask.Yield(PlayerLoopTiming.Update);

                timeAgo += Time.deltaTime * timeScale;
            }
        }


        private async UniTaskVoid DoMove(RectTransform t, SCGPivot pivot, float duration, float timeScale)
        {
            Vector3 defaultPosition = t.localPosition;
            Vector3 targetPosition = GetPositionByIndex(pivot, -1);

            float timeAgo = 0f;
            while (timeAgo < duration)
            {
                t.localPosition = Vector3.Lerp(defaultPosition, targetPosition, timeAgo / duration);

                await UniTask.Yield(PlayerLoopTiming.Update);

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
            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ID", GUILayout.Width(100f));
                parameters[0] = EditorGUILayout.TextField(parameters[0]);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("페이드 아웃", GUILayout.Width(100f));
                parameters[1] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[1])).ToString();
            GUILayout.EndHorizontal();

            if (ExParser.ParseBoolOrDefault(parameters[1]))
            {
                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("시간", GUILayout.Width(100f));
                    parameters[2] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[2])).ToString();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("움직이며 퇴장", GUILayout.Width(100f));
                parameters[3] = EditorGUILayout.Toggle(ExParser.ParseBoolOrDefault(parameters[3])).ToString();
            GUILayout.EndHorizontal();

            if (ExParser.ParseBoolOrDefault(parameters[3]))
            {
                GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("퇴장 방향", GUILayout.Width(100f));
                    SCGPivot selectedPivot = ExParser.ParseEnumOrDefault<SCGPivot>(parameters[4]);
                    parameters[4] = EditorGUILayout.EnumPopup(selectedPivot).ToString();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    EditorGUILayout.LabelField("시간", GUILayout.Width(100f));
                    parameters[5] = EditorGUILayout.FloatField(ExParser.ParseFloatOrDefault(parameters[5])).ToString();
                GUILayout.EndHorizontal();
            }
        }
    }
}