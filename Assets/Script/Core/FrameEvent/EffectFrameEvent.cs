using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public enum AngleDirectionType
{
    identity,
    Direction,
    AttackPoint,
}

[System.Serializable]
public class ActionFrameEvent_ParticleEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_ParticleEffect;}

    public string               _effectPath = "";
    public bool                _toTarget = false;
    public bool                _attach = false;
    public bool                _followDirection = false;
    public bool                _castShadow = false;
    public float               _lifeTime = 0f;

    public Vector3             _spawnOffset = Vector3.zero;
    public Quaternion          _effectRotation = Quaternion.identity;
    public EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + _spawnOffset;
        requestData._rotation = getAngleByType(executeEntity, requestData._position);
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.ParticleEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }


        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    public Quaternion getAngleByType(ObjectBase executeEntity, Vector3 effectPosition)
    {
        switch(_angleDirectionType)
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

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
            {
                _effectPath = attributes[i].Value;
            }
            else if(attributes[i].Name == "Offset")
            {
                string[] vector = attributes[i].Value.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert(false, "invalid vector3 data: {0}",attributes[i].Value);
                    return;
                }

                _spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                _spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                _spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "AngleType")
            {
                _angleDirectionType = (AngleDirectionType)System.Enum.Parse(typeof(AngleDirectionType), attributes[i].Value);
            }
            else if(attributes[i].Name == "LifeTime")
            {
                _lifeTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
            else if(attributes[i].Name == "FollowDirection")
            {
                _followDirection = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "CastShadow")
            {
                _castShadow = bool.Parse(attributes[i].Value);
            }
        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential");
    }
}

[System.Serializable]
public class ActionFrameEvent_TimelineEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_TimelineEffect;}

    public string               _effectPath = "";
    public string               _attackPresetName = "";
    public bool                _toTarget = false;
    public bool                _attach = false;
    public bool                _followDirection = false;
    public bool                _castShadow = false;
    public float               _lifeTime = 0f;

    public Vector3             _spawnOffset = Vector3.zero;
    public EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + _spawnOffset;
        requestData._rotation = getAngleByType(executeEntity, requestData._position);
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.TimelineEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;
        requestData._attackDataName = _attackPresetName;

        if(_attach)
        {
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;
            requestData._timelineAnimator = _toTarget ? targetEntity.getAnimator() : executeEntity.getAnimator();
        }


        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    public Quaternion getAngleByType(ObjectBase executeEntity, Vector3 effectPosition)
    {
        switch(_angleDirectionType)
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

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
            {
                _effectPath = attributes[i].Value;
            }
            else if(attributes[i].Name == "Offset")
            {
                string[] vector = attributes[i].Value.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert(false, "invalid vector3 data: {0}",attributes[i].Value);
                    return;
                }

                _spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                _spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                _spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "AngleType")
            {
                _angleDirectionType = (AngleDirectionType)System.Enum.Parse(typeof(AngleDirectionType), attributes[i].Value);
            }
            else if(attributes[i].Name == "LifeTime")
            {
                _lifeTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
            else if(attributes[i].Name == "FollowDirection")
            {
                _followDirection = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "CastShadow")
            {
                _castShadow = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "AttackPreset")
            {
                _attackPresetName = attributes[i].Value;
            }
        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential");
    }
}

public class ActionFrameEvent_AnimationEffect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_AnimationEffect;}

    public string              _effectPath = "";
    public bool                _toTarget = false;
    public bool                _attach = false;
    public bool                _attachToSpriteObject = false;
    public bool                _followDirection = false;
    public bool                _castShadow = false;
    public float               _lifeTime = 0f;

    public Vector3             _spawnOffset = Vector3.zero;
    public EffectUpdateType    _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public AngleDirectionType  _angleDirectionType = AngleDirectionType.identity;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._position = centerPosition + _spawnOffset;
        requestData._rotation = getAngleByType(executeEntity, requestData._position);
        requestData._updateType = _effectUpdateType;
        requestData._effectType = EffectType.TimelineEffect;
        requestData._lifeTime = _lifeTime;
        requestData._executeEntity = executeEntity;
        requestData._followDirection = _followDirection;
        requestData._castShadow = _castShadow;

        if(_attach)
        {
            requestData._parentTransform = _toTarget 
                            ? (_attachToSpriteObject ? targetEntity.getSpriteRendererTransform() : targetEntity.transform ) 
                            : (_attachToSpriteObject ? executeEntity.getSpriteRendererTransform() : executeEntity.transform);
        }

        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    public Quaternion getAngleByType(ObjectBase executeEntity, Vector3 effectPosition)
    {
        switch(_angleDirectionType)
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

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
            {
                _effectPath = attributes[i].Value;
            }
            else if(attributes[i].Name == "Offset")
            {
                string[] vector = attributes[i].Value.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert(false, "invalid vector3 data: {0}",attributes[i].Value);
                    return;
                }

                _spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                _spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                _spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "AngleType")
            {
                _angleDirectionType = (AngleDirectionType)System.Enum.Parse(typeof(AngleDirectionType), attributes[i].Value);
            }
            else if(attributes[i].Name == "LifeTime")
            {
                _lifeTime = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
            else if(attributes[i].Name == "FollowDirection")
            {
                _followDirection = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "CastShadow")
            {
                _castShadow = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "MaterialEffect")
            {
                _attachToSpriteObject = bool.Parse(attributes[i].Value);
            }
        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential");
    }
}


[System.Serializable]
public class ActionFrameEvent_Effect : ActionFrameEventBase
{
    public override FrameEventType getFrameEventType(){return FrameEventType.FrameEvent_Effect;}

    public string _effectPath = "";

    public float _framePerSecond = 1f;

    public float _spawnAngle = 0f;

    public bool _random = false;
    public Vector2 _randomValue = Vector2.zero;

    public bool _followEntity = false;
    public bool _toTarget = false;

    public bool _attach = false;

    public Vector3 _spawnOffset = Vector3.zero;

    public bool _usePhysics = false;
    public bool _useFlip = false;
    public bool _castShadow = false;
    public PhysicsBodyDescription _physicsBodyDesc = new PhysicsBodyDescription(null);
    public EffectUpdateType _effectUpdateType = EffectUpdateType.ScaledDeltaTime;

    public override bool onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        Vector3 centerPosition;
        if(_toTarget)
            centerPosition = targetEntity.transform.position;
        else
            centerPosition = executeEntity.transform.position;
        
        Quaternion directionAngle = Quaternion.Euler(0f,0f,Vector3.SignedAngle(Vector3.right, executeEntity.getDirection(), Vector3.forward));

        EffectRequestData requestData = MessageDataPooling.GetMessageData<EffectRequestData>();
        requestData.clearRequestData();
        requestData._effectPath = _effectPath;
        requestData._startFrame = 0f;
        requestData._endFrame = -1f;
        requestData._framePerSecond = _framePerSecond;
        requestData._position = centerPosition + (directionAngle  * _spawnOffset);
        requestData._usePhysics = _usePhysics;
        requestData._rotation = directionAngle;
        requestData._effectType = EffectType.SpriteEffect;
        requestData._updateType = _effectUpdateType;
        requestData._castShadow = _castShadow;

        if(_attach)
            requestData._parentTransform = _toTarget ? targetEntity.transform : executeEntity.transform;

        if(_useFlip && executeEntity is GameEntityBase == true)
        {
            GameEntityBase requester = (GameEntityBase)executeEntity;
            requestData._useFlip = requester.getFlipState().xFlip;
        }
        
        PhysicsBodyDescription physicsBody = _physicsBodyDesc;
        if(_usePhysics)
        {
            float angle = 0f;
            if(executeEntity is GameEntityBase)
            {
                GameEntityBase requester = (GameEntityBase)executeEntity;
                MathEx.directionToAngle(executeEntity.getDirection());
                if(_useFlip && requester.getFlipState().xFlip)
                {
                    angle -= 180f;
                    angle *= -1f;
                }
            }

            physicsBody._velocity.setValue(Quaternion.Euler(0f,0f, angle) * physicsBody._velocity.getValue());
        }

        requestData._physicsBodyDesc = physicsBody;

        if(_followEntity == true)
        {
            requestData._angle = executeEntity.getSpriteRendererTransform().rotation.eulerAngles.z;
        }
        else if(_random == true)
        {
            requestData._angle = Random.Range(_randomValue.x,_randomValue.y);
        }
        else
        {
            requestData._angle = _spawnAngle;
        }

        executeEntity.SendMessageEx(MessageTitles.effect_spawnEffect,UniqueIDBase.QueryUniqueID("EffectManager"),requestData);

        return true;
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Path")
                _effectPath = attributes[i].Value;
            // else if(attributes[i].Name == "StartFrame")
            //     _aniStartFrame = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            // else if(attributes[i].Name == "EndFrame")
            //     _aniEndFrame = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "FramePerSecond")
                _framePerSecond = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            else if(attributes[i].Name == "Offset")
            {
                string[] vector = attributes[i].Value.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert(false, "invalid vector3 data: {0} [FileName: {1}] [Line: {2}]",attributes[i].Value,node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node));
                    return;
                }

                _spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                _spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                _spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attributes[i].Name == "Angle")
            {
                if(attributes[i].Value.Contains("Random_"))
                {
                    string data = attributes[i].Value.Replace("Random_","");
                    string[] randomData = data.Split('^');
                    if(randomData == null || randomData.Length != 2)
                    {
                        DebugUtil.assert(false, "invalid float2 data: {0}, {1} [FileName: {2}] [Line: {3}]",attributes[i].Name, attributes[i].Value,node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node));
                        return;
                    }
                    
                    _randomValue = new Vector2(XMLScriptConverter.valueToFloatExtend(randomData[0]),XMLScriptConverter.valueToFloatExtend(randomData[1]));
                    _random = true;
                }
                else if(attributes[i].Value == "FollowEntity")
                {
                    _followEntity = true;
                }
                else
                {
                    float angleValue = 0f;
                    if(float.TryParse(attributes[i].Value,out angleValue) == false)
                    {
                        DebugUtil.assert(false, "invalid float data: {0}, {1} [FileName: {2}] [Line: {3}]",attributes[i].Name, attributes[i].Value,node.BaseURI,XMLScriptConverter.getLineNumberFromXMLNode(node));
                        return;
                    }

                    _spawnAngle = angleValue;
                }
            }
            else if(attributes[i].Name == "ToTarget")
            {
                _toTarget = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UseFlip")
            {
                _useFlip = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "UpdateType")
            {
                _effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attributes[i].Value);
            }
            else if(attributes[i].Name == "Attach")
            {
                _attach = bool.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "CastShadow")
            {
                _castShadow = bool.Parse(attributes[i].Value);
            }

        }

        if(node.HasChildNodes)
        {
            XmlNodeList childNodes = node.ChildNodes;
            for(int i = 0; i < childNodes.Count; ++i)
            {
                string attrName = childNodes[i].Name;
                string attrValue = childNodes[i].Value;

                if(attrName == "Physics")
                {
                    _usePhysics = true;
                    XmlAttributeCollection physicsAttributes = childNodes[i].Attributes;
                    for(int j = 0; j < physicsAttributes.Count; ++j)
                    {
                        if(physicsAttributes[j].Name == "UseGravity")
                        {
                            _physicsBodyDesc._useGravity = bool.Parse(physicsAttributes[j].Value);
                        }
                        else if(physicsAttributes[j].Name == "Velocity")
                        {
                            _physicsBodyDesc._velocity.loadFromXML(physicsAttributes[j].Value);// =  new Vector3(StringDataUtil.readFloat(floatList[0]),StringDataUtil.readFloat(floatList[1]),0f);
                        }
                        else if(physicsAttributes[j].Name == "Friction")
                        {
                            _physicsBodyDesc._friction = XMLScriptConverter.valueToFloatExtend(physicsAttributes[j].Value);
                        }
                        else if(physicsAttributes[j].Name == "Torque")
                        {
                            _physicsBodyDesc._torque.loadFromXML(physicsAttributes[j].Value);
                        }
                        else if(physicsAttributes[j].Name == "AngularFriction")
                        {
                            _physicsBodyDesc._angularFriction = StringDataUtil.readFloat(physicsAttributes[j].Value);
                        }
                        else
                        {
                            DebugUtil.assert(false,"invalid physics attribute data: {0}", physicsAttributes[j].Name);
                        }
                    }
                }
            }

        }

        if(_effectPath == "")
            DebugUtil.assert(false, "effect path is essential [Line: {0}]", XMLScriptConverter.getLineFromXMLNode(node));
    }
}