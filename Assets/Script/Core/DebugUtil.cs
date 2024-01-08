#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.Diagnostics;
#endif

public static class DebugUtil
{
    public static bool _ignoreAssert = false;

    public static void log(string errorLog, params System.Object[] errorArgs)
    {
        string resultString = string.Format("{0}",string.Format(errorLog,errorArgs));
        UnityEngine.Debug.Log(resultString);
    }

    public static bool assert_fileOpen(bool condition, string errorLog, string filePath, int lineNumber, params System.Object[] errorArgs)
    {
#if UNITY_EDITOR
        if(condition)
            return true;

        if(_ignoreAssert == false)
        {
            StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
            string resultString = string.Format("{0}\n\n{1} ({2})",string.Format(errorLog,errorArgs),stackFrame.GetFileName(), stackFrame.GetFileLineNumber());
            
            CustomDialog_OpenFile.ShowCustomDialog("Assert",resultString, "OK",filePath, lineNumber);
           // bool result = EditorUtility.DisplayDialog("Assert",resultString,"Throw Exception","Ignore");
    
            //if(result == true)
            {
                UnityEngine.Debug.Break();
                throw new System.Exception(resultString);
            }
        }
        
#else
        return true;

#endif       
        return false;     
    }

    public static bool assert(bool condition, string errorLog, params System.Object[] errorArgs)
    {
#if UNITY_EDITOR
        if(condition)
            return true;

        if(_ignoreAssert == false)
        {
            StackFrame stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);
            string resultString = string.Format("{0}\n\n{1} ({2})",string.Format(errorLog,errorArgs),stackFrame.GetFileName(), stackFrame.GetFileLineNumber());
            
            CustomDialog.ShowCustomDialog("Assert",resultString, "OK");
           // bool result = EditorUtility.DisplayDialog("Assert",resultString,"Throw Exception","Ignore");
    
            //if(result == true)
            {
                UnityEngine.Debug.Break();
                throw new System.Exception(resultString);
            }
        }
        
#else
        return true;

#endif       
        return false;     
    }

#if UNITY_EDITOR
    public static void sendDiscordMessage_chat(string message)
    {
        GameObject coroutineObject = new GameObject("DiscordWebHookSender");
        UnityWebRequestHelper helepr = coroutineObject.AddComponent<UnityWebRequestHelper>();
        helepr.sendWebHook(UnityWebRequestHelper._webHookCHAT, message);
    }

    public static void sendDiscordMessage_dev(string message)
    {
        GameObject coroutineObject = new GameObject("DiscordWebHookSender");
        UnityWebRequestHelper helepr = coroutineObject.AddComponent<UnityWebRequestHelper>();
        helepr.sendWebHook(UnityWebRequestHelper._webHookDEV, message);
    }
#endif
}

#if UNITY_EDITOR
public class CustomDialog_OpenFile : EditorWindow
{
    public Texture2D customImage;
    public string titleText = "Custom Dialog Title";
    public string messageText = "Custom Dialog Message";
    public string buttonText = "OK";

    public string _filePath = "";
    public int _fileLineNumber = 0;

    public static void ShowCustomDialog(string title, string message, string buttonLabel, string filePath, int fileLineNumber )
    {
        CustomDialog_OpenFile window = (CustomDialog_OpenFile)EditorWindow.GetWindow(typeof(CustomDialog_OpenFile));
        window.titleContent = new GUIContent(title);
        window.minSize = new Vector2(750, 150);
        window.maxSize = new Vector2(750, 200);
        window.customImage = AssetDatabase.LoadAssetAtPath("Assets/Editor/DialogTitle.png",typeof(Texture2D)) as Texture2D;
        window.messageText = message;
        window.buttonText = buttonLabel;

        window._filePath = filePath;
        window._fileLineNumber = fileLineNumber;

        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
            GUILayout.Label(customImage, GUILayout.Width(100), GUILayout.Height(100));

            GUILayout.Space(10);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
            labelStyle.fontSize = 12;

            EditorGUILayout.LabelField(messageText, labelStyle);

            GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        bool buttonTriggered = false;
        if (focusedWindow == this && Event.current.type == EventType.KeyDown && ( Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
        {
            buttonTriggered = true;

            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            GUILayoutUtility.GetLastRect();
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            Event.current.Use();
        }

        GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Send Discord",GUILayout.Width(100f), GUILayout.Height(30f)))
            {
                DebugUtil.sendDiscordMessage_dev(messageText);
                this.Close();
            }

            if (GUILayout.Button("Open File",GUILayout.Width(100f), GUILayout.Height(30f)))
            {
                FileDebugger.OpenFileWithCursor(_filePath, _fileLineNumber);
                this.Close();
            }

            GUILayout.Space(10f);

            if (buttonTriggered || GUILayout.Button(buttonText,GUILayout.Width(100f), GUILayout.Height(30f)))
                this.Close();

            GUILayout.Space(10f);

            
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
    }
}
#endif



#if UNITY_EDITOR
public class CustomDialog : EditorWindow
{
    public Texture2D customImage;
    public string titleText = "Custom Dialog Title";
    public string messageText = "Custom Dialog Message";
    public string buttonText = "OK";

    public static void ShowCustomDialog(string title, string message, string buttonLabel)
    {
        CustomDialog window = (CustomDialog)EditorWindow.GetWindow(typeof(CustomDialog));
        window.titleContent = new GUIContent(title);
        window.minSize = new Vector2(750, 150);
        window.maxSize = new Vector2(750, 200);
        window.customImage = AssetDatabase.LoadAssetAtPath("Assets/Editor/DialogTitle.png",typeof(Texture2D)) as Texture2D;
        window.messageText = message;
        window.buttonText = buttonLabel;

        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
            GUILayout.Label(customImage, GUILayout.Width(100), GUILayout.Height(100));

            GUILayout.Space(10);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
            labelStyle.fontSize = 12;

            EditorGUILayout.LabelField(messageText, labelStyle);

            GUILayout.Space(10);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        bool buttonTriggered = false;
        if (focusedWindow == this && Event.current.type == EventType.KeyDown && ( Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
        {
            buttonTriggered = true;

            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            GUILayoutUtility.GetLastRect();
            GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
            Event.current.Use();
        }

        GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Send Discord",GUILayout.Width(100f), GUILayout.Height(30f)))
            {
                DebugUtil.sendDiscordMessage_dev(messageText);
                this.Close();
            }

            if (buttonTriggered || GUILayout.Button(buttonText,GUILayout.Width(100f), GUILayout.Height(30f)))
                this.Close();

            GUILayout.Space(10f);

            
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
    }
}
#endif


