
using System.Collections.Generic;

//todo : animation start, ed frame, 
public class ProjectileGraph
{
    private ProjectileGraphBaseData _projectileGraphBaseData;
    private ProjectileGraphShotInfoData _projectileGraphShotInfoData;
    private AnimationPlayer _animationPlayer = new AnimationPlayer();

    public ProjectileGraph(){}
    public ProjectileGraph(ProjectileGraphBaseData baseData){_projectileGraphBaseData = baseData;}

    private UnityEngine.Vector3 _movementOfFrame = UnityEngine.Vector3.zero;

    private bool _isEnd = false;

    private int     _currentPenetrateCount = 0;

    private float _currentVelocity = 0f;
    private float _currentAngle = 0f;
    private float _currentLifeTime = 0f;

    public void initialize()
    {
        initialize(_projectileGraphBaseData._defaultProjectileShotInfoData);
    }

    public void initialize(ProjectileGraphShotInfoData shotInfoData)
    {
        setShotInfo(shotInfoData);

        _animationPlayer.initialize();
        _animationPlayer.changeAnimation(_projectileGraphBaseData._animationPlayData[0]);

        _movementOfFrame = UnityEngine.Vector3.zero;
        _currentPenetrateCount = _projectileGraphBaseData._penetrateCount;

        _isEnd = false;
    }

    private void setShotInfo(ProjectileGraphShotInfoData shotInfoData)
    {
        _projectileGraphShotInfoData = shotInfoData;

        _currentVelocity = _projectileGraphShotInfoData._deafaultVelocity;

        if(_projectileGraphShotInfoData._useRandomAngle)
        {
            _currentAngle = UnityEngine.Random.Range(_projectileGraphShotInfoData._randomAngle.x,_projectileGraphShotInfoData._randomAngle.y);
        }
        else
        {
            _currentAngle = _projectileGraphShotInfoData._defaultAngle;
        }
        _currentLifeTime = _projectileGraphShotInfoData._lifeTime;
    }

    public bool progress(float deltaTime, ObjectBase targetEntity)
    {
        if(isEnd() == true)
            return true;

        if(isOutOfBound() == true)
            return true;

        _animationPlayer.progress(deltaTime,targetEntity);

        _currentVelocity += _projectileGraphShotInfoData._acceleration * deltaTime;

        if(_projectileGraphShotInfoData._friction != 0f)
            _currentVelocity = MathEx.convergence0(_currentVelocity,_projectileGraphShotInfoData._friction * deltaTime);
        if(_projectileGraphShotInfoData._angularAcceleration != 0f)
            _currentAngle += _projectileGraphShotInfoData._angularAcceleration * deltaTime;
        
        _movementOfFrame += (_currentVelocity * deltaTime) * (UnityEngine.Quaternion.Euler(0f,0f,_currentAngle) * UnityEngine.Vector3.right);

        return isEnd();
    }

    public void release()
    {
        _animationPlayer.Release();
    }

    public void executeChildFrameEvent(ProjectileChildFrameEventType eventType, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        if(_projectileGraphBaseData._projectileChildFrameEvent == null || _projectileGraphBaseData._projectileChildFrameEvent.ContainsKey(eventType) == false)
            return;
        
        ChildFrameEventItem childFrameEventItem = _projectileGraphBaseData._projectileChildFrameEvent[eventType];
        for(int i = 0; i < childFrameEventItem._childFrameEventCount; ++i)
        {
            if(executeEntity is GameEntityBase && childFrameEventItem._childFrameEvents[i].checkCondition(executeEntity as GameEntityBase) == false)
                continue;
            
            childFrameEventItem._childFrameEvents[i].initialize();
            childFrameEventItem._childFrameEvents[i].onExecute(executeEntity, targetEntity);
        }
    }

    public UnityEngine.Vector3 getMovementOfFrame() 
    {
        UnityEngine.Vector3 movement = _movementOfFrame;
        _movementOfFrame = UnityEngine.Vector3.zero;
        return movement;
    }
    public UnityEngine.Sprite getCurrentSprite() {return _animationPlayer.getCurrentSprite();}
    public UnityEngine.Quaternion getCurrentAnimationRotation() {return _animationPlayer.getCurrentAnimationRotation();}
    public bool getCurrentAnimationScale(out UnityEngine.Vector3 outScale) {return _animationPlayer.getCurrentAnimationScale(out outScale);}
    public bool getCurrentAnimationTranslation(out UnityEngine.Vector3 outTranslation) { return _animationPlayer.getCurrentAnimationTranslation(out outTranslation); }

    public bool isOutOfBound()
    {
        return false;
    }

    public void setData(ProjectileGraphBaseData baesData) 
    {
        _projectileGraphBaseData = baesData;
        initialize();
    }

    public bool isEventExecuteBySummoner() {return _projectileGraphBaseData._executeBySummoner;}

    public void decreasePenetrateCount() {--_currentPenetrateCount;}
    public int getPenetrateCount() {return _currentPenetrateCount;}

    public bool isPenetrateEnd() {return _currentPenetrateCount <= 0;}

    public void updateLifeTime(float deltaTime)
    {
        _currentLifeTime -= deltaTime;
        _isEnd = _currentLifeTime <= 0f;
    }

    public bool isEnd()
    {
        if(_isEnd == true || (_currentPenetrateCount != -1 && _currentPenetrateCount == 0))
            return true;

        return _isEnd;
    }
};