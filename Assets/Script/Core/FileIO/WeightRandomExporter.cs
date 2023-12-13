using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class WeightRandomExporter
{
    public static Dictionary<string, WeightGroupData> readFromXML(string path)
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

        Dictionary<string, WeightGroupData> weightGroupDataList = new Dictionary<string, WeightGroupData>();

        XmlNodeList weightGroupNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < weightGroupNodes.Count; ++nodeIndex)
        {
            WeightGroupData baseData = readWeightGroupData(weightGroupNodes[nodeIndex]);
            
            if(baseData.isValid() == false)
                return null;

            if(weightGroupDataList.ContainsKey( baseData._groupKey))
            {
                DebugUtil.assert(false,"target weight is already exists: group: {0}",baseData._groupKey);
                return null;
            }

            weightGroupDataList.Add(baseData._groupKey,baseData);
        }

        return weightGroupDataList;
    }

    private static WeightGroupData readWeightGroupData(XmlNode node)
    {
        WeightGroupData group = new WeightGroupData();
        group._groupKey = node.Name;

        List<WeightData> weightDataList = new List<WeightData>();
        XmlNodeList weightDataNodes = node.ChildNodes;

        float totalWeight = 0f;

        for(int i = 0; i < weightDataNodes.Count; ++i)
        {
            WeightData data = new WeightData();
            data._key = weightDataNodes[i].Name;

            XmlAttributeCollection attributes = weightDataNodes[i].Attributes;
            for(int j = 0; j < attributes.Count; ++j)
            {
                string attrName = attributes[j].Name;
                string attrValue = attributes[j].Value;

                if(attrName == "Value")
                {
                    data._weight = float.Parse(attrValue);
                    if(data._weight == 0f)
                    {
                        DebugUtil.assert(false, "weight data cannot be zero  groupname: {0}",group._groupKey);
                        continue;
                    }
                }
                else
                {
                    DebugUtil.assert(false, "invalid attribute name from weightGroup: {0}",attrName);
                    continue;
                }
            }

            totalWeight += data._weight;
            weightDataList.Add(data);
            
        }

        weightDataList.Sort((x,y)=>{
            return x._weight.CompareTo(y._weight);
        });

        group._weightCount = weightDataList.Count;
        for(int i = 0; i < group._weightCount; ++i)
        {
            WeightData data = new WeightData();
            data._key = weightDataList[i]._key;
            data._weight = weightDataList[i]._weight / totalWeight;

            weightDataList[i] = data;
        }

        group._weights = weightDataList.ToArray();
        return group;
    }

}
