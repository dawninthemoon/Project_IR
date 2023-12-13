using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public abstract class EffectInfoDataBase
{
    public abstract EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity);
    public abstract bool compareMaterial(CommonMaterial attackMaterial, CommonMaterial defenceMaterial);
    public virtual bool compareMaterialExactly(CommonMaterial attackMaterial, CommonMaterial defenceMaterial) {return true;}
    protected Quaternion getAngleByType(ObjectBase executeEntity, Vector3 effectPosition, AngleDirectionType angleDirectionType)
    {
        switch(angleDirectionType)
        {
            case AngleDirectionType.identity:
                return Quaternion.identity;
            case AngleDirectionType.Direction:
                return Quaternion.Euler(0f,0f,MathEx.directionToAngle(executeEntity.getDirection()));
            case AngleDirectionType.AttackPoint:
            {
                if(executeEntity is GameEntityBase == false)
                    return Quaternion.identity;

                Vector3 direction = effectPosition - (executeEntity as GameEntityBase).getAttackPoint();
                direction.Normalize();

                return Quaternion.Euler(0f,0f,MathEx.directionToAngle(direction));
            }
        }

        return Quaternion.identity;
    }
}

public class ParticleEffectInfoData : EffectInfoDataBase
{
    public string              _key = "";
    public string              _effectPath = "";
    public bool                _toTarget = false;
    public bool                _attach = false;
    public bool                _followDirection = false;
    public bool                _castShadow = false;
    public bool                _dependentAction = false;
    public float               _angleOffset = 0f;
    public float               _lifeTime = 0f;

    public CommonMaterial      _attackMaterial = CommonMaterial.Empty;
    public CommonMaterial      _defenceMaterial = CommonMaterial.Empty;

    public Vector3             _spawnOffset = Vector3.zero;
    public Quaternion          _effectRotation = Quaternion.identity;
    public EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;

    public override EffectRequestData createEffectRequestData(ObjectBase executeEntity, ObjectBase targetEntity)
    {
        Vector3 centerPosition;
        if(_toTarget && targetEntity == null)
        {
            DebugUtil.assert(false,"대상이 존재하는 상황에서만 사용할 수 있는 이펙트 입니다. [EffectInfo: {0}] [ToTarget: {1}]",_key,_toTarget);
            return null;
        }

        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._rotation = getAngleByType(executeEntity, requestData._position, _angleDirectionType);
        requestData._position = centerPosition + requestData._rotation * _spawnOffset;
        requestData._rotation *=  Quaternion.Euler(0f,0f,_angleOffset);
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.ParticleEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;
        requestData._dependentAction = _dependentAction;

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }

        return requestData;
    }

    public override bool compareMaterial(CommonMaterial attackMaterial, CommonMaterial defenceMaterial)
    {
        if(_attackMaterial == CommonMaterial.Empty && _defenceMaterial != CommonMaterial.Empty)
            return _defenceMaterial == defenceMaterial;
        else if(_attackMaterial != CommonMaterial.Empty && _defenceMaterial == CommonMaterial.Empty)
            return _attackMaterial == defenceMaterial;
        else if(_attackMaterial == CommonMaterial.Empty && _defenceMaterial == CommonMaterial.Empty)
            return true;
            
        return compareMaterialExactly(attackMaterial, defenceMaterial);
    }

    public override bool compareMaterialExactly(CommonMaterial attackMaterial, CommonMaterial defenceMaterial) 
    {
        return _attackMaterial == attackMaterial && _defenceMaterial == defenceMaterial;
    }

}
