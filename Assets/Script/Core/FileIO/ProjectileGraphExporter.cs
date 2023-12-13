using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class ProjectileGraphLoader : LoaderBase<ProjectileGraphBaseData[]>
{
    static string _currentFileName = "";
    public override ProjectileGraphBaseData[] readFromXML(string path)
    {
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

        if(xmlDoc.FirstChild.Name.Equals("ProjectileGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",xmlDoc.FirstChild.Name);
            return null;
        }

        List<ProjectileGraphBaseData> projectileBaseDataList = new List<ProjectileGraphBaseData>();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            ProjectileGraphBaseData baseData = readProjectile(projectileNodes[nodeIndex],_currentFileName);
            if(baseData == null)
                return null;

            projectileBaseDataList.Add(baseData);
        }

        return projectileBaseDataList.ToArray();
    }

    private static ProjectileGraphBaseData readProjectile(XmlNode node, string filePath)
    {
        float defaultFramePerSecond = 0f;

        ProjectileGraphBaseData projectileBaseData = new ProjectileGraphBaseData();
        readTitle(node,projectileBaseData,out defaultFramePerSecond);

        List<AnimationPlayDataInfo> animationPlayDataInfos = new List<AnimationPlayDataInfo>();

        XmlNodeList nodes = node.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            string targetName = nodes[nodeIndex].Name;
            string targetValue = nodes[nodeIndex].Value;

            if(targetName == "Animation")
            {
                AnimationPlayDataInfo playData = ActionGraphLoader.ReadActionAnimation(nodes[nodeIndex],"",defaultFramePerSecond,_currentFileName);
                if(playData == null)
                {
                    DebugUtil.assert(false,"invalid animation : {0} [Line: {1}] [FileName: {2}]",node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                    return null;
                }

                animationPlayDataInfos.Add(playData);
            }
            else if(targetName == "DefaultShotInfo")
            {
                projectileBaseData._defaultProjectileShotInfoData = readDefaultShotInfo(nodes[nodeIndex]);
            }
            else if(targetName == "CollisionInfo")
            {
                XmlAttributeCollection attributes = nodes[nodeIndex].Attributes;
                for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
                {
                    if(attributes[attrIndex].Name == "Radius")
                        projectileBaseData._collisionRadius = float.Parse(attributes[attrIndex].Value);
                    else if(attributes[attrIndex].Name == "Angle")
                        projectileBaseData._collisionAngle = float.Parse(attributes[attrIndex].Value);
                    else if(attributes[attrIndex].Name == "StartDistance")
                        projectileBaseData._collisionStartDistance = float.Parse(attributes[attrIndex].Value);
                }
            }
            else if(targetName == "Event")
            {
                readChildFrameEvent(nodes[nodeIndex],projectileBaseData, filePath);
            }
                
        }

        projectileBaseData._animationPlayData = animationPlayDataInfos.ToArray();

        return projectileBaseData;
    }

    private static ProjectileGraphShotInfoData readDefaultShotInfo(XmlNode node)
    {
        ProjectileGraphShotInfoData shotInfoData = new ProjectileGraphShotInfoData();
        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "Velocity")
            {
                shotInfoData._deafaultVelocity = float.Parse(targetValue);
            }
            else if(targetName == "Acceleration")
            {
                shotInfoData._acceleration = float.Parse(targetValue);
            }
            else if(targetName == "Friction")
            {
                shotInfoData._friction = float.Parse(targetValue);
            }
            else if(targetName == "Angle")
            {
                if(targetValue.Contains("Random"))
                {
                    string randomData = targetValue.Replace("Random_","");
                    string[] randomValue = randomData.Split('^');
                    if(randomValue == null || randomValue.Length != 2)
                    {
                        DebugUtil.assert(false, "invalid random angle attrubute: {0} [Line: {1}] [FileName: {2}]", targetValue, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                        continue;
                    }

                    shotInfoData._useRandomAngle = true;
                    shotInfoData._randomAngle = new Vector2(float.Parse(randomValue[0]),float.Parse(randomValue[1]));
                }
                else
                {
                    shotInfoData._defaultAngle = float.Parse(targetValue);
                }
            }
            else if(targetName == "AngularAcceleration")
            {
                shotInfoData._angularAcceleration = float.Parse(targetValue);
            }
            else if(targetName == "LifeTime")
            {
                shotInfoData._lifeTime = float.Parse(targetValue);
            }
        }

        return shotInfoData;
    }
    private static void readTitle(XmlNode node, ProjectileGraphBaseData baseData, out float defaultFPS)
    {
        defaultFPS = -1f;

        baseData._name = node.Name;

        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "DefaultFramePerSecond")
            {
                defaultFPS = float.Parse(targetValue);
            }
            else if(targetName == "PenetrateCount")
            {
                baseData._penetrateCount = int.Parse(targetValue);
            }
            else if(targetName == "UseSpriteRotation")
            {
                baseData._useSpriteRotation = bool.Parse(targetValue);
            }
            else if(targetName == "CastShadow")
            {
                baseData._castShadow = bool.Parse(targetValue);
            }
            else if(targetName == "ExecuteBySummoner")
            {
                baseData._executeBySummoner = bool.Parse(targetValue);
            }
            else
            {
                DebugUtil.assert(false,"invalid Attribute : {0} [Line: {1}] [FileName: {2}]", targetName, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
            }

        }
    }

    private static void readChildFrameEvent(XmlNode node, ProjectileGraphBaseData baseData, string filePath)
    {
        XmlNodeList childNodeList = node.ChildNodes;

        if(childNodeList == null || childNodeList.Count == 0)
            return;

        Dictionary<ProjectileChildFrameEventType, ChildFrameEventItem> childFrameEventList = new Dictionary<ProjectileChildFrameEventType, ChildFrameEventItem>();

        for(int i = 0; i < childNodeList.Count; ++i)
        {
            string targetName = childNodeList[i].Name;

            ChildFrameEventItem childItem = new ChildFrameEventItem();
            ProjectileChildFrameEventType eventType = ProjectileChildFrameEventType.Count;

            if(targetName == "OnHit")
                eventType = ProjectileChildFrameEventType.ChildFrameEvent_OnHit;
            else if(targetName == "OnHitEnd")
                eventType = ProjectileChildFrameEventType.ChildFrameEvent_OnHitEnd;
            else if(targetName == "OnEnd")
                eventType = ProjectileChildFrameEventType.ChildFrameEvent_OnEnd;

            List<ActionFrameEventBase> actionFrameEventList = new List<ActionFrameEventBase>();
            XmlNodeList childNodes = childNodeList[i].ChildNodes;
            for(int j = 0; j < childNodes.Count; ++j)
            {
                actionFrameEventList.Add(FrameEventLoader.readFromXMLNode(childNodes[j], filePath));
            }

            actionFrameEventList.Sort((x,y)=>{
                return x._startFrame.CompareTo(y._startFrame);
            });

            childItem._childFrameEventCount = actionFrameEventList.Count;
            childItem._childFrameEvents = actionFrameEventList.ToArray();

            childFrameEventList.Add(eventType, childItem);
        }

        baseData._projectileChildFrameEvent = childFrameEventList;
    }

}
