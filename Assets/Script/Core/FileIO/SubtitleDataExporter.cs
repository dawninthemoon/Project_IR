using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class SubtitleSimpleTalkDataLoader
{
    public static Dictionary<string,SubtitleData_SimpleTalk> readFromXML(string path)
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

        Dictionary<string,SubtitleData_SimpleTalk> simpleTalkDataDictionary = new Dictionary<string,SubtitleData_SimpleTalk>();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            if(projectileNodes[nodeIndex].Name != "SimpleTalk")
                continue;

            XmlNodeList simpleTalkNodes = projectileNodes[nodeIndex].ChildNodes;
            for(int index = 0; index < simpleTalkNodes.Count; ++index)
            {
                SubtitleData_SimpleTalk simpleTalkData = readSimpleTalkData(simpleTalkNodes[index]);
                if(simpleTalkData == null)
                    return null;

                simpleTalkDataDictionary.Add(simpleTalkNodes[index].Name, simpleTalkData);
            }
        }

        return simpleTalkDataDictionary;
    }

    private static SubtitleData_SimpleTalk readSimpleTalkData(XmlNode node)
    {
        if(node == null)
            return null;

        SubtitleData_SimpleTalk characterInfoData = new SubtitleData_SimpleTalk();
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if(attrName == "Text")
                characterInfoData._text = attrValue;
            else if(attrName == "Time")
                characterInfoData._time = float.Parse(attrValue);
        }

        return characterInfoData;
    }
}
