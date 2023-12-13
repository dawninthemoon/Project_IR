using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;
using System;

public static class BuffDataLoader
{
    static string _currentFileName = "";
    public static Dictionary<int, BuffData> readFromXML(string path)
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

        Dictionary<int, BuffData> buffDataList = new Dictionary<int, BuffData>();

        try
        {
            XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
            for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
            {
                BuffData baseData = readBuffData(projectileNodes[nodeIndex], ref buffDataList);
                if(baseData == null)
                    return null;

                if(buffDataList.ContainsKey(baseData._buffKey) == true)
                {
                    DebugUtil.assert(false, "buff key overlap: key {0} [FileName: {1}]", baseData._buffKey, _currentFileName);
                    continue;
                }
                buffDataList.Add(baseData._buffKey,baseData);
            }
        }
        catch(Exception ex)
        {
            DebugUtil.assert(false,"xml parsing exception : {0}\n",ex.Message,xmlDoc.BaseURI);
            return null;
        }

        return buffDataList;
    }

    private static BuffData readBuffData(XmlNode node, ref Dictionary<int, BuffData> buffDataList)
    {
        BuffData buffData = new BuffData();
        buffData._buffName = node.Name;

        XmlAttributeCollection buffDataNodes = node.Attributes;

        for(int i = 0; i < buffDataNodes.Count; ++i)
        {
            string attrName = buffDataNodes[i].Name;
            string attrValue = buffDataNodes[i].Value;

            if(attrName == "Parent")
            {
                int targetKey = int.Parse(attrValue);
                if(buffDataList.ContainsKey(targetKey) == false)
                {
                    DebugUtil.assert(false, "대상 부모 버프가 존재하지 않습니다. : [Key: {0}] [Line: {1}] [FileName: {2}]", targetKey, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                    return null;
                }

                buffData.copy(buffDataList[targetKey]);
                continue;
            }

            if(attrName == "Key")
                buffData._buffKey = int.Parse(attrValue);
            else if(attrName == "StatusName")
                buffData._targetStatusName = attrValue;
            else if(attrName == "UpdateType")
                buffData._buffUpdateType = (BuffUpdateType)System.Enum.Parse(typeof(BuffUpdateType), attrValue);
            else if(attrName == "ApplyType")
                buffData._buffApplyType = (BuffApplyType)System.Enum.Parse(typeof(BuffApplyType), attrValue);
            else if(attrName == "Factor")
                buffData._buffVaryStatFactor = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "Time")
                buffData._buffCustomValue0 = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "TargetStatusName")
                buffData._buffCustomStatusName = attrValue;
            else if(attrName == "MinValue")
                buffData._buffCustomValue0 = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "MaxValue")
                buffData._buffCustomValue1 = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "ButtonList")
                buffData._buffCustomValue2 = attrValue.Split(' ');
            else if(attrName == "AllowOverlap")
                buffData._allowOverlap = bool.Parse(attrValue);
            else if(attrName == "ParticleEffect")
            {
                GameObject prefab = ResourceContainerEx.Instance().GetPrefab(attrValue);
                if(prefab == null)
                    DebugUtil.assert(false,"대상 ParticleEffect가 존재하지 않습니다. [BuffInfo: {0}] [ParticleEffect: {1}] [Line: {2}] [FileName: {3}]",node.Name,attrValue, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);

                buffData._particleEffect = attrValue;
            }
            else if(attrName == "TimelineEffect")
            {
                GameObject prefab = ResourceContainerEx.Instance().GetPrefab(attrValue);
                if(prefab == null)
                    DebugUtil.assert(false,"대상 TimelineEffect가 존재하지 않습니다. [BuffInfo: {0}] [TimelineEffect: {1}] [Line: {2}] [FileName: {3}]",node.Name,attrValue, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);

                buffData._timelineEffect = attrValue;
            }
            else if(attrName == "AnimationEffect")
            {
                GameObject prefab = ResourceContainerEx.Instance().GetPrefab(attrValue);
                if(prefab == null)
                    DebugUtil.assert(false,"대상 AnimationEffect가 존재하지 않습니다. [BuffInfo: {0}] [AnimationEffect: {1}] [Line: {2}] [FileName: {3}]",node.Name,attrValue, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);

                buffData._animationEffect = attrValue;
            }
            else if(attrName == "AudioID")
            {
                buffData._audioID = int.Parse(attrValue);
            }
            else if(attrName == "ParameterID")
            {
                string[] paramIDArray = attrValue.Split(' ');
                List<int> idList = new List<int>();
                foreach(var item in paramIDArray)
                {
                    idList.Add(int.Parse(item));
                }

                buffData._audioParameter = idList.ToArray();
            }
            else
            {
                DebugUtil.assert(false, "invalid attribute name from buffInfo: {0} [Line: {1}] [FileName: {2}]",node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                continue;
            }

        }

        return buffData;
    }

}
