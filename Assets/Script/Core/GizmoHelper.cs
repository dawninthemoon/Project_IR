using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GizmoHelper : MonoBehaviour
{
    public static GizmoHelper instance;
    class GizmoCircleData
    {
        public Vector3 _center;
        public float _radius;
        public int _accuracy;

        public float _timer;

        public Color _color;
        
    };

    class GizmoPolygonData
    {
        public Vector3[] _verticies;

        public float _timer;
        public Color _color;
    }

    class GizmoLineData
    {
        public Vector3 _start;
        public Vector3 _end;

        public float _timer;
        public Color _color;
    }

    class GizmoArcData
    {
        public Vector3 _start;
        public Vector3 _direction;
        public float _angle;
        public float _radius;

        public float _timer;
        public Color _color;

    }

    private SimplePool<GizmoCircleData> _circleDataPool = new SimplePool<GizmoCircleData>();
    private SimplePool<GizmoPolygonData> _polygonDataPool = new SimplePool<GizmoPolygonData>();
    private SimplePool<GizmoLineData> _lineDataPool = new SimplePool<GizmoLineData>();
    private SimplePool<GizmoArcData> _arcDataPool = new SimplePool<GizmoArcData>();

    private List<GizmoCircleData> _circleRequestData = new List<GizmoCircleData>();
    private List<GizmoPolygonData> _polygonData = new List<GizmoPolygonData>();
    private List<GizmoLineData> _lineData = new List<GizmoLineData>();
    private List<GizmoArcData> _arcData = new List<GizmoArcData>();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        for(int index = 0; index < _lineData.Count; ++index)
        {
            _lineData[index]._timer -= Time.deltaTime;
        }

        for(int index = 0; index < _circleRequestData.Count; ++index)
        {
            _circleRequestData[index]._timer -= Time.deltaTime;
        }

        for(int index = 0; index < _polygonData.Count; ++index)
        {
            _polygonData[index]._timer -= Time.deltaTime;
        }

        for(int index = 0; index < _arcData.Count; ++index)
        {
            _arcData[index]._timer -= Time.deltaTime;
        }
    }

    public void drawCircle(Vector3 center, float radius, int accuracy, Color color, float time = 0f)
    {
        if(AreGizmosVisible() == false)
            return;

        GizmoCircleData circleData = _circleDataPool.dequeue();// new GizmoCircleData();
        circleData._center = center;
        circleData._radius = radius;
        circleData._accuracy = accuracy;
        circleData._color = color;
        circleData._timer = time;

        _circleRequestData.Add(circleData);
    }

    public void drawPolygon(Vector3[] verticies, Color color, float time = 0f)
    {
        if(AreGizmosVisible() == false)
            return;
            
        if(verticies.Length < 3)
        {
            DebugUtil.assert(false,"is not polygon");
            return;
        }

        GizmoPolygonData polygonData = _polygonDataPool.dequeue();// new GizmoPolygonData();
        polygonData._verticies = verticies;
        polygonData._color = color;
        polygonData._timer = time;

        _polygonData.Add(polygonData);
    }

    public void drawArc(Vector3 start, float radius, float angle, Vector3 direction, Color color, float time = 0f)
    {
        if(AreGizmosVisible() == false)
            return;
            
        GizmoArcData arcData = _arcDataPool.dequeue();// new GizmoArcData();
        arcData._angle = angle;
        arcData._color = color;
        arcData._direction = direction;
        arcData._radius = radius;
        arcData._start = start;
        arcData._timer = time;

        _arcData.Add(arcData);
    }

    public void drawRectangle(Vector3 center, Vector3 widthHeightHalf, Color color, float time = 0f)
    {
        if(AreGizmosVisible() == false)
            return;
            
        Vector3 leftTop = new Vector3(center.x - widthHeightHalf.x, center.y + widthHeightHalf.y, 0f);
        Vector3 rightTop = new Vector3(center.x + widthHeightHalf.x, center.y + widthHeightHalf.y, 0f);
        Vector3 leftBottom = new Vector3(center.x - widthHeightHalf.x, center.y - widthHeightHalf.y, 0f);
        Vector3 rightbottom = new Vector3(center.x + widthHeightHalf.x, center.y - widthHeightHalf.y, 0f);

        drawLine(leftTop, rightTop, color, time);
        drawLine(rightTop, rightbottom, color, time);
        drawLine(rightbottom, leftBottom, color, time);
        drawLine(leftBottom, leftTop, color, time);
    }


    public void drawLine(Vector3 start, Vector3 end, Color color, float time = 0f)
    {
        if(AreGizmosVisible() == false)
            return;
            
        GizmoLineData lineData = _lineDataPool.dequeue();// new GizmoLineData();
        lineData._start = start;
        lineData._end = end;
        lineData._color = color;
        lineData._timer = time;

        _lineData.Add(lineData);
    }

    private void drawLine()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _lineData.Count;)
        {
            GizmoLineData item = _lineData[index];

            Gizmos.color = item._color;
            Gizmos.DrawLine(item._start, item._end);

            if(item._timer <= 0f)
            {
                _lineDataPool.enqueue(_lineData[index]);
                _lineData.RemoveAt(index);
            }
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void drawArc()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _arcData.Count;)
        {
            GizmoArcData item = _arcData[index];
            Gizmos.color = item._color;

            float directionAngle = MathEx.clampDegree(Vector3.SignedAngle(item._direction, Vector3.right,Vector3.forward));
            directionAngle += item._angle * 0.5f;

            float accur = item._angle / 36f;
            for(int i = 0; i < 36; ++i)
            {   
                
                float x = Mathf.Cos((accur * i - directionAngle) * Mathf.Deg2Rad);
                float y = Mathf.Sin((accur * i - directionAngle) * Mathf.Deg2Rad);

                float x2 = Mathf.Cos((accur * (i + 1) - directionAngle) * Mathf.Deg2Rad);
                float y2 = Mathf.Sin((accur * (i + 1) - directionAngle) * Mathf.Deg2Rad);

                if(i == 0)
                {
                    Gizmos.DrawLine(new Vector3(x,y) * item._radius + item._start,item._start);
                }
                else if(i == 35)
                {
                    Gizmos.DrawLine(item._start,new Vector3(x2,y2) * item._radius + item._start);
                }
                Gizmos.DrawLine(new Vector3(x,y) * item._radius + item._start,new Vector3(x2,y2) * item._radius + item._start);
            }

            if(item._timer <= 0f)
            {
                _arcDataPool.enqueue(_arcData[index]);
                _arcData.RemoveAt(index);
            }
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void drawPolygon()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _polygonData.Count;)
        {
            GizmoPolygonData item = _polygonData[index];

            Gizmos.color = item._color;
            for(int i = 0; i < item._verticies.Length - 1; ++i)
            {
                Gizmos.DrawLine(item._verticies[i], item._verticies[i + 1]);
            }

            Gizmos.DrawLine(item._verticies[item._verticies.Length - 1], item._verticies[0]);

            if(item._timer <= 0f)
            {
                _polygonDataPool.enqueue(_polygonData[index]);
                _polygonData.RemoveAt(index);
            }
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void drawCircle()
    {
        Color color = Gizmos.color;
        for(int index = 0; index < _circleRequestData.Count;)
        {
            GizmoCircleData item = _circleRequestData[index];
            Gizmos.color = item._color;

            float accur = 360f / (float)item._accuracy;
            for(int i = 0; i < 36; ++i)
            {
                float x = Mathf.Cos(10f * i * Mathf.Deg2Rad);
                float y = Mathf.Sin(10f * i * Mathf.Deg2Rad);

                float x2 = Mathf.Cos(10f * (i + 1) * Mathf.Deg2Rad);
                float y2 = Mathf.Sin(10f * (i + 1) * Mathf.Deg2Rad);

                Gizmos.DrawLine(new Vector3(x,y) * item._radius + item._center,new Vector3(x2,y2) * item._radius + item._center);
            }

            if(item._timer <= 0f)
            {
                _circleDataPool.enqueue(_circleRequestData[index]);
                _circleRequestData.RemoveAt(index);
            }
            else
                ++index;
        }

        Gizmos.color = color;
    }

    private void OnDrawGizmos()
    {
        drawCircle();
        drawLine();
        drawPolygon();
        drawArc();
    }



    bool AreGizmosVisible()
    {
#if UNITY_EDITOR
        Assembly asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        Type type = asm.GetType("UnityEditor.GameView");
        if (type != null)
        {
            EditorWindow window = GetWindowPrivate(type, false, ""); //EditorWindow.GetWindow(type);
            FieldInfo gizmosField = type.GetField("m_Gizmos", BindingFlags.NonPublic | BindingFlags.Instance);
            if(gizmosField != null)
                return (bool)gizmosField.GetValue(window);
        }
#endif
        return false;
    }
#if UNITY_EDITOR
    EditorWindow GetWindowPrivate(System.Type t, bool utility, string title)
    {
        UnityEngine.Object[] wins = Resources.FindObjectsOfTypeAll(t);
        EditorWindow win = wins.Length > 0 ? (EditorWindow)(wins[0]) : null;
    
        if (!win)
        {
            win = ScriptableObject.CreateInstance(t) as EditorWindow;
            if (title != null)
                win.titleContent = new GUIContent(title);
            if (utility)
                win.ShowUtility();
            else
                win.Show();
        }
    
        return win;
    }
#endif
}
