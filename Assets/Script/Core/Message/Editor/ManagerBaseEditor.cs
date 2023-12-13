using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ManagerBase),true),CanEditMultipleObjects]
public class ManagerBaseEditor : MessageReceiverEditor
{
    const string receiverListField_Text = "Message Receivers : ";

    protected ManagerBase managerControl;
    public override void OnEnable()
    {
        base.OnEnable();
        managerControl = (ManagerBase)target;
    }

    public override void OnInspectorGUI()
    {
        var receivers = managerControl.Debug_GetReceivers();

        GUILayout.Label(receiverListField_Text + receivers.Count);
        
        GUILayout.BeginVertical("box");
        foreach(var receiver in receivers.Values)
        {
            if(receiver == null)
                continue;
            DrawReceivers(receiver);
        }

        foreach(var receiver in receivers.Keys)
        {
            if(receivers[receiver] == null)
                GUILayout.Label(receiver + " null object !!!!");
        }


        GUILayout.EndVertical();

        DrawUILine(Color.gray);
        
        base.OnInspectorGUI();

    }

    public void DrawReceivers(MessageReceiver receiver)
    {
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("<",GUILayout.Width(20f)))
        {
            EditorGUIUtility.PingObject(receiver);
        }

        GUILayout.Label(receiver.GetUniqueID().ToString() + " _ " + receiver.name);

        GUILayout.EndHorizontal();
    }
}
