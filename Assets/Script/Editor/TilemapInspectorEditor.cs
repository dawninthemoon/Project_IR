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

        private void OnEnable() {
            _context = (TilemapEditorScript)target;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (_context.Config != null)
            {
                EditorGUILayout.LabelField("현재 타일맵 이름");
                _context.Config._tilemapName = EditorGUILayout.TextField(_context.Config._tilemapName);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5f);

            var defaultColor = GUI.backgroundColor;

            EditorGUILayout.Space(5f);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Create New", GUILayout.Height(40f))) {
                if (EditorUtility.DisplayDialog("Warning", "저장하지 않은 데이터가 사라질 수 있습니다. 새로 생성하시겠습니까?","네","아니오"))
                {
                    var newTilemapConfig = CreateNewAsset();
                    if (newTilemapConfig != null)
                        _context.Import(newTilemapConfig);
                }
            }
            if (GUILayout.Button("Import", GUILayout.Height(40f))) {
                var tilemaps = GetAllTilemaps();

                TilemapImportWindow.OpenWindow();
            }

            if (GUILayout.Button("Save", GUILayout.Height(40f))) {
                Save();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.Space(5f);

            GUI.backgroundColor = defaultColor;
            if (GUILayout.Button("Clear Backgrounds")) {
                if (EditorUtility.DisplayDialog("Warning", "모든 배경 타일을 지우시겠습니까?", "예", "아니오")) {
                    _context.ClearAllBackgrounds();
                }
            }

            if (GUILayout.Button("Clear Walls")) {
                if (EditorUtility.DisplayDialog("Warning", "모든 충돌 타일을 지우시겠습니까?", "예", "아니오")) {
                    _context.ClearAllWalls();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void Save() {
            if (_context.Config == null)
            {
                EditorUtility.DisplayDialog("alert", "저장할 타일맵이 없습니다. 먼저 타일맵을 불러오거나 생성해주세요.", "확인");
                return;
            }

            TilemapConfig asset = _context.LoadTilemapConfig(_context.Config._tilemapName);

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

        private TilemapConfig CreateNewAsset()
        {
            TilemapConfig asset = ScriptableObject.CreateInstance<TilemapConfig>();

            string defaultName = "Tilemap_NewTilemap.asset";
            string path = EditorUtility.SaveFilePanel(
                "Save Stage Data",
                "Assets/TilemapData/",
                defaultName,
                "asset"
            );

            if (string.IsNullOrEmpty(path)) 
                return null;

            path = FileUtil.GetProjectRelativePath(path);
            AssetDatabase.CreateAsset(asset, path);

            return asset;
        }

        public static List<TilemapConfig> GetAllTilemaps(string searchString = null) {
            List<TilemapConfig> tilemaps = new List<TilemapConfig>();

            string[] allTilemapFiles = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);
            foreach(string tilemapFile in allTilemapFiles) {
                if ((searchString == null) || tilemapFile.ToLower().Contains(searchString.ToLower())) {
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