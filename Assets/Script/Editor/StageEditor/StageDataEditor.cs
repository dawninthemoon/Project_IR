using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;
using NUnit.Framework.Internal;
using System.Net;

[InitializeOnLoad]
public class StageDataEditor : EditorWindow
{
    private class StagePointDataEditObject
    {
        public StagePointData _stagePointData;

        public GameObject _gizmoItem;
        public List<SpriteRenderer> _characterObjectList = new List<SpriteRenderer>();
        public List<StagePointCharacterSpawnData> _characterSpawnDataList = new List<StagePointCharacterSpawnData>();

        public bool syncPosition(bool isMiniStage)
        {
            if(_stagePointData == null || _gizmoItem == null)
                return false;

            bool syncSuccess = false;
            
            if(isMiniStage == false)
            {
                syncSuccess = _stagePointData._stagePoint != _gizmoItem.transform.position;
                _stagePointData._stagePoint = _gizmoItem.transform.position;
            }
            else
            {
                _gizmoItem.transform.position = Vector3.zero;
            }
            
            if(syncSuccess == false)
            {
                for(int index = 0; index < _characterObjectList.Count; ++index)
                {
                    if(_characterObjectList[index] == null)
                    {
                        _characterObjectList.RemoveAt(index);
                        _characterSpawnDataList.RemoveAt(index);

                        _stagePointData._characterSpawnData = _characterSpawnDataList.ToArray();
                        --index;
                        continue;
                    }

                    syncSuccess |= _stagePointData._characterSpawnData[index]._localPosition == _characterObjectList[index].transform.position - _stagePointData._stagePoint;
                    _stagePointData._characterSpawnData[index]._localPosition = _characterObjectList[index].transform.position - _stagePointData._stagePoint;
                }
            }
            else
            {
                for(int index = 0; index < _characterObjectList.Count; ++index)
                {
                    _characterObjectList[index].transform.position = _stagePointData._stagePoint + _stagePointData._characterSpawnData[index]._localPosition;
                }
            }
            

            return syncSuccess;
        }
    }
    
    private class MiniStageDataEditObject
    {
        public MiniStageListItem    _miniStageData = null;
        public GameObject           _gizmoItem = null;

        private Vector3             _pivotPosition = Vector3.zero;

        public List<SpriteRenderer> _characterObjectList = new List<SpriteRenderer>();

        public bool syncPosition(Vector3 pivotPosition)
        {
            if(_miniStageData == null || _gizmoItem == null)
                return false;

            bool syncSuccess = _pivotPosition != pivotPosition;
            if(syncSuccess)
            {
                _pivotPosition = pivotPosition;
                _gizmoItem.transform.position = _pivotPosition + _miniStageData._localStagePosition;
            }
            else
            {
                _miniStageData._localStagePosition = _gizmoItem.transform.position - pivotPosition;
            }

            if(_miniStageData._data._stagePointData.Count > 0)
            {
                for(int index = 0; index < _characterObjectList.Count; ++index)
                {
                    _characterObjectList[index].transform.position = _miniStageData._data._stagePointData[0]._characterSpawnData[index]._localPosition + _gizmoItem.transform.position;
                }
            }

            return syncSuccess;
        }
    }

    private class MarkerDataEditObject
    {
        public MarkerItem           _markerData = null;
        public GameObject           _gizmoItem = null;

        public bool syncPosition()
        {
            if(_markerData == null || _gizmoItem == null)
                return false;

            _markerData._position = _gizmoItem.transform.position;
            return true;
        }
    }

    private class MovementTrackDataEditObject
    {
        public enum SelectInfo
        {
            Point,
            BezierPoint,
            BezierPointInv,
        }

        public MovementTrackData    _trackData = null;
        public GameObject           _selectedPointGizmo = null;
        public SelectInfo           _pointSelectinfo = SelectInfo.Point;

        public Vector2              _pointListScroll = Vector2.zero;

        public int _selectedPointIndex = 0;

        public bool syncPosition()
        {
            if(_trackData == null || _selectedPointGizmo == null || _selectedPointIndex >= _trackData._trackPointData.Count || _selectedPointIndex < 0)
                return false;

            switch(_pointSelectinfo)
            {
                case SelectInfo.Point:
                    Vector3 bezierPointVector = _trackData._trackPointData[_selectedPointIndex]._bezierPoint - _trackData._trackPointData[_selectedPointIndex]._point;
                    _trackData._trackPointData[_selectedPointIndex]._point = _selectedPointGizmo.transform.position;
                    _trackData._trackPointData[_selectedPointIndex]._bezierPoint = _selectedPointGizmo.transform.position + bezierPointVector;
                break;
                case SelectInfo.BezierPoint:
                    _trackData._trackPointData[_selectedPointIndex]._bezierPoint = _selectedPointGizmo.transform.position;
                break;
                case SelectInfo.BezierPointInv:
                    _trackData._trackPointData[_selectedPointIndex]._bezierPoint = _trackData._trackPointData[_selectedPointIndex].convertInverseBezierPointToBezierPoint(_selectedPointGizmo.transform.position);
                break;
            }

            return true;
        }

        public bool syncGizmoPosition()
        {
            if(_trackData == null || _selectedPointGizmo == null || _selectedPointIndex >= _trackData._trackPointData.Count || _selectedPointIndex < 0)
                return false;

            switch(_pointSelectinfo)
            {
                case SelectInfo.Point:
                    _selectedPointGizmo.transform.position = _trackData._trackPointData[_selectedPointIndex]._point;
                break;
                case SelectInfo.BezierPoint:
                    _selectedPointGizmo.transform.position = _trackData._trackPointData[_selectedPointIndex]._bezierPoint;
                break;
                case SelectInfo.BezierPointInv:
                    _selectedPointGizmo.transform.position = _trackData._trackPointData[_selectedPointIndex].getInverseBezierPoint();
                break;
            }

            return true;
        }
    }

    private class SequencerPathEditor
    {
        bool _listOpen = false;
        bool _viewerOpen = false;
        bool _updateFileList = true;
        
        string _currentFilePath = "";
        string _currentFolderName = "";
        string _label = "";

        string _searchString = "";
        string[] _searchStringSplit = null;

        string _outFilePath = "";

        Vector2 _sequencerViewerScroll = Vector2.zero;
        Vector2 _fileViewerScroll = Vector2.zero;

        List<System.IO.DirectoryInfo> _directoryInfoList = new List<System.IO.DirectoryInfo>();
        List<System.IO.FileInfo> _fileInfoList = new List<System.IO.FileInfo>();

        Stack<System.IO.DirectoryInfo> _openFolderPath = new Stack<System.IO.DirectoryInfo>();

        public SequencerPathEditor(string label)
        {
            _label = label;
        }


        public void draw(ref string[] targetList)
        {
            EditorGUILayout.BeginHorizontal();

            Color colorOrigin = GUI.color;
            GUI.color = _listOpen ? Color.green : colorOrigin;
            if(GUILayout.Button(_listOpen ? "▼" : "▶", GUILayout.Width(25f)))
            {
                _listOpen = !_listOpen; 
                if(_listOpen == false)
                {
                    _viewerOpen = false;
                    clear();
                }
            }
            GUI.color = colorOrigin;

            GUI.color = _viewerOpen ? Color.green : colorOrigin;
            if(GUILayout.Button(_viewerOpen ? "○" : "•", GUILayout.Width(25f)))
            {
                _viewerOpen = !_viewerOpen;
                if(_viewerOpen)
                    _listOpen = true;
                clear();
            }
            GUI.color = colorOrigin;

            GUI.color = _listOpen ? Color.green : colorOrigin;
            GUILayout.Label(_label,GUILayout.ExpandWidth(true));
            GUI.color = colorOrigin;

            EditorGUILayout.EndHorizontal();

            if(_listOpen)
            {
                drawSequencerList(ref targetList);
            }

            if(_viewerOpen)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                if(drawFileList())
                    addItem(ref targetList);
            }

        }

        private void deleteItem(int index, ref string[] targetList)
        {
            List<string> stringList = new List<string>();
            stringList.AddRange(targetList);

            stringList.RemoveAt(index);

            targetList = stringList.ToArray();
        }

        private void addItem(ref string[] targetList)
        {
            if(targetList == null)
            {
                targetList = new string[1];
                targetList[0] = _outFilePath;

                return;
            }
            List<string> stringList = new List<string>();
            stringList.AddRange(targetList);

            stringList.Add(_outFilePath);

            targetList = stringList.ToArray();
        }

        private void drawSequencerList(ref string[] targetList)
        {
            _sequencerViewerScroll = GUILayout.BeginScrollView(_sequencerViewerScroll, "box");
            GUILayout.BeginHorizontal();

            GUILayout.Label("Sequencer List");
            if(GUILayout.Button("New", GUILayout.Width(40f)))
            {
                string defaultName = "NewSequencer" + ".xml";
                string filePath = EditorUtility.SaveFilePanel(
                    "New Sequencer",
                    "Assets/Data/SequencerGraph/",
                    defaultName,
                    "xml"
                );

                if(filePath == null || filePath == "")
                    return;

                var file = File.CreateText(filePath);
                if(file != null)
                {
                    string templatePath = IOControl.PathForDocumentsFile("Assets/ScriptTemplates/79-Action Script__Sequencer Graph-NewSequencerGraph.xml.txt");
                    _outFilePath = filePath.Replace(IOControl.PathForDocumentsFile("Assets/Data/SequencerGraph/").Replace('\\','/'),"");

                    StreamReader streamReader = new StreamReader(templatePath);
                    
                    string templateResult = streamReader.ReadToEnd();
                    templateResult = templateResult.Replace("#SCRIPTNAME#", _outFilePath.Remove(_outFilePath.IndexOf('.'), 4));

                    file.WriteLine(templateResult);
                    file.Flush();
                    file.Close();

                    addItem(ref targetList);

                    FileDebugger.OpenFileWithCursor(filePath,0);
                    _outFilePath = "";
                }
            }

            GUILayout.EndHorizontal();

            int deleteIndex = -1;
            for(int index = 0; index < targetList.Length; ++index)
            {
                GUILayout.BeginHorizontal();

                GUI.enabled = index > 0;
                if(GUILayout.Button("▲",GUILayout.Width(25f)))
                {
                    string temp = targetList[index];
                    targetList[index] = targetList[index - 1];
                    targetList[index - 1] = temp;
                }

                GUI.enabled = index < targetList.Length - 1;
                if(GUILayout.Button("▼",GUILayout.Width(25f)))
                {
                    string temp = targetList[index];
                    targetList[index] = targetList[index + 1];
                    targetList[index + 1] = temp;
                }

                GUI.enabled = true;
                if(GUILayout.Button("Open", GUILayout.Width(45f)))
                {
                    string fullPath = IOControl.PathForDocumentsFile("Assets/Data/SequencerGraph/") + targetList[index];
                    FileDebugger.OpenFileWithCursor(fullPath,0);
                }

                if(GUILayout.Button("X",GUILayout.Width(25f)))
                    deleteIndex = index;

                GUILayout.Label(targetList[index]);
                GUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            GUILayout.EndScrollView();

            if(deleteIndex >= 0)
                deleteItem(deleteIndex, ref targetList);
        }

        private bool drawFileList()
        {
            if(_updateFileList)
                IOControl.getAllFileList(_currentFilePath,".xml",ref _directoryInfoList,ref _fileInfoList);

            GUILayout.BeginVertical("box");

            Color colorOrigin = GUI.color;
            GUI.color = Color.green;
            GUILayout.Label("File Selector");

            GUI.color = colorOrigin;

            string searchString = EditorGUILayout.TextField("Search",_searchString);
            if(_searchString != searchString)
            {
                _searchString = searchString;
                _searchStringSplit = _searchString.Split(' ');
            }

            _fileViewerScroll = GUILayout.BeginScrollView(_fileViewerScroll, "box");

            GUILayout.BeginHorizontal();
            
            GUILayout.Space(25f);
            if(GUILayout.Button("<", GUILayout.Width(25f)) && _openFolderPath.Count > 0)
            {
                var folder = _openFolderPath.Pop();
                _currentFilePath = folder.FullName;
                _currentFolderName = folder.Name;
                
                _updateFileList = true;
                return false;
            }
            
            GUILayout.Label(_currentFolderName);

            GUILayout.EndHorizontal();

            foreach(var item in _directoryInfoList)
            {
                if(stringSearch(item.Name) == false)
                    continue;

                GUILayout.BeginHorizontal();
                if(GUILayout.Button(">", GUILayout.Width(25f)))
                {
                    _openFolderPath.Push(item.Parent);

                    _currentFilePath = item.FullName;
                    _currentFolderName = item.Name;

                    _updateFileList = true;
                }
                GUILayout.Label(item.Name);
                GUILayout.EndHorizontal();
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;

            foreach(var item in _fileInfoList)
            {
                if(stringSearch(item.Name) == false)
                    continue;

                GUILayout.BeginHorizontal();
                GUILayout.Space(25f);
                if(GUILayout.Button("Open", GUILayout.Width(45f)))
                {
                    string fullPath = item.FullName;
                    FileDebugger.OpenFileWithCursor(fullPath,0);
                }
                
                if(GUILayout.Button(item.Name,buttonStyle))
                {
                    clear();
                    _outFilePath = item.FullName;
                    _outFilePath = _outFilePath.Replace(IOControl.PathForDocumentsFile("Assets\\Data\\SequencerGraph\\").Replace('/','\\'),"").Replace('\\','/');

                    _viewerOpen = false;
                    return true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            return false;
        }

        public bool stringSearch(string targetString)
        {
            if(_searchStringSplit == null)
                return true;

            string lower = targetString.ToLower();

            foreach(var item in _searchStringSplit)
            {
                if(lower.Contains(item.ToLower()) == false)
                    return false;
            }
            
            return true;
        }

        public void clear()
        {
            _currentFilePath = IOControl.PathForDocumentsFile("Assets/Data/SequencerGraph");
            _currentFolderName = "SequencerGraph";
            _outFilePath = "";
            _fileViewerScroll = Vector2.zero;
            _directoryInfoList.Clear();
            _fileInfoList.Clear();
            _openFolderPath.Clear();

            _updateFileList = true;
        }
    }
    
    public StageData _editStageData;

    private static StageDataEditor _window;
    private GameObject _editItemParent = null;
    private GameObject _editItemGizmoParent = null;
    private GameObject _editItemCharacterParent = null;

    private Queue<GameObject> _gizmoItemPool = new Queue<GameObject>();
    private Queue<SpriteRenderer> _characterItemPool = new Queue<SpriteRenderer>();

    private List<StagePointDataEditObject>  _editingStagePointList = new List<StagePointDataEditObject>();
    private List<MiniStageDataEditObject>   _editingMiniStageDataList = new List<MiniStageDataEditObject>();
    private List<MarkerDataEditObject>      _editingMarkerDataList = new List<MarkerDataEditObject>();
    private List<MovementTrackDataEditObject> _editingTrackDataList = new List<MovementTrackDataEditObject>();

    private GameObject _tilemapSettingPrefabObject = null;
    private GameObject _backgroundPrefabObject = null;
    
    private Vector2 _pointItemScroll = Vector2.zero;
    private Vector2 _characterSpawnScroll = Vector2.zero;
    private Vector2 _miniStageScroll = Vector2.zero;
    private Vector2 _markerScroll = Vector2.zero;
    private Vector2 _trackScroll = Vector2.zero;

    private CharacterInfoView _characterInfoView = new CharacterInfoView();
    private MiniStageListView _miniStageListView = new MiniStageListView();
    public SerializedObject _stageDataSerializedObject;
    public SerializedProperty _stageDataListProperty;

    private MovementTrackProcessor _trackProcessor = new MovementTrackProcessor();

    private string[] _editItemMenuStrings = 
    {
        "Point",
        "Character",
        "MiniStage",
        "Marker",
        "Track",
    };

    private string[] _editMenuStrings = 
    {
        "Inspector",
        "Character Palette",
        "MiniStage Palette",
    };

    private SequencerPathEditor _onEnterSequencerPathEditor = new SequencerPathEditor("On Enter Sequencer Path");
    private SequencerPathEditor _onExitSequencerPathEditor = new SequencerPathEditor("On Exit Sequencer Path");

    private int         _pointSelectedIndex = -1;
    private int         _characterSelectedIndex = -1;
    private int         _editItemMenuSelectedIndex = 0;
    private int         _editMenuSelectedIndex = 0;

    private int         _miniStageSelectedIndex = 0;
    private int         _markerSelectedIndex = 0;
    private int         _trackSelectedIndex = 0;

    private string      _pointCharacterSearchString = "";
    private string[]    _pointCharacterSearchStringList;
    private string      _pointCharacterSearchStringCompare = "";

    private string      _miniStageSearchString = "";
    private string[]    _miniStageSearchStringList;
    private string      _miniStageSearchStringCompare = "";

    private string      _markerSearchString = "";
    private string[]    _markerSearchStringList;
    private string      _markerSearchStringCompare = "";

    private string      _trackSearchString = "";
    private string[]    _trackSearchStringList;
    private string      _trackSearchStringCompare = "";

    private bool _drawScreenToMousePoint = false;
    private bool _drawTriggerBound = false;
    private bool _enableBackground = true;
    float _prevTime = 0f;

    [MenuItem("Tools/StageDataEditor", priority = 0)]
    public static void ShowWindow()
    {
        _window = (StageDataEditor)EditorWindow.GetWindow(typeof(StageDataEditor));
    }

    private void Awake()
    {
        createOrFindEditorItem();
        constructGizmoPoints();

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnGUI()
    {
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl("");
            Repaint();
        }

        bool reloadData = false;
        GUILayout.BeginHorizontal();
            StageData editStageData = EditorGUILayout.ObjectField("Stage Data", _editStageData, typeof(StageData), true) as StageData;
            MiniStageData editMiniStage = _miniStageListView.getNewMiniStageData();
            if(editMiniStage != null)
            {
                editStageData = editMiniStage;
            }

            if(GUILayout.Button("New", GUILayout.Width(40f)))
            {
                bool createNew = true;
                if(editStageData != null)
                    createNew = EditorUtility.DisplayDialog("alert","이미 편집중인 스테이지가 존재합니다. 새로 생성 하시겠습니까?","네","아니오");

                if(createNew)
                {
                    editStageData = ScriptableObject.CreateInstance<StageData>();
                    editStageData._stageName = "NewStage";

                    StagePointData stagePointData = new StagePointData(Vector3.zero);
                    stagePointData._cameraZoomSize = Camera.main.orthographicSize;
                    editStageData._stagePointData.Add(stagePointData);
                }
            }

            GUI.enabled = editStageData != null;
            if(GUILayout.Button("Refresh", GUILayout.Width(70f)))
                reloadData = true;
            GUI.enabled = true;
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if(Application.isPlaying)
        {
            if(_editItemParent != null && _editItemParent.activeSelf)
            {
                _editItemParent.SetActive(false);
            }
            return;
        }
        else
        {
            if(_editItemParent != null && _editItemParent.activeSelf == false)
            {
                _editItemParent.SetActive(true);
            }
        }
        
        if(reloadData || editStageData != _editStageData)
        {
            _editStageData = editStageData;
            _enableBackground = true;

            constructGizmoPoints();
            loadStageData();
        }

        if(_editStageData == null)
            return;

        editorKeyCheck();

        GUILayout.BeginVertical("box");
            _editStageData._stageName = EditorGUILayout.TextField("Stage Name",_editStageData._stageName);

            _editStageData._tilemapConfigPath = EditorGUILayout.ObjectField("Tilemap Config",_editStageData._tilemapConfigPath, typeof(TilemapConfig), true) as TilemapConfig;
            if (_editStageData._tilemapConfigPath == null && _tilemapSettingPrefabObject != null) 
                DestroyImmediate(_tilemapSettingPrefabObject);

            GUILayout.BeginHorizontal();
            _editStageData._backgroundPrefabPath = EditorGUILayout.ObjectField("Background Prefab",_editStageData._backgroundPrefabPath, typeof(GameObject), true) as GameObject;
            if(_editStageData._backgroundPrefabPath == null && _backgroundPrefabObject != null)
                DestroyImmediate(_backgroundPrefabObject);

            bool enableBackground = EditorGUILayout.Toggle(_enableBackground,GUILayout.Width(15f));
            if(_enableBackground != enableBackground)
            {
                _enableBackground = enableBackground;
                _backgroundPrefabObject?.SetActive(_enableBackground);
            }
            if(GUILayout.Button("New", GUILayout.Width(40f)) && _editItemParent != null)
            {
                bool createNew = true;
                if(_editStageData._backgroundPrefabPath != null)
                    createNew = EditorUtility.DisplayDialog("alert","이미 편집중인 배경이 존재합니다. 새로 생성 하시겠습니까?","네","아니오");

                if(createNew)
                {
                    string defaultName = _editStageData._stageName + ".prefab";
                    string filePath = EditorUtility.SaveFilePanel(
                        "Save Stage Background",
                        "Assets/Resources/Prefab/StageBackground/",
                        defaultName,
                        "prefab"
                    );

                    if (string.IsNullOrEmpty(filePath)) 
                        return;

                    if(_backgroundPrefabObject != null)
                        DestroyImmediate(_backgroundPrefabObject);

                    _backgroundPrefabObject = new GameObject();
                    _backgroundPrefabObject.name = filePath.Remove(0, filePath.LastIndexOf("/") + 1);
                    _backgroundPrefabObject.name = _backgroundPrefabObject.name.Replace(".prefab","");
                    _backgroundPrefabObject.transform.position = _editStageData._stagePointData.Count == 0 ? Vector3.zero : _editStageData._stagePointData[0]._stagePoint;
                    _backgroundPrefabObject.transform.SetParent(_editItemParent.transform);
                    _backgroundPrefabObject.layer = LayerMask.NameToLayer("Background");

                    filePath = FileUtil.GetProjectRelativePath(filePath);
                    _editStageData._backgroundPrefabPath = PrefabUtility.SaveAsPrefabAsset(_backgroundPrefabObject,filePath);
                }
            }
            GUILayout.EndHorizontal();

            Color currentColor = GUI.color;

            GUILayout.BeginHorizontal();
                GUI.color = _drawScreenToMousePoint ? Color.green : Color.red;
                if(GUILayout.Button("Camera Bound"))
                {
                    _drawScreenToMousePoint = !_drawScreenToMousePoint;
                    SceneView.RepaintAll();
                }
                GUI.color = _drawTriggerBound ? Color.green : Color.red;
                if(GUILayout.Button("Trigger Bound"))
                {
                    _drawTriggerBound = !_drawTriggerBound;
                    SceneView.RepaintAll();
                }
                GUI.color = currentColor;

                if(GUILayout.Button("Save Data"))
                    saveCurrentData();
            GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.BeginVertical("box",GUILayout.Height(200f));
            int selectedIndex = GUILayout.SelectionGrid(_editItemMenuSelectedIndex,_editItemMenuStrings,_editItemMenuStrings.Length);
            if(_editItemMenuSelectedIndex != selectedIndex)
            {
                _editItemMenuSelectedIndex = selectedIndex;
                if(_editItemMenuSelectedIndex == 0)
                    selectPoint(_pointSelectedIndex);
                else if(_editItemMenuSelectedIndex == 1)
                    selectCharacter(_pointSelectedIndex,_characterSelectedIndex);
                else if(_editItemMenuSelectedIndex == 2)
                    selectMiniStage(_miniStageSelectedIndex);
                else if(_editItemMenuSelectedIndex == 3)
                    selectMarker(_markerSelectedIndex);
                else if(_editItemMenuSelectedIndex == 4)
                    selectTrack(_trackSelectedIndex);
            }

            GUILayout.Space(5f);
            if(_editItemMenuSelectedIndex == 0)
                onPointGUI();
            else if(_editItemMenuSelectedIndex == 1)
                onCharacterGUI();
            else if(_editItemMenuSelectedIndex == 2)
                onMiniStageGUI();
            else if(_editItemMenuSelectedIndex == 3)
                onMarkerGUI();
            else if(_editItemMenuSelectedIndex == 4)
                onTrackGUI();
        GUILayout.EndVertical();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            _editMenuSelectedIndex = GUILayout.SelectionGrid(_editMenuSelectedIndex,_editMenuStrings,_editMenuStrings.Length);
            GUILayout.Space(5f);
            if(_editMenuSelectedIndex == 0)
            {
                if(_editItemMenuSelectedIndex == 0)
                    onPointInspectorGUI();
                else if(_editItemMenuSelectedIndex == 1)
                    onCharacterInspectorGUI();
                else if(_editItemMenuSelectedIndex == 2)
                    onMiniStageInspectorGUI();
                else if(_editItemMenuSelectedIndex == 3)
                    onMarkerInspectorGUI();
                else if(_editItemMenuSelectedIndex == 4)
                    onTrackInspectorGUI();
            }
            else if(_editMenuSelectedIndex == 1)
            {
                _characterInfoView.OnGUI();
                string addedCharacter = _characterInfoView.getAddedCharacter();
                if(addedCharacter != "")
                {
                    addCharacterToPoint(_pointSelectedIndex,addedCharacter);
                }
            }
            else if(_editMenuSelectedIndex == 2)
            {
                _miniStageListView.OnGUI();
                MiniStageData miniStageData = _miniStageListView.getAddedMiniStage();
                if(miniStageData != null)
                    addMiniStageToStage(miniStageData);
            }
        GUILayout.EndVertical();
    }

    private void editorKeyCheck()
    {
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
            if (Event.current.keyCode == (KeyCode.Period))
            {
                if(_editItemMenuSelectedIndex == 0)
                {
                    if(_editingStagePointList.Count - 1 > _pointSelectedIndex)
                        selectPoint(_pointSelectedIndex + 1);
                }
                else if(_editItemMenuSelectedIndex == 1)
                {
                    if(_editingStagePointList[_pointSelectedIndex]._stagePointData._characterSpawnData.Length - 1 > _characterSelectedIndex)
                        selectCharacter(_pointSelectedIndex, _characterSelectedIndex + 1);
                }
                else if(_editItemMenuSelectedIndex == 2)
                {
                    if(_editingMiniStageDataList.Count - 1 > _miniStageSelectedIndex)
                        selectMiniStage(_miniStageSelectedIndex + 1);
                }
                else if(_editItemMenuSelectedIndex == 3)
                {
                    if(_editingMarkerDataList.Count - 1 > _markerSelectedIndex)
                        selectMarker(_markerSelectedIndex + 1);
                }
            }
            else if(Event.current.keyCode == KeyCode.Comma)
            {
                if(_editItemMenuSelectedIndex == 0)
                {
                    if(_pointSelectedIndex > 0)
                        selectPoint(_pointSelectedIndex - 1);
                }
                else if(_editItemMenuSelectedIndex == 1)
                {
                    if(_characterSelectedIndex > 0)
                        selectCharacter(_pointSelectedIndex, _characterSelectedIndex - 1);
                }
                else if(_editItemMenuSelectedIndex == 2)
                {
                    if(_miniStageSelectedIndex > 0)
                        selectMiniStage(_miniStageSelectedIndex - 1);
                }
                else if(_editItemMenuSelectedIndex == 3)
                {
                    if(_markerSelectedIndex > 0)
                        selectMarker(_markerSelectedIndex - 1);
                }
            }
            // else if(Event.current.keyCode == KeyCode.Alpha1)
            // {
            //     _editItemMenuSelectedIndex = 0;
            // }
            // else if(Event.current.keyCode == KeyCode.Alpha2)
            // {
            //     _editItemMenuSelectedIndex = 1;
            // }
            // else if(Event.current.keyCode == KeyCode.Alpha3)
            // {
            //     _editItemMenuSelectedIndex = 2;
            // }
            // else if(Event.current.keyCode == KeyCode.Alpha4)
            // {
            //     _editItemMenuSelectedIndex = 3;
            // }
            // else if(Event.current.keyCode == KeyCode.Alpha5)
            // {
            //     _editItemMenuSelectedIndex = 4;
            // }
            break;
        }
    }

    private void onMiniStageGUI()
    {
        if(_editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0 )
            return;
        
        _miniStageSearchString = EditorGUILayout.TextField("Search",_miniStageSearchString);
        if(_miniStageSearchStringCompare != _miniStageSearchString)
        {
            if(_miniStageSearchString == "")
                _miniStageSearchStringList = null;
            else
                _miniStageSearchStringList = _miniStageSearchString.Split(' ');
            _miniStageSearchStringCompare = _miniStageSearchString;
        }

        _miniStageScroll = GUILayout.BeginScrollView(_miniStageScroll,"box");

            for(int i = 0; i < _editStageData._miniStageData.Count; ++i)
            {
                if(_pointCharacterSearchString != "" && (searchStringCompare(_editStageData._miniStageData[i]._data._stageName,_miniStageSearchStringList) == false 
                        && searchStringCompare(_editStageData._miniStageData[i]._data.name,_miniStageSearchStringList) == false))
                    continue;

                if(_editStageData._miniStageData[i]._data == null)
                    deleteMiniStage(i--);

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _miniStageSelectedIndex ? Color.green : currentColor;

                bool selected = i == _miniStageSelectedIndex;
                if(selected)
                    GUILayout.BeginHorizontal("box");

                string targetName = _editStageData._miniStageData[i]._data.name + ": " + _editStageData._miniStageData[i]._data._stageName;

                GUILayout.Label(targetName,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectMiniStage(i);

                GUI.color = new Color(1f,0.2f,0.2f);
                if(GUILayout.Button("Delete MiniStage"))
                {
                    deleteMiniStage(i--);
                    if(i == _miniStageSelectedIndex)
                        _miniStageSelectedIndex = -1;
                }

                GUI.color = currentColor;

                if(selected)
                    GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
            }

        GUILayout.EndScrollView();
    }

    private void onMarkerGUI()
    {
        GUILayout.BeginHorizontal();
        _markerSearchString = EditorGUILayout.TextField("Search",_markerSearchString);
        if(_markerSearchStringCompare != _markerSearchString)
        {
            if(_markerSearchString == "")
                _markerSearchStringList = null;
            else
                _markerSearchStringList = _markerSearchString.Split(' ');
            _markerSearchStringCompare = _markerSearchString;
        }

        if(GUILayout.Button("New", GUILayout.Width(40f)))
        {
            addMarker();
        }

        GUILayout.EndHorizontal();

        _markerScroll = GUILayout.BeginScrollView(_markerScroll,"box");

            for(int i = 0; i < _editStageData._markerData.Count; ++i)
            {
                if(_markerSearchString != "" && (searchStringCompare(_editStageData._markerData[i]._name,_markerSearchStringList) == false))
                    continue;

                if(_editStageData._markerData[i] == null)
                    deleteMarker(i--);

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _markerSelectedIndex ? Color.green : currentColor;

                bool selected = i == _markerSelectedIndex;
                if(selected)
                    GUILayout.BeginHorizontal("box");

                string targetName = _editStageData._markerData[i]._name;

                GUILayout.Label(targetName,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectMarker(i);

                GUI.color = new Color(1f,0.2f,0.2f);
                if(GUILayout.Button("Delete Marker"))
                {
                    deleteMarker(i--);
                    if(i == _markerSelectedIndex)
                        _markerSelectedIndex = -1;
                }

                GUI.color = currentColor;

                if(selected)
                    GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
            }

        GUILayout.EndScrollView();
    }

    private void onTrackGUI()
    {
        GUILayout.BeginHorizontal();
        _trackSearchString = EditorGUILayout.TextField("Search",_trackSearchString);
        if(_trackSearchStringCompare != _trackSearchString)
        {
            if(_trackSearchString == "")
                _trackSearchStringList = null;
            else
                _trackSearchStringList = _trackSearchString.Split(' ');
            _trackSearchStringCompare = _trackSearchString;
        }

        if(GUILayout.Button("New", GUILayout.Width(40f)))
            addTrack();

        GUILayout.EndHorizontal();

        _trackScroll = GUILayout.BeginScrollView(_trackScroll,"box");

            for(int i = 0; i < _editStageData._trackData.Count; ++i)
            {
                if(_trackSearchString != "" && (searchStringCompare(_editStageData._trackData[i]._name,_trackSearchStringList) == false))
                    continue;

                if(_editStageData._trackData[i] == null)
                    deleteTrack(i--);

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _trackSelectedIndex ? Color.green : currentColor;

                bool selected = i == _trackSelectedIndex;
                if(selected)
                    GUILayout.BeginHorizontal("box");

                string targetName = _editStageData._trackData[i]._name;

                GUILayout.Label(targetName,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectTrack(i);

                GUI.color = new Color(1f,0.2f,0.2f);
                if(GUILayout.Button("Delete Track"))
                {
                    deleteTrack(i--);
                    if(i == _trackSelectedIndex)
                        _trackSelectedIndex = -1;
                }

                GUI.color = currentColor;

                if(selected)
                    GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
            }

        GUILayout.EndScrollView();
    }

    private void onCharacterGUI()
    {
        if(_editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0 )
            return;
        
        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        if(stagePointData == null || stagePointData._characterSpawnData == null)
            return;

        _pointCharacterSearchString = EditorGUILayout.TextField("Search",_pointCharacterSearchString);
        if(_pointCharacterSearchStringCompare != _pointCharacterSearchString)
        {
            if(_pointCharacterSearchString == "")
                _pointCharacterSearchStringList = null;
            else
                _pointCharacterSearchStringList = _pointCharacterSearchString.Split(' ');
            _pointCharacterSearchStringCompare = _pointCharacterSearchString;
        }

        _characterSpawnScroll = GUILayout.BeginScrollView(_characterSpawnScroll,"box");
            
            for(int i = 0; i < stagePointData._characterSpawnData.Length; ++i)
            {
                if(_pointCharacterSearchString != "" && (searchStringCompare(stagePointData._characterSpawnData[i]._characterKey,_pointCharacterSearchStringList) == false 
                        && searchStringCompare(stagePointData._characterSpawnData[i]._uniqueKey,_pointCharacterSearchStringList) == false
                        && searchStringCompare(stagePointData._characterSpawnData[i]._uniqueGroupKey,_pointCharacterSearchStringList) == false))
                    continue;

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _characterSelectedIndex ? Color.green : currentColor;

                bool selected = i == _characterSelectedIndex;
                if(selected)
                    GUILayout.BeginHorizontal("box");

                string targetName = stagePointData._characterSpawnData[i]._characterKey;
                if(stagePointData._characterSpawnData[i]._uniqueKey != "")
                    targetName += " [Key: " + stagePointData._characterSpawnData[i]._uniqueKey + "]";
                if(stagePointData._characterSpawnData[i]._uniqueGroupKey != "")
                    targetName += " [Group: " + stagePointData._characterSpawnData[i]._uniqueGroupKey + "]";

                GUILayout.Label(targetName,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectCharacter(_pointSelectedIndex, i);

                GUI.color = new Color(1f,0.2f,0.2f);
                if(GUILayout.Button("Delete Character"))
                {
                    deleteCharacter(_pointSelectedIndex, i);
                    if(i == _characterSelectedIndex)
                        _characterSelectedIndex = -1;
                }

                GUI.color = currentColor;

                if(selected)
                    GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
            }
        GUILayout.EndScrollView();
    }

    private bool searchStringCompare(string target, string[] searchStringList)
    {
        string lowerTarget = target.ToLower();
        foreach(var stringItem in searchStringList)
        {
            if(lowerTarget.Contains(stringItem))
                return true;
        }

        return false;
    }

    private void onCharacterInspectorGUI()
    {
        if(_editStageData == null || _editStageData._stagePointData == null 
            || _editStageData._stagePointData.Count <= _pointSelectedIndex 
            || _pointSelectedIndex < 0 
            || _editStageData._stagePointData[_pointSelectedIndex] == null
            || _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData == null 
            || _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData.Length <= _characterSelectedIndex 
            || _characterSelectedIndex < 0
            || _editingStagePointList.Count <= _pointSelectedIndex)
            return;
 
        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointCharacterSpawnData characterSpawnData = _editStageData._stagePointData[_pointSelectedIndex]._characterSpawnData[_characterSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        Vector3 worldPositionOrigin = characterSpawnData._localPosition + stagePointData._stagePoint;
        Vector3 localPositionOrigin = characterSpawnData._localPosition;

        Vector3 newWorldPositionOrigin = EditorGUILayout.Vector3Field("World Position",worldPositionOrigin);
        if(worldPositionOrigin != newWorldPositionOrigin)
        {
            characterSpawnData._localPosition = newWorldPositionOrigin - stagePointData._stagePoint;
            localPositionOrigin = characterSpawnData._localPosition;
            stagePointDataEditObject._characterSpawnDataList[_characterSelectedIndex]._localPosition = characterSpawnData._localPosition;
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].transform.position = stagePointData._stagePoint + characterSpawnData._localPosition;
        }

        characterSpawnData._localPosition = EditorGUILayout.Vector3Field("Local Position",characterSpawnData._localPosition);
        if(characterSpawnData._localPosition != localPositionOrigin)
        {
            stagePointDataEditObject._characterSpawnDataList[_characterSelectedIndex]._localPosition = characterSpawnData._localPosition;
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].transform.position = stagePointData._stagePoint + characterSpawnData._localPosition;
        }

        characterSpawnData._flip = EditorGUILayout.Toggle("Flip",characterSpawnData._flip);
        characterSpawnData._hideWhenDeactive = EditorGUILayout.Toggle("Hide When Deactive", characterSpawnData._hideWhenDeactive);
        characterSpawnData._searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", characterSpawnData._searchIdentifier);
        characterSpawnData._activeType = (StageSpawnCharacterActiveType)EditorGUILayout.EnumPopup("Active Type", characterSpawnData._activeType);

        characterSpawnData._uniqueKey = EditorGUILayout.TextField("Unique Key",characterSpawnData._uniqueKey);
        characterSpawnData._uniqueGroupKey = EditorGUILayout.TextField("Unique Group Key",characterSpawnData._uniqueGroupKey);

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        if(characterInfo.ContainsKey(characterSpawnData._characterKey) == false)
        {
            DebugUtil.assert(false,"말이 안되는 상황");
            return;
        }

        CharacterInfoData characterInfoData = characterInfo[characterSpawnData._characterKey];
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        if(baseData == null)
        {
            DebugUtil.assert(false,"말이 안되는 상황");
            return;
        }

        List<string> actionNameList = new List<string>();
        for(int index = 1; index < baseData._actionNodeCount; ++index)
        {
            actionNameList.Add(baseData._actionNodeData[index]._nodeName);
        }

        int currentIndex = actionNameList.FindIndex((x)=>{return x == characterSpawnData._startAction;});
        if(currentIndex == -1)
            currentIndex = baseData._defaultActionIndex - 1;

        string newStartAction = actionNameList[EditorGUILayout.Popup("Start Action", currentIndex,actionNameList.ToArray())];
        if(newStartAction != characterSpawnData._startAction)
        {
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].sprite = getActionSpriteFromCharacter(characterInfoData, newStartAction);
            characterSpawnData._startAction = newStartAction;
        }

        if(characterSpawnData._hideWhenDeactive)
        {
            Color alphaClor = Color.white;
            alphaClor.a = 0.5f;
            stagePointDataEditObject._characterObjectList[_characterSelectedIndex].color = alphaClor;
        }
        
        stagePointDataEditObject._characterObjectList[_characterSelectedIndex].flipX = characterSpawnData._flip;
    }

    private void onMiniStageInspectorGUI()
    {
        if(_editingMiniStageDataList == null || _editingMiniStageDataList.Count == 0 || _editStageData._miniStageData.Count <= _miniStageSelectedIndex || _miniStageSelectedIndex < 0)
            return;

        MiniStageListItem miniStageListItem = _editStageData._miniStageData[_miniStageSelectedIndex];
        MiniStageDataEditObject miniStageDataEditObject = _editingMiniStageDataList[_miniStageSelectedIndex];

        GUILayout.BeginVertical("box");
        GUILayout.Label("Trigger");
        bool isChanged = false;
        SearchIdentifier searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", miniStageListItem._overrideTargetSearchIdentifier);
        float triggerWidth = EditorGUILayout.FloatField("Width", miniStageListItem._overrideTriggerWidth);
        float triggerHeight = EditorGUILayout.FloatField("Height", miniStageListItem._overrideTriggerHeight);
        Vector3 triggerOffset = EditorGUILayout.Vector3Field("Offset", miniStageListItem._overrideTriggerOffset);

        isChanged |= searchIdentifier != miniStageListItem._overrideTargetSearchIdentifier;
        isChanged |= triggerWidth != miniStageListItem._overrideTriggerWidth;
        isChanged |= triggerHeight != miniStageListItem._overrideTriggerHeight;
        isChanged |= triggerOffset != miniStageListItem._overrideTriggerOffset;
        GUILayout.EndVertical();

        if(isChanged)
        {
            miniStageListItem._overrideTargetSearchIdentifier = searchIdentifier;
            miniStageListItem._overrideTriggerWidth = triggerWidth;
            miniStageListItem._overrideTriggerHeight = triggerHeight;
            miniStageListItem._overrideTriggerOffset = triggerOffset;
            SceneView.RepaintAll();
        }
    }

    private void onMarkerInspectorGUI()
    {
        if(_editingMarkerDataList == null || _editingMarkerDataList.Count == 0 || _editStageData._markerData.Count <= _markerSelectedIndex || _markerSelectedIndex < 0)
            return;

        MarkerItem markerItem = _editStageData._markerData[_markerSelectedIndex];
        MarkerDataEditObject editObject = _editingMarkerDataList[_markerSelectedIndex];

        markerItem._name = EditorGUILayout.TextField("Name",markerItem._name);

        GUILayout.BeginHorizontal();
        markerItem._position = EditorGUILayout.Vector3Field("World Position", markerItem._position);
        if(GUILayout.Button("Copy", GUILayout.Width(40f)))
        {
            string copyString = MathEx.round( markerItem._position.x,2).ToString() + " ";
            copyString += MathEx.round( markerItem._position.y,2).ToString() + " ";
            copyString += MathEx.round( markerItem._position.z,2).ToString();

            GUIUtility.systemCopyBuffer = copyString;
        }
        GUILayout.EndHorizontal();
    }

    private void onTrackInspectorGUI()
    {
        if(_editingTrackDataList == null || _editingTrackDataList.Count == 0 || _editStageData._trackData.Count <= _trackSelectedIndex || _trackSelectedIndex < 0)
            return;

        MovementTrackData trackItem = _editStageData._trackData[_trackSelectedIndex];
        MovementTrackDataEditObject editObject = _editingTrackDataList[_trackSelectedIndex];

        trackItem._name = EditorGUILayout.TextField("Name",trackItem._name);
        trackItem._startBlend = EditorGUILayout.Toggle("Start Blend", trackItem._startBlend);
        trackItem._endBlend = EditorGUILayout.Toggle("End Blend", trackItem._endBlend);

        GUILayout.BeginScrollView(editObject._pointListScroll, "box", GUILayout.Height(200f));
            for(int i = 0; i < trackItem._trackPointData.Count; ++i)
            {
                if(trackItem._trackPointData[i] == null)
                    deleteTrackPoint(i--);

                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == editObject._selectedPointIndex ? Color.green : currentColor;

                bool selected = i == editObject._selectedPointIndex;
                if(selected)
                    GUILayout.BeginHorizontal("box");

                GUILayout.Label("Point " + i,GUILayout.Width(200f));

                if(GUILayout.Button("Pick"))
                    selectTrackPoint(i, MovementTrackDataEditObject.SelectInfo.Point);

                GUI.color = new Color(1f,0.2f,0.2f);
                if(GUILayout.Button("Delete Point"))
                {
                    deleteTrackPoint(i--);
                    if(i == editObject._selectedPointIndex)
                        editObject._selectedPointIndex = -1;
                }

                GUI.color = currentColor;

                if(selected)
                    GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
            }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
            if(GUILayout.Button("New Point"))
            {
                addTrackPoint();
            }
            if(GUILayout.Button("Test Play"))
            {
                trackItem.calculateTrackLength();
                _trackProcessor.initialize(trackItem);
            }
        GUILayout.EndHorizontal();

        if(editObject._selectedPointIndex < 0 || trackItem._trackPointData.Count <= editObject._selectedPointIndex)
            return;

        GUILayout.Space(10f);
        GUILayout.BeginVertical("box");
            MovementTrackPointData pointData = trackItem._trackPointData[editObject._selectedPointIndex];
            pointData._easeType = (MathEx.EaseType)EditorGUILayout.EnumPopup("Ease Type", pointData._easeType);
            pointData._speedToNextPoint = EditorGUILayout.FloatField("Speed", pointData._speedToNextPoint);
            pointData._waitSecond = EditorGUILayout.FloatField("Wait Second", pointData._waitSecond);
            bool isLinear = EditorGUILayout.Toggle("Is Linear Path",pointData._isLinearPath);
            if(pointData._isLinearPath != isLinear)
            {
                pointData._isLinearPath = isLinear;
                SceneView.RepaintAll();
            }
            GUILayout.Space(10f);
            GUILayout.BeginHorizontal();
                if(GUILayout.Button("Point"))
                    selectTrackPoint(editObject._selectedPointIndex, MovementTrackDataEditObject.SelectInfo.Point);
                if(GUILayout.Button("Bezier"))
                    selectTrackPoint(editObject._selectedPointIndex, MovementTrackDataEditObject.SelectInfo.BezierPoint);
                if(GUILayout.Button("BezierInv"))
                    selectTrackPoint(editObject._selectedPointIndex, MovementTrackDataEditObject.SelectInfo.BezierPointInv);
            GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void onPointInspectorGUI()
    {
        if(_editingStagePointList == null || _editingStagePointList.Count == 0 || _editStageData._stagePointData.Count <= _pointSelectedIndex || _pointSelectedIndex < 0)
            return;

        StagePointData stagePointData = _editStageData._stagePointData[_pointSelectedIndex];
        StagePointDataEditObject stagePointDataEditObject = _editingStagePointList[_pointSelectedIndex];

        GUILayout.BeginHorizontal();

        GUILayout.Label("Position: " + stagePointData._stagePoint.ToString()); 
        if(GUILayout.Button("Copy", GUILayout.Width(40f)))
        {
            string copyString = MathEx.round( stagePointData._stagePoint.x,2).ToString() + " ";
            copyString += MathEx.round( stagePointData._stagePoint.y,2).ToString() + " ";
            copyString += MathEx.round( stagePointData._stagePoint.z,2).ToString();

            GUIUtility.systemCopyBuffer = copyString;
        }

        GUILayout.EndHorizontal();

        if(_editStageData._isMiniStage == false)
        {
            stagePointData._pointName = EditorGUILayout.TextField("Name",stagePointData._pointName);
            stagePointData._maxLimitedDistance = EditorGUILayout.FloatField("CameraBound Radius", stagePointData._maxLimitedDistance);
            stagePointData._cameraZoomSize = EditorGUILayout.FloatField("ZoomSize", stagePointData._cameraZoomSize);
            stagePointData._cameraZoomSpeed = EditorGUILayout.FloatField("ZoomSpeed", stagePointData._cameraZoomSpeed);
            stagePointData._lerpCameraZoom = EditorGUILayout.Toggle("LerpToNextZoom", stagePointData._lerpCameraZoom);
            stagePointData._lockCameraInBound = EditorGUILayout.Toggle("Camera Bound Lock", stagePointData._lockCameraInBound);
        }
        else if(_editStageData is MiniStageData)
        {
            MiniStageData miniStageData = _editStageData as MiniStageData;
            GUILayout.BeginVertical("box");
            GUILayout.Label("Trigger");
            bool isChanged = false;
            SearchIdentifier searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", miniStageData._targetSearchIdentifier);
            float triggerWidth = EditorGUILayout.FloatField("Width", miniStageData._triggerWidth);
            float triggerHeight = EditorGUILayout.FloatField("Height", miniStageData._triggerHeight);
            Vector3 triggerOffset = EditorGUILayout.Vector3Field("Offset", miniStageData._triggerOffset);

            isChanged |= searchIdentifier != miniStageData._targetSearchIdentifier;
            isChanged |= triggerWidth != miniStageData._triggerWidth;
            isChanged |= triggerHeight != miniStageData._triggerHeight;
            isChanged |= triggerOffset != miniStageData._triggerOffset;
            GUILayout.EndVertical();

            if(isChanged)
            {
                miniStageData._targetSearchIdentifier = searchIdentifier;
                miniStageData._triggerWidth = triggerWidth;
                miniStageData._triggerHeight = triggerHeight;
                miniStageData._triggerOffset = triggerOffset;
                SceneView.RepaintAll();
            }
        }
        
        GUILayout.Space(10f);
        bool useTriggerBound = EditorGUILayout.Toggle("Use Trigger Bound", stagePointData._useTriggerBound);
        if(stagePointData._useTriggerBound != useTriggerBound)
        {
            stagePointData._useTriggerBound = useTriggerBound;
            SceneView.RepaintAll();
        }

        if(stagePointData._useTriggerBound)
        {
            bool isChanged = false;
            GUILayout.BeginVertical("box");
            SearchIdentifier searchIdentifier = (SearchIdentifier)EditorGUILayout.EnumPopup("Search Identifier", stagePointData._targetSearchIdentifier);
            float triggerWidth = EditorGUILayout.FloatField("Width", stagePointData._triggerWidth);
            float triggerHeight = EditorGUILayout.FloatField("Height", stagePointData._triggerHeight);
            Vector3 triggerOffset = EditorGUILayout.Vector3Field("Offset", stagePointData._triggerOffset);

            GUILayout.EndVertical();

            isChanged |= searchIdentifier != stagePointData._targetSearchIdentifier;
            isChanged |= triggerWidth != stagePointData._triggerWidth;
            isChanged |= triggerHeight != stagePointData._triggerHeight;
            isChanged |= triggerOffset != stagePointData._triggerOffset;

            if(isChanged)
            {
                stagePointData._targetSearchIdentifier = searchIdentifier;
                stagePointData._triggerWidth = triggerWidth;
                stagePointData._triggerHeight = triggerHeight;
                stagePointData._triggerOffset = triggerOffset;
                SceneView.RepaintAll();
            }
        }

        GUILayout.Space(10f);
        _onEnterSequencerPathEditor.draw(ref stagePointData._onEnterSequencerPath);
        //_onExitSequencerPathEditor.draw(ref stagePointData._onExitSequencerPath);

        // if(stagePointDataEditObject._onEnterSequencerPathProperty == null || stagePointDataEditObject._onExitSequencerPathProperty == null)
        // {
        //     if(_stageDataListProperty != null && _stageDataListProperty.hasChildren && _stageDataListProperty.arraySize > _pointSelectedIndex)
        //     {
        //         SerializedProperty stagePointDataProperty = _stageDataListProperty.GetArrayElementAtIndex(_pointSelectedIndex);
        //         stagePointDataEditObject._onEnterSequencerPathProperty = stagePointDataProperty.FindPropertyRelative("_onEnterSequencerPath");
        //         stagePointDataEditObject._onExitSequencerPathProperty = stagePointDataProperty.FindPropertyRelative("_onExitSequencerPath");
        //     }
        // }
        // else
        // {
        //     EditorGUILayout.PropertyField(stagePointDataEditObject._onEnterSequencerPathProperty);
        //     EditorGUILayout.PropertyField(stagePointDataEditObject._onExitSequencerPathProperty);
        // }
    }

    private void onPointGUI()
    {
        GUILayout.BeginHorizontal();

            if(_editStageData._isMiniStage)
            {
                MiniStageData miniStageData = _editStageData as MiniStageData;
                GUI.enabled = miniStageData._stagePointData.Count < 1;
            }

            if(GUILayout.Button("Add Point"))
            {
                addStagePoint();
            }

            GUI.enabled = GUI.enabled ? _pointSelectedIndex >= 0 && _pointSelectedIndex < _editingStagePointList.Count - 1 : false;
            if(GUILayout.Button("Insert Point Next"))
            {
                insertNextStagePoint(_pointSelectedIndex);
            }
            GUI.enabled = true;

        GUILayout.EndHorizontal();

        _pointItemScroll = GUILayout.BeginScrollView(_pointItemScroll,"box");
            for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
            {
                GUILayout.BeginHorizontal();

                Color currentColor = GUI.color;
                GUI.color = i == _pointSelectedIndex ? Color.green : currentColor;

                GUI.enabled = i > 0;

                bool selected = i == _pointSelectedIndex;
                if(selected)
                    GUILayout.BeginHorizontal("box");

                if(GUILayout.Button("▲",GUILayout.Width(25f)))
                {
                    StagePointData temp = _editStageData._stagePointData[i];
                    _editStageData._stagePointData[i] = _editStageData._stagePointData[i - 1];
                    _editStageData._stagePointData[i - 1] = temp;

                    StagePointDataEditObject temp2 = _editingStagePointList[i];
                    _editingStagePointList[i] = _editingStagePointList[i - 1];
                    _editingStagePointList[i - 1] = temp2;

                    _pointSelectedIndex = i - 1;
                    SceneView.RepaintAll();
                }

                GUI.enabled = i < _editStageData._stagePointData.Count - 1;
                if(GUILayout.Button("▼",GUILayout.Width(25f)))
                {
                    StagePointData temp = _editStageData._stagePointData[i];
                    _editStageData._stagePointData[i] = _editStageData._stagePointData[i + 1];
                    _editStageData._stagePointData[i + 1] = temp;

                    StagePointDataEditObject temp2 = _editingStagePointList[i];
                    _editingStagePointList[i] = _editingStagePointList[i + 1];
                    _editingStagePointList[i + 1] = temp2;

                    _pointSelectedIndex = i + 1;
                    SceneView.RepaintAll();
                }

                GUI.enabled = true;
                GUILayout.Label(i + ". " + _editStageData._stagePointData[i]._pointName,GUILayout.Width(150f));

                if(GUILayout.Button("Pick"))
                    selectPoint(i);

                GUI.color = new Color(1f,0.2f,0.2f);
                if(GUILayout.Button("Delete Point"))
                {
                    deleteStagePoint(i);
                    if(i == _pointSelectedIndex)
                        _pointSelectedIndex = -1;
                }

                GUI.color = currentColor;

                if(selected)
                    GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
            }
        GUILayout.EndScrollView();
    }

    private void addCharacterToPoint(int index, string characterKey)
    {
        if(_editStageData._stagePointData.Count <= index || index < 0)
            return;

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        if(characterInfo.ContainsKey(characterKey) == false)
        {
            DebugUtil.assert(false,"말이 안되는 상황");
            return;
        }

        StagePointData stagePointData = _editStageData._stagePointData[index];
        StagePointDataEditObject stagePointDataEdit = _editingStagePointList[index];
        
        StagePointCharacterSpawnData spawnData = new StagePointCharacterSpawnData();
        spawnData._characterKey = characterKey;
        spawnData._flip = true;
        spawnData._localPosition = Vector3.zero;
        spawnData._searchIdentifier = characterInfo[characterKey]._searchIdentifer;

        SpriteRenderer characterEditItem = getCharacterItem();
        characterEditItem.sprite = getFirstActionSpriteFromCharacter(characterInfo[characterKey]);
        characterEditItem.sortingLayerName = "Character";
        characterEditItem.sortingOrder = 10;
        characterEditItem.transform.position = stagePointData._stagePoint;

        stagePointDataEdit._characterSpawnDataList.Add(spawnData);
        stagePointDataEdit._characterObjectList.Add(characterEditItem);
        stagePointData._characterSpawnData = stagePointDataEdit._characterSpawnDataList.ToArray();

        _characterSpawnScroll.y = float.MaxValue;
        selectCharacter(_pointSelectedIndex, stagePointDataEdit._characterSpawnDataList.Count - 1);
    }

    private void addMiniStageToStage( MiniStageData miniStageData)
    {
        MiniStageListItem listItem = new MiniStageListItem();
        listItem._localStagePosition = Vector3.zero;
        listItem._data = miniStageData;
        
        listItem._overrideTargetSearchIdentifier = miniStageData._targetSearchIdentifier;
        listItem._overrideTriggerWidth = miniStageData._triggerWidth;
        listItem._overrideTriggerHeight = miniStageData._triggerHeight;
        listItem._overrideTriggerOffset = miniStageData._triggerOffset;

        MiniStageDataEditObject editObject = new MiniStageDataEditObject();
        editObject._miniStageData = listItem;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = _editStageData._stagePointData[0]._stagePoint;

        _editingMiniStageDataList.Add(editObject);
        _editStageData._miniStageData.Add(listItem);

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        for(int index = 0; index < miniStageData._stagePointData[0]._characterSpawnData.Length; ++index)
        {
            var spawnData = miniStageData._stagePointData[0]._characterSpawnData[index];
            SpriteRenderer characterEditItem = getCharacterItem();
            characterEditItem.sprite = getActionSpriteFromCharacter(characterInfo[spawnData._characterKey],spawnData._startAction);
            characterEditItem.sortingLayerName = "Character";
            characterEditItem.sortingOrder = 10;
            characterEditItem.transform.position = editObject._gizmoItem.transform.position + spawnData._localPosition;
            characterEditItem.flipX = spawnData._flip;
    
            editObject._characterObjectList.Add(characterEditItem);
        }

        _miniStageScroll.y = float.MaxValue;
        selectMiniStage(_editingMiniStageDataList.Count - 1);
    }

    void Update() 
    {
        if(_editStageData == null)
            return;

        if(Application.isPlaying)
            return;

        if((_editStageData._stagePointData.Count != 0 && _editingStagePointList.Count == 0) ||
            (_editStageData._miniStageData.Count != 0 && _editingMiniStageDataList.Count == 0) ||
            (_editStageData._markerData.Count != 0 && _editingMarkerDataList.Count == 0))
            constructGizmoPoints();

        bool repaint = false;
        for(int i = 0; i < _editingStagePointList.Count; ++i)
        {
            if(_editingStagePointList[i]._gizmoItem == null)
            {
                deleteStagePoint(i);
                --i;
                continue;
            }

            if(i == 0)
                _editingStagePointList[i]._gizmoItem.transform.position = _editingStagePointList[i]._stagePointData._stagePoint;

            repaint |= _editingStagePointList[i].syncPosition(_editStageData._isMiniStage);

            if(i == 0 && _backgroundPrefabObject != null)
                _backgroundPrefabObject.transform.position = _editingStagePointList[i]._stagePointData._stagePoint;
        }

        if(_editStageData._stagePointData.Count != 0)
        {
            for(int i = 0; i < _editingMiniStageDataList.Count; ++i)
            {
                if(_editingMiniStageDataList[i]._gizmoItem == null)
                {
                    deleteMiniStage(i);
                    --i;
                    continue;
                }

                repaint |= _editingMiniStageDataList[i].syncPosition(_editStageData._stagePointData[0]._stagePoint);
            }
        }

        if(_editStageData._markerData.Count != 0)
        {
            for(int i = 0; i < _editingMarkerDataList.Count; ++i)
            {
                if(_editingMarkerDataList[i]._gizmoItem == null)
                {
                    deleteMarker(i);
                    --i;
                    continue;
                }

                repaint |= _editingMarkerDataList[i].syncPosition();
            }
        }

        if(_editStageData._trackData.Count != 0)
        {
            for(int i = 0; i < _editingTrackDataList.Count; ++i)
            {
                if(_editingTrackDataList[i]._selectedPointGizmo == null)
                {
                    deleteTrack(i);
                    --i;
                    continue;
                }

                repaint |= _editingTrackDataList[i].syncPosition();
            }
        }

        if(repaint)
            Repaint();

        float deltaTime = (float)EditorApplication.timeSinceStartup - _prevTime;
        Vector2 trackPosition;
        if(_trackProcessor.isEnd() == false && _trackProcessor.processTrack(deltaTime,out trackPosition))
        {
            GameObject gizmoHelper = GameObject.FindGameObjectWithTag("GizmoHelper");
            if(gizmoHelper != null)
            {
                gizmoHelper.GetComponent<GizmoHelper>().drawCircle(trackPosition,0.1f,6,Color.magenta);
                SceneView.RepaintAll();
            }
        }

        _prevTime = (float)EditorApplication.timeSinceStartup;
    }

    public void saveCurrentData()
    {
        if(_editStageData == null)
            return;

        roundPixelPerfect();

        foreach(var item in _editingTrackDataList)
        {
            item._trackData.calculateTrackLength();
        }

        EditorUtility.SetDirty(_editStageData);

        if(_backgroundPrefabObject != null)
        {
            string filePath = "";
            if(AssetDatabase.Contains(_editStageData._backgroundPrefabPath) == false)
            {
                DebugUtil.assert(false, "Background Prefab이 Scene Object입니다. Prefab을 넣어 주세요");
                return;
                // string defaultName = _editStageData._stageName + ".prefab";
                // filePath = EditorUtility.SaveFilePanel(
                //     "Save Stage Background",
                //     "Assets/Resources/Prefab/StageBackground/",
                //     defaultName,
                //     "prefab"
                // );
    
                // if (string.IsNullOrEmpty(filePath)) 
                //     return;
    
                // filePath = FileUtil.GetProjectRelativePath(filePath);
                // AssetDatabase.CreateAsset(_backgroundPrefabObject, filePath);
                // AssetDatabase.SaveAssets();
            }
            else
            {
                filePath = AssetDatabase.GetAssetPath(_editStageData._backgroundPrefabPath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(_backgroundPrefabObject, filePath,InteractionMode.AutomatedAction);
            }
        }

        if(AssetDatabase.Contains(_editStageData) == false)
        {
            string defaultName = _editStageData._stageName + ".asset";
            string path = EditorUtility.SaveFilePanel(
                "Save Stage Data",
                "Assets/Resources/StageData/",
                defaultName,
                "asset"
            );

            if (string.IsNullOrEmpty(path)) 
                return;

            path = FileUtil.GetProjectRelativePath(path);
            AssetDatabase.CreateAsset(_editStageData, path);
            AssetDatabase.SaveAssets();
        }

        AssetDatabase.SaveAssets();
    }

    public void roundPixelPerfect()
    {
        foreach(var item in _editingStagePointList)
        {
            item._stagePointData._stagePoint = MathEx.round(item._stagePointData._stagePoint, 2);
            item._gizmoItem.transform.position = item._stagePointData._stagePoint;

            for(int index = 0; index < item._characterSpawnDataList.Count; ++index)
            {
                item._characterSpawnDataList[index]._localPosition = MathEx.round(item._characterSpawnDataList[index]._localPosition, 2);
                item._characterObjectList[index].transform.position = item._stagePointData._stagePoint + item._characterSpawnDataList[index]._localPosition;
            }
        }

        if(_backgroundPrefabObject != null)
            roundPixelPerfectRecursive(_backgroundPrefabObject.transform);
    }

    public void roundPixelPerfectRecursive(Transform root)
    {
        root.transform.position = MathEx.round(root.transform.position,2);

        for(int index = 0; index < root.childCount; ++index)
        {
            roundPixelPerfectRecursive(root.GetChild(index));
        }
    }

    public void setEditStageData(StageData stageData)
    {
        _editStageData = stageData;
        constructGizmoPoints();
    }

    void OnFocus() 
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;

        if(_editItemParent != null)
            DestroyImmediate(_editItemParent);
    }
 
    private void OnSceneGUI(SceneView sceneView) 
    {
        if(_editStageData == null)
            return;

        if(Application.isPlaying)
            return;

        editorKeyCheck();

        // if(_drawScreenToMousePoint)
        // {
        //     Vector3 mousePosition = Event.current.mousePosition;
        //     mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        //     mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
        //     mousePosition.z = 0f;

        //     float mainCamSize = Camera.main.orthographicSize;
        //     float camHeight = mainCamSize * 2f;
		//     float camWidth = camHeight * ((float)800f / (float)600f);

        //     Rect rectangle = new Rect();
        //     rectangle.Set(mousePosition.x - (camWidth * 0.5f),mousePosition.y - (camHeight * 0.5f),camWidth,camHeight);
        //     Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,0f,0f,0f),Color.blue);
        // }

        if(_editStageData._isMiniStage)
        {
            MiniStageData miniStageData = _editStageData as MiniStageData;

            Rect rectangle = new Rect();
            rectangle.Set(miniStageData._triggerOffset.x - (miniStageData._triggerWidth * 0.5f),miniStageData._triggerOffset.y - (miniStageData._triggerHeight * 0.5f),miniStageData._triggerWidth,miniStageData._triggerHeight);
            Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,1f,0f,0.05f),Color.green);
        }

        for(int i = 0; i < _editStageData._stagePointData.Count; ++i)
        {
            StagePointData stagePointData = _editStageData._stagePointData[i];
            if(stagePointData == null)
                continue;

            Vector3 itemPosition = stagePointData._stagePoint;
            Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
            };

            Color currentColor = Handles.color;
            
            if(stagePointData._characterSpawnData != null)
            {
                for(int index = 0; index < stagePointData._characterSpawnData.Length; ++index)
                {
                    Vector3 characterWorld = stagePointData._stagePoint + stagePointData._characterSpawnData[index]._localPosition;

                    Handles.color = i == _pointSelectedIndex ? Color.green : Color.gray;
                    Handles.DrawLine(stagePointData._stagePoint,characterWorld);

                    characterWorld += Vector3.right * 0.1f;
                    if(stagePointData._characterSpawnData[index]._uniqueKey != "")
                        Handles.Label(characterWorld,"Unique Key: " + stagePointData._characterSpawnData[index]._uniqueKey);
                    
                    if(stagePointData._characterSpawnData[index]._uniqueGroupKey != "")
                        Handles.Label(characterWorld + Vector3.down * 0.1f,"Unique Group Key: " + stagePointData._characterSpawnData[index]._uniqueGroupKey);

                    Handles.color = currentColor;
                    Handles.Label(characterWorld + Vector3.down * 0.2f,stagePointData._characterSpawnData[index]._activeType.ToString());
                }
            }
            
            Handles.color = i == _pointSelectedIndex ? Color.green : currentColor;

            // if(stagePointData._maxLimitedDistance > 0f)
            //     drawCircleWithHandle(stagePointData._stagePoint,stagePointData._maxLimitedDistance);

            Handles.Label(stagePointData._stagePoint, i.ToString());
            if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                selectPoint(i);

            Handles.color = currentColor;
            if(_drawScreenToMousePoint || (i == _pointSelectedIndex && _editStageData._isMiniStage == false))
            {
                drawInGameScreenSection(stagePointData._stagePoint,stagePointData._cameraZoomSize,stagePointData._maxLimitedDistance);
                if(stagePointData._maxLimitedDistance > 0f)
                    drawInGameScreenSection(stagePointData._stagePoint,stagePointData._cameraZoomSize,0f);
            }
            
            if( ( i == _pointSelectedIndex || _drawTriggerBound ) && stagePointData._useTriggerBound)
            {
                Rect rectangle = new Rect();
                Vector3 centerPosition = stagePointData._stagePoint + stagePointData._triggerOffset;
                rectangle.Set(centerPosition.x - (stagePointData._triggerWidth * 0.5f),centerPosition.y - (stagePointData._triggerHeight * 0.5f),stagePointData._triggerWidth,stagePointData._triggerHeight);
                Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,1f,0f,0.05f),Color.green);
            }

            if(_drawScreenToMousePoint && i > 0)
                drawScreenSectionConnectLine(
                    _editStageData._stagePointData[i - 1]._stagePoint,_editStageData._stagePointData[i - 1]._cameraZoomSize,_editStageData._stagePointData[i - 1]._maxLimitedDistance,
                    stagePointData._stagePoint,stagePointData._cameraZoomSize,stagePointData._maxLimitedDistance);

            if(i < _editStageData._stagePointData.Count - 1 )
            {
                Color arrowColor = i == _pointSelectedIndex ? Color.green : currentColor;
                drawArrow(stagePointData._stagePoint, _editStageData._stagePointData[i + 1]._stagePoint, 0.3f, arrowColor);
            }
        }

        for(int i = 0; i < _editStageData._markerData.Count; ++i)
        {
            MarkerItem markerItem = _editStageData._markerData[i];
            if(markerItem == null)
                continue;

            Vector3 itemPosition = markerItem._position;
            Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                Handles.CircleHandleCap(controlID, position, rotation, size, eventType);
            };

            Color currentColor = Handles.color;
            
            Handles.color = i == _markerSelectedIndex ? Color.green : currentColor;

            Handles.Label(itemPosition, "Marker: " + markerItem._name);
            if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                selectMarker(i);
            
            Handles.color = currentColor;
        }

        if(_editItemMenuSelectedIndex == 4)
        {
            for(int i = 0; i < _editStageData._trackData.Count; ++i)
            {
                if(_trackSelectedIndex != i)
                    continue;

                MovementTrackData trackData = _editStageData._trackData[_trackSelectedIndex];
                if(trackData == null)
                    continue;

                for(int index = 0; index < trackData._trackPointData.Count; ++index)
                {
                    MovementTrackPointData pointData = trackData._trackPointData[index];
                    Vector3 itemPosition = pointData._point;
                    Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                        Handles.CircleHandleCap(controlID, position, rotation, size, eventType);
                    };

                    Color currentColor = Handles.color;

                    if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                        selectTrackPoint(index, MovementTrackDataEditObject.SelectInfo.Point);

                    if(pointData._isLinearPath == false)
                    {
                        Handles.color = Color.red;
                        Handles.DrawLine(itemPosition, pointData._bezierPoint);
                        if(Handles.Button(pointData._bezierPoint,Camera.current.transform.rotation,0.05f,0.1f,capFunction))
                            selectTrackPoint(index, MovementTrackDataEditObject.SelectInfo.BezierPoint);

                        Handles.color = Color.blue;
                        Handles.DrawLine(itemPosition, pointData.getInverseBezierPoint());
                        if(Handles.Button(pointData.getInverseBezierPoint(),Camera.current.transform.rotation,0.05f,0.1f,capFunction))
                            selectTrackPoint(index, MovementTrackDataEditObject.SelectInfo.BezierPointInv);
                    }
                    
                    Handles.color = currentColor;

                    if(index == 0)
                    {
                        Handles.Label(itemPosition, "Track: " + trackData._name);
                    }

                    if(index < trackData._trackPointData.Count - 1)
                    {
                        MovementTrackPointData pointData2 = trackData._trackPointData[index + 1];
                        if(pointData._isLinearPath == false)
                        {
                            float sample = 0f;
                            float sampleRate = 0.05f;
                            for(int loop = 0; loop < 20; ++loop)
                            {
                                Vector2 startPoint = pointData._point;
                                Vector2 endPoint = pointData2._point;
                                Vector2 bezierPoint0 = pointData._bezierPoint;
                                Vector2 bezierPoint1 = pointData2.getInverseBezierPoint();

                                Vector2 first = MathEx.getPointOnBezierCurve(startPoint, bezierPoint0, bezierPoint1, endPoint, sample);
                                Vector2 second = MathEx.getPointOnBezierCurve(startPoint, bezierPoint0, bezierPoint1, endPoint, sample + sampleRate);

                                Handles.DrawLine(first, second);
                                sample += sampleRate;
                            }
                        }
                        else
                        {
                            Handles.DrawLine(pointData._point, pointData2._point);
                        }
                        
                    }
                }
            }
        }
        

        if(_editStageData._stagePointData.Count != 0)
        {
            for(int i = 0; i < _editStageData._miniStageData.Count; ++i)
            {
                MiniStageListItem miniStageListItem = _editStageData._miniStageData[i];
                MiniStageData miniStageData = miniStageListItem._data;
                Vector3 itemPosition = miniStageListItem._localStagePosition + _editStageData._stagePointData[0]._stagePoint;
                Handles.CapFunction capFunction = (controlID, position, rotation, size, eventType)=>{
                    Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
                };

                if(Handles.Button(itemPosition,Camera.current.transform.rotation,0.1f,0.2f,capFunction))
                {
                    _editMenuSelectedIndex = 2;
                    selectMiniStage(i);
                }

                Rect rectangle = new Rect();
                Vector3 centerPosition = itemPosition + miniStageListItem._overrideTriggerOffset;
                rectangle.Set(centerPosition.x - (miniStageListItem._overrideTriggerWidth * 0.5f),centerPosition.y - (miniStageListItem._overrideTriggerHeight * 0.5f),miniStageListItem._overrideTriggerWidth,miniStageListItem._overrideTriggerHeight);
                Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,1f,0f,0.05f),Color.green);
            }
        }
    }

    public void drawArrow(Vector3 start, Vector3 end, float arrowLength, Color color)
    {
        Vector3 direction = (end - start).normalized;
        Vector3 arrowUp = new Vector3(-1f,1f).normalized * arrowLength;
        Vector3 arrowDown = new Vector3(-1f,-1f).normalized * arrowLength;

        float angle = Vector3.SignedAngle(Vector3.right, direction,Vector3.forward);
        Quaternion rotate = Quaternion.Euler(0f,0f,angle);
        arrowUp = rotate * arrowUp;
        arrowDown = rotate * arrowDown;

        Color beforeColor = Handles.color;
        Handles.color = color;

        Handles.DrawLine(start, end,2f);
        Handles.DrawLine(end, end + arrowUp,2f);
        Handles.DrawLine(end, end + arrowDown,2f);

        Handles.color = beforeColor;
    }

    private void selectPoint(int index)
    {
        if(_editingStagePointList.Count <= index || index < 0)
            return;

        PingTarget(_editingStagePointList[index]._gizmoItem);
        Repaint();

        _pointSelectedIndex = index;
        _characterSelectedIndex = -1;
    }

    private void selectCharacter(int pointIndex, int characterIndex)
    {
        if(_editingStagePointList.Count <= pointIndex || pointIndex < 0)
            return;
        
        if(_editingStagePointList[pointIndex]._characterObjectList.Count <= characterIndex || characterIndex < 0)
            return;

        PingTarget(_editingStagePointList[pointIndex]._characterObjectList[characterIndex].gameObject);
        Repaint();

        _characterSelectedIndex = characterIndex;
    }

    private void selectMiniStage(int miniStageIndex)
    {
        if(_editingMiniStageDataList.Count <= miniStageIndex || miniStageIndex < 0)
            return;

        PingTarget(_editingMiniStageDataList[miniStageIndex]._gizmoItem);
        _miniStageSelectedIndex = miniStageIndex;
    }

    private void selectMarker(int markerIndex)
    {
        if(_editingMarkerDataList.Count <= markerIndex || markerIndex < 0)
            return;
            
        PingTarget(_editingMarkerDataList[markerIndex]._gizmoItem);
        _markerSelectedIndex = markerIndex;
    }

    private void selectTrack(int trackIndex)
    {
        if(_editingTrackDataList.Count <= trackIndex || trackIndex < 0)
            return;
            
        PingTarget(_editingTrackDataList[trackIndex]._selectedPointGizmo);
        _trackSelectedIndex = trackIndex;
    }

    private void PingTarget(GameObject obj)
    {
        EditorGUIUtility.PingObject(obj);
        Selection.activeGameObject = obj;
    }

    private void addStagePoint()
    {
        Vector3 spawnPosition = Vector3.zero;
        if(_editingStagePointList.Count == 1)
        {
            spawnPosition = _editingStagePointList[0]._stagePointData._stagePoint + Vector3.right;
        }
        else if(_editingStagePointList.Count > 1)
        {
            spawnPosition = _editingStagePointList[_editingStagePointList.Count - 1]._gizmoItem.transform.position;
            spawnPosition += (_editingStagePointList[_editingStagePointList.Count - 1]._gizmoItem.transform.position - _editingStagePointList[_editingStagePointList.Count - 2]._gizmoItem.transform.position).normalized;
        }

        StagePointDataEditObject editObject = new StagePointDataEditObject();
        StagePointData stagePointData = new StagePointData(spawnPosition);
        stagePointData._cameraZoomSize = Camera.main.orthographicSize;
        stagePointData._pointName = "New Point " + _editingStagePointList.Count;


        
        _editStageData._stagePointData.Add(stagePointData);

        editObject._stagePointData = stagePointData;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = stagePointData._stagePoint;

        _editingStagePointList.Add(editObject);

        _pointItemScroll.y = float.MaxValue;
        selectPoint(_editStageData._stagePointData.Count - 1);

        EditorUtility.SetDirty(_editStageData);
    }

    private void addMarker()
    {
        Vector3 spawnPosition = SceneView.lastActiveSceneView.camera.transform.position;
        spawnPosition.z = 0f;

        MarkerDataEditObject editObject = new MarkerDataEditObject();
        MarkerItem markerItem = new MarkerItem();
        markerItem._name = "New Marker " + _editingMarkerDataList.Count;
        markerItem._position = spawnPosition;
        
        editObject._markerData = markerItem;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = spawnPosition;

        _editStageData._markerData.Add(markerItem);
        _editingMarkerDataList.Add(editObject);

        _markerScroll.y = float.MaxValue;
        selectMarker(_editingMarkerDataList.Count - 1);

        EditorUtility.SetDirty(_editStageData);
    }

    private void addTrack()
    {
        Vector3 spawnPosition = SceneView.lastActiveSceneView.camera.transform.position;
        spawnPosition.z = 0f;

        MovementTrackDataEditObject editObject = new MovementTrackDataEditObject();
        MovementTrackData trackData = new MovementTrackData();
        trackData._name = "New Track " + _editingTrackDataList.Count;

        MovementTrackPointData pointData = new MovementTrackPointData();
        pointData._point = spawnPosition;
        pointData._bezierPoint = pointData._point + Vector2.right;

        MovementTrackPointData pointData2 = new MovementTrackPointData();
        pointData2._point = spawnPosition + Vector3.right * 3f;
        pointData2._bezierPoint = pointData2._point + Vector2.right;
        
        trackData._trackPointData.Add(pointData);
        trackData._trackPointData.Add(pointData2);

        editObject._trackData = trackData;
        editObject._selectedPointGizmo = getGizmoItem();
        editObject._selectedPointGizmo.transform.position = spawnPosition;

        _editStageData._trackData.Add(trackData);
        _editingTrackDataList.Add(editObject);

        _trackScroll.y = float.MaxValue;
        selectTrack(_editingMarkerDataList.Count - 1);
        selectTrackPoint(0, MovementTrackDataEditObject.SelectInfo.Point);

        EditorUtility.SetDirty(_editStageData);
    }

    private void addTrackPoint()
    {
        if(_editingTrackDataList.Count <= _trackSelectedIndex || _trackSelectedIndex < 0)
            return;
        int pointCount = _editingTrackDataList[_trackSelectedIndex]._trackData._trackPointData.Count;
        MovementTrackPointData pointData = new MovementTrackPointData();
        pointData._point = pointCount == 0 ? (Vector2)SceneView.lastActiveSceneView.camera.transform.position : _editingTrackDataList[_trackSelectedIndex]._trackData._trackPointData[pointCount - 1]._point + Vector2.right * 3f;
        pointData._bezierPoint = pointData._point + Vector2.right;

        _editingTrackDataList[_trackSelectedIndex]._trackData._trackPointData.Add(pointData);
        selectTrackPoint(pointCount, MovementTrackDataEditObject.SelectInfo.Point);
    }

    private void selectTrackPoint(int index, MovementTrackDataEditObject.SelectInfo selectInfo)
    {
        if(_editingTrackDataList.Count <= _trackSelectedIndex || _trackSelectedIndex < 0)
            return;

        if(_editingTrackDataList[_trackSelectedIndex]._trackData._trackPointData.Count <= index || index < 0)
            return;

        _editingTrackDataList[_trackSelectedIndex]._pointSelectinfo = selectInfo;
        _editingTrackDataList[_trackSelectedIndex]._selectedPointIndex = index;

        _editingTrackDataList[_trackSelectedIndex].syncGizmoPosition();
        PingTarget(_editingTrackDataList[_trackSelectedIndex]._selectedPointGizmo);
    }

    private void insertNextStagePoint(int index)
    {
        if(index < 0 && _editingStagePointList.Count - 1 >= index)
            return;

        Vector3 spawnPosition = Vector3.zero;
        if(index == _editingStagePointList.Count)
        {
            spawnPosition = _editingStagePointList[index]._gizmoItem.transform.position;
            spawnPosition += (_editingStagePointList[index]._gizmoItem.transform.position - _editingStagePointList[index - 1]._gizmoItem.transform.position).normalized;
        }
        else
        {
            spawnPosition = _editingStagePointList[index]._gizmoItem.transform.position;
            spawnPosition += (_editingStagePointList[index + 1]._gizmoItem.transform.position - _editingStagePointList[index]._gizmoItem.transform.position) * 0.5f;
        }

        StagePointDataEditObject editObject = new StagePointDataEditObject();
        StagePointData stagePointData = new StagePointData(spawnPosition);
        stagePointData._cameraZoomSize = Camera.main.orthographicSize;
        stagePointData._pointName = "New Point " + (index + 1);

        _editStageData._stagePointData.Insert(index + 1, stagePointData);

        editObject._stagePointData = stagePointData;
        editObject._gizmoItem = getGizmoItem();
        editObject._gizmoItem.transform.position = stagePointData._stagePoint;

        _editingStagePointList.Insert(index + 1, editObject);

        selectPoint(index + 1);

        EditorUtility.SetDirty(_editStageData);
    }

    private void deleteStagePoint(int index)
    {
        if(index < 0 || _editStageData._stagePointData.Count <= index)
            return;

        _editStageData._stagePointData.RemoveAt(index);

        for(int characterIndex = 0; characterIndex < _editingStagePointList[index]._characterObjectList.Count; ++characterIndex)
        {
            SpriteRenderer characterItem = _editingStagePointList[index]._characterObjectList[characterIndex];
            returnCharacterItem(characterItem);
        }

        returnGizmoItem(_editingStagePointList[index]._gizmoItem);
        _editingStagePointList.RemoveAt(index);
    }

    private void deleteCharacter(int pointIndex, int characterIndex)
    {
        if(_editStageData._stagePointData.Count <= pointIndex || pointIndex < 0 ||
            _editStageData._stagePointData[pointIndex]._characterSpawnData.Length <= characterIndex || characterIndex < 0)
            return;

        _editingStagePointList[pointIndex]._characterSpawnDataList.RemoveAt(characterIndex);
        _editStageData._stagePointData[pointIndex]._characterSpawnData = _editingStagePointList[pointIndex]._characterSpawnDataList.ToArray();

        returnCharacterItem(_editingStagePointList[pointIndex]._characterObjectList[characterIndex]);
        _editingStagePointList[pointIndex]._characterObjectList.RemoveAt(characterIndex);
    }

    private void deleteMiniStage(int miniStageIndex)
    {
        if(_editStageData._miniStageData.Count <= miniStageIndex || miniStageIndex < 0 )
            return;

        _editStageData._miniStageData.RemoveAt(miniStageIndex);

        for(int characterIndex = 0; characterIndex < _editingMiniStageDataList[miniStageIndex]._characterObjectList.Count; ++characterIndex)
        {
            SpriteRenderer characterItem = _editingMiniStageDataList[miniStageIndex]._characterObjectList[characterIndex];
            returnCharacterItem(characterItem);
        }

        returnGizmoItem(_editingMiniStageDataList[miniStageIndex]._gizmoItem);
        _editingMiniStageDataList.RemoveAt(miniStageIndex);
    }

    private void deleteMarker(int markerIndex)
    {
        if(_editStageData._markerData.Count <= markerIndex || markerIndex < 0 )
            return;

        _editStageData._markerData.RemoveAt(markerIndex);

        returnGizmoItem(_editingMarkerDataList[markerIndex]._gizmoItem);
        _editingMarkerDataList.RemoveAt(markerIndex);
    }

    private void deleteTrack(int trackIndex)
    {
        if(_editStageData._trackData.Count <= trackIndex || trackIndex < 0 )
            return;

        _editStageData._trackData.RemoveAt(trackIndex);

        returnGizmoItem(_editingTrackDataList[trackIndex]._selectedPointGizmo);
        _editingTrackDataList.RemoveAt(trackIndex);
    }

    private void deleteTrackPoint(int trackIndex)
    {
        if(_editStageData._trackData[_trackSelectedIndex]._trackPointData.Count <= trackIndex || trackIndex < 0 )
            return;

        _editStageData._trackData[_trackSelectedIndex]._trackPointData.RemoveAt(trackIndex);
    }

    private void loadStageData()
    {
        _pointSelectedIndex = 0;
        _characterSelectedIndex = 0;
        _miniStageSelectedIndex = 0;

        _editMenuSelectedIndex = 0;
        _editItemMenuSelectedIndex = 0;

        if(_editStageData == null)
            return;

        if (_tilemapSettingPrefabObject != null)
            DestroyImmediate(_tilemapSettingPrefabObject);

        if (_editStageData._tilemapConfigPath != null) {
            GameObject tileSettingPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/TilemapSettings.prefab", typeof(GameObject)) as GameObject;
            _tilemapSettingPrefabObject = Instantiate(tileSettingPrefab, _editItemParent.transform);

            Tilemap tilemap = _tilemapSettingPrefabObject.GetComponentInChildren<Tilemap>();
            TilemapConfig tilemapConfig = _editStageData._tilemapConfigPath;
            tilemap.SetTiles(tilemapConfig._positions, tilemapConfig._tileBases);
        }

        if(_backgroundPrefabObject != null)
            DestroyImmediate(_backgroundPrefabObject);
        
        if(_editStageData._backgroundPrefabPath != null)
        {
            _backgroundPrefabObject = Instantiate(_editStageData._backgroundPrefabPath);
            _backgroundPrefabObject.transform.position = _editStageData._stagePointData.Count == 0 ? Vector3.zero : _editStageData._stagePointData[0]._stagePoint;
            _backgroundPrefabObject.transform.SetParent(_editItemParent.transform);
        }

        for(int index = 0; index < _editStageData._stagePointData.Count; ++index)
        {
            if(_editStageData._stagePointData[index]._pointName == "")
                _editStageData._stagePointData[index]._pointName = "New Point " + index;
        }

        _stageDataSerializedObject = new SerializedObject(_editStageData);
        _stageDataListProperty = _stageDataSerializedObject.FindProperty("_stagePointData");
    }

    private void constructGizmoPoints()
    {
        clearStagePointList();

        if(_editStageData == null)
        {
            if (_tilemapSettingPrefabObject != null)
                DestroyImmediate(_tilemapSettingPrefabObject);
            if(_backgroundPrefabObject != null)
                DestroyImmediate(_backgroundPrefabObject);
            return;
        }

        var characterInfo = ResourceContainerEx.Instance().getCharacterInfo("Assets\\Data\\StaticData\\CharacterInfo.xml");
        
        foreach(var item in _editStageData._stagePointData)
        {
            StagePointDataEditObject editObject = new StagePointDataEditObject();
            editObject._stagePointData = item;
            editObject._gizmoItem = getGizmoItem();
            editObject._gizmoItem.transform.position = item._stagePoint;

            editObject._characterSpawnDataList = new List<StagePointCharacterSpawnData>();
            if(item._characterSpawnData != null)
            {
                foreach(var spawnData in item._characterSpawnData)
                {
                    editObject._characterSpawnDataList.Add(spawnData);
                }
            }
            for(int index = 0; index < editObject._characterSpawnDataList.Count; ++index)
            {
                SpriteRenderer characterEditItem = getCharacterItem();
                characterEditItem.sprite = getActionSpriteFromCharacter(characterInfo[editObject._characterSpawnDataList[index]._characterKey],editObject._characterSpawnDataList[index]._startAction);
                characterEditItem.sortingOrder = 10;
                characterEditItem.transform.position = item._stagePoint + editObject._characterSpawnDataList[index]._localPosition;
                characterEditItem.flipX = editObject._characterSpawnDataList[index]._flip;

                editObject._characterObjectList.Add(characterEditItem);
            }

            _editingStagePointList.Add(editObject);
        }

        if(_editStageData._stagePointData.Count != 0)
        {
            foreach(var item in _editStageData._miniStageData)
            {
                MiniStageDataEditObject editObject = new MiniStageDataEditObject();
                editObject._miniStageData = item;
                editObject._gizmoItem = getGizmoItem();
                editObject._gizmoItem.transform.position = _editStageData._stagePointData[0]._stagePoint + item._localStagePosition;
                _editingMiniStageDataList.Add(editObject);

                if(item._data._stagePointData.Count == 0)
                    continue;

                for(int index = 0; index < item._data._stagePointData[0]._characterSpawnData.Length; ++index)
                {
                    var spawnData = item._data._stagePointData[0]._characterSpawnData[index];
                    SpriteRenderer characterEditItem = getCharacterItem();
                    characterEditItem.sprite = getActionSpriteFromCharacter(characterInfo[spawnData._characterKey],spawnData._startAction);
                    characterEditItem.sortingOrder = 10;
                    characterEditItem.transform.position = editObject._gizmoItem.transform.position + spawnData._localPosition;
                    characterEditItem.flipX = spawnData._flip;
    
                    editObject._characterObjectList.Add(characterEditItem);
                }

            }
            
        }

        if(_editStageData._markerData.Count != 0)
        {
            Vector3 offsetPosition = Vector3.zero;
            if(_editStageData._stagePointData.Count != 0)
                offsetPosition = _editStageData._stagePointData[0]._stagePoint;

            foreach(var item in _editStageData._markerData)
            {
                MarkerDataEditObject editObject = new MarkerDataEditObject();
                editObject._markerData = item;
                editObject._gizmoItem = getGizmoItem();
                editObject._gizmoItem.transform.position = item._position;
                _editingMarkerDataList.Add(editObject);
            }
            
        }

        if(_editStageData._trackData.Count != 0)
        {
            Vector3 offsetPosition = Vector3.zero;
            if(_editStageData._stagePointData.Count != 0)
                offsetPosition = _editStageData._stagePointData[0]._stagePoint;

            foreach(var item in _editStageData._trackData)
            {
                MovementTrackDataEditObject editObject = new MovementTrackDataEditObject();
                editObject._trackData = item;
                editObject._selectedPointGizmo = getGizmoItem();
                if(item._trackPointData.Count != 0)
                    editObject._selectedPointGizmo.transform.position = item._trackPointData[0]._point;
                _editingTrackDataList.Add(editObject);
            }
            
        }
    }

    private void clearStagePointList()
    {
        for(int index = 0; index < _editingStagePointList.Count; ++index)
        {
            for(int characterIndex = 0; characterIndex < _editingStagePointList[index]._characterObjectList.Count; ++characterIndex)
            {
                SpriteRenderer characterItem = _editingStagePointList[index]._characterObjectList[characterIndex];
                returnCharacterItem(characterItem);
            }

            returnGizmoItem(_editingStagePointList[index]._gizmoItem);
        }

        for(int index = 0; index < _editingMiniStageDataList.Count; ++index)
        {
            for(int characterIndex = 0; characterIndex < _editingMiniStageDataList[index]._characterObjectList.Count; ++characterIndex)
            {
                SpriteRenderer characterItem = _editingMiniStageDataList[index]._characterObjectList[characterIndex];
                returnCharacterItem(characterItem);
            }

            returnGizmoItem(_editingMiniStageDataList[index]._gizmoItem);
        }

        for(int index = 0; index < _editingMarkerDataList.Count; ++index)
        {
            returnGizmoItem(_editingMarkerDataList[index]._gizmoItem);
        }

        _gizmoItemPool.Clear();
        _characterItemPool.Clear();
        _editingStagePointList.Clear();
        _editingMiniStageDataList.Clear();
        _editingMarkerDataList.Clear();
        _editingTrackDataList.Clear();

        if(_editItemParent == null)
            _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");

        if(_editItemParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        Transform gizmoParent = _editItemParent.transform.Find("Editor_Gizmos");
        if(gizmoParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        Transform characterParent = _editItemParent.transform.Find("Editor_Characters");
        if(characterParent == null)
        {
            Debug.LogError("뭔가 잘못됐습니다. 에디터를 다시 켜 주세요");
            _window?.Close();
            return;
        }

        _editItemGizmoParent = gizmoParent.gameObject;
        _editItemCharacterParent = characterParent.gameObject;

        for(int index = 0; index < gizmoParent.childCount; ++index)
        {
            Transform child = gizmoParent.GetChild(index);
            if(child.name.Contains("Editor_") == false)
            {
                child.parent = null;
                DestroyImmediate(child.gameObject);
                --index;
            }
        }

        for(int index = 0; index < _editItemGizmoParent.transform.childCount; ++index)
        {
            _gizmoItemPool.Enqueue(_editItemGizmoParent.transform.GetChild(index).gameObject);
        }

        for(int index = 0; index < _editItemCharacterParent.transform.childCount; ++index)
        {
            _characterItemPool.Enqueue(_editItemCharacterParent.transform.GetChild(index).GetComponent<SpriteRenderer>());
        }
    }

    private void createOrFindEditorItem()
    {
        _editItemParent = GameObject.FindGameObjectWithTag("EditorItem");
        if(_editItemParent != null)
        {
            _gizmoItemPool.Clear();
            Transform gizmoParent = _editItemParent.transform.Find("Editor_Gizmos");
            if(gizmoParent == null)
            {
                gizmoParent = new GameObject("Editor_Gizmos").transform;
                gizmoParent.transform.SetParent(_editItemParent.transform);
            }
            else
            {
                for(int index = 0; index < gizmoParent.childCount; ++index)
                {
                    _gizmoItemPool.Enqueue(gizmoParent.GetChild(index).gameObject);
                }
            }
            _editItemGizmoParent = gizmoParent.gameObject;

            _characterItemPool.Clear();
            Transform characterParent = _editItemParent.transform.Find("Editor_Characters");
            if(characterParent == null)
            {
                characterParent = new GameObject("Editor_Characters").transform;
                characterParent.transform.SetParent(_editItemParent.transform);
            }
            else
            {
                for(int index = 0; index < characterParent.childCount; ++index)
                {
                    _characterItemPool.Enqueue(characterParent.GetChild(index).GetComponent<SpriteRenderer>());
                }
            }
            _editItemCharacterParent = characterParent.gameObject;

            for(int index = 0; index < gizmoParent.childCount; ++index)
            {
                Transform child = gizmoParent.GetChild(index);
                if(child.name.Contains("Editor_") == false)
                {
                    child.parent = null;
                    DestroyImmediate(child.gameObject);
                    --index;
                }
            }

            return;
        }

        _editItemParent = new GameObject("EditorItemXXXXXX");
        _editItemParent.tag = "EditorItem";

        _editItemGizmoParent = new GameObject("Editor_Gizmos");
        _editItemGizmoParent.transform.SetParent(_editItemParent.transform);

        _editItemCharacterParent = new GameObject("Editor_Characters");
        _editItemCharacterParent.transform.SetParent(_editItemParent.transform);
    }

    private GameObject getGizmoItem()
    {
        if(_editItemParent == null || _editItemGizmoParent == null)
            return null;

        GameObject gizmoItem = null;
        if(_gizmoItemPool.Count == 0)
            gizmoItem = new GameObject("GizmoItem");
        else
            gizmoItem = _gizmoItemPool.Dequeue();
        
        gizmoItem.SetActive(true);
        gizmoItem.transform.SetParent(_editItemGizmoParent.transform);

        return gizmoItem;
    }

    private SpriteRenderer getCharacterItem()
    {
        if(_editItemParent == null || _editItemCharacterParent == null)
            return null;
        
        SpriteRenderer characterItem = null;
        if(_characterItemPool.Count == 0)
            characterItem = new GameObject("CharacterItem").AddComponent<SpriteRenderer>();
        else
            characterItem = _characterItemPool.Dequeue();
        
        characterItem.gameObject.SetActive(true);
        characterItem.gameObject.layer = LayerMask.NameToLayer("Character");
        characterItem.transform.SetParent(_editItemCharacterParent.transform);
        characterItem.sortingLayerName = "Character";
        characterItem.sprite = null;
        characterItem.flipX = false;

        return characterItem;
    }

    private void returnGizmoItem(GameObject gizmoItem)
    {
        if(gizmoItem == null)
            return;

        gizmoItem.SetActive(false);
        _gizmoItemPool.Enqueue(gizmoItem);
    }

    private void returnCharacterItem(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.gameObject.SetActive(false);
        _characterItemPool.Enqueue(spriteRenderer);
    }

    private Rect getInGameScreenSection(Vector3 position, float zoomSize, float radius)
    {
        float mainCamSize = zoomSize;
        float camHeight = (mainCamSize) * 2f;
		float camWidth = camHeight * ((float)800f / (float)600f);

        camHeight += radius * 2f;
        camWidth += radius * 2f;

        Rect rectangle = new Rect();
        rectangle.Set(position.x - (camWidth * 0.5f),position.y - (camHeight * 0.5f),camWidth,camHeight);

        return rectangle;
    }

    private enum Side
    {
        LeftTop,
        LeftBottom,
        RightTop,
        RightBottom,
        Count,
    };

    private void drawScreenSectionConnectLine(Vector3 one, float oneZoomSize,float oneRadius, Vector3 two, float twoZoomSize, float twoRadius)
    {
        Side side = Side.Count;
        if(one.x > two.x && one.y > two.y)
            side = Side.RightTop;
        else if(one.x > two.x && one.y < two.y)
            side = Side.RightBottom;
        else if(one.x < two.x && one.y > two.y)
            side = Side.LeftTop;
        else// if(one.x < two.x && one.y < two.y)
            side = Side.LeftBottom;
        
        Rect oneRect = getInGameScreenSection(one,oneZoomSize,oneRadius);
        Rect twoRect = getInGameScreenSection(two,twoZoomSize,twoRadius);

        Color currentColor = Handles.color;
        Handles.color = Color.blue;
        switch(side)
        {
            case Side.LeftTop:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMin),new Vector3(twoRect.xMin,twoRect.yMin));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMax),new Vector3(twoRect.xMax,twoRect.yMax));
            }
            break;
            case Side.LeftBottom:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMax),new Vector3(twoRect.xMin,twoRect.yMax));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMin),new Vector3(twoRect.xMax,twoRect.yMin));
            }
            break;
            case Side.RightTop:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMax),new Vector3(twoRect.xMin,twoRect.yMax));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMin),new Vector3(twoRect.xMax,twoRect.yMin));
            }
            break;
            case Side.RightBottom:
            {
                Handles.DrawLine(new Vector3(oneRect.xMin,oneRect.yMin),new Vector3(twoRect.xMin,twoRect.yMin));
                Handles.DrawLine(new Vector3(oneRect.xMax,oneRect.yMax),new Vector3(twoRect.xMax,twoRect.yMax));
            }
            break;
        }

        Handles.color = currentColor;
    }

    private void drawInGameScreenSection(Vector3 position, float zoomSize, float radius)
    {
        Rect rectangle = getInGameScreenSection(position, zoomSize, radius);
        Handles.DrawSolidRectangleWithOutline(rectangle,new Color(0f,0f,0f,0f),Color.blue);
    }

    private void drawCircleWithHandle(Vector3 position, float radius)
    {
        for(int i = 0; i < 36; ++i)
        {
            float x = Mathf.Cos(10f * i * Mathf.Deg2Rad);
            float y = Mathf.Sin(10f * i * Mathf.Deg2Rad);

            float x2 = Mathf.Cos(10f * (i + 1) * Mathf.Deg2Rad);
            float y2 = Mathf.Sin(10f * (i + 1) * Mathf.Deg2Rad);

            Handles.DrawLine(new Vector3(x,y) * radius + position,new Vector3(x2,y2) * radius + position);
        }
    }

    private Sprite getFirstActionSpriteFromCharacter(CharacterInfoData characterInfoData)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[baseData._defaultActionIndex]._animationInfoIndex][0];

        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }

    private Sprite getActionSpriteFromCharacter(CharacterInfoData characterInfoData, string actionName)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        if(baseData == null)
            return null;

        int targetIndex = -1;
        for(int index = 0; index < baseData._actionNodeData.Length; ++index)
        {
            if(baseData._actionNodeData[index]._nodeName == actionName)
            {
                targetIndex = index;
                break;
            }
        }

        if(targetIndex < 0)
            targetIndex = baseData._defaultActionIndex;

        if(baseData._actionNodeData[targetIndex]._animationInfoIndex == -1)
            return null;

        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[targetIndex]._animationInfoIndex][0];
        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }
}


public class MiniStageListView
{
    static private string _miniStagePath = "StageData/MiniStageData/";
   
    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;
    private MiniStageData _addedMiniStageData = null;
    private MiniStageData _newMiniStageData = null;

    public void OnGUI()
    {
        if(GUILayout.Button("New"))
        {
            string defaultName = ".asset";
            string filePath = EditorUtility.SaveFilePanel(
                "Save Mini Stage",
                "Assets/Resources/StageData/MiniStageData/",
                defaultName,
                "asset"
            );

            if (string.IsNullOrEmpty(filePath)) 
                return;

            filePath = FileUtil.GetProjectRelativePath(filePath);
            MiniStageData miniStgaeData = ScriptableObject.CreateInstance<MiniStageData>();
            miniStgaeData._stageName = "NewStage";

            StagePointData stagePointData = new StagePointData(Vector3.zero);
            stagePointData._cameraZoomSize = Camera.main.orthographicSize;
            miniStgaeData._stagePointData.Add(stagePointData);

            AssetDatabase.CreateAsset(miniStgaeData, filePath);
            AssetDatabase.SaveAssets();

            _newMiniStageData = miniStgaeData;
        }

        _searchString = EditorGUILayout.TextField("Search",_searchString);
        if(_searchStringCompare != _searchString)
        {
            if(_searchString == "")
                _searchStringList = null;
            else
                _searchStringList = _searchString.Split(' ');

            _searchStringCompare = _searchString;
        }

        UnityEngine.Object[] miniStageDataArray = Resources.LoadAll(_miniStagePath,typeof(MiniStageData));
        if(miniStageDataArray == null)
            return;

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        foreach(var item in miniStageDataArray)
        {
            MiniStageData data = item as MiniStageData;
            if(_searchString != "" && (searchStringCompare(data._stageName) == false))
                continue;

            GUILayout.BeginHorizontal("box");

            if(GUILayout.Button("Show",GUILayout.Width(50f)))
            {
                PingTarget(data);
            }

            if(GUILayout.Button("Edit",GUILayout.Width(50f)))
            {
                _newMiniStageData = data;
            }
            
            if(GUILayout.Button("Add",GUILayout.Width(50f)))
            {
                _addedMiniStageData = data;
            }

            GUILayout.Label(data.name + ": " + data._stageName);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private void PingTarget(ScriptableObject obj)
    {
        EditorUtility.FocusProjectWindow();
        EditorGUIUtility.PingObject(obj);
    }

    public MiniStageData getAddedMiniStage()
    {
        MiniStageData data = _addedMiniStageData;
        _addedMiniStageData = null;

        return data;
    }

    public MiniStageData getNewMiniStageData()
    {
        MiniStageData data = _newMiniStageData;
        _newMiniStageData = null;

        return data;
    }

    private bool searchStringCompare(string target)
    {
        string lowerTarget = target.ToLower();
        foreach(var stringItem in _searchStringList)
        {
            if(lowerTarget.Contains(stringItem))
                return true;
        }

        return false;
    }

}

public class CharacterInfoView
{
    private CharacterInfoData _selectedData = null;

    private string _searchString = "";
    private string[] _searchStringList;
    private string _searchStringCompare = "";

    private Vector2 _scrollPosition;
    private Vector2 _characterInfoScrollPosition;
    private string _addedCharacterKey = "";

    private Texture _characterTexture = null;

    public void OnGUI()
    {
        _searchString = EditorGUILayout.TextField("Search",_searchString);
        if(_searchStringCompare != _searchString)
        {
            if(_searchString == "")
                _searchStringList = null;
            else
                _searchStringList = _searchString.Split(' ');

            _searchStringCompare = _searchString;
        }

        const string kCharacterInfoPath = "Assets\\Data\\StaticData\\CharacterInfo.xml";
        Dictionary<string,CharacterInfoData> characterInfo = ResourceContainerEx.Instance().getCharacterInfo(kCharacterInfoPath);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        foreach(var item in characterInfo)
        {
            if(_searchString != "" && (searchStringCompare(item.Key) == false && searchStringCompare(item.Value._displayName) == false))
                continue;

            GUILayout.BeginHorizontal("box");
            
            if(GUILayout.Button("Show",GUILayout.Width(50f)))
            {
                _selectedData = item.Value;

                Sprite characterSprite = getFirstActionSpriteFromCharacter(item.Value);
                _characterTexture = characterSprite?.texture;
            }

            if(GUILayout.Button("Add",GUILayout.Width(50f)))
            {
                _addedCharacterKey = item.Key;
            }

            GUILayout.Label(item.Key + ": " + item.Value._displayName);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if(_selectedData != null)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal("box");

            if(_characterTexture != null)
            {
                Rect rect = GUILayoutUtility.GetRect(_characterTexture.width, _characterTexture.height);

                EditorGUIUtility.ScaleAroundPivot(Vector3.one,rect.center);
                GUI.DrawTexture(rect, _characterTexture,ScaleMode.ScaleToFit);
            }

            GUILayout.BeginVertical();

            _characterInfoScrollPosition = GUILayout.BeginScrollView(_characterInfoScrollPosition);

            EditorGUILayout.LabelField(_selectedData._displayName);
            EditorGUILayout.LabelField("ActionGraph: " + _selectedData._actionGraphPath);
            EditorGUILayout.LabelField("AIGraph: " + _selectedData._aiGraphPath);
            
            EditorGUILayout.LabelField("Status: " + _selectedData._statusName);
            EditorGUILayout.LabelField("Radius: " + _selectedData._characterRadius);
            EditorGUILayout.LabelField("HeadUpOffset: " + _selectedData._headUpOffset);
            EditorGUILayout.LabelField("SearchIdentifier: " + _selectedData._searchIdentifer);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
    }

    private Sprite getFirstActionSpriteFromCharacter(CharacterInfoData characterInfoData)
    {
        StaticDataLoader.loadStaticData();
        ActionGraphBaseData baseData = ResourceContainerEx.Instance().GetActionGraph(characterInfoData._actionGraphPath);
        AnimationPlayDataInfo playDataInfo = baseData._animationPlayData[baseData._actionNodeData[baseData._defaultActionIndex]._animationInfoIndex][0];

        Sprite[] sprites = ResourceContainerEx.Instance().GetSpriteAll(playDataInfo._path);
        if(sprites == null)
            return null;
        
        return sprites[0];
    }

    public string getAddedCharacter()
    {
        string characterKey = _addedCharacterKey;
        _addedCharacterKey = "";

        return characterKey;
    }

    private bool searchStringCompare(string target)
    {
        string lowerTarget = target.ToLower();
        foreach(var stringItem in _searchStringList)
        {
            if(lowerTarget.Contains(stringItem))
                return true;
        }

        return false;
    }

}