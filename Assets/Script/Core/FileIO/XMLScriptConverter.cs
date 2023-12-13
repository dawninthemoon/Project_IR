using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class XMLScriptConverter
{
    public static MemoryStream convertXMLScriptSymbol(string path)
    {
        string xmlFile = "";
        try
        {
            xmlFile = File.ReadAllText(path);
        }
        catch(Exception exp)
        {
            DebugUtil.assert(false,"failed to load xml file\n{0}\n{1}",path,exp.Message);
            return null;
        }

        int startOffset = 0;
        int endOffset = 0;
        while(true)
        {
            if(xmlFile.Length <= startOffset)
                break;

            int offset = xmlFile.IndexOf('"',startOffset);
            if(offset == -1)
            {
                break;
            }

            startOffset = offset + 1;
            endOffset = xmlFile.IndexOf('"',startOffset);
            if(offset == -1)
            {
                DebugUtil.assert(false,"invalid file: {0}",offset);
                return null;
            }

            for(int i = startOffset; i < endOffset; ++i)
            {
                if(xmlFile[i] == '&')
                {
                    xmlFile = xmlFile.Remove(i,1).Insert(i,"&#38;");
                    endOffset += 4;
                    i+=4;
                }
                else if(xmlFile[i] == '>')
                {
                    xmlFile = xmlFile.Remove(i,1).Insert(i,"&gt;");
                    endOffset += 3;
                    i+=3;
                }
                else if(xmlFile[i] == '<')
                {
                    xmlFile = xmlFile.Remove(i,1).Insert(i,"&lt;");
                    endOffset += 3;
                    i+=3;
                }
            }

            startOffset = endOffset + 1;
        }

        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlFile));
    }

    public static string getLineFromXMLNode(XmlNode xmlNode)
    {
        return ((PositionXmlElement) xmlNode).LineNumber.ToString();
    }

    public static int getLineNumberFromXMLNode(XmlNode xmlNode)
    {
        return ((PositionXmlElement) xmlNode).LineNumber;
    }
        
    public static float valueToFloatExtend(string valueString)
    {
        if(float.TryParse(valueString,out float result))
            return result;

        if(valueString.Contains("Random_") == false)
            throw new Exception($"잘못된 Float Type Value : {valueString}");

        string[] randomRangeString = valueString.Replace("Random_",string.Empty).Split('^');
        if(randomRangeString.Length != 2)
            throw new Exception($"잘못된 Float Type Value : {valueString}");

        float min, max;
        if(float.TryParse(randomRangeString[0],out min) == false)
            throw new Exception($"잘못된 Float Type Value : {valueString}");

        if(float.TryParse(randomRangeString[1],out max) == false)
            throw new Exception($"잘못된 Float Type Value : {valueString}");

        return UnityEngine.Random.Range(min,max);
    }

    public static Vector3 valueToVector3(string valueString)
    {
        string[] splitted = valueString.Split(' ');
        if(splitted.Length != 3)
            throw new Exception($"잘못된 Vector3 Type Value : {valueString}");

        return new Vector3(valueToFloatExtend(splitted[0]), valueToFloatExtend(splitted[1]), valueToFloatExtend(splitted[2]));
    }

    public static Color valueToLinearColor(string valueString)
    {
        string[] splitted = valueString.Split(' ');
        if(splitted.Length != 4)
            throw new Exception($"잘못된 Color Type Value : {valueString}");

        return new Color(valueToFloatExtend(splitted[0]), valueToFloatExtend(splitted[1]), valueToFloatExtend(splitted[2]), valueToFloatExtend(splitted[3]));
    }
}