using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;
using System.Linq;

public class ActionGraphLoader : LoaderBase<ActionGraphBaseData>
{
    private static Dictionary<string, string> _globalVariables = new Dictionary<string, string>();

    private static string _currentFileName = "";
    PositionXmlDocument _xmlDoc = null;
    public override ActionGraphBaseData readFromXML(string path)
    {
        _currentFileName = path;
        _xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(XMLScriptConverter.convertXMLScriptSymbol(path),readerSettings))
            {
                _xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(_xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }

        Dictionary<string, XmlNodeList> branchSetDic = new Dictionary<string, XmlNodeList>();

        XmlNode node = _xmlDoc.FirstChild;
        
        if(node.Name.Equals("ActionGraph") == false)
        {
            DebugUtil.assert_fileOpen(false,"wrong xml type. name : {0} [FileName {1}]", _currentFileName,0,node.Name);
            return null;
        }
        
        float defaultFramePerSecond = 0f;
        string defaultActionName = "";

        ActionGraphBaseData actionBaseData = new ActionGraphBaseData();

#if UNITY_EDITOR
        actionBaseData._fullPath = path;
#endif

        ReadTitle(node,actionBaseData,out defaultFramePerSecond, out defaultActionName, path);

        List<ActionGraphNodeData> nodeDataList = new List<ActionGraphNodeData>();
        List<ActionGraphBranchData> branchDataList = new List<ActionGraphBranchData>();
        List<ActionGraphConditionCompareData> compareDataList = new List<ActionGraphConditionCompareData>();
        List<AnimationPlayDataInfo[]> animationDataList = new List<AnimationPlayDataInfo[]>();

        _globalVariables.Clear();
        Dictionary<ActionGraphBranchData, string> actionCompareDic = new Dictionary<ActionGraphBranchData, string>();
        Dictionary<string, int> actionIndexDic = new Dictionary<string, int>();
        XmlNodeList nodeList = node.ChildNodes;

        nodeDataList.Add( createDummyActionNode() );
        actionIndexDic.Add("SequencerDummyAction",0);
        actionBaseData._dummyActionIndex = 0;

        int actionIndex = 1;
        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "BranchSet")
            {
                readBranchSet(nodeList[i],ref branchSetDic, path);
                continue;
            }
            else if(nodeList[i].Name == "GlobalVariable")
            {
                readGlobalVariable(nodeList[i], ref _globalVariables, path);
                continue;
            }
            
            ActionGraphNodeData nodeData = ReadAction(nodeList[i],defaultFramePerSecond, ref animationDataList, ref actionCompareDic, ref branchDataList,ref compareDataList, in branchSetDic, path);
            if(nodeData == null)
            {
                DebugUtil.assert_fileOpen(false,"node data is null : [NodeName: {0}] [Line: {1}] [FileName: {2}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node),nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                return null;
            }

            nodeDataList.Add(nodeData);

            if(actionIndexDic.ContainsKey(nodeData._nodeName))
            {
                DebugUtil.assert_fileOpen(false,"중복된 액션이 존재합니다. 확인해 주세요 : [NodeName: {0}] [Line: {1}] [FileName: {2}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node),nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(node), _currentFileName);
                return null;
            }
            actionIndexDic.Add(nodeData._nodeName,actionIndex++);
        }

        foreach(var item in actionCompareDic)
        {
            if(actionIndexDic.ContainsKey(item.Value) == false)
            {
                DebugUtil.assert_fileOpen(false,"target action is not exists : {0} [FileName: {1}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node),item.Value, _currentFileName);
                return null;
            }
            else if(actionBaseData._defaultActionIndex == -1 && item.Value == defaultActionName)
            {
                actionBaseData._defaultActionIndex = actionIndexDic[item.Value];
            }

            item.Key._branchActionIndex = actionIndexDic[item.Value];
        }

        if(actionBaseData._defaultActionIndex == -1)
        {
            if(actionIndexDic.ContainsKey(defaultActionName) == false)
            {
                DebugUtil.assert_fileOpen(false, "invalid default action name: {0} [FileName: {1}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node),defaultActionName, _currentFileName);
                return null;
            }

            actionBaseData._defaultActionIndex = actionIndexDic[defaultActionName];
        }

        actionBaseData._actionNodeCount = nodeDataList.Count;
        actionBaseData._branchCount = branchDataList.Count;
        actionBaseData._conditionCompareDataCount = compareDataList.Count;
        actionBaseData._animationPlayDataCount = animationDataList.Count;

        actionBaseData._actionNodeData = nodeDataList.ToArray();
        actionBaseData._branchData = branchDataList.ToArray();
        actionBaseData._conditionCompareData = compareDataList.ToArray();
        actionBaseData._animationPlayData = animationDataList.ToArray();

        actionBaseData._actionIndexMap = actionIndexDic;

        return actionBaseData;
    }

    private static void readGlobalVariable(XmlNode node, ref Dictionary<string, string> targetDic, string filePath)
    {
        string name = "";
        string value = "";
        for(int i = 0; i < node.Attributes.Count; ++i)
        {
            if(node.Attributes[i].Name == "Name")
                name = node.Attributes[i].Value;
            else if(node.Attributes[i].Name == "Value")
                value = node.Attributes[i].Value;
        }

        if(name == "" || value == "" || name.Contains("gv_") == false )
        {
            DebugUtil.assert_fileOpen(false, "invalid globalVariable, [name: {0}] [value: {1}] [Line: {2}] [FileName: {3}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node),name,value, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return;
        }

        targetDic.Add(name,value);
    }

    public static Dictionary<string, string> getGlobalVariableContainer()
    {
        return _globalVariables;
    }

    public static string getGlobalVariable(string value)
    {
        if(_globalVariables.ContainsKey(value))
            return _globalVariables[value];

        return value;
    }


    public static string getGlobalVariable(string value, Dictionary<string,string> globalVariableContainer)
    {
        if(globalVariableContainer.ContainsKey(value))
            return globalVariableContainer[value];

        return value;
    }

    public static void readBranchSet(XmlNode branchSetParent, ref Dictionary<string, XmlNodeList> targetDic, string filePath)
    {
        string branchSetName = "";
        XmlAttributeCollection branchSetAttr = branchSetParent.Attributes;
        for(int attrIndex = 0; attrIndex < branchSetAttr.Count; ++attrIndex)
        {
            string targetName = branchSetAttr[attrIndex].Name;
            string targetValue = branchSetAttr[attrIndex].Value;

            if(targetName == "Name")
            {
                branchSetName = targetValue;
            }
        }

        if(branchSetName == "")
        {
            DebugUtil.assert_fileOpen(false, "branchSet name can not be Empty", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(branchSetParent));
            return;
        }

        if(branchSetParent.ChildNodes.Count == 0)
        {
            DebugUtil.assert_fileOpen(false, "branchSet is empty : [Name: {0}] [Line: {1}] [FileName: {2}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(branchSetParent),branchSetName, XMLScriptConverter.getLineFromXMLNode(branchSetParent), filePath);
            return;
        }

        targetDic.Add(branchSetName,branchSetParent.ChildNodes);
    }

    private static ActionGraphNodeData ReadAction(XmlNode node, float defaultFPS, ref List<AnimationPlayDataInfo[]> animationDataList,  ref Dictionary<ActionGraphBranchData, string> actionCompareDic,ref List<ActionGraphBranchData> branchDataList, ref List<ActionGraphConditionCompareData> compareDataList, in Dictionary<string, XmlNodeList> branchSetDic, string filePath)
    {
        ActionGraphNodeData nodeData = new ActionGraphNodeData();
        nodeData._nodeName = node.Name;
#if UNITY_EDITOR
        nodeData._lineNumber = XMLScriptConverter.getLineNumberFromXMLNode(node);
#endif
        //action attribute
        XmlAttributeCollection actionAttributes = node.Attributes;
        if(actionAttributes == null)
        {
            Debug.Log(node.Name);
            return null;
        }

        float actionTime = -1f;

        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value,_globalVariables);

            if(targetName == "MovementType")
            {
                nodeData._movementType = (MovementBase.MovementType)System.Enum.Parse(typeof(MovementBase.MovementType), targetValue);
            }
            else if(targetName == "DirectionType")
            {
                nodeData._directionType = (DirectionType)System.Enum.Parse(typeof(DirectionType), targetValue);
            }
            else if(targetName == "DefenceDirectionType")
            {
                nodeData._defenceDirectionType = (DefenceDirectionType)System.Enum.Parse(typeof(DefenceDirectionType), targetValue);
            }
            else if(targetName == "MovementGraphPreset")
            {
                MovementGraphPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\MovementGraphPreset") as MovementGraphPreset;
                nodeData._movementGraphPresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "FlipType")
            {
                nodeData._flipType = (FlipType)System.Enum.Parse(typeof(FlipType), targetValue);
            }
            else if(targetName == "MoveScale")
            {
                nodeData._moveScale = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "IsActionSelection")
            {
                nodeData._isActionSelection = bool.Parse(targetValue);
            }
            else if(targetName == "RotationType")
            {
                nodeData._rotationType = (RotationType)System.Enum.Parse(typeof(RotationType), targetValue);
            }
            else if(targetName == "DefenceType")
            {
                nodeData._defenceType = (DefenceType)System.Enum.Parse(typeof(DefenceType), targetValue);
            }
            else if(targetName == "CharacterMaterial")
            {
                nodeData._characterMaterial = (CommonMaterial)System.Enum.Parse(typeof(CommonMaterial), targetValue);
            }
            else if(targetName == "DefenceAngle")
            {
                nodeData._defenceAngle = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "DirectionAngle")
            {
                nodeData._additionalDirectionAngle = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "ApplyBuff")
            {
                string[] buffList = targetValue.Split(' ');
                nodeData._applyBuffList = new int[buffList.Length];

                for(int i = 0; i < buffList.Length; ++i)
                {
                    bool parse = int.TryParse(buffList[i],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[i]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert_fileOpen(false, "invalidBuff : {0} [Line: {1}] [FileName: {2}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node), buffList[i], XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        continue;
                    }

                    nodeData._applyBuffList[i] = buffKey;
                }
            }
            else if(targetName == "IgnoreAttackType")
            {
                string[] targetTypeArray = targetValue.Split(' ');
                foreach(var item in targetTypeArray)
                {
                    nodeData._ignoreAttackType |= (AttackType)System.Enum.Parse(typeof(AttackType), item);
                }
            }
            else if(targetName == "NormalizedSpeed")
            {
                nodeData._normalizedSpeed = bool.Parse(targetValue);
            }
            else if(targetName == "RotateBySpeed")
            {
                nodeData._rotateBySpeed = bool.Parse(targetValue);
            }
            else if(targetName == "RotateSpeed")
            {
                nodeData._rotateSpeed = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "DirectionUpdateOnce")
            {
                nodeData._directionUpdateOnce = bool.Parse(targetValue);
            }
            else if(targetName == "FlipTypeUpdateOnce")
            {
                nodeData._flipTypeUpdateOnce = bool.Parse(targetValue);
            }
            else if(targetName == "HasAngleSector")
            {
                nodeData._hasAngleSector = bool.Parse(targetValue);
            }
            else if(targetName == "AngleSectorCount")
            {
                nodeData._angleSectorCount = int.Parse(targetValue);
            }
            else if(targetName == "Time")
            {
                actionTime = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName =="Flags")
            {
                string[] flags = targetValue.Split(' ');
                
                for(int i = 0; i < flags.Length; ++i)
                {
                    nodeData._actionFlag |= (ulong)((ActionFlags)System.Enum.Parse(typeof(ActionFlags), flags[i]));
                }
            }
            else if(targetName == "HeadUpOffset")
            {
                nodeData._headUpOffset = float.Parse(targetValue);
            }
            else if(targetName == "ActiveCollision")
            {
                nodeData._activeCollision = bool.Parse(targetValue);
            }
            else
            {
                DebugUtil.assert_fileOpen(false,"invalid attribute type !!! : {0} [Line: {1}] [FileName: {2}]", _currentFileName, XMLScriptConverter.getLineNumberFromXMLNode(node), targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            }
        }

        XmlNodeList nodeList = node.ChildNodes;
        int branchStartIndex = branchDataList.Count;

        List<AnimationPlayDataInfo> animationPlayDataInfoList = new List<AnimationPlayDataInfo>();

        string masterPath = "";

        for(int i = 0; i < nodeList.Count; ++i)
        {
            if(nodeList[i].Name == "AnimationHeader")
            {
                XmlAttributeCollection xmlAttributes = nodeList[i].Attributes;
                for(int index = 0; index < xmlAttributes.Count; ++index)
                {
                    string attrName = xmlAttributes[index].Name;
                    string attrValue = getGlobalVariable(xmlAttributes[index].Value, _globalVariables);

                    if(attrName == "MasterPath")
                    {
                        masterPath = attrValue;
                    }
                }
                

            }
            else if(nodeList[i].Name == "Animation")
            {
                AnimationPlayDataInfo animationData = ReadActionAnimation(nodeList[i],masterPath,defaultFPS, filePath, actionTime);
                nodeData._animationInfoIndex = animationDataList.Count;
                animationData._hasMovementGraph = nodeData._movementType == MovementBase.MovementType.RootMotion;
                animationPlayDataInfoList.Add(animationData);

#if UNITY_EDITOR
                animationData._lineNumber = XMLScriptConverter.getLineNumberFromXMLNode(nodeList[i]);
#endif
            }
            else if(nodeList[i].Name == "Branch")
            {
                ActionGraphBranchData branchData = ReadActionBranch(nodeList[i],ref actionCompareDic,ref compareDataList, ref _globalVariables, filePath);
                if(branchData == null)
                {
                    DebugUtil.assert_fileOpen(false,"invalid branch data [Line: {0}] [FileName: {1}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }
                    
#if UNITY_EDITOR
                branchData._lineNumber = XMLScriptConverter.getLineNumberFromXMLNode(nodeList[i]);
#endif
                branchDataList.Add(branchData);
            }
            else if(nodeList[i].Name == "UseBranchSet")
            {
                string branchSetName = "";
                XmlAttributeCollection branchSetAttr = nodeList[i].Attributes;
                for(int branchSetAttrIndex = 0; branchSetAttrIndex < branchSetAttr.Count; ++branchSetAttrIndex)
                {
                    if(branchSetAttr[branchSetAttrIndex].Name == "Name")
                    {
                        branchSetName = branchSetAttr[branchSetAttrIndex].Value;
                    }
                }

                if(branchSetDic.ContainsKey(branchSetName) == false)
                {
                    DebugUtil.assert_fileOpen(false, "branch set not exists : {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),branchSetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }

                XmlNodeList branchSetNodeList = branchSetDic[branchSetName];
                for(int branchSetNodeListIndex = 0; branchSetNodeListIndex < branchSetNodeList.Count; ++branchSetNodeListIndex)
                {
                    if(branchSetNodeList[branchSetNodeListIndex].Name != "Branch")
                    {
                        DebugUtil.assert_fileOpen(false, "wrong branch type : {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),branchSetNodeList[branchSetNodeListIndex].Name, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        return null;
                    }

                    ActionGraphBranchData branchData = ReadActionBranch(branchSetNodeList[branchSetNodeListIndex],ref actionCompareDic,ref compareDataList, ref _globalVariables, filePath);
                    if(branchData == null)
                    {
                        DebugUtil.assert_fileOpen(false,"invalid branch data [Line: {0}] [FileName: {1}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        return null;
                    }

#if UNITY_EDITOR
                    branchData._lineNumber = XMLScriptConverter.getLineNumberFromXMLNode(nodeList[i]);
#endif

                    branchDataList.Add(branchData);
                }
            }
        }

        // if(branchStartIndex == branchDataList.Count)
        // {
        //     DebugUtil.assert_fileOpen(false,"branch data not exists [Line: {0}] [FileName: {1}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), XMLScriptConverter.getLineFromXMLNode(node), filePath);
        //     return null;
        // }

        animationDataList.Add(animationPlayDataInfoList.ToArray());

        nodeData._animationInfoCount = animationPlayDataInfoList.Count;
        nodeData._branchIndexStart = branchStartIndex;
        nodeData._branchCount = branchDataList.Count - branchStartIndex;

        return nodeData;
    }

    public static AnimationPlayDataInfo ReadActionAnimation(XmlNode node, string masterPath, float defaultFPS, string filePath, float actionTime = -1f)
    {
        AnimationPlayDataInfo playData = new AnimationPlayDataInfo();
        playData._framePerSec = actionTime == -1f ? defaultFPS : -1f;
        playData._actionTime = actionTime;

        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, _globalVariables);

            if(targetName == "Path")
            {
                playData._path = masterPath + targetValue;
            }
            else if(targetName == "FramePerSecond")
            {
                if(actionTime != -1f)
                {
                    DebugUtil.assert_fileOpen(false, "ActionTime이 존재하면 FramePerSecond를 쓰면 안됩니다 [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    continue;
                }

                playData._framePerSec = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "StartFrame")
            {
                playData._startFrame = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "EndFrame")
            {
                playData._endFrame = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "XFlip")
            {
                playData._flipState.xFlip = bool.Parse(targetValue);
            }
            else if(targetName == "YFlip")
            {
                playData._flipState.yFlip = bool.Parse(targetValue);
            }
            else if(targetName == "Loop")
            {
                playData._isLoop = bool.Parse(targetValue);
            }
            else if(targetName == "RotationPreset")
            {
                AnimationRotationPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationRotationPreset") as AnimationRotationPreset;
                playData._rotationPresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "ScalePreset")
            {
                AnimationScalePreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationScalePreset") as AnimationScalePreset;
                playData._scalePresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "TranslationPreset")
            {
                AnimationTranslationPreset preset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationTranslationPreset") as AnimationTranslationPreset;
                playData._translationPresetData = preset.getPresetData(targetValue);
            }
            else if(targetName == "AngleBaseAnimation")
            {
                playData._isAngleBaseAnimation = bool.Parse(targetValue);
            }
            else if(targetName == "MultiSelectUpdateOnce")
            {
                playData._multiSelectConditionUpdateOnce = bool.Parse(targetValue);
            }
            else if(targetName == "AngleBaseAnimationCount")
            {
                playData._angleBaseAnimationSpriteCount = int.Parse(targetValue);
            }
            else if(targetName == "LoopCount")
            {
                playData._animationLoopCount = int.Parse(targetValue);
            }
            else if(targetName == "Preset")
            {
                string path = masterPath + targetValue;
                playData._path = path;

                ScriptableObject[] scriptableObjects = ResourceContainerEx.Instance().GetScriptableObjects(path);
                if(scriptableObjects == null || (scriptableObjects[0] is AnimationCustomPreset) == false)
                {
                    DebugUtil.assert_fileOpen(false, "애니메이션 프리셋이 존재하지 않습니다. [Path: {0}] [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),path, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }

                AnimationCustomPreset animationCustomPreset = (scriptableObjects[0] as AnimationCustomPreset);
                playData._customPresetData = animationCustomPreset._animationCustomPresetData;
                playData._customPreset = animationCustomPreset;

                if(animationCustomPreset._rotationPresetName != "")
                {
                    AnimationRotationPreset rotationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationRotationPreset") as AnimationRotationPreset;
                    playData._rotationPresetData = rotationPreset.getPresetData(animationCustomPreset._rotationPresetName);
                }
                
                if(animationCustomPreset._scalePresetName != "")
                {
                    AnimationScalePreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationScalePreset") as AnimationScalePreset;
                    playData._scalePresetData = scalePreset.getPresetData(animationCustomPreset._scalePresetName);
                }

                if(animationCustomPreset._translationPresetName != "")
                {
                    AnimationTranslationPreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationTranslationPreset") as AnimationTranslationPreset;
                    playData._translationPresetData = scalePreset.getPresetData(animationCustomPreset._translationPresetName);
                }
                
                playData._frameEventDataCount = playData._frameEventData == null ? 0 : playData._frameEventData.Length;
            }
            else if(targetName == "Duration")
            {
                playData._duration = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else
            {
                DebugUtil.assert_fileOpen(false, "invalid animation attribute: {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                return null;
            }
        }

        if((playData._startFrame > playData._endFrame) && playData._endFrame != -1)
        {
            DebugUtil.assert_fileOpen(false, "시작 프레임은 끝 프레임보다 커질 수 없습니다.: [Path: {0}] [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),playData._path, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        if(node.HasChildNodes == true)
        {
            XmlNodeList nodeList = node.ChildNodes;
            List<ActionFrameEventBase> frameEventList = new List<ActionFrameEventBase>();
            List<ActionFrameEventBase> timeEventList = new List<ActionFrameEventBase>();
            List<MultiSelectAnimationData> multiSelectAnimationList = new List<MultiSelectAnimationData>();
            for(int i = 0; i < nodeList.Count; ++i)
            {
                if(nodeList[i].Name == "FrameEvent")
                {
                    ActionFrameEventBase frameEvent = FrameEventLoader.readFromXMLNode(nodeList[i], filePath);
                    if(frameEvent == null)
                        continue;
                    
                    if(frameEvent._isTimeBase)
                        timeEventList.Add(frameEvent);
                    else
                        frameEventList.Add(frameEvent);
                }
                else if(nodeList[i].Name == "MultiSelectAnimation")
                {
                    MultiSelectAnimationData animationData = readMultiSelectAnimationData(nodeList[i], filePath);
                    if(animationData == null)
                        continue;

                    multiSelectAnimationList.Add(animationData);
                }
                else
                {
                    DebugUtil.assert_fileOpen(false,"애니메이션 차일드로 들어올 수 없는 타입입니다. 오타인가요? : {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),nodeList[i].Name, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                    return null;
                }
            }

            

            timeEventList.Sort((x,y)=>{
                return x._startFrame.CompareTo(y._startFrame);
            });

            if(playData._frameEventData == null || playData._frameEventData.Length == 0)
            {
                frameEventList.Sort((x,y)=>{
                    return x._startFrame.CompareTo(y._startFrame);
                });

                playData._frameEventDataCount = frameEventList.Count;
                playData._frameEventData = frameEventList.ToArray();
            }
            
            playData._timeEventDataCount = timeEventList.Count;
            playData._timeEventData = timeEventList.ToArray();

            playData._multiSelectAnimationDataCount = multiSelectAnimationList.Count;
            playData._multiSelectAnimationData = multiSelectAnimationList.ToArray();
        }

        return playData;
    }

    private static MultiSelectAnimationData readMultiSelectAnimationData(XmlNode node, string filePath)
    {
        MultiSelectAnimationData animationData = new MultiSelectAnimationData();

        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, _globalVariables);

            if(targetName == "Path")
            {
                animationData._path = targetValue;
            }
            else if(targetName == "Condition")
            {
                ActionGraphConditionCompareData compareData = ReadConditionCompareData(targetValue, _globalVariables, node, filePath);
                if(compareData == null)
                    return null;

                animationData._actionConditionData = compareData;
            }
            else
            {
                DebugUtil.assert_fileOpen(false,"유효하지 않은 MultiSelectAnimation 어트리뷰트 입니다.: {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                return null;
            }
        }

        if(animationData._path == "")
        {
            DebugUtil.assert_fileOpen(false,"애니메이션 경로가 데이터에 존재하지 않습니다. [Line: {0}] [FileName: {1}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }
        else if(animationData._actionConditionData == null)
        {
            DebugUtil.assert_fileOpen(false,"MultiSelectAnimation은 반드시 Condition 데이터가 있어야 합니다. [Line: {0}] [FileName: {1}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        return animationData;
    }

    public static ActionGraphNodeData createDummyActionNode()
    {
        ActionGraphNodeData nodeData = new ActionGraphNodeData();
        AnimationPlayDataInfo animationPlayDataInfo = new AnimationPlayDataInfo();

        nodeData._nodeName = "SequencerDummyAction";

        nodeData._animationInfoCount = 1;
        nodeData._animationInfoIndex = -1;

        nodeData._branchCount = 0;
        nodeData._branchIndexStart = -1;

        nodeData._isDummyAction = true;

        animationPlayDataInfo._hasMovementGraph = false;

        return nodeData;
    }

    public static ActionGraphBranchData ReadActionBranch(XmlNode node, ref Dictionary<ActionGraphBranchData, string> actionCompareDic,  ref List<ActionGraphConditionCompareData> compareDataList, ref Dictionary<string, string> globalVariableContainer, string filePath)
    {
        ActionGraphBranchData branchData = new ActionGraphBranchData();
        XmlAttributeCollection actionAttributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < actionAttributes.Count; ++attrIndex)
        {
            string targetName = actionAttributes[attrIndex].Name;
            string targetValue = getGlobalVariable(actionAttributes[attrIndex].Value, globalVariableContainer);

            if(targetName == "Condition")
            {
                if(targetValue == "")
                    continue;

                ActionGraphConditionCompareData conditionData = ReadConditionCompareData(targetValue, globalVariableContainer, node, filePath);
                if(conditionData == null)
                    return null;

                branchData._conditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(conditionData);
            }
            else if(targetName == "Key")
            {
                if(targetValue == "")
                    continue;
                    
                ActionGraphConditionCompareData keyConditionData = ReadConditionCompareData(targetValue, globalVariableContainer, node, filePath);
                if(keyConditionData == null)
                    return null;

                branchData._keyConditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(keyConditionData);
            }
            else if(targetName == "Weight")
            {
                if(targetValue == "")
                    continue;
                
                targetValue = "getWeight_" + targetValue;
                ActionGraphConditionCompareData keyConditionData = ReadConditionCompareData(targetValue, globalVariableContainer, node, filePath);
                if(keyConditionData == null)
                    return null;

                branchData._weightConditionCompareDataIndex = compareDataList.Count;
                compareDataList.Add(keyConditionData);
            }
            else if(targetName == "Execute")
            {
                actionCompareDic.Add(branchData, targetValue);
            }
            else
            {
                DebugUtil.assert_fileOpen(false, "invalid branch attribute: {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),targetName, XMLScriptConverter.getLineFromXMLNode(node), filePath);
                return null;
            }
        }


        return branchData;
    }

    public static ActionGraphConditionCompareData ReadConditionCompareData(string formula, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        formula = formula.Replace(" ","");
        List<ActionGraphConditionNodeData> symbolList = new List<ActionGraphConditionNodeData>();
        List<ConditionCompareType> compareTypeList = new List<ConditionCompareType>();
        
        //int end;
        //int resultIndex = 0;
        //DebugUtil.assert(ReadConditionFormula(formula,0, ref resultIndex, out end,symbolList,compareTypeList) == true,"Tlqkfsusdk");
        DebugUtil.assert_fileOpen(readConditionFormula(formula,ref symbolList,ref compareTypeList, globalVariableContainer, node, filePath) == true,"[Line: {0}] [FileName: {1}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), XMLScriptConverter.getLineFromXMLNode(node), filePath);

        if(symbolList.Count == 1)
        {
            ActionGraphConditionNodeData conditionNodeData = symbolList[symbolList.Count - 1];
            DebugUtil.assert_fileOpen(ActionGraph.isNodeType(conditionNodeData,ConditionNodeType.Bool) == true, "Condition 결과가 Boolean 타입이 아닙니다. [Type: {0}]",filePath,XMLScriptConverter.getLineNumberFromXMLNode(node),ConditionNodeInfoPreset._nodePreset[conditionNodeData._symbolName]._nodeType);
        }

        ActionGraphConditionCompareData compareData = new ActionGraphConditionCompareData();
        compareData._compareTypeArray = compareTypeList.ToArray();
        compareData._compareTypeCount = compareTypeList.Count;
        compareData._conditionNodeDataArray = symbolList.ToArray();
        compareData._conditionNodeDataCount = symbolList.Count;

        return compareData;
    }

    private static bool readConditionFormula(string formula, ref List<ActionGraphConditionNodeData> symbolList, ref List<ConditionCompareType> compareTypeList, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        string calcFormula = formula;
        calcFormula = calcFormula.Insert(0,"(");
        calcFormula += ")";

        int result = 0;
        int finalIndex = readFormulaBracket(ref calcFormula,ref result,0,ref symbolList,ref compareTypeList, globalVariableContainer, node, filePath);

        return finalIndex != -1;
    }

    private static int readFormulaBracket(ref string formula, ref int resultIndex, int startOffset, ref List<ActionGraphConditionNodeData> symbolList, ref List<ConditionCompareType> compareTypeList, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath )
    {
        if(formula.Length <= startOffset || formula[startOffset] == ')')
        {
            DebugUtil.assert_fileOpen(false, "condition formular is invalid {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), formula, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return -1;
        }

        int stringLength = formula.Length;
        int endOffset = -1;
        for(int i = startOffset + 1; i < stringLength; ++i)
        {
            if(formula[i] == '(')
            {
                int length = readFormulaBracket(ref formula, ref resultIndex, i, ref symbolList, ref compareTypeList, globalVariableContainer, node, filePath);
                if(length == -1)
                    return -1;

                formula = formula.Remove(i, length + 1);
                formula = formula.Insert(i, "RESULT_" + resultIndex++);
            }
            
            if(formula[i] == ')')
            {
                endOffset = i;
                break;
            }
        }

        if(endOffset == -1)
        {
            DebugUtil.assert_fileOpen(false, "condition formular is invalid {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), formula, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return -1;
        }

        int finalLength = endOffset - startOffset;
        string calcFormula = formula.Substring(startOffset + 1,finalLength - 1);

        SplitToMark(calcFormula, ref resultIndex, in symbolList, in compareTypeList, globalVariableContainer, node, filePath);
        return finalLength;
    }

    private static void SplitToMark(string formula, ref int resultIndex, in List<ActionGraphConditionNodeData> symbolList, in List<ConditionCompareType> compareTypeList, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        string calcFormula = formula;
        int symbolEndIndex = 0;
        int markLength = 0;
        ConditionCompareType compareType = ConditionCompareType.Count;

        int loopCount = 0;
        while(ReadNearestMark(calcFormula,out symbolEndIndex,out markLength, out compareType) == true)
        {
            string symbol = calcFormula.Substring(0,symbolEndIndex);
            calcFormula = calcFormula.Remove(0,symbolEndIndex + markLength);

            symbolList.Add(getConditionNodeData(symbol, globalVariableContainer, node, filePath));
            compareTypeList.Add(compareType);

            if(++loopCount >= 2)
                symbolList.Add(getConditionNodeData("RESULT_" + resultIndex++, globalVariableContainer, node, filePath));
  
        }
        

        symbolList.Add(getConditionNodeData(calcFormula,globalVariableContainer, node, filePath));

        return;
    }

    private static ActionGraphConditionNodeData getConditionNodeData(string symbol, Dictionary<string, string> globalVariableContainer, XmlNode node, string filePath)
    {
        symbol = getGlobalVariable(symbol, globalVariableContainer);
        ActionGraphConditionNodeData nodeData = isLiteral(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isResult(symbol, node, filePath);
        if(nodeData != null)
            return nodeData;

        nodeData = isStatus(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isKey(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isCustomValue(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isTargetFrameTag(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isFrameTag(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isWeight(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = isAIGraphCoolTime(symbol);
        if(nodeData != null)
            return nodeData;

        nodeData = new ActionGraphConditionNodeData();
    
        if(ConditionNodeInfoPreset._nodePreset.ContainsKey(symbol) == false)
        {
            DebugUtil.assert_fileOpen(false,"대상 Condition Symbol이 존재하지 않습니다. 오타는 아닌가요? : {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),symbol, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        nodeData._symbolName = symbol;
        return nodeData;
    }

    private static ActionGraphConditionNodeData_AICustomValue isCustomValue(string symbol)
    {
        if(symbol.Contains("customValue_") == false)
            return null;
        
        ActionGraphConditionNodeData_AICustomValue item = new ActionGraphConditionNodeData_AICustomValue();
        item._symbolName = "CustomValue";
        item._customValueName = symbol.Replace("customValue_","");
        return item;
    }

    private static ActionGraphConditionNodeData_FrameTag isTargetFrameTag(string symbol)
    {
        if(symbol.Contains("getTargetFrameTag_") == false)
            return null;
        
        ActionGraphConditionNodeData_FrameTag item = new ActionGraphConditionNodeData_FrameTag();
        item._symbolName = "TargetFrameTag";
        item._targetFrameTag = symbol.Replace("getTargetFrameTag_","");
        return item;
    }

    private static ActionGraphConditionNodeData_FrameTag isFrameTag(string symbol)
    {
        if(symbol.Contains("getFrameTag_") == false)
            return null;

        ActionGraphConditionNodeData_FrameTag item = new ActionGraphConditionNodeData_FrameTag();
        item._symbolName = "FrameTag";
        item._targetFrameTag = symbol.Replace("getFrameTag_","");
        return item;
    }

    private static ActionGraphConditionNodeData_Weight isWeight(string symbol)
    {
        if(symbol.Contains("getWeight_") == false)
            return null;

        symbol = symbol.Replace("getWeight_","");
        string[] groupData = symbol.Split('^');

        if(groupData == null || groupData.Length > 2 || groupData.Length < 2)
        {
            DebugUtil.assert(false, "invalid weight data: {0}",symbol);
            return null;
        }

        ActionGraphConditionNodeData_Weight item = new ActionGraphConditionNodeData_Weight();
        item._symbolName = "Weight";
        item._weightGroupKey = groupData[0];
        item._weightName = groupData[1];
        return item;
    }

    private static ActionGraphConditionNodeData_Status isStatus(string symbol)
    {
        if(symbol.Contains("getStat_") == false)
            return null;

        ActionGraphConditionNodeData_Status item = new ActionGraphConditionNodeData_Status();
        item._symbolName = "Status";
        item._targetStatus = symbol.Replace("getStat_","");
        return item;
    }

    private static ActionGraphConditionNodeData_AIGraphCoolTime isAIGraphCoolTime(string symbol)
    {
        if(symbol.Contains("aiGraphCoolTime_") == false)
            return null;

        ActionGraphConditionNodeData_AIGraphCoolTime item = new ActionGraphConditionNodeData_AIGraphCoolTime();
        item._symbolName = "AIGraphCoolTime";
        item._graphNodeName = symbol.Replace("aiGraphCoolTime_","");
        return item;
    }

    private static ActionGraphConditionNodeData_Key isKey(string symbol)
    {
        if(symbol.Contains("getKey_") == false)
            return null;

        ActionGraphConditionNodeData_Key item = new ActionGraphConditionNodeData_Key();
        item._symbolName = "Key";
        item._targetKeyName = symbol.Replace("getKey_","");
        return item;
    }

    private static ActionGraphConditionNodeData_ConditionResult isResult(string symbol, XmlNode node, string filePath)
    {
        if(symbol.Contains("RESULT_") == false)
            return null;

        string index = symbol.Substring(7);
        int indexInt = 0;
        if(int.TryParse(index,out indexInt) == false)
        {
            DebugUtil.assert_fileOpen(false,"result index invalid: {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node),symbol, XMLScriptConverter.getLineFromXMLNode(node), filePath);
            return null;
        }

        ActionGraphConditionNodeData_ConditionResult result = new ActionGraphConditionNodeData_ConditionResult();
        result._resultIndex = indexInt;
        result._symbolName = "RESULT";

        return result;
    }

    private static ActionGraphConditionNodeData_Literal isLiteral(string symbol)
    {
        if(int.TryParse(symbol,out int intRresult))
        {
            ActionGraphConditionNodeData_Literal literal = new ActionGraphConditionNodeData_Literal();
            literal._symbolName = "Literal_Int";
            literal.setLiteral(System.BitConverter.GetBytes(intRresult));

            return literal;
        }
        else if(float.TryParse(symbol,out float floatResult))
        {
            ActionGraphConditionNodeData_Literal literal = new ActionGraphConditionNodeData_Literal();
            literal._symbolName = "Literal_Float";
            literal.setLiteral(System.BitConverter.GetBytes(floatResult));

            return literal;
        }
        else if(bool.TryParse(symbol,out bool boolResult))
        {
            ActionGraphConditionNodeData_Literal literal = new ActionGraphConditionNodeData_Literal();
            literal._symbolName = "Literal_Bool";
            literal.setLiteral(System.BitConverter.GetBytes(boolResult));

            return literal;
        }

        return null;
    }

    private static bool ReadNearestMark(string formula, out int symbolEndIndex, out int markLength, out ConditionCompareType compareType)
    {
        compareType = ConditionCompareType.Count;
        symbolEndIndex = int.MaxValue;
        markLength = 0;
        if(formula.Contains("&&") == true && formula.IndexOf("&&") < symbolEndIndex)
        {
            compareType = ConditionCompareType.And;
            symbolEndIndex = formula.IndexOf("&&");
            markLength = 2;
        }
        if(formula.Contains("||") == true && formula.IndexOf("||") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Or;
            symbolEndIndex = formula.IndexOf("||");
            markLength = 2;
        }
        if(formula.Contains("==") == true && formula.IndexOf("==") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Equals;
            symbolEndIndex = formula.IndexOf("==");
            markLength = 2;
        }

        if(formula.Contains(">") == true && formula.IndexOf(">") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Greater;
            symbolEndIndex = formula.IndexOf(">");
            markLength = 1;
        }
        if(formula.Contains(">=") == true && formula.IndexOf(">=") <= symbolEndIndex)
        {
            compareType = ConditionCompareType.GreaterEqual;
            symbolEndIndex = formula.IndexOf(">=");
            markLength = 2;
        }

        if(formula.Contains("<") == true && formula.IndexOf("<") < symbolEndIndex)
        {
            compareType = ConditionCompareType.Smaller;
            symbolEndIndex = formula.IndexOf("<");
            markLength = 1;
        }
        if(formula.Contains("<=") == true && formula.IndexOf("<=") <= symbolEndIndex)
        {
            compareType = ConditionCompareType.SmallerEqual;
            symbolEndIndex = formula.IndexOf("<=");
            markLength = 2;
        }
        if(formula.Contains("!=") == true && formula.IndexOf("!=") < symbolEndIndex)
        {
            compareType = ConditionCompareType.NotEquals;
            symbolEndIndex = formula.IndexOf("!=");
            markLength = 2;
        }

        if(markLength == 0 || symbolEndIndex == int.MaxValue || compareType == ConditionCompareType.Count)
            return false;

        return true;
    }

    private static void ReadTitle(XmlNode node, ActionGraphBaseData baseData, out float defaultFPS, out string defaultAction, string filePath)
    {
        defaultFPS = -1f;
        defaultAction = "";

        XmlAttributeCollection attributes = node.Attributes;
        for(int attrIndex = 0; attrIndex < attributes.Count; ++attrIndex)
        {
            string targetName = attributes[attrIndex].Name;
            string targetValue = attributes[attrIndex].Value;

            if(targetName == "Name")
            {
                baseData._name = targetValue;
            }
            else if(targetName == "DefaultFramePerSecond")
            {
                defaultFPS = XMLScriptConverter.valueToFloatExtend(targetValue);
            }
            else if(targetName == "DefaultAction")
            {
                defaultAction = targetValue;
            }
            else if(targetName == "DefaultBuff")
            {
                string[] buffList = targetValue.Split(' ');

                int[] buffKeyList = new int[buffList.Length];
                for(int j = 0; j < buffList.Length; ++j)
                {
                    bool parse = int.TryParse(buffList[j],out int buffKey);
                    if(parse == false)
                        buffKey = StatusInfo.getBuffKeyFromName(buffList[j]);

                    if(buffKey == -1)
                    {
                        DebugUtil.assert_fileOpen(false, "invalidBuff : {0} [Line: {1}] [FileName: {2}]", filePath, XMLScriptConverter.getLineNumberFromXMLNode(node), buffList[j], XMLScriptConverter.getLineFromXMLNode(node), filePath);
                        continue;
                    }

                    buffKeyList[j] = buffKey;
                }

                baseData._defaultBuffList = buffKeyList;
            }

        }
    }


    // public static MovementGraph readFromBinary(string path)
    // {
    //     if(File.Exists(path) == false)
    //     {
    //         DebugUtil.assert(false,"file does not exists : {0}", path);
    //         return null;
    //     }

    //     var fileStream = File.Open(path, FileMode.Create);
    //     var reader = new BinaryReader(fileStream,Encoding.UTF8,false);
    //     MovementGraph graph = ScriptableObject.CreateInstance<MovementGraph>();

    //     graph.deserialize(reader);

    //     reader.Close();
    //     fileStream.Close();
    //     return graph;
    // }

    // public static MovementGraph readFromXMLAndExportToBinary(string xmlPath, string binaryPath)
    // {
    //     MovementGraph graph = readFromXML(xmlPath);
    //     if(graph == null)
    //     {
    //         return null;
    //     }

    //     exportToBinary(graph, binaryPath);
    //     return graph;
    // }

    // public static void exportToBinary(MovementGraph graph, string path)
    // {
    //     var fileStream = File.Open(path, FileMode.Create);
    //     var writer = new BinaryWriter(fileStream,Encoding.UTF8,false);

    //     graph.serialize(writer);

    //     writer.Close();
    //     fileStream.Close();
    // }
}
