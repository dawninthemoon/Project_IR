using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class MovementGraphExporter : LoaderBase<MovementGraph>
{
    public override MovementGraph readFromXML(string path)
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

        XmlNode node = xmlDoc.FirstChild;
        if(node.Name.Equals("MovementGraph") == false)
        {
            DebugUtil.assert(false,"wrong xml type. name : {0}",node.Name);
            return null;
        }
        
        XmlNodeList nodeList = node.ChildNodes;
        MovementGraphData[] graphDataArray = new MovementGraphData[nodeList.Count];
        Vector3 totalMove = Vector3.zero;

        for(int i = 0; i < nodeList.Count; ++i)
        {
            XmlNode frameNode = nodeList[i];
            XmlAttributeCollection attributes = frameNode.Attributes;

            graphDataArray[i] = new MovementGraphData(Vector3.zero,Vector3.zero,Vector2Int.zero);
            graphDataArray[i]._index = int.Parse(frameNode.Name);

            for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
            {
                string targetName = attributes[attrIndex].Name;
                string targetValue = attributes[attrIndex].Value;

                if(targetName == "X")
                {
                    graphDataArray[i]._pixelPosition.x = int.Parse(targetValue);
                }
                else if(targetName == "Y")
                {
                    graphDataArray[i]._pixelPosition.y = int.Parse(targetValue);
                }
                else
                {
                    //위에 구현 추가
                    DebugUtil.assert(false,"invalid attribute name : {0}", targetName);
                    return null;
                }
            }

            Vector2Int pivotDiff = (i == 0 ? Vector2Int.zero : graphDataArray[i]._pixelPosition - graphDataArray[i - 1]._pixelPosition);

            graphDataArray[i]._moveFactor = new Vector3((float)pivotDiff.x, (float)pivotDiff.y) * 0.01f;
            totalMove = totalMove + graphDataArray[i]._moveFactor;
            graphDataArray[i]._totalMoveFactor = totalMove;
        }

        MovementGraph newGraph = ScriptableObject.CreateInstance<MovementGraph>();
        newGraph.setData(graphDataArray,node.Attributes[0].Value);

        return newGraph;
    }

    public static MovementGraph readFromBinary(string path)
    {
        if(File.Exists(path) == false)
        {
            DebugUtil.assert(false,"file does not exists : {0}", path);
            return null;
        }

        var fileStream = File.Open(path, FileMode.Create);
        var reader = new BinaryReader(fileStream,Encoding.UTF8,false);
        MovementGraph graph = ScriptableObject.CreateInstance<MovementGraph>();

        graph.deserialize(reader);

        reader.Close();
        fileStream.Close();
        return graph;
    }

    public MovementGraph readFromXMLAndExportToBinary(string xmlPath, string binaryPath)
    {
        MovementGraph graph = readFromXML(xmlPath);
        if(graph == null)
        {
            return null;
        }

        exportToBinary(graph, binaryPath);
        return graph;
    }

    public static void exportToBinary(MovementGraph graph, string path)
    {
        var fileStream = File.Open(path, FileMode.Create);
        var writer = new BinaryWriter(fileStream,Encoding.UTF8,false);

        graph.serialize(writer);

        writer.Close();
        fileStream.Close();
    }
}
