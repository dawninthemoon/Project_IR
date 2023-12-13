
using UnityEngine;


public enum SearchIdentifier
{
    Player = 0,
    Enemy,
    Projectile,
    Count,
}

public enum TargetSearchType
{
    None = 0,
    Near,
    NearDirection,
    NearMousePointDirection,
    Count,
}

public enum CallAIEventTargetType
{
    Range,
    Self,
    FrameEventTarget,
    Summoner,
}


public enum AttackType
{
    None = 0,
    Default = 0b001,
    GuardBreak = 0b010,
    Catch = 0b100,
    Count = 3,
}

public enum AttackState
{
    Default,
    AttackSuccess,
    AttackGuarded,
    AttackParried,
    AttackEvade,
    AttackGuardBreak,
    AttackGuardBreakFail,
    AttackCatch,
}

public enum DefenceState
{
    Default,
    Hit,
    Catched,
    DefenceSuccess,
    DefenceCrash,
    ParrySuccess,
    EvadeSuccess,
    GuardBroken,
    GuardBreakFail,
}

public enum CommonMaterial
{
    Empty,
    Skin,
    Metal,
    Katana,
    Laser,
    Count,
}


public struct Triangle
{
    private Vector3[] _vertices;

    public Triangle(bool check)
    {
        _vertices = new Vector3[3];
    }

    public void makeTriangle(Vector3 centerPosition, float radius, float theta, float directionAngle)
    {
        MathEx.makeTriangle(centerPosition,radius,theta,directionAngle, out _vertices[0], out _vertices[1], out _vertices[2]);
    }

    public Vector3 get(int index) {return _vertices[index];}

    public Vector3[] getVertices() {return _vertices;}
}

[System.Serializable]
public class BoundBox
{
    private Vector3[] _vertices;
    private float _width;
    private float _height;
    private Vector3 _worldCenter;
    private Vector3 _localPivot;

    private float _l,_r,_b,_t;

    public BoundBox(float width, float height, Vector3 localPivot)
    {
        _vertices = new Vector3[4];
        setData(width,height,localPivot);
    }

    public bool isValid()
    {
        return _width > 0f && _height > 0f;
    }

    public void setPivot(Vector3 pivot)
    {
        _localPivot = pivot;
    }

    public void setData(float width, float height, Vector3 localPivot)
    {
        _width = width;
        _height = height;

        _worldCenter = Vector3.zero;
        _localPivot = localPivot;

        _l = _r = _b = _t = 0f;
    }

    public void updateBoundBox(Triangle triangle)
    {
        Vector3 triangleA = triangle.get(0);
        Vector3 triangleB = triangle.get(1);
        Vector3 triangleC = triangle.get(2);

        _l = triangleA.x;
        _l = _l > triangleB.x ? triangleB.x : _l;
        _l = _l > triangleC.x ? triangleC.x : _l;

        _t = triangleA.y;
        _t = _t < triangleB.y ? triangleB.y : _t;
        _t = _t < triangleC.y ? triangleC.y : _t;

        _r = triangleA.x;
        _r = _r < triangleB.x ? triangleB.x : _r;
        _r = _r < triangleC.x ? triangleC.x : _r;

        _b = triangleA.y;
        _b = _b > triangleB.y ? triangleB.y : _b;
        _b = _b > triangleC.y ? triangleC.y : _b;

        _vertices[0].x = _l;
        _vertices[0].y = _b;
        
        _vertices[1].x = _l;
        _vertices[1].y = _t;

        _vertices[2].x = _r;
        _vertices[2].y = _t;

        _vertices[3].x = _r;
        _vertices[3].y = _b;

    }

    public void updateBoundBox(Vector3 worldPosition)
    {
        _worldCenter = worldPosition;

        Vector3 pivotCenter = worldPosition + _localPivot;

        _l = pivotCenter.x - _width;
        _r = pivotCenter.x + _width;

        _b = pivotCenter.y - _height;
        _t = pivotCenter.y + _height;

        _vertices[0].x = _l;
        _vertices[0].y = _b;
        
        _vertices[1].x = _l;
        _vertices[1].y = _t;

        _vertices[2].x = _r;
        _vertices[2].y = _t;

        _vertices[3].x = _r;
        _vertices[3].y = _b;
    }

    public Vector3[] getVertices() {return _vertices;}

    public bool intersection(Vector3 point)
    {
        return (_l < point.x && _r > point.x) && (_b < point.y && _t > point.y);
    }
    public bool intersection(BoundBox target)
    {
        if (_l > target._r || target._l > _r)
            return false;
    
        if (_b > target._t || target._b > _t)
            return false;
    
        return true;
    }


    public float getWidth() {return _width;}
    public float getHeight() {return _height;}
    public float getBottom() {return _b;}

}