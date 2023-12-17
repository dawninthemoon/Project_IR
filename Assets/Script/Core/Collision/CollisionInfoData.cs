
using System.Diagnostics;
using System.Numerics;

public class CollisionInfoData
{
    private BoundBox _boundBox;
    private CollisionType _collisionType = CollisionType.Default;
    private float _width = 0f;
    private float _height = 0f;

    public CollisionInfoData(float width, float height, CollisionType collisionType)
    {
        _width = width;
        _height = height;
        _boundBox = new BoundBox(_width * 0.5f,_height * 0.5f,UnityEngine.Vector3.zero);
        _collisionType = collisionType;
    }

    public bool isValid()
    {
        return _width != 0f && _height != 0f && _boundBox.isValid();
    }

    public float getWidth() {return _width;}
    public float getHeight() {return _height;}

    public BoundBox getBoundBox() {return _boundBox;}
    public CollisionType getCollisionType() {return _collisionType;}
    public Vector2 getWidthHeight() {return new Vector2(_width,_height);}

    public void setCollisionInfoData(float width, float height, CollisionType type)
    {
        _width = width;
        _height = height;

        _collisionType = type;
        _boundBox.setData(_width * 0.5f,_height * 0.5f,UnityEngine.Vector3.zero);
    }
}


public enum CollisionType
{
    Default = 0,
    Character,
    Projectile,
    Attack,
    Count,
}