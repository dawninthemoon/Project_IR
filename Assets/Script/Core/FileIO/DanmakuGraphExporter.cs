using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class DanmakuGraphLoader : LoaderBase<DanmakuGraphBaseData>
{
    static string _currentFileName = "";
    public override DanmakuGraphBaseData readFromXML(string path)
    {
        path = "Assets\\Data\\DanmakuGraph\\" + path;
        _currentFileName = path;
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(path,readerSettings))
            {
                xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }

        if(xmlDoc.FirstChild.Name.Equals("DanmakuGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",xmlDoc.FirstChild.Name);
            return null;
        }

        XmlNode danmakuNodes = xmlDoc.FirstChild;
        DanmakuGraphBaseData baseData = readDanmakuGraph(danmakuNodes);

        return baseData;
    }

    private static DanmakuGraphBaseData readDanmakuGraph(XmlNode node)
    {
        DanmakuGraphBaseData danmakuBaseData = new DanmakuGraphBaseData();
        readTitle(node,danmakuBaseData);

        List<DanmakuEventBase> danamkuEvents = new List<DanmakuEventBase>();

        XmlNodeList nodes = node.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            danamkuEvents.Add(readDanmakuEvent(nodes[nodeIndex]));
        }

        danmakuBaseData._danamkuEventCount = danamkuEvents.Count;
        danmakuBaseData._danamkuEventList = danamkuEvents.ToArray();

        return danmakuBaseData;
    }

    private static DanmakuEventBase readDanmakuEvent(XmlNode node)
    {

        if(node.Name == "Loop")
        {
            return readDanmakuLoopEvent(node);
        }
        else if(node.Name == "Projectile")
        {
            DanmakuProjectileEventData projectileEvent = new DanmakuProjectileEventData();

            XmlAttributeCollection attributes = node.Attributes;
            for(int i = 0; i < attributes.Count; ++i)
            {
                if(attributes[i].Name == "Name")
                {
                    projectileEvent._projectileName = attributes[i].Value;
                }
                else if(attributes[i].Name == "ShotInfoUseType")
                {
                    projectileEvent._shotInfoUseType = (ActionFrameEvent_Projectile.ShotInfoUseType)System.Enum.Parse(typeof(ActionFrameEvent_Projectile.ShotInfoUseType),attributes[i].Value);
                }
                else if(attributes[i].Name == "DirectionType")
                {
                    projectileEvent._directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), attributes[i].Value);
                }
                else if(attributes[i].Name == "StartTerm")
                {
                    projectileEvent._startTerm = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
                }
                else if(attributes[i].Name == "PredictionAccuracy")
                {
                    projectileEvent._predictionAccuracy = int.Parse(attributes[i].Value);
                    projectileEvent._pathPredictionArray = new Vector3[projectileEvent._predictionAccuracy];
                }
                else if(attributes[i].Name == "PredictionType")
                {
                    projectileEvent._predictionType = (ActionFrameEvent_Projectile.PredictionType)System.Enum.Parse(typeof(ActionFrameEvent_Projectile.PredictionType), attributes[i].Value);
                }
                else if(attributes[i].Name == "SpawnTargetType")
                {
                    if(attributes[i].Value == "Self")
                        projectileEvent._setTargetType = SetTargetType.SetTargetType_Self;
                    else if(attributes[i].Value == "Target")
                        projectileEvent._setTargetType = SetTargetType.SetTargetType_Target;
                    else if(attributes[i].Value == "AITarget")
                        projectileEvent._setTargetType = SetTargetType.SetTargetType_AITarget;
                    else
                    {
                        DebugUtil.assert(false,"invalid targetType: {0}", attributes[i].Value);
                    }
                }
                
            }

            return projectileEvent;
        }
        else if(node.Name == "Wait")
        {
            DanmakuWaitEventData waitEvent = new DanmakuWaitEventData();
            waitEvent._waitTime = XMLScriptConverter.valueToFloatExtend(node.Attributes[0].Value);

            return waitEvent;
        }
        else
        {
            DanmakuVariableType targetType = DanmakuVariableType.Count;
            if(System.Enum.TryParse<DanmakuVariableType>(node.Name, out targetType))
            {
                DanmakuVariableEventData variableEventData = new DanmakuVariableEventData();
                List<DanmakuVariableEventType> eventTypeList = new List<DanmakuVariableEventType>();
                List<FloatEx> eventValueList = new List<FloatEx>();

                XmlAttributeCollection attributes = node.Attributes;
                for(int i = 0; i < attributes.Count; ++i)
                {
                    DanmakuVariableEventType variableEventType = DanmakuVariableEventType.Count;
                    if(System.Enum.TryParse<DanmakuVariableEventType>(attributes[i].Name, out variableEventType) == false)
                    {
                        DebugUtil.assert(false,"invalid danmaku variable event type: {0} [Line: {1}] [FileName: {2}]", attributes[i].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                        return null;
                    }

                    eventTypeList.Add(variableEventType);

                    FloatEx newFloat = new FloatEx();
                    newFloat.loadFromXML(attributes[i].Value);
                        
                    eventValueList.Add(newFloat);
                }

                variableEventData._eventCount = eventTypeList.Count;
                variableEventData._eventType = eventTypeList.ToArray();
                variableEventData._type = targetType;
                variableEventData._value = eventValueList.ToArray();

                return variableEventData;
            }
            else
            {
                DebugUtil.assert(false,"invalid danmaku event type: {0} [Line: {1}] [FileName: {2}]", node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
            }
        }

        return null;
    }

    private static DanmakuLoopEventData readDanmakuLoopEvent(XmlNode node)
    {
        DanmakuLoopEventData loopEvent = new DanmakuLoopEventData();

        XmlAttributeCollection attributes = node.Attributes;
        for(int i = 0; i < attributes.Count; ++i)
        {
            if(attributes[i].Name == "Count")
            {
                loopEvent._loopCount = int.Parse(attributes[i].Value);
            }
            else if(attributes[i].Name == "Term")
            {
                loopEvent._term = XMLScriptConverter.valueToFloatExtend(attributes[i].Value);
            }
        }

        List<DanmakuEventBase> eventList = new List<DanmakuEventBase>();

        XmlNodeList childNodes = node.ChildNodes;
        for(int i = 0; i < childNodes.Count; ++i)
        {
            eventList.Add(readDanmakuEvent(childNodes[i]));
        }

        loopEvent._eventCount = eventList.Count;
        loopEvent._events = eventList.ToArray();

        return loopEvent;
    }

    private static void readTitle(XmlNode node, DanmakuGraphBaseData baseData)
    {

        baseData._name = node.Attributes[0].Value;
    }

}
