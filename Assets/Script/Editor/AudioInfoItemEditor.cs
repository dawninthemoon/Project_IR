using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioInfoItem))]
public class SoundInfoItemEditor : Editor
{
    AudioInfoItem controll;

	void OnEnable()
    {
        controll = (AudioInfoItem)target;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("CreateInfoFromCSV"))
        {
            controll.CreateInfoFromCSV();
            EditorUtility.SetDirty(controll);
        }

		base.OnInspectorGUI();

        // if(controll.soundData == null)
        //     return;
        
        // foreach(var data in controll.soundData)
        // {
        //     GUILayout.Label("key : " + data.Key + ", path : " + data.Value.path);
        //     foreach(var param in data.Value.parameters)
        //     {
        //         GUILayout.BeginHorizontal();
        //         GUILayout.Space(10f);
        //         GUILayout.Label("key : " + param.Key + ", name : " + param.Value.name);
        //         GUILayout.EndHorizontal();
        //     }
        // }
    }
}
