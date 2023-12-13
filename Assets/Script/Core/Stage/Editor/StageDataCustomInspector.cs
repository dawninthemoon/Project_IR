using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(StageData))]
public class StageDataInspectorEditor : Editor
{
    StageData controll;

	void OnEnable()
    {
        controll = (StageData)target;
    }

    public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();

        if(GUILayout.Button("Open With Editor"))
        {
            StageDataEditor window = (StageDataEditor)EditorWindow.GetWindow(typeof(StageDataEditor));
            window.setEditStageData(controll);
            window.Show();
        }
    }
}