using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TilemapEditor;

public class TilemapImportWindow : EditorWindow
{
    private string _searchString;
    public static void OpenWindow()
    {
        TilemapImportWindow window = (TilemapImportWindow)EditorWindow.GetWindow(typeof(TilemapImportWindow));
        Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Sprites/Gear.png");
        GUIContent titleContent = new GUIContent("Tilemaps", icon);
        window.titleContent = titleContent;
        window.Show();
    }

    private void OnDisable() {
        GC.Collect();
    }

    private void OnEnable()
    {
        isGuiStyleInitedWhenProSkin = !EditorGUIUtility.isProSkin;
        InitEyeIcon();
    }

    private void OnGUI() {
        InitColors();
        InitStyles();

        Color defColor = GUI.color;

        GUILayout.BeginVertical(topMenuStyle);
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Tilemaps");
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search", GUILayout.MaxWidth(60f));
        _searchString = GUILayout.TextField(_searchString);
        GUILayout.EndHorizontal();

        bool roomClicked = false;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, (GUIStyle)"hostview");
        {
            _nowRoomIndex = 0;
            GUILayout.BeginVertical(sceneBoxStyle);
            {
                GUILayout.Label("Saved Tilemaps", (GUIStyle)"OL Title");

                List<TilemapConfig> tilemaps = TilemapInspectorEditor.GetAllTilemaps(_searchString);
                int numOfTilemaps = tilemaps.Count;

                for (int i = 0; i < numOfTilemaps; i++)
                {
                    bool nowClicked = false;
                    DrawTilemapButton(tilemaps[i], defColor, out nowClicked);
                    roomClicked = roomClicked || nowClicked;
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndScrollView();

        if (Event.current.type == EventType.MouseDown && !roomClicked)
        {
            _selectedRoomIndex = -1;
        }

        GUI.color = defColor;
        Repaint();
    }

    private void DrawTilemapButton(TilemapConfig tilemap, Color defColor, out bool nowClicked) {
        nowClicked = false;

        bool isNowSelectedRoom = (_nowRoomIndex == _selectedRoomIndex);

        GUIContent roomText = new GUIContent(tilemap.name);
        Rect roomTextRect = GUILayoutUtility.GetRect(roomText, sceneTextStyle);

        if (isNowSelectedRoom) {
            if (focusedWindow == this)
                GUI.color = EditorGUIUtility.isProSkin ? proSelectBackground : stdSelectBackground ;
            else
                GUI.color = EditorGUIUtility.isProSkin ? proLostFocusSelectBackground : stdLostFocusSelectBackground;
            GUI.DrawTexture(roomTextRect, EditorGUIUtility.whiteTexture);
        }

        GUI.color = (EditorGUIUtility.isProSkin ? proTextColor : (isNowSelectedRoom ? stdSelectTextColor : stdTextColor));
        GUIStyle nowSceneTextStyle = isNowSelectedRoom ? sceneSelectedTextStyle : sceneTextStyle;
        nowSceneTextStyle.fontStyle = FontStyle.Bold;

        GUI.Label(roomTextRect, roomText, nowSceneTextStyle);

        #region when click
        Event e = Event.current;
        
        if (roomTextRect.Contains(e.mousePosition))
        {

            if (e.type == EventType.MouseDown)
            {
                nowClicked = true;
                _selectedRoomIndex = _nowRoomIndex;
            }
            else if (e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    if (_lastClickedScene == _nowRoomIndex && Time.realtimeSinceStartup - _lastClickTime <= DoubleClickDelay) {
                        TilemapEditorScript editor = GameObject.Find("TilemapEditor").GetComponent<TilemapEditorScript>();
                        if (EditorUtility.DisplayDialog("Are you sure?", "Importing this tilemap will overlap the current one without saving it.", "Okay", "Cancel"))
                        {
                            editor.Import(tilemap);
                            TilemapInspectorEditor.CurrentTilemapName = tilemap._tilemapName;
                        }	
                    }
                    else
                    {
                        _lastClickedScene = _nowRoomIndex;
                        _lastClickTime = Time.realtimeSinceStartup;
                    }
                }
            }
        }
        #endregion

        _nowRoomIndex++;
        GUI.color = defColor;
    }


    private int _lastClickedScene = -1;
    private float _lastClickTime = 0;
    private const float DoubleClickDelay = 1f;
    private int _nowRoomIndex = -1;
    private int _selectedRoomIndex;

    #region GUI Styles
    bool isGuiStyleInitedWhenProSkin = false;
    GUIStyle topMenuStyle = null;
    GUIStyle viewOptionPopupStyle = null;
    GUIStyle sceneBoxStyle = null;

    GUIStyle sceneTextStyle = null;
    GUIStyle sceneSelectedTextStyle = null;
    #endregion

    #region Colors
    Color proTextColor = Color.clear;
    Color proDisableTextColor = Color.clear;

    Color proSelectBackground = Color.clear;
    Color proDisableSelectBackground = Color.clear;
    Color proLostFocusSelectBackground = Color.clear;
    Color proLostFocusDisableSelectBackground = Color.clear;

    Color stdTextColor = Color.clear;
    Color stdSelectTextColor = Color.clear;
    Color stdDisableTextColor = Color.clear;
    Color stdSelectDisableTextColor = Color.clear;

    Color stdSelectBackground = Color.clear;
    Color stdDisableSelectBackground = Color.clear;
    Color stdLostFocusSelectBackground = Color.clear;
    Color stdLostFocusDisableSelectBackground = Color.clear;
    #endregion

    Texture2D eyeIcon = null;

    public Vector2 scrollPosition = Vector2.zero;

    void InitEyeIcon()
    {
        eyeIcon = EditorGUIUtility.FindTexture("ViewToolOrbit");
    }

    void InitStyles()
    {
        if (isGuiStyleInitedWhenProSkin != EditorGUIUtility.isProSkin)
        {
            InitEyeIcon();
        }

        //Make Top Menu Style
        if (topMenuStyle == null || isGuiStyleInitedWhenProSkin != EditorGUIUtility.isProSkin)
        {
            topMenuStyle = new GUIStyle((GUIStyle)"Toolbar");

            topMenuStyle.fixedHeight = 20;
        }

        //Make View Option Popup style
        if (viewOptionPopupStyle == null || isGuiStyleInitedWhenProSkin != EditorGUIUtility.isProSkin)
        {
            viewOptionPopupStyle = new GUIStyle((GUIStyle)"TE Toolbar");
            GUIStyleState style = new GUIStyleState();
            style.background = eyeIcon;
            style.textColor = Color.clear;
            viewOptionPopupStyle.normal = style;
        }

        if (sceneBoxStyle == null || isGuiStyleInitedWhenProSkin != EditorGUIUtility.isProSkin)
        {
            sceneBoxStyle = new GUIStyle((GUIStyle)"OL box");
            sceneBoxStyle.margin = new RectOffset(10, 10, 4, 4);
            sceneBoxStyle.border = new RectOffset(2, 2, 2, 2);
            sceneBoxStyle.stretchHeight = false;
            sceneBoxStyle.stretchWidth = true;
        }
        if (sceneTextStyle == null || isGuiStyleInitedWhenProSkin != EditorGUIUtility.isProSkin)
        {
            sceneTextStyle = new GUIStyle(EditorStyles.label);
            sceneTextStyle.padding = new RectOffset(10, 10, 2, 2);
            sceneTextStyle.margin = new RectOffset(1, 0, 0, 0);
            sceneTextStyle.active = sceneTextStyle.onActive = sceneTextStyle.normal = sceneTextStyle.onNormal;
        }
        if (sceneSelectedTextStyle == null || isGuiStyleInitedWhenProSkin != EditorGUIUtility.isProSkin)
        {
            sceneSelectedTextStyle = new GUIStyle(EditorStyles.label);
            sceneSelectedTextStyle.padding = new RectOffset(10, 10, 2, 2);
            sceneSelectedTextStyle.margin = new RectOffset(1, 0, 0, 0);
            sceneSelectedTextStyle.onNormal.textColor = Color.white;
            sceneSelectedTextStyle.active = sceneSelectedTextStyle.onActive = sceneSelectedTextStyle.normal = sceneSelectedTextStyle.onNormal;
        }

        isGuiStyleInitedWhenProSkin = EditorGUIUtility.isProSkin;
    }
    void InitColors()
    {
        proTextColor = (Color)new Color32(255, 255, 255, 255);
        proDisableTextColor = (Color)new Color32(255, 255, 255, 120);

        proSelectBackground = (Color)new Color32(62, 95, 150, 255);
        proDisableSelectBackground = (Color)new Color32(62, 95, 150, 200);

        proLostFocusSelectBackground = (Color)new Color32(255, 255, 255, 20);
        proLostFocusDisableSelectBackground = (Color)new Color32(255, 255, 255, 10);

        stdTextColor = (Color)new Color32(0, 0, 0, 255);
        stdSelectTextColor = (Color)new Color32(255, 255, 255, 255);
        stdDisableTextColor = (Color)new Color32(0, 0, 0, 120);
        stdSelectDisableTextColor = (Color)new Color32(255, 255, 255, 120);

        stdSelectBackground = (Color)new Color32(62, 125, 231, 255);
        stdDisableSelectBackground = (Color)new Color32(62, 125, 231, 200);

        stdLostFocusSelectBackground = (Color)new Color32(0, 0, 0, 80);
        stdLostFocusDisableSelectBackground = (Color)new Color32(0, 0, 0, 50);
    }
}