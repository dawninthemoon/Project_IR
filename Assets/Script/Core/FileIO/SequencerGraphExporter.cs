using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public class SequencrGraphLoader : LoaderBase<SequencerGraphBaseData>
{
    static string _currentFileName = "";
    public override SequencerGraphBaseData readFromXML(string path)
    {
        _currentFileName = "Assets\\Data\\SequencerGraph\\" + path;
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(_currentFileName),readerSettings))
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


        SequencerGraphBaseData baseData = new SequencerGraphBaseData();

        XmlAttributeCollection firstNodeAttribute = xmlDoc.FirstChild.Attributes;
        for(int i = 0; i < firstNodeAttribute.Count; ++i)
        {
            string attrName = firstNodeAttribute[i].Name;
            string attrValue = firstNodeAttribute[i].Value;

            if(attrName == "Name")
            {
                baseData._sequencerName = attrValue;
            }
        }

        XmlNodeList nodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
        {
            XmlNode phaseNode = nodes[nodeIndex];
            SequencerGraphPhaseData phaseData = readPhaseData(phaseNode);

            if(phaseNode.Name == "InitializePhase")
                baseData._sequencerGraphPhase[(int)SequencerGraphPhaseType.Initialize] = phaseData;
            else if(phaseNode.Name == "UpdatePhase")
                baseData._sequencerGraphPhase[(int)SequencerGraphPhaseType.Update] = phaseData;
            else if(phaseNode.Name == "EndPhase")
                baseData._sequencerGraphPhase[(int)SequencerGraphPhaseType.End] = phaseData;
        }

        for(int index = 0; index < (int)SequencerGraphPhaseType.Count; ++index)
        {
            if(baseData._sequencerGraphPhase[index] == null)
                baseData._sequencerGraphPhase[index] = new SequencerGraphPhaseData();
        }

        return baseData;
    }

    private static SequencerGraphPhaseData readPhaseData(XmlNode node)
    {
        SequencerGraphPhaseData phaseData = new SequencerGraphPhaseData();

        List<SequencerGraphEventBase> eventList = new List<SequencerGraphEventBase>();
        XmlNodeList eventNodes = node.ChildNodes;
        for(int i = 0; i < eventNodes.Count; ++i)
        {
            SequencerGraphEventBase eventData = readEventData(eventNodes[i]);
            eventList.Add(eventData);
        }

        phaseData._sequencerGraphEventList = eventList.ToArray();
        phaseData._sequencerGraphEventCount = eventList.Count;
        return phaseData;
    }

    public static SequencerGraphEventBase readEventData(XmlNode node)
    {
        SequencerGraphEventBase spawnEvent = null;
        if(node.Name == "SpawnCharacter")
            spawnEvent = new SequencerGraphEvent_SpawnCharacter();
        else if(node.Name == "WaitSecond")
            spawnEvent = new SequencerGraphEvent_WaitSecond(); 
        else if(node.Name == "SetCameraTarget")
            spawnEvent = new SequencerGraphEvent_SetCameraTarget();
        else if(node.Name == "SetCameraPosition")
            spawnEvent = new SequencerGraphEvent_SetCameraPosition();
        else if(node.Name == "SetAudioListner")
            spawnEvent = new SequencerGraphEvent_SetAudioListner();
        else if(node.Name == "SetCrossHair")
            spawnEvent = new SequencerGraphEvent_SetCrossHair();
        else if(node.Name == "SetHPSphere")
            spawnEvent = new SequencerGraphEvent_SetHPSphere();
        else if(node.Name == "WaitTargetDead")
            spawnEvent = new SequencerGraphEvent_WaitTargetDead();
        else if(node.Name == "TeleportTargetTo")
            spawnEvent = new SequencerGraphEvent_TeleportTargetTo();
        else if(node.Name == "ApplyPostProcessProfile")
            spawnEvent = new SequencerGraphEvent_ApplyPostProcessProfile();
        else if(node.Name == "SaveEventExecuteIndex")
            spawnEvent = new SequencerGraphEvent_SaveEventExecuteIndex();
        else if(node.Name == "CallAIEvent")
            spawnEvent = new SequencerGraphEvent_CallAIEvent();
        else if(node.Name == "WaitSignal")
            spawnEvent = new SequencerGraphEvent_WaitSignal();
        else if(node.Name == "SetCameraZoom")
            spawnEvent = new SequencerGraphEvent_SetCameraZoom();
        else if(node.Name == "FadeOut")
            spawnEvent = new SequencerGraphEvent_FadeIn();
        else if(node.Name == "FadeIn")
            spawnEvent = new SequencerGraphEvent_FadeOut();
        else if(node.Name == "ForceQuit")
            spawnEvent = new SequencerGraphEvent_ForceQuit();
        else if(node.Name == "BlockInput")
            spawnEvent = new SequencerGraphEvent_BlockInput();
        else if(node.Name == "BlockAI")
            spawnEvent = new SequencerGraphEvent_BlockAI();
        else if(node.Name == "SetAction")
            spawnEvent = new SequencerGraphEvent_SetAction();
        else if(node.Name == "PlayAnimation")
            spawnEvent = new SequencerGraphEvent_PlayAnimation();
        else if(node.Name == "AIMove")
            spawnEvent = new SequencerGraphEvent_AIMove();
        else if(node.Name == "QTEFence")
            spawnEvent = new SequencerGraphEvent_QTEFence();
        else if(node.Name == "DeadFence")
            spawnEvent = new SequencerGraphEvent_DeadFence();
        else if(node.Name == "SetHideUI")
            spawnEvent = new SequencerGraphEvent_SetHideUI();
        else if(node.Name == "SetTimeScale")
            spawnEvent = new SequencerGraphEvent_SetTimeScale();
        else if(node.Name == "NextStage")
            spawnEvent = new SequencerGraphEvent_NextStage();
        else if(node.Name == "ShakeEffect")
            spawnEvent = new SequencerGraphEvent_ShakeEffect();
        else if(node.Name == "ZoomEffect")
            spawnEvent = new SequencerGraphEvent_ZoomEffect();
        else if(node.Name == "ToastMessage")
            spawnEvent = new SequencerGraphEvent_ToastMessage();
        else if(node.Name == "Task")
            spawnEvent = new SequencerGraphEvent_Task();
        else if(node.Name == "LetterBoxShow")
            spawnEvent = new SequencerGraphEvent_LetterBoxShow();
        else if(node.Name == "LetterBoxHide")
            spawnEvent = new SequencerGraphEvent_LetterBoxHide();
        else if(node.Name == "TalkBalloon")
            spawnEvent = new SequencerGraphEvent_TalkBalloon();
        else if(node.Name == "CameraTrack")
            spawnEvent = new SequencerGraphEvent_CameraTrack();
        else if(node.Name == "TaskFence")
            spawnEvent = new SequencerGraphEvent_TaskFence();

        if(spawnEvent == null)
        {
            DebugUtil.assert(false,"invalid sequencer graph event type: {0} [Line: {1}] [FileName: {2}]", node.Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
            return null;
        }

        spawnEvent.loadXml(node);
        return spawnEvent;
    }

}
