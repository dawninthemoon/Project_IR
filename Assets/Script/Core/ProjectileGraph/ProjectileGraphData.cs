using UnityEngine;
using System.Collections.Generic;

public enum ProjectileChildFrameEventType
{
    ChildFrameEvent_OnHit,
    ChildFrameEvent_OnHitEnd,
    ChildFrameEvent_OnEnd,
    Count,
}

public class ProjectileGraphBaseData
{
    public string                       _name;
    public ProjectileType               _projectileType;
    public ProjectileGraphShotInfoData  _defaultProjectileShotInfoData;
    public AnimationPlayDataInfo[]      _animationPlayData;

    public Dictionary<ProjectileChildFrameEventType,ChildFrameEventItem> _projectileChildFrameEvent = null;

    public float                        _collisionRadius = 0.1f;
    public float                        _collisionAngle = 0f;
    public float                        _collisionStartDistance = 0f;

    public bool                         _useSpriteRotation = false;
    public bool                         _castShadow = false;

    public bool                         _executeBySummoner = false;

    public int                          _penetrateCount = 1;
}

public struct ProjectileGraphShotInfoData
{
    public float                    _deafaultVelocity;
    public float                    _acceleration;
    public float                    _friction;
    public float                    _defaultAngle;
    public float                    _angularAcceleration;
    public float                    _lifeTime;

    public bool                     _useRandomAngle;
    public Vector2                  _randomAngle;

    public ProjectileGraphShotInfoData(float defaultVelocity, float acceleration, float friction, float defaultAngle, float angularAcceleration, float lifeTime, bool useRandomAngle, Vector2 randomAngle)
    {
        _deafaultVelocity = defaultVelocity;
        _acceleration = acceleration;
        _friction = friction;
        _defaultAngle = defaultAngle;
        _angularAcceleration = angularAcceleration;
        _lifeTime = lifeTime;

        _useRandomAngle = useRandomAngle;
        _randomAngle = randomAngle;
    }

    public static ProjectileGraphShotInfoData operator +(ProjectileGraphShotInfoData a, ProjectileGraphShotInfoData b)
    {
        return new ProjectileGraphShotInfoData{
            _acceleration = a._acceleration + b._acceleration
            , _angularAcceleration = a._angularAcceleration + b._angularAcceleration
            , _deafaultVelocity = a._deafaultVelocity + b._deafaultVelocity
            , _defaultAngle = a._defaultAngle + b._defaultAngle
            , _friction = a._friction + b._friction
            , _lifeTime = a._lifeTime + b._lifeTime
            , _useRandomAngle = a._useRandomAngle | b._useRandomAngle
            , _randomAngle = a._randomAngle + b._randomAngle};
    }

    public void clear()
    {
        _deafaultVelocity = 0f;
        _acceleration = 0f;
        _friction = 0f;
        _defaultAngle = 0f;
        _angularAcceleration = 0f;
        _lifeTime = 0f;

        _useRandomAngle = false;
        _randomAngle = Vector2.zero;
    }

}


public enum ProjectileType
{
    Count,
}