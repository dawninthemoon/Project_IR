using UnityEngine;

public class CollisionInfo
{
    private static int _uniqueIDPointer;
    private CollisionInfoData _collisionInfoData;
    private Vector3 _centerPosition;
    private BoundBox _boundBox;

    private int _uniqueID = 0;

    public CollisionInfo(CollisionInfoData data)
    {
        _collisionInfoData = data;
        _centerPosition = Vector3.zero;
        
        _boundBox = data.getBoundBox();
        _uniqueID = _uniqueIDPointer++;
    }

    public bool isValid()
    {
        return _collisionInfoData != null && _collisionInfoData.isValid();
    }

    public bool collisionCheck(CollisionInfo target)
    {
        if(isValid() == false || target.isValid() == false)
        {
            DebugUtil.assert(false,"충돌 데이터가 유효하지 않습니다. 통보 요망 : [{0}/{1}], [{2}/{3}]",getCollisionType(),isValid(),target.getCollisionType(), target.isValid());
            return false;
        }

        return _boundBox.intersection(target.getBoundBox());
    }

    public void drawCollosionArea(Color color, float time = 0f)
    {
        drawBoundBox(Color.red,time);
    }

    public void drawBoundBox(Color color, float time)
    {
        GizmoHelper.instance.drawPolygon(_boundBox.getVertices(),color,time);
    }

    public void setCollisionInfo(float width, float height, CollisionType type)
    {
        _collisionInfoData.setCollisionInfoData(width,height,type);
    }

    public void updateCollisionInfo(Vector3 position)
    {
        _centerPosition = position;

        _boundBox.updateBoundBox(position);
    }

    public int getUniqueID() {return _uniqueID;}
    public Vector3 getCenterPosition() {return _centerPosition;}
    public CollisionType getCollisionType() {return _collisionInfoData.getCollisionType();}
    public CollisionInfoData getCollisionInfoData() {return _collisionInfoData;}
    public BoundBox getBoundBox() {return _boundBox;}
}