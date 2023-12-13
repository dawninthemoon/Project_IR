
public class CollisionInfoData
{
    private BoundBox _boundBox;
    private CollisionType _collisionType = CollisionType.Default;
    private float _radius = 0f;
    private float _angle = 0f;
    private float _startDistance = 0f;
    private float _rayRadius = 0f;

    public CollisionInfoData(float radius, float angle, float startDistance, float rayRadius, CollisionType collisionType)
    {
        _boundBox = new BoundBox(radius,radius,UnityEngine.Vector3.zero);
        _radius = radius;
        _angle = angle;
        _startDistance = startDistance;
        _rayRadius = rayRadius;
        _collisionType = collisionType;
    }

    public bool isValid()
    {
        return _radius != 0f && _boundBox.isValid();
    }

    public BoundBox getBoundBox() {return _boundBox;}
    public CollisionType getCollisionType() {return _collisionType;}
    public float getRadius() {return _radius;}
    public float getSqrRadius() {return _radius * _radius;}

    public float getAngle() {return _angle;}
    public float getRayRadius() {return _rayRadius;}
    public float getStartDistance() {return _startDistance;}

    public void setCollisionInfoData(float radius, float angle, float startDistance, CollisionType type)
    {
        _radius = radius;
        _angle = angle;
        _startDistance = startDistance;
        _collisionType = type;

        _boundBox.setData(radius,radius,UnityEngine.Vector3.zero);
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