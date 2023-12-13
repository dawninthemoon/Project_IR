using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MasterManager),true),CanEditMultipleObjects]
public class MasterManagerEditor : MessageReceiverEditor
{
    const string messageCountField_Text = "Message Count : ";


    public override void OnInspectorGUI()
    {
        GUILayout.Label(messageCountField_Text + MessagePool.GetMessageCountAll().ToString());
        EditorGUILayout.Space();
        
        base.OnInspectorGUI();

    }
}
