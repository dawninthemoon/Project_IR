using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProjectileEntityBase : ObjectBase
{
    protected ProjectileGraph _projectileGraph;

    private CollisionInfo _collisionInfo;

    private Color _debugColor = Color.red;
    private CollisionDelegate _collisionDelegate;

    private HashSet<int> _collisionUniqueIDList = new HashSet<int>();

    private bool _spriteRotation = false;

    public override void assign()
    {
        base.assign();
        _projectileGraph = new ProjectileGraph();
        CollisionInfoData data = new CollisionInfoData(0f,0f,CollisionType.Projectile);
        _collisionInfo = new CollisionInfo(data);
        _collisionDelegate = onProjectileHit;
        
        createSpriteRenderObject();
    }

    public override void initialize()
    {
        base.initialize();

        _collisionUniqueIDList.Clear();
    }

    public void setData(ProjectileGraphBaseData baseData)
    {
        _spriteRotation = baseData._useSpriteRotation;
        _projectileGraph.setData(baseData);
        _collisionInfo.setCollisionInfo(baseData._collisionRadius * 2f, baseData._collisionRadius * 2f,CollisionType.Projectile);

        _spriteRenderer.gameObject.layer = baseData._castShadow ? LayerMask.NameToLayer("Character") : LayerMask.NameToLayer("EffectEtc");
    }

    public void shot(Vector3 startPosition)
    {
        transform.position = startPosition;
        _projectileGraph.initialize();

        RegisterRequest(QueryUniqueID("ProjectileManager"));
        CollisionManager.Instance().registerObject(_collisionInfo, this);
    }

    public void shot(ProjectileGraphShotInfoData shotInfoData, Vector3 startPosition)
    {
        transform.position = startPosition;

        if(_spriteRotation)
            _spriteObject.transform.localRotation = Quaternion.Euler(0f,0f,shotInfoData._defaultAngle);
        else
            _spriteObject.transform.localRotation = Quaternion.identity;

        _spriteObject.transform.localScale = Vector3.one;
        _spriteRenderer.transform.localPosition = Vector3.zero;

        _projectileGraph.initialize(shotInfoData);

        RegisterRequest(QueryUniqueID("ProjectileManager"));
        CollisionManager.Instance().registerObject(_collisionInfo, this);
    }

    public override void progress(float deltaTime)
    {
        if(_projectileGraph.isEnd() == true)
        {
            if(_projectileGraph.isPenetrateEnd() == false)
            {
                ObjectBase executeTargetEntity = this;
                if(_projectileGraph.isEventExecuteBySummoner())
                    executeTargetEntity = getSummonObject() == null ? this : getSummonObject();

                _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnEnd,executeTargetEntity,null);
            }

            DeregisterRequest();
            CollisionManager.Instance().deregisterObject(_collisionInfo.getCollisionInfoData(),this);
            return;
        }

        base.progress(deltaTime);

        bool isEnd = _projectileGraph.progress(deltaTime, this);
        _projectileGraph.updateLifeTime(deltaTime);

        Vector3 movementOfFrame = _projectileGraph.getMovementOfFrame();
        transform.position += movementOfFrame;

        _spriteRenderer.sprite = _projectileGraph.getCurrentSprite();
        _spriteRenderer.transform.localRotation = _projectileGraph.getCurrentAnimationRotation();

        Vector3 outScale = Vector3.one;
        _projectileGraph.getCurrentAnimationScale(out outScale);
        _spriteRenderer.transform.localScale = outScale;

        if(_spriteRotation && movementOfFrame.sqrMagnitude != 0f)
            _spriteObject.transform.localRotation *= Quaternion.Euler(0f,0f,Mathf.Atan2(movementOfFrame.y,movementOfFrame.x) * Mathf.Rad2Deg);

        Vector3 outTranslation = Vector3.zero;
        _projectileGraph.getCurrentAnimationTranslation(out outTranslation);
        _spriteRenderer.transform.localPosition = outTranslation;

        _collisionInfo.updateCollisionInfo(transform.position);
        CollisionManager.Instance().collisionRequest(_collisionInfo,this,_collisionDelegate,null);
        
        GizmoHelper.instance.drawRectangle(transform.position,_collisionInfo.getBoundBox().getWidthHeightHalf(),_debugColor);
    }

    private void onProjectileHit(CollisionSuccessData successData)
    {
        if(successData._requester is ProjectileEntityBase == false || successData._target is GameEntityBase == false || _projectileGraph.isEnd())
            return;

        ProjectileEntityBase requester = successData._requester as ProjectileEntityBase;
        GameEntityBase target = successData._target as GameEntityBase;

        if(requester._searchIdentifier == target._searchIdentifier)
            return;

        if(_collisionUniqueIDList.Contains(target.GetUniqueID()))
            return;

        if(_projectileGraph.getPenetrateCount() == -1)
            return;
        
        _collisionUniqueIDList.Add(target.GetUniqueID());

        ObjectBase executeTargetEntity = requester;
        if(_projectileGraph.isEventExecuteBySummoner())
            executeTargetEntity = requester.getSummonObject() == null ? requester : requester.getSummonObject();

        _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnHit,executeTargetEntity,target);

        _projectileGraph.decreasePenetrateCount();
        if(_projectileGraph.isPenetrateEnd() == true)
            _projectileGraph.executeChildFrameEvent(ProjectileChildFrameEventType.ChildFrameEvent_OnHitEnd,executeTargetEntity,target);    
    }

    
}
