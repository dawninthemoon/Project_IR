using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using UnityEngine;

public enum AIEventType
{
    AIEvent_Test,
    AIEvent_SetAngleDirection,
    AIEvent_SetDirectionToTarget,
    AIEvent_SetAction,
    AIEvent_ClearTarget,
    AIEvent_ExecuteState,
    AIEvent_TerminatePackage,
    AIEvent_KillEntity,
    AIEvent_RotateDirection,
    AIEvent_CallAIEvent,
    AIEvent_SetCustomValue,
    AIEvent_AddCustomValue,
    AIEvent_SequencerSignal,
    Count,
}

public enum AIChildEventType
{
    AIChildEvent_OnExecute = 0,
    AIChildEvent_OnExit,
    AIChildEvent_OnFrame,
    AIChildEvent_OnUpdate,

    AIChildEvent_OnAttack,
    AIChildEvent_OnAttacked,

    AIChildEvent_OnGuard,
    AIChildEvent_OnGuarded,

    AIChildEvent_OnParry,
    AIChildEvent_OnParried,

    AIChildEvent_OnEvade,
    AIChildEvent_OnEvaded,

    AIChildEvent_OnGuardBreak,
    AIChildEvent_OnGuardBroken,

    AIChildEvent_OnGuardBreakFail,
    AIChildEvent_OnAttackGuardBreakFail,

    AIChildEvent_OnCatchTarget,
    AIChildEvent_OnCatched,

    AIChildEvent_Custom,
    

    Count,
}

public abstract class AIEventBase
{
    public abstract AIEventType getFrameEventType();
    public virtual void initialize(){}
    public abstract void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null);
    public abstract void loadFromXML(XmlNode node);

}


public class AIEvent_SequencerSignal : AIEventBase
{
    private string _signal = "";
    private float _value;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SequencerSignal;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).addSequencerSignal(_signal);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Signal")
                _signal = attrValue;
        }
    }
}

public class AIEvent_AddCustomValue : AIEventBase
{
    private string _customValueName = "";
    private float _value;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_AddCustomValue;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).addCustomValue(_customValueName, _value);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Name")
            {
                _customValueName = attrValue;
            }
            else if(attrName == "Value")
            {
                _value = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }
}

public class AIEvent_SetCustomValue : AIEventBase
{
    private string _customValueName = "";
    private float _value;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetCustomValue;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).setCustomValue(_customValueName,_value);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Name")
            {
                _customValueName = attrValue;
            }
            else if(attrName == "Value")
            {
                _value = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }
}

public class AIEvent_CallAIEvent : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_CallAIEvent;}

    
    public string _customAiEventName = "";
    
    public CallAIEventTargetType _targetType = CallAIEventTargetType.Self;
    public float _range = 0f;

    public override void initialize()
    {
    }

    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        ObjectBase executeTargetEntity = null;
        switch(_targetType)
        {
            case CallAIEventTargetType.Self:
            {
                executeTargetEntity = executeEntity;
            }
            break;
            case CallAIEventTargetType.FrameEventTarget:
            {
                executeTargetEntity = targetEntity;
            }
            break;
            case CallAIEventTargetType.Summoner:
            {
                executeTargetEntity = executeEntity.getSummonObject();
            }
            break;
        }

        if(executeTargetEntity == null || executeTargetEntity is GameEntityBase == false)
            return;

        (executeTargetEntity as GameEntityBase).executeCustomAIEvent(_customAiEventName);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attributes[i].Name == "EventName")
            {
                _customAiEventName = attributes[i].Value;
            }
            else if(attrName == "EventTargetType")
            {
                _targetType = (CallAIEventTargetType)System.Enum.Parse(typeof(CallAIEventTargetType), attrValue);
            }

        }
    }
}

public class AIEvent_RotateDirection : AIEventBase
{
    private float _time;
    private float _rotateAngle;

    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_RotateDirection;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;

        (executeEntity as GameEntityBase).setAIDirectionRotateProcess(_time,_rotateAngle);
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Time")
            {
                _time = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "RotateAngle")
            {
                _rotateAngle = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }
}

public class AIEvent_KillEntity : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_KillEntity;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        executeEntity.deactive();
        executeEntity.DeregisterRequest();
    }

    public override void loadFromXML(XmlNode node) 
    {
        
    }
}


public class AIEvent_SetAction : AIEventBase
{
    string actionName = "";
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetAction;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setAction(actionName);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Action")
            {
                actionName = attrValue;
            }
        }
    }
}

public class AIEvent_TerminatePackage : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_TerminatePackage;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.terminateAIPackage();
    }

    public override void loadFromXML(XmlNode node) 
    {

    }
}

public class AIEvent_ExecuteState : AIEventBase
{
    public int targetStateIndex = -1;
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_ExecuteState;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setAIState(targetStateIndex);
    }

    public override void loadFromXML(XmlNode node) 
    {

    }
}

public class AIEvent_ClearTarget : AIEventBase
{
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_ClearTarget;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setTargetEntity(null);
    }

    public override void loadFromXML(XmlNode node) {}
}


public class AIEvent_SetDirectionToTarget : AIEventBase
{
    float _directionAngle = 0f;
    float _rotateSpeed = -1f;
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetDirectionToTarget;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        if(executeGameEntity.getCurrentTargetEntity() == null)
            return;

        Vector3 direction = executeGameEntity.getCurrentTargetEntity().transform.position - executeGameEntity.transform.position;
        direction.Normalize();
        direction = Quaternion.Euler(0f,0f,_directionAngle) * direction;

        if(_rotateSpeed == -1f)
        {
            executeGameEntity.setAiDirection(direction);
        }
        else
        {
            Vector3 currentDirection = executeGameEntity.getAiDirection();
            float angle = Vector3.SignedAngle(currentDirection, direction, Vector3.forward);
            float theta = Mathf.MoveTowardsAngle(0f,angle,_rotateSpeed * GlobalTimer.Instance().getSclaedDeltaTime());

            executeGameEntity.setAiDirection(Quaternion.Euler(0f,0f,theta) * currentDirection);
        }
        
    }

    public override void loadFromXML(XmlNode node) 
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Angle")
            {
                _directionAngle = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "Speed")
            {
                _rotateSpeed = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }
}

public class AIEvent_SetAngleDirection : AIEventBase
{
    public float angleDirection;
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_SetAngleDirection;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        if(executeEntity is GameEntityBase == false)
            return;
        
        GameEntityBase executeGameEntity = (GameEntityBase)executeEntity;
        executeGameEntity.setAiDirection(angleDirection);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Angle")
            {
                angleDirection = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
        }
    }   
}

public class AIEvent_Test : AIEventBase
{
    string log = "";
    public override AIEventType getFrameEventType() {return AIEventType.AIEvent_Test;}
    public override void onExecute(ObjectBase executeEntity, ObjectBase targetEntity = null)
    {
        DebugUtil.log(log);
    }

    public override void loadFromXML(XmlNode node)
    {
        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            string attrName = attributes[i].Name;
            string attrValue = attributes[i].Value;

            if(attrName == "Log")
            {
                log = attrValue;
            }
        }
    }
}

public class AIChildFrameEventItem
{
    public bool _consume = false;

    public AIEventBase[] _childFrameEvents;
    public int _childFrameEventCount;

}

public class CustomAIChildFrameEventItem : AIChildFrameEventItem
{
    public string _eventName = "";
}