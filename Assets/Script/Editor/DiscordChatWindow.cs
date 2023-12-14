using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class DiscordChatWindow : EditorWindow
{
    private static DiscordChatWindow _window;

    private string _text = "";

    [MenuItem("Tools/DiscordChatWindow", priority = 0)]
    private static void ShowWindow()
    {
        _window = (DiscordChatWindow)EditorWindow.GetWindow(typeof(DiscordChatWindow));
    }

    public void OnGUI()
    {
        GUILayout.Label("이로하로 메시지를 보내보자");
        _text = EditorGUILayout.TextArea(_text,GUILayout.ExpandHeight(true));

        if(GUILayout.Button("Send!") && _text.Trim() != "")
            sendMessage();
    }

    private void sendMessage()
    {
        DebugUtil.sendDiscordMessage_chat(_text);
        GUI.FocusControl("");
        _text = "";
    }
}
