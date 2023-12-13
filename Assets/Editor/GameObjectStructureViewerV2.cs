using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;
using System.Collections.Generic;

 
public class GameObjectStructureViewerV2 : EditorWindow
{
    [System.Serializable]
    public class ObjectItem
    {
        public Transform transform;
        public int parent;
        public Vector2 viewPosition;
        public int deep;
    }

    [SerializeField]List<ObjectItem> _objectItems = new List<ObjectItem>();

    private Matrix4x4 _modelMatrix;
    private Rect _viewRect;

    private Vector2 _centerPosition;
    private Vector2 _prevMousePosition;
 
    private Vector3 _modelPos = Vector3.zero;
    private Quaternion _modelRotation = Quaternion.identity;
    private float _modelScale = 1f;

    private float _scaleFactor = 1f;
    private float _buttonScale = 10f;
    private float _sceneButtonScale = 0.01f;

    private bool _xFlip = false;
    private bool _yFlip = true;
    private bool _zFlip = false;
 

    [SerializeField] static string[] tagArray;
    [SerializeField] static string[] layerArray; 
    private int selectedTag = ~0;
    private int selectedLayer = ~0;


    [MenuItem("CustomWindow/StructureViewer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GameObjectStructureViewerV2),false,"StructureViewer");
        tagArray = InternalEditorUtility.tags;
        layerArray = InternalEditorUtility.layers;
    }

    void OnGUI() 
    {
        if(tagArray == null || layerArray == null)
        { 
            tagArray = InternalEditorUtility.tags;
            layerArray = InternalEditorUtility.layers;
        }
        
        GUILayout.BeginHorizontal("box");
        GUILayout.BeginVertical();
        GUILayout.Label("ScalingFactor");
        _scaleFactor = EditorGUILayout.FloatField(_scaleFactor);
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical();
        GUILayout.Label("SceneButtonSize");
        _sceneButtonScale = EditorGUILayout.FloatField(_sceneButtonScale);
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("ButtonSize");
        _buttonScale = EditorGUILayout.FloatField(_buttonScale);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("Tag");
        selectedTag = EditorGUILayout.MaskField(selectedTag, tagArray);
        GUILayout.Label("Layer");
        selectedLayer = EditorGUILayout.MaskField(selectedLayer, layerArray);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("X-Flip");
        _xFlip = EditorGUILayout.Toggle(_xFlip);
        GUILayout.Label("Y-Flip");
        _yFlip = EditorGUILayout.Toggle(_yFlip);
        GUILayout.Label("Z-Flip");
        _zFlip = EditorGUILayout.Toggle(_zFlip);
        EditorGUILayout.EndHorizontal();


        KeyCheck();
        DrawStructure("",new Vector2(5,108),new Vector2(-10f,-5f),new Vector3());


        string rootLabel = "root : " + (_objectItems.Count == 0 ? "" : _objectItems[0].transform.name);
        string posLabel = "position : " + _modelPos.ToString();
        string rotationLabel = "rotation : " + _modelRotation.eulerAngles.ToString();
        string scaleLabel = "scale : " + _modelScale.ToString();

        Rect lable = _viewRect; 
        lable.height = 15f;
        GUI.Label(lable,rootLabel);
        lable.y += 15f;
        GUI.Label(lable,posLabel);
        lable.y += 15f;
        GUI.Label(lable,rotationLabel);
        lable.y += 15f;
        GUI.Label(lable,scaleLabel);
    }

    private void KeyCheck()
    {
        Event e = Event.current;

        //rotate
        if(e.type == EventType.MouseDown && e.button == 1)
        {
            _prevMousePosition = e.mousePosition;
        }
        else if(e.type == EventType.MouseDrag && e.button == 1)
        {
            Vector3 factor = e.mousePosition - _prevMousePosition;

            AddRotation(new Vector2(factor.y,factor.x));

            _prevMousePosition = e.mousePosition;
            e.Use();

            Repaint();
        }
        //translate
        else if(e.type == EventType.MouseDown && e.button == 2)
        {
            _prevMousePosition = e.mousePosition;
        }
        else if(e.type == EventType.MouseDrag && e.button == 2)
        {
            Vector3 factor = e.mousePosition - _prevMousePosition;

            AddPosition(factor);

            _prevMousePosition = e.mousePosition;
            e.Use();

            Repaint();
        }
        //scale
        else if(e.type == EventType.ScrollWheel)
        {
            AddScale(-e.delta.y * _scaleFactor, true);
            e.Use();
            
            Repaint();
        }
    }

    private void DrawStructure(string targetName, Vector2 viewPos, Vector2 align,Vector3 euler)
    {
        _viewRect = new Rect(viewPos.x,viewPos.y,position.width + align.x, position.height - viewPos.y + align.y);
        _centerPosition = new Vector2(_viewRect.width * .5f, _viewRect.height * .5f);

        DropAreaGUI(_viewRect);

        GUI.Box(_viewRect,targetName);
        GUI.BeginClip(_viewRect,Vector2.zero,Vector2.zero,false);

        var root = _objectItems.Count == 0 ? null : _objectItems[0];

        if(root != null && root.transform != null)
        {
            _modelMatrix = Matrix4x4.Translate(_modelPos) * 
                        Matrix4x4.Rotate(_modelRotation) *
                        Matrix4x4.Scale(new Vector3(_modelScale,_modelScale,_modelScale));

            var center = root.transform.position;
        
            foreach(var item in _objectItems)
            {
                if(item.transform == null)
                {
                    Dispose();
                    Repaint();
                    return;
                }

                if(!TagAndLayerCheck(item.transform))
                    continue;


                Vector4 pos = (item.transform.position - center);
                pos.x *= _xFlip ? -1f : 1f;
                pos.y *= _yFlip ? -1f : 1f;
                pos.z *= _zFlip ? -1f : 1f;
                pos.w = 1f;
                pos = _modelMatrix * pos;

                item.viewPosition = new Vector2(pos.x,pos.y) + _centerPosition;


                if(DrawButton(item.viewPosition,new Vector2(_buttonScale,_buttonScale),item.transform.name))
                {
                    PingTarget(item.transform.gameObject);
                }

                if(Selection.activeGameObject == item.transform.gameObject)
                {
                    DrawCrossLine(item.viewPosition,10f,Color.green);
                }

                if(item.parent != -1 && TagAndLayerCheck(GetItem(item.parent).transform))
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(item.viewPosition,GetItem(item.parent).viewPosition);
                }
            }
            
        }
        else
        {
            Dispose();
            Repaint();
        }

        

        GUI.EndClip();
    }
    
    private void DrawCrossLine(Vector2 pos,float length, Color color)
    {
        var vS = new Vector2(pos.x - length,pos.y);
        var vE = new Vector2(pos.x + length,pos.y);
        var hS = new Vector2(pos.x,pos.y + length);
        var hE = new Vector2(pos.x,pos.y - length);

        Handles.color = color;
        Handles.DrawLine(vS,vE);
        Handles.DrawLine(hS,hE);
        
    }

    private bool DrawButton(Vector2 pos,Vector2 size, string tooltip)
    {
        pos -= size * .5f;

        return GUI.Button(new Rect(pos.x,pos.y,size.x,size.y),new GUIContent("",tooltip));
    }

    private void DrawListItem(string name, int deep)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space((float)(deep * 10f));
        Rect drop_area = GUILayoutUtility.GetRect(0, 20.0f,GUILayout.ExpandWidth (true));
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.MiddleLeft;
        style.normal.textColor = Color.white;
        GUI.Box (drop_area, name, style);

        GUILayout.EndHorizontal();

        GUILayout.Space(5f);
    }

    private void PingTarget(GameObject obj)
    {
        EditorGUIUtility.PingObject(obj);
        Selection.activeGameObject = obj;
    }

    private void AddScale(float factor, bool translate)
    {
        _modelScale += factor;
    }

    private void AddPosition(Vector3 factor)
    {
        _modelPos += factor;
    }

    private void AddRotation(Vector3 factor)
    {
        _modelRotation = _modelRotation * Quaternion.Euler(factor);
    }

    private void InitModelValues()
    {
        _modelPos = Vector3.zero;
        _modelRotation = Quaternion.identity;
        _modelScale = 1f;
    }

    private ObjectItem SetItems(Transform root, int parentDeep,int parentPos)
    {
        ObjectItem item = CreateObjectItem();
        item.transform = root;
        item.parent = parentPos;
        item.deep = parentDeep + 1;

        _objectItems.Add(item);
        int back = _objectItems.Count - 1;

        for(int i = 0; i < root.childCount; ++i)
        {
            SetItems(root.GetChild(i),parentDeep,back);
        }

        return item;
    }

    private ObjectItem CreateObjectItem()
    {
        //cached

        return new ObjectItem();
    }

    private void Dispose()
    {
        foreach(var item in _objectItems)
        {
            ClearItem(item);
        }
        _objectItems.Clear();
    }

    private ObjectItem GetItem(int pos) 
    {
        return _objectItems[pos];
    }

    private void ClearItem(ObjectItem item)
    {
        item.transform = null;
    }

    private bool TagAndLayerCheck(Transform tp)
    {
        bool tagMask = ((1 << FindTag(tp.tag)) & selectedTag) != 0;
        bool layerMask = ((1 << tp.gameObject.layer) & (selectedLayer)) != 0;

        return tagMask && layerMask;
    }

    private int FindTag(string tag)
    { 
        if(tagArray == null)
            tagArray = InternalEditorUtility.tags;
            
        for(int i = 0; i < tagArray.Length; ++i)
        {
            if(tagArray[i] == tag)
                return i;
        }

        return -2;
    }

    private void DropAreaGUI (Rect dropArea)
    {
        Event evt = Event.current;

        switch (evt.type) {
        case EventType.DragUpdated:
        case EventType.DragPerform:
            if (!dropArea.Contains (evt.mousePosition))
                return;
             
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
         
            if (evt.type == EventType.DragPerform) {
                DragAndDrop.AcceptDrag ();
             
                var obj = ((GameObject)DragAndDrop.objectReferences[0]);
                if(obj != null)
                {
                    var transform = obj.GetComponent<Transform>();
                    Dispose();

                    InitModelValues();
                    SetItems(transform, -1,-1);
                }

                EditorUtility.SetDirty(this);
                
            }
            break;
        }
    }

    void OnFocus() 
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }
 
    void OnDestroy() 
    {
        Dispose();
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }
 
    void OnSceneGUI(SceneView sceneView) 
    {
        if(Event.current.type == EventType.Repaint)
        {
            Handles.color = Color.green;
        }

        foreach(var item in _objectItems)
        {
            if(item.transform == null)
            {
                Dispose();
                Repaint();
                return;
            }

            if(!TagAndLayerCheck(item.transform))
                    continue;

            if(item.parent != -1 && TagAndLayerCheck(GetItem(item.parent).transform))
                Handles.DrawLine(item.transform.position,GetItem(item.parent).transform.position);
            
            Handles.CapFunction del = (controlID, position, rotation, size, eventType)=>{
                Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
                if(eventType == EventType.Repaint && HandleUtility.nearestControl == controlID)
                {
                    position.y += 0.05f;
                    Handles.Label(position, item.transform.name);
                }  
            };

            if(Handles.Button(item.transform.position,Camera.current.transform.rotation,_sceneButtonScale,_sceneButtonScale * 2f,del))
            {
                PingTarget(item.transform.gameObject);
                Repaint();
            }

        }
    

    }

}
