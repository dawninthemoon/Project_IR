using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class ActionFrameEvent_Projectile : ActionFrameEventBase
{
    public enum ShotInfoUseType
    {
        UseDefault,
        Overlap,
        Add,
    };

    public enum PredictionType
    {
        Path,
        StartPosition,
    }

    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Projectile;}

    public string                           _projectileGraphName = "";

    private ProjectileGraphShotInfoData     _shotInfo;
    private ShotInfoUseType                 _useType = ShotInfoUseType.UseDefault;
    private DirectionType                   _directionType = DirectionType.Count;

    private SetTargetType                   _setTargetType = SetTargetType.SetTargetType_Self;

    private Vector3                         _positionOffset = Vector3.zero;

    private SearchIdentifier                _targetSearchIdentifier = SearchIdentifier.Count;
    private PredictionType                  _predictionType = PredictionType.Path;
    private Vector3[]                       _pathPredictionArray = null;
    private int                             _predictionAccuracy = 0;
    private float                           _startTerm = 0f;
    private float                           _directionAngle = 0f;
    bool                                    _useFlip = false;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(ProjectileManager._instance == null)
        {
            DebugUtil.assert(false, "projectile execute order error!!");
            return true;
        }

        if(executeEntity is GameEntityBase == false)
            return true;

        GameEntityBase gameEntityBase = executeEntity as GameEntityBase;

        float defaultAngle = getDefaultAngle(gameEntityBase, _directionType) + _directionAngle;

        ProjectileGraphShotInfoData shotInfo;
        getShotInfo(_projectileGraphName,_useType,defaultAngle,ref _shotInfo,out shotInfo);
        
        Vector3 offsetPosition = _positionOffset;
        float angle = MathEx.directionToAngle(executeEntity.getDirection());
        if(_useFlip && gameEntityBase.getFlipState().xFlip)
            offsetPosition.y *= -1f;

        offsetPosition = Quaternion.Euler(0f,0f,angle) * offsetPosition;

        Vector3 spawnPosition = getSpawnPosition(_setTargetType, executeEntity, targetEntity) + offsetPosition;
        
        if(_startTerm != 0f)
        {
            if(_pathPredictionArray != null)
            {
                switch(_predictionType)
                {
                    case PredictionType.Path:
                    {
                        predictionPath(_predictionAccuracy, _pathPredictionArray, ref shotInfo);

                        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
                        requestData.clearRequestData();
                        requestData._updateType = EffectUpdateType.NoneScaledDeltaTime;
                        requestData._effectType = EffectType.TrailEffect;
                        requestData._lifeTime = _startTerm;
                        requestData._parentTransform = executeEntity.transform;
                        requestData._trailWidth = ProjectileManager._instance.getProjectileGraphData(_projectileGraphName)._collisionRadius * 2f;
                        requestData._trailMaterial = ResourceContainerEx.Instance().GetMaterial("Material/Material_TrailBase");
                        requestData._trailPositionData = _pathPredictionArray;

                        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);
                    }
                    break;
                    case PredictionType.StartPosition:
                    {
                        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
                        requestData.clearRequestData();
                        requestData._effectPath = "Resources/Sprites/Effect/ProjectilePredictionPosition";
                        requestData._startFrame = 0f;
                        requestData._endFrame = -1f;
                        requestData._framePerSecond = 1.0f / _startTerm;
                        requestData._position = spawnPosition;
                        requestData._usePhysics = false;
                        requestData._rotation = Quaternion.identity;
                        requestData._effectType = EffectType.SpriteEffect;
                        requestData._updateType = EffectUpdateType.NoneScaledDeltaTime;
                        requestData._castShadow = false;
                        float radius = ProjectileManager._instance.getProjectileGraphData(_projectileGraphName)._collisionRadius * 2f;
                        requestData._scale = new Vector3(radius,radius,1f);

                        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);
                    }
                    break;
                }
                
            }
            
            ProjectileManager._instance.spawnProjectileDelayed(_projectileGraphName, _startTerm,executeEntity,targetEntity,_setTargetType,ref shotInfo,executeEntity,_targetSearchIdentifier == SearchIdentifier.Count ? executeEntity._searchIdentifier : _targetSearchIdentifier);
        }
        else
        {
            ProjectileManager._instance.spawnProjectile(_projectileGraphName,ref shotInfo,spawnPosition,executeEntity,_targetSearchIdentifier == SearchIdentifier.Count ? executeEntity._searchIdentifier : _targetSearchIdentifier);
        }
        
        return true;
    }

    public static void predictionPath(int accuracy, Vector3[] predictionArray, ref ProjectileGraphShotInfoData shotInfo)
    {
        float currentVelocity = shotInfo._deafaultVelocity;
        float currentAngle = shotInfo._defaultAngle;

        float stepDeltaTime = shotInfo._lifeTime / (float)(accuracy - 1);
        Vector3 evaluatePosition = Vector3.zero;

        int stepIndex = 0;
        predictionArray[stepIndex] = evaluatePosition;

        while(++stepIndex < accuracy)
        {
            currentVelocity += shotInfo._acceleration * stepDeltaTime;

            if(shotInfo._friction != 0f)
                currentVelocity = MathEx.convergence0(currentVelocity,shotInfo._friction * stepDeltaTime);
            if(shotInfo._angularAcceleration != 0f)
                currentAngle += shotInfo._angularAcceleration * stepDeltaTime;

            evaluatePosition += (currentVelocity * stepDeltaTime) * (UnityEngine.Quaternion.Euler(0f,0f,currentAngle) * UnityEngine.Vector3.right);

            predictionArray[stepIndex] = evaluatePosition;
        }

    }

    public static float getDefaultAngle(GameEntityBase executeEntity, DirectionType directionType)
    {
        Vector3 direction = Vector3.zero;
        if(directionType != DirectionType.Count)
            direction = ((GameEntityBase)executeEntity).getDirectionFromType(directionType);

        return Quaternion.Euler(0f,0f,Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg).eulerAngles.z;
    }

    public static Vector3 getSpawnPosition(SetTargetType targetType, ObjectBase executeEntity, ObjectBase targetEntity)
    {
        Vector3 spawnPosition = Vector3.zero;
        switch(targetType)
        {
            case SetTargetType.SetTargetType_Self:
            spawnPosition = executeEntity.transform.position;
            break;
            case SetTargetType.SetTargetType_Target:
            spawnPosition = targetEntity == null ? executeEntity.transform.position : targetEntity.transform.position;
            break;
            case SetTargetType.SetTargetType_AITarget:
            GameEntityBase aiTarget = ((GameEntityBase)executeEntity).getCurrentTargetEntity();
            spawnPosition = aiTarget == null ? executeEntity.transform.position : aiTarget.transform.position;
            break;
        }

        return spawnPosition;
    }

    

    public static bool getShotInfo(string projectileGraphName, ShotInfoUseType useType, float defaultAngle, ref ProjectileGraphShotInfoData defaultShotInfo, out ProjectileGraphShotInfoData outShotInfo)
    {
        ProjectileGraphBaseData baseData = ProjectileManager._instance.getProjectileGraphData(projectileGraphName);

        if(baseData == null)
        {
            outShotInfo = new ProjectileGraphShotInfoData();
            return false;
        }

        outShotInfo = baseData._defaultProjectileShotInfoData;

        switch(useType)
        {
            case ShotInfoUseType.UseDefault:
            break;
            case ShotInfoUseType.Overlap:
            outShotInfo = defaultShotInfo;
            break;
            case ShotInfoUseType.Add:
            outShotInfo += defaultShotInfo;
            break;
        }

        outShotInfo._defaultAngle += defaultAngle;
        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        _shotInfo.clear();

        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "GraphName")
            {
                _projectileGraphName = attrValue;
            }
            else if(attrName == "StartTerm")
            {
                _startTerm = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "PredictionAccuracy")
            {
                _predictionAccuracy = int.Parse(attrValue);
                _pathPredictionArray = new Vector3[_predictionAccuracy];
            }
            else if(attrName == "Velocity")
            {
                _shotInfo._deafaultVelocity = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "Acceleration")
            {
                _shotInfo._acceleration = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "Friction")
            {
                _shotInfo._friction = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "Angle")
            {
                if(attrValue.Contains("Random"))
                {
                    string randomData = attrValue.Replace("Random_","");
                    string[] randomValue = randomData.Split('^');
                    if(randomValue == null || randomValue.Length != 2)
                    {
                        DebugUtil.assert(false, "invalid random angle attrubute: {0}", attrValue);
                        continue;
                    }

                    _shotInfo._useRandomAngle = true;
                    _shotInfo._randomAngle = new Vector2(XMLScriptConverter.valueToFloatExtend(randomValue[0]),XMLScriptConverter.valueToFloatExtend(randomValue[1]));
                }
                else
                {
                    _shotInfo._defaultAngle = XMLScriptConverter.valueToFloatExtend(attrValue);
                }
            }
            else if(attrName == "AngularAcceleration")
            {
                _shotInfo._angularAcceleration = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "LifeTime")
            {
                _shotInfo._lifeTime = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "DirectionType")
            {
                _directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), attrValue);
            }
            else if(attrName == "ShotInfoUseType")
            {
                _useType = (ShotInfoUseType)System.Enum.Parse(typeof(ShotInfoUseType), attrValue);
            }
            else if(attrName == "PredictionType")
            {
                _predictionType = (PredictionType)System.Enum.Parse(typeof(PredictionType), attrValue);
            }
            else if(attrName == "SpawnTargetType")
            {
                if(attrValue == "Self")
                    _setTargetType = SetTargetType.SetTargetType_Self;
                else if(attrValue == "Target")
                    _setTargetType = SetTargetType.SetTargetType_Target;
                else if(attrValue == "AITarget")
                    _setTargetType = SetTargetType.SetTargetType_AITarget;
                else
                {
                    DebugUtil.assert(false,"invalid targetType: {0}", attrValue);
                }
            }
            else if(attrName == "Offset")
            {
                _positionOffset = XMLScriptConverter.valueToVector3(attrValue);
            }
            else if(attrName == "UseFlip")
            {
                _useFlip = bool.Parse(attrValue);
            }
            else if(attrName == "SearchIdentifier")
            {
                _targetSearchIdentifier = (SearchIdentifier)System.Enum.Parse(typeof(SearchIdentifier), attrValue);
            }
            else if(attrName == "DirectionAngle")
            {
                _directionAngle = float.Parse(attrValue);
            }
        }

        if(_projectileGraphName == "")
            DebugUtil.assert(false, "projectile graph is essential");
    }
}