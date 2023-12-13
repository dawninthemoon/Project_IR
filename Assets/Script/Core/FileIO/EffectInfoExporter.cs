using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class EffectInfoExporter
{
    public static Dictionary<string,EffectInfoDataBase[]> readFromXML(string path)
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

        Dictionary<string,List<EffectInfoDataBase>> effectInfoDataDictionary = new Dictionary<string,List<EffectInfoDataBase>>();

        XmlNodeList effectInfoNodes = xmlDoc.FirstChild.ChildNodes;
        for(int index = 0; index < effectInfoNodes.Count; ++index)
        {
            if(effectInfoNodes[index].Name == "ParticleEffect")
            {
                ParticleEffectInfoData effectInfoData = readParticleEffectInfoData(effectInfoNodes[index], path);
                if(effectInfoData == null)
                    return null;

                if(effectInfoDataDictionary.ContainsKey(effectInfoData._key) == false)
                    effectInfoDataDictionary.Add(effectInfoData._key, new List<EffectInfoDataBase>());
                
                foreach(var item in effectInfoDataDictionary[effectInfoData._key])
                {
                    if(item.compareMaterialExactly(effectInfoData._attackMaterial,effectInfoData._defenceMaterial))
                    {
                        DebugUtil.assert_fileOpen(false, "ParticleEffect는 서로 다른 AttackMaterial, DefenceMaterial을 가지고 있어야 같은 이름으로 선언할 수 있습니다. [EffectInfo Key: {0}]",path,XMLScriptConverter.getLineNumberFromXMLNode(effectInfoNodes[index]), effectInfoData._key);
                        return null;
                    }
                }

                if(effectInfoData.compareMaterialExactly(CommonMaterial.Empty,CommonMaterial.Empty))
                    effectInfoDataDictionary[effectInfoData._key].Add(effectInfoData);
                else
                    effectInfoDataDictionary[effectInfoData._key].Insert(0,effectInfoData);
            }
        }

        Dictionary<string,EffectInfoDataBase[]> resultEffectInfoDataDictionary = new Dictionary<string,EffectInfoDataBase[]>();
        foreach(var item in effectInfoDataDictionary)
        {
            resultEffectInfoDataDictionary.Add(item.Key,item.Value.ToArray());
        }

        return resultEffectInfoDataDictionary;
    }

    private static ParticleEffectInfoData readParticleEffectInfoData(XmlNode node, string filePath)
    {
        if(node == null)
            return null;

        ParticleEffectInfoData effectInfoData = new ParticleEffectInfoData();
        for(int index = 0; index < node.Attributes.Count; ++index)
        {
            string attrName = node.Attributes[index].Name;
            string attrValue = node.Attributes[index].Value;

            if(attrValue == "")
                continue;

            if(attrName == "Path")
            {
                effectInfoData._effectPath = attrValue;
            }
            else if(attrName == "Key")
            {
                effectInfoData._key = attrValue;
            }
            else if(attrName == "Offset")
            {
                string[] vector = attrValue.Split(' ');
                if(vector == null || vector.Length != 3)
                {
                    DebugUtil.assert_fileOpen(false, "invalid vector3 data: {0}", node.BaseURI, XMLScriptConverter.getLineNumberFromXMLNode(node),attrValue);
                    return null;
                }

                effectInfoData._spawnOffset.x = XMLScriptConverter.valueToFloatExtend(vector[0]);
                effectInfoData._spawnOffset.y = XMLScriptConverter.valueToFloatExtend(vector[1]);
                effectInfoData._spawnOffset.z = XMLScriptConverter.valueToFloatExtend(vector[2]);
            }
            else if(attrName == "ToTarget")
            {
                effectInfoData._toTarget = bool.Parse(attrValue);
            }
            else if(attrName == "UpdateType")
            {
                effectInfoData._effectUpdateType = (EffectUpdateType)System.Enum.Parse(typeof(EffectUpdateType), attrValue);
            }
            else if(attrName == "Attach")
            {
                effectInfoData._attach = bool.Parse(attrValue);
            }
            else if(attrName == "AngleType")
            {
                effectInfoData._angleDirectionType = (AngleDirectionType)System.Enum.Parse(typeof(AngleDirectionType), attrValue);
            }
            else if(attrName == "AngleOffset")
            {
                effectInfoData._angleOffset = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "LifeTime")
            {
                effectInfoData._lifeTime = XMLScriptConverter.valueToFloatExtend(attrValue);
            }
            else if(attrName == "FollowDirection")
            {
                effectInfoData._followDirection = bool.Parse(attrValue);
            }
            else if(attrName == "CastShadow")
            {
                effectInfoData._castShadow = bool.Parse(attrValue);
            }
            else if(attrName == "AttackMaterial")
            {
                effectInfoData._attackMaterial = (CommonMaterial)System.Enum.Parse(typeof(CommonMaterial), attrValue);
            }
            else if(attrName == "DefenceMaterial")
            {
                effectInfoData._defenceMaterial = (CommonMaterial)System.Enum.Parse(typeof(CommonMaterial), attrValue);
            }
            else if(attrName == "DependentAction")
            {
                effectInfoData._dependentAction = bool.Parse(attrValue);
            }
            else
            {
                DebugUtil.assert_fileOpen(false, "알 수 없는 EffectInfo Attribute", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node));    
            }
        }

        if(effectInfoData._key == "")
            DebugUtil.assert_fileOpen(false, "key is essential", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node));

        if(effectInfoData._effectPath == "")
            DebugUtil.assert_fileOpen(false, "effect path is essential", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node));

        return effectInfoData;
    }
}
