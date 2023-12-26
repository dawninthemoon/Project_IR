using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

namespace TilemapEditor {
    [CustomEditor(typeof(TilemapEditorScript)), CanEditMultipleObjects]
    public class TilemapInspectorEditor : Editor
    {
        private TilemapEditorScript _context;
        public static string CurrentTilemapName;

        private void OnEnable() {
            _context = (TilemapEditorScript)target;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Tilemap Name");
            CurrentTilemapName = EditorGUILayout.TextField(CurrentTilemapName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5f);

            var defaultColor = GUI.backgroundColor;

            EditorGUILayout.Space(5f);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Import", GUILayout.Height(40f))) {
                var tilemaps = GetAllTilemaps();

                TilemapImportWindow.OpenWindow();
            }

            if (GUILayout.Button("Save", GUILayout.Height(40f))) {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure? The RoomBase will be overlaped!", "Save", "Do Not Export")) {
                    Save();
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.Space(5f);

            GUI.backgroundColor = defaultColor;
            if (GUILayout.Button("Clear Backgrounds")) {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure?", "Clear", "Do Not Clear")) {
                    _context.ClearAllBackgrounds();
                }
            }

            if (GUILayout.Button("Clear Walls")) {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure?", "Clear", "Do Not Clear")) {
                    _context.ClearAllWalls();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Save() {
            TilemapConfig asset = _context.LoadTilemapConfig(CurrentTilemapName);

            if (!AssetDatabase.Contains(asset))
            {
                string defaultName = "Tilemap_" + asset._tilemapName + ".asset";
                string path = EditorUtility.SaveFilePanel(
                    "Save Stage Data",
                    "Assets/TilemapData/",
                    defaultName,
                    "asset"
                );

                if (string.IsNullOrEmpty(path)) 
                    return;

                path = FileUtil.GetProjectRelativePath(path);
                AssetDatabase.CreateAsset(asset, path);
            }

            AssetDatabase.SaveAssets();
        }

        public static List<TilemapConfig> GetAllTilemaps(string searchString = null) {
            List<TilemapConfig> tilemaps = new List<TilemapConfig>();

            string[] allTilemapFiles = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
            foreach(string tilemapFile in allTilemapFiles) {
                if ((searchString == null) || tilemapFile.Contains(searchString)) {
                    string assetPath = "Assets" + tilemapFile.Replace(Application.dataPath, "").Replace('\\', '/');
                    TilemapConfig source = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TilemapConfig)) as TilemapConfig;
                    if (source) {
                        tilemaps.Add(source);
                    }
                }
            }

            return tilemaps;
        }
    }
}