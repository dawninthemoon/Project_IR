using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(DialogueCharacterData)), CanEditMultipleObjects]
public class DialogueCharacterDataEditor : Editor
{
    private DialogueCharacterData _context;

    private void OnEnable()
    {
        _context = (DialogueCharacterData)target;
    }

    public override void OnInspectorGUI() 
    {
        serializedObject.Update();

        Color defaultColor = GUI.color;

        _context._defaultPath = EditorGUILayout.TextField("기본 경로", _context._defaultPath);
        for (int i = 0; i < _context.DialogueCharacterList.Count; ++i)
        {
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
                _context.DialogueCharacterList[i]._characterID
                    = EditorGUILayout.TextField("캐릭터 ID", _context.DialogueCharacterList[i]._characterID);
                    GUI.color = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(20f)))
                {
                    _context.DialogueCharacterList.RemoveAt(i);
                }
                GUI.color = defaultColor;
            GUILayout.EndHorizontal();

            _context.DialogueCharacterList[i]._characterName_Kor
                = EditorGUILayout.TextField("캐릭터 이름(한국어)", _context.DialogueCharacterList[i]._characterName_Kor);
                
            _context.DialogueCharacterList[i]._nameColor
                = EditorGUILayout.TextField("이름 색깔", _context.DialogueCharacterList[i]._nameColor);

            DrawCharacterInfo(i);
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Add New"))
        {
            _context.DialogueCharacterList.Add(new DialogueCharacterInfo());
        }

        AssetDatabase.SaveAssets();
    }

    private void DrawCharacterInfo(int index)
    {
        GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("상태 이름", GUILayout.Width(110f));
            EditorGUILayout.LabelField("경로", GUILayout.Width(200f));
        GUILayout.EndHorizontal();

        DialogueCharacterInfo info = _context.DialogueCharacterList[index];
        for (int i = 0; i < info._scgPathList.Count; ++i)
        {
            GUILayout.BeginHorizontal();
                info._scgPathList[i] = EditorGUILayout.TextField(info._scgPathList[i], GUILayout.Width(100f));
                GUILayout.Space(10f);
                info._stateNameList[i] = EditorGUILayout.TextField(info._stateNameList[i]);

                if (GUILayout.Button("-", GUILayout.Width(20f)))
                {
                    info._scgPathList.RemoveAt(i);
                    info._stateNameList.RemoveAt(i);
                }
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+", GUILayout.Width(20f)))
        {
            info._scgPathList.Add(null);
            info._stateNameList.Add(null);
        }
    }
}
