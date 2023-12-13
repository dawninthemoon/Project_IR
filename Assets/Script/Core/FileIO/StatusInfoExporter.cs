using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class StatusInfoLoader
{
    public static Dictionary<string, StatusInfoData> readFromXML(string path)
    {
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

        Dictionary<string, StatusInfoData> StatusInfoDataList = new Dictionary<string, StatusInfoData>();
        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        try
        {
            for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
            {
                StatusInfoData baseData = readStatusInfoData(projectileNodes[nodeIndex]);
                if(baseData == null)
                    return null;

                StatusInfoDataList.Add(baseData._statusInfoName,baseData);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml parsing exception : {0}\n",ex.Message,xmlDoc.BaseURI);
            return null;
        }

        return StatusInfoDataList;
    }

    private static StatusInfoData readStatusInfoData(XmlNode node)
    {
        List<StatusDataFloat> statusInfoDataList = new List<StatusDataFloat>();
        List<StatusGraphicInterfaceData> graphicInterfaceDataList = new List<StatusGraphicInterfaceData>();

        HashSet<string> nameCheck = new HashSet<string>();
        XmlNodeList statusNodes = node.ChildNodes;

        for(int i = 0; i < statusNodes.Count; ++i)
        {
            if(statusNodes[i].Name == "Stat")
            {
                StatusDataFloat data = new StatusDataFloat();
                XmlAttributeCollection attributes = statusNodes[i].Attributes;
                for(int j = 0; j < attributes.Count; ++j)
                {
                    string attrName = attributes[j].Name;
                    string attrValue = attributes[j].Value;

                    if(attrName == "Type")
                    {
                        data._statusType = (StatusType)System.Enum.Parse(typeof(StatusType), attrValue);
                        data._statusName = data._statusType.ToString();
                    }
                    else if(attrName == "Name")
                        data._statusName = attrValue;
                    else if(attrName == "Max")
                        data._maxValue = float.Parse(attrValue);
                    else if(attrName == "Min")
                        data._minValue = float.Parse(attrValue);
                    else if(attrName == "Init")
                        data._initialValue = float.Parse(attrValue);
                    else
                    {
                        DebugUtil.assert(false, "invalid attribute name from statusInfo: {0}",attrName);
                        continue;
                    }
                }

                if(nameCheck.Contains(data._statusName))
                {
                    DebugUtil.assert(false,"status overlapError name: {0}",data._statusName);
                    return null;
                }

                nameCheck.Add(data._statusName);

                statusInfoDataList.Add(data);
            }
            else if(statusNodes[i].Name == "DeclareGraphicInterface")
            {
                StatusGraphicInterfaceData data = new StatusGraphicInterfaceData();
                XmlAttributeCollection attributes = statusNodes[i].Attributes;
                for(int j = 0; j < attributes.Count; ++j)
                {
                    string attrName = attributes[j].Name;
                    string attrValue = attributes[j].Value;

                    if(attrName == "Target")
                        data._targetStatus = attrValue;
                    else if(attrName == "Color")
                        data._interfaceColor = XMLScriptConverter.valueToLinearColor(attrValue);
                    else if(attrName == "HorizontalGap")
                        data._horizontalGap = float.Parse(attrValue);
                    else
                    {
                        DebugUtil.assert(false, "invalid attribute name from DeclareGraphicInterface: {0}",attrName);
                        continue;
                    }
                }

                graphicInterfaceDataList.Add(data);
            }
        }

        StatusInfoData statusInfoData = new StatusInfoData(node.Name,statusInfoDataList.ToArray(),graphicInterfaceDataList.ToArray());

        return statusInfoData;
    }

}
