using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MovementDataExporterWindow : EditorWindow
{
    private static MovementDataExporterWindow _window;

    [MenuItem("Tools/MovementDataExporter", priority = 0)]
    private static void SetSpritePivot()
    {
        _window = (MovementDataExporterWindow)EditorWindow.GetWindow(typeof(MovementDataExporterWindow), true, null, false);
    }

    private string _filePath = "Resources/Sprites/";
    private Color _pivotColor = Color.green;
    private void OnGUI()
    {
        _filePath = EditorGUILayout.TextField("path",_filePath);
        _pivotColor = EditorGUILayout.ColorField("pivot Color",_pivotColor);

        if(GUILayout.Button("Export"))
        {
            if(_filePath == null || _filePath == "")
            {
                DebugUtil.assert(false,"path invalid");
                return;
            }

            MovementDataExporter dataExporter = new MovementDataExporter();
            dataExporter.readAndExportData(_filePath, _pivotColor);
        }
    }

}
