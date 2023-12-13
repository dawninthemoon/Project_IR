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

        private List<TilemapConfig> _tilemapsList;

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

            if (GUILayout.Button("Export", GUILayout.Height(40f))) {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure? The RoomBase will be overlaped!", "Export", "Do Not Export")) {
                    Export();
                        
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.Space(5f);

            GUI.backgroundColor = defaultColor;
            if (GUILayout.Button("Clear All")) {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure?", "Clear", "Do Not Clear")) {
                    _context.ClearAll();
                }
            }

            if (GUILayout.Button("Clear Tilemaps")) {
                if (EditorUtility.DisplayDialog("Warning", "Are you sure?", "Clear", "Do Not Clear")) {
                    _context.ClearAllTilemaps();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Export() {
            TilemapConfig asset = _context.RequestExport(CurrentTilemapName);
            string path = "Assets/TilemapData/Tilemap_" + asset._tilemapName + ".asset";
            AssetDatabase.CreateAsset(asset, path);
        }

        public static List<TilemapConfig> GetAllTilemaps() {
            List<TilemapConfig> tilemaps = new List<TilemapConfig>();

            string[] allTilemapFiles = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
            foreach(string tilemapFile in allTilemapFiles) {
                string assetPath = "Assets" + tilemapFile.Replace(Application.dataPath, "").Replace('\\', '/');
                TilemapConfig source = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TilemapConfig)) as TilemapConfig;
                if (source) {
                    tilemaps.Add(source);
                }
            }

            return tilemaps;
        }
    }
}