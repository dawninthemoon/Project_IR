using System.Collections.Generic;
using UnityEngine;

public class AIGraph
{
    struct ReservedAIEventItem
    {
        public AIChildEventType _eventType;
        public Dictionary<AIChildEventType,AIChildFrameEventItem> _eventDic;
    }

    struct AIGraphStateCoolDownSet
    {
        public float _checkStartTime;
        public float _coolTime;
    }

    private int _currentAINodeIndex = -1;
    private int _prevAINodeIndex = -1;

    private int _currentPackageStateIndex = -1;
    private int _prevPackageStateIndex = -1;

    private int _changePackageStateIndex = -1;

    private float _updateTimer = 0f;
    private float _arriveThreshold = 0f;

    private float _aiPackageExecutedTimer = 0f;
    private float _aiGraphExecutedTimer = 0f;

    private bool _packageEnd = false;
    private bool _arrived = false;

    private bool _aiStateChanged = false;
    private bool _aiPackageStateChanged = false;

    private bool _rotateProcess = false;
    private float _rotateProcessTime = 0f;
    private float _rotateProcessTimer = 0f;
    private float _rotateAngle = 0f;
    private Vector3 _rotateStartDirection = Vector3.right;

    private Vector3 _recentlyAiDirection = Vector3.zero;

    private Vector3 _recentlyAiTargetPosition = Vector3.zero;

    private ActionGraph _actionGraph;

    private AIGraphBaseData _aiGraphBaseData;
    private List<AIChildEventType> _reservedEvents = new List<AIChildEventType>();
    private List<ReservedAIEventItem> _reservedTargetEvents = new List<ReservedAIEventItem>();

    private List<string> _reservedCustomEvents = new List<string>();

    private Dictionary<string, AIGraphStateCoolDownSet> _graphStateCoolTimeMap = new Dictionary<string, AIGraphStateCoolDownSet>();


    public AIGraph(){}
    public AIGraph(ActionGraph actionGraph, AIGraphBaseData baseData){_actionGraph = actionGraph; _aiGraphBaseData = baseData;}

    public bool isValid()
    {
        return _aiGraphBaseData != null && _actionGraph != null && _aiGraphBaseData._aiPackageCount != 0;
    }

    public void assign()
    {
    }

    public void initialize(GameEntityBase targetEntity, ActionGraph actionGraph, AIGraphBaseData baseData)
    {
        _actionGraph = actionGraph;
        _aiGraphBaseData = baseData;
        
        claerAIGraph();
        setDefaultAINode(targetEntity);
    }

    public void claerAIGraph()
    {
        _currentAINodeIndex = -1;
        _prevAINodeIndex = -1;

        _currentPackageStateIndex = -1;
        _prevPackageStateIndex = -1;

        _changePackageStateIndex = -1;

        _updateTimer = 0f;
        _arriveThreshold = 0f;

        _aiPackageExecutedTimer = 0f;
        _aiGraphExecutedTimer = 0f;

        _packageEnd = false;
        _arrived = false;

        _rotateProcess = false;
        _rotateProcessTime = 0f;
        _rotateProcessTimer = 0f;
        _rotateAngle = 0f;

        _rotateStartDirection = Vector3.right;
        _recentlyAiDirection = Vector3.zero;
        _recentlyAiTargetPosition = Vector3.zero;

        _reservedEvents.Clear();
        _reservedTargetEvents.Clear();
        _reservedCustomEvents.Clear();
        _graphStateCoolTimeMap.Clear();
    }

    public void setDefaultAINode(GameEntityBase targetEntity)
    {
        if(_aiGraphBaseData._defaultAIIndex != -1)
            changeAINode(_aiGraphBaseData._defaultAIIndex, targetEntity, true);
    }

    public bool progress(float deltaTime, GameEntityBase targetEntity)
    {
        if(isValid() == false)
            return false;

        for(int i =0; i < _reservedTargetEvents.Count; ++i)
        {
            processAIEvent(_reservedTargetEvents[i]._eventType,targetEntity, _reservedTargetEvents[i]._eventDic);
        }

        for(int i =0; i < _reservedEvents.Count; ++i)
        {
            processAIEvent(_reservedEvents[i],targetEntity);
        }

        for(int i =0; i < _reservedCustomEvents.Count; ++i)
        {
            processCustomAIEvent(_reservedCustomEvents[i],targetEntity);
        }

        _reservedTargetEvents.Clear();
        _reservedEvents.Clear();
        _reservedCustomEvents.Clear();

        _aiStateChanged = processAINode(deltaTime, getCurrentAINode(), targetEntity);
        _aiPackageStateChanged = processAIPackage(deltaTime, getCurrentAIPackageNode(), targetEntity);

        processRotate(deltaTime);

        if(_actionGraph != null)
        {
            foreach(var item in _graphStateCoolTimeMap)
            {
                _actionGraph.setAIGraphCoolTimeValue(item.Key, graphStateCoolDownCheck(item.Key));
            }
        }


        return true;
    }

    public void updateConditionData()
    {
        
    }

    public void release()
    {

    }

    public bool isAIStateChanged() {return _aiStateChanged;}
    public bool isAIPackageStateChanged() {return _aiPackageStateChanged;}

    public void processRotate(float deltaTime)
    {
        if(_rotateProcess == false)
            return;

        _rotateProcessTimer += deltaTime;
        _rotateProcessTimer = _rotateProcessTimer > _rotateProcessTime ? _rotateProcessTime : _rotateProcessTimer;

        _recentlyAiDirection = Quaternion.Euler(0f,0f,Mathf.Lerp(0f,_rotateAngle,_rotateProcessTimer * (1f / _rotateProcessTime))) * _rotateStartDirection;

        if(_rotateProcessTimer == _rotateProcessTime)
            _rotateProcess = false;
    }

    public void setRotateProcess(float time, float angle)
    {
        _rotateStartDirection = _recentlyAiDirection;
        if(_rotateStartDirection.sqrMagnitude <= 0f)
            _rotateStartDirection = Vector3.right;
        
        _rotateProcess = true;
        _rotateProcessTime = time;
        _rotateProcessTimer = 0f;
        _rotateAngle = angle;
    }

    public void executeAIEvent(AIChildEventType eventType)
    {
        _reservedEvents.Add(eventType);
    }

    public void executeCustomAIEvent(string eventName)
    {
        _reservedCustomEvents.Add(eventName);
    }

    public void executeAIEventTargetReserved(AIChildEventType eventType, Dictionary<AIChildEventType,AIChildFrameEventItem> aiEventDic)
    {
        ReservedAIEventItem item;
        item._eventType = eventType;
        item._eventDic = aiEventDic;

        _reservedTargetEvents.Add(item);
    }

    private bool processAINode(float deltaTime, AIGraphNodeData aiGraphNode, GameEntityBase targetEntity)
    {
        bool nodeChanged = false;
        int startIndex = aiGraphNode._branchIndexStart;
        for(int i = startIndex; i < startIndex + aiGraphNode._branchCount; ++i)
        {
            if(processActionBranch(_aiGraphBaseData._branchData[i],_aiGraphBaseData._conditionCompareData) == true)
            {
                nodeChanged = changeAINode(_aiGraphBaseData._branchData[i]._branchActionIndex, targetEntity, false);
                break;
            }
        }

        _aiGraphExecutedTimer += deltaTime;

        if(nodeChanged == true)
        {
            AIGraphNodeData nodeData = getCurrentAINode();
            if(nodeData._coolDownTime != 0)
                setGraphStateCoolDown(nodeData._nodeName,nodeData._coolDownTime);

            _rotateProcess = false;

            _packageEnd = false;
            _changePackageStateIndex = -1;

            _aiGraphExecutedTimer = 0f;

            processAIEvent(AIChildEventType.AIChildEvent_OnExit, targetEntity, getPrevAINode()._aiEvents);
            processAIEvent(AIChildEventType.AIChildEvent_OnExecute, targetEntity, getCurrentAINode()._aiEvents);
        }

        return nodeChanged;
    }

    private bool processAIPackage(float deltaTime, AIPackageNodeData aiPackageNode, GameEntityBase targetEntity)
    {
        if(_packageEnd == true)
            return false;

        bool stateChanged = false;

        _aiPackageExecutedTimer += deltaTime;

        if(_changePackageStateIndex == -1)
        {
            AIPackageNodeData currentPackageNode = getCurrentAIPackageNode();
            _updateTimer -= deltaTime;

            processAIEvent(AIChildEventType.AIChildEvent_OnFrame, targetEntity, currentPackageNode._aiEvents);

            if(currentPackageNode._hasTargetPosition)
            {
                _recentlyAiDirection = (_recentlyAiTargetPosition - targetEntity.transform.position);
                _arrived = _recentlyAiDirection.sqrMagnitude < (_arriveThreshold * _arriveThreshold);
                _recentlyAiDirection.Normalize();
            }

            if(_updateTimer <= 0f)
                _updateTimer = currentPackageNode._updateTime;
            else
                return false;

            processAIEvent(AIChildEventType.AIChildEvent_OnUpdate, targetEntity, currentPackageNode._aiEvents);

            int startIndex = aiPackageNode._branchIndexStart;
            for(int i = startIndex; i < startIndex + aiPackageNode._branchCount; ++i)
            {
                if(processActionBranch(getCurrentAIPackage()._branchData[i],getCurrentAIPackage()._conditionCompareData) == true)
                {
                    stateChanged = changeAIPackageState(getCurrentAIPackage()._branchData[i]._branchActionIndex);
                    break;
                }

            }
        }
        else
        {
            stateChanged = changeAIPackageState(_changePackageStateIndex);
            _changePackageStateIndex = -1;
        }

        if(stateChanged == true)
        {
            _rotateProcess = false;

            processAIEvent(AIChildEventType.AIChildEvent_OnExit, targetEntity, getPrevAIPackageNode()._aiEvents);
            processAIEvent(AIChildEventType.AIChildEvent_OnExecute, targetEntity, getCurrentAIPackageNode()._aiEvents);

            _updateTimer = getCurrentAIPackageNode()._updateTime;
        }

        return stateChanged;
    }

    private bool changeAINode(int aiPackageIndex, GameEntityBase targetEntity, bool reserveEvent)
    {
        if(aiPackageIndex < 0 || aiPackageIndex >= _aiGraphBaseData._aiNodeCount)
        {
            DebugUtil.assert(false,"잘못된 AI Package Index 입니다");
            return false;
        }

        int prevIndex = _prevAINodeIndex;
        int currIndex = _currentAINodeIndex;

        _prevAINodeIndex = _currentAINodeIndex;
        _currentAINodeIndex = aiPackageIndex;

        changeAIPackageState(getCurrentAIPackage()._defaultAIIndex);
        _prevPackageStateIndex = -1;

        if(reserveEvent)
            executeAIEventTargetReserved(AIChildEventType.AIChildEvent_OnExecute, getCurrentAIPackageNode()._aiEvents);
        else
            processAIEvent(AIChildEventType.AIChildEvent_OnExecute, targetEntity, getCurrentAIPackageNode()._aiEvents);
        _updateTimer = getCurrentAIPackageNode()._updateTime;

        return true;
    }

    public void changeAIPackageStateOther(int aiPackageStateIndex)
    {
        if(aiPackageStateIndex == -1)
        {
            DebugUtil.assert(false,"잘못된 AI Package State 인덱스 입니다: {0}",aiPackageStateIndex);
            return;
        }
        _changePackageStateIndex = aiPackageStateIndex;
        _packageEnd = false;
    }

    private bool changeAIPackageState(int aiPackageStateIndex)
    {
        if(aiPackageStateIndex < 0 || aiPackageStateIndex >= getCurrentAIPackage()._aiNodeCount)
        {
            DebugUtil.assert(false,"잘못된 AI Package Index 입니다 [Index: {0}]",aiPackageStateIndex);
            return false;
        }

        int prevIndex = _prevPackageStateIndex;
        int currIndex = _currentPackageStateIndex;

        _prevPackageStateIndex = _currentPackageStateIndex;
        _currentPackageStateIndex = aiPackageStateIndex;

        _aiPackageExecutedTimer = 0f;

        _arrived = false;
        _arriveThreshold = getCurrentAIPackageNode()._arriveThreshold;
        _recentlyAiTargetPosition = getCurrentAIPackageNode()._targetPosition;

        return true;
    }

    private void processAIEvent(AIChildEventType aiEventType, GameEntityBase targetEntity)
    {
        if(processAIEvent(aiEventType,targetEntity, getCurrentAIPackageNode()._aiEvents))
            return;
        else if(processAIEvent(aiEventType,targetEntity, getCurrentAIPackage()._aiEvents))
            return;
        else if(processAIEvent(aiEventType,targetEntity, getCurrentAINode()._aiEvents))
            return;
        else if(processAIEvent(aiEventType,targetEntity, _aiGraphBaseData._aiEvents))
            return;
    }

    private void processCustomAIEvent(string eventName, GameEntityBase targetEntity)
    {
        if(processCustomAIEvent(eventName,targetEntity, getCurrentAIPackageNode()._customAIEvents))
            return;
        else if(processCustomAIEvent(eventName,targetEntity, getCurrentAIPackage()._customAIEvents))
            return;
        else if(processCustomAIEvent(eventName,targetEntity, getCurrentAINode()._customAIEvents))
            return;
        else if(processCustomAIEvent(eventName,targetEntity, _aiGraphBaseData._customAIEvents))
            return;
    }

    private bool processAIEvent(AIChildEventType aiEventType, GameEntityBase targetEntity, Dictionary<AIChildEventType,AIChildFrameEventItem> aiEventDic)
    {
        if(aiEventDic.ContainsKey(aiEventType) == true)
        {
            AIChildFrameEventItem item = aiEventDic[aiEventType];
            for(int i = 0; i < item._childFrameEventCount; ++i)
            {
                item._childFrameEvents[i].initialize();
                item._childFrameEvents[i].onExecute(targetEntity);
            }

            if(item._consume)
                return true;
        }

        return false;
    }

    private bool processCustomAIEvent(string customEvent, GameEntityBase targetEntity, Dictionary<string,AIChildFrameEventItem> aiEventDic)
    {
        if(aiEventDic.ContainsKey(customEvent) == true)
        {
            AIChildFrameEventItem item = aiEventDic[customEvent];
            for(int i = 0; i < item._childFrameEventCount; ++i)
            {
                item._childFrameEvents[i].initialize();
                item._childFrameEvents[i].onExecute(targetEntity);
            }

            if(item._consume)
                return true;
        }

        return false;
    }

    public bool processActionBranch(ActionGraphBranchData branchData, ActionGraphConditionCompareData[] compareDatas)
    {
        bool weightCondition = true;
        if(branchData._weightConditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData compareData = compareDatas[branchData._weightConditionCompareDataIndex];
            weightCondition = _actionGraph.processActionCondition(compareData);
        }

        bool keyCondition = true;
        if(branchData._keyConditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData keyCompareData = compareDatas[branchData._keyConditionCompareDataIndex];
            keyCondition = _actionGraph.processActionCondition(keyCompareData);
        }

        bool condition = true;
        if(branchData._conditionCompareDataIndex != -1)
        {
            ActionGraphConditionCompareData compareData = compareDatas[branchData._conditionCompareDataIndex];
            condition = _actionGraph.processActionCondition(compareData);
        }

        return weightCondition && keyCondition && condition;

    }

    public Vector3 getRecentlyAIDirection() {return _recentlyAiDirection;}
    public void setAIDirection(Vector3 direction)
    {
        _rotateProcess = false;

        _recentlyAiDirection = direction;
    }
    public void setAIDirection(float angle)
    {
        _rotateProcess = false;

        angle = angle * Mathf.Deg2Rad;
        _recentlyAiDirection.x = Mathf.Cos(angle);
        _recentlyAiDirection.y = Mathf.Sin(angle);
        _recentlyAiDirection.z = 0f;
    }

    private void setGraphStateCoolDown(string aiGraphStateName,float coolTime)
    {
        if(coolTime <= 0f)
            return;
        
        _graphStateCoolTimeMap[aiGraphStateName] = new AIGraphStateCoolDownSet{_checkStartTime = GlobalTimer.Instance().getScaledGlobalTime(), _coolTime = coolTime};
    }

    public bool graphStateCoolDownCheck(string aiGraphStateName)
    {
        if(_graphStateCoolTimeMap.ContainsKey(aiGraphStateName) == false)
            return true;

        AIGraphStateCoolDownSet coolDownSet = _graphStateCoolTimeMap[aiGraphStateName];
        return GlobalTimer.Instance().getScaledGlobalTime() - coolDownSet._checkStartTime >= coolDownSet._coolTime; 
    }

    public AIGraphCustomValue[] getCustomValueData() {return _aiGraphBaseData._customValueData;}

    public TargetSearchType getCurrentTargetSearchType() {return getCurrentAIPackageNode()._targetSearchType;}
    public SearchIdentifier getCurrentSearchIdentifier() {return getCurrentAIPackageNode()._searchIdentifier;}

    public void terminatePackage() {_packageEnd = true;}
    public bool isCurrentPackageEnd() {return _packageEnd;}
    public bool isAIArrivedTarget() {return _arrived;}
    public bool hasTargetPosition() {return getCurrentAIPackageNode()._hasTargetPosition;}

    public float getCurrentGraphExecutedTime() {return _aiGraphExecutedTimer;}
    public float getCurrentPackageExecutedTime() {return _aiPackageExecutedTimer;}
    public float getCurrentTargetSearchRange() {return getCurrentAIPackageNode()._targetSearchRange;}
    public float getCurrentTargetSearchStartRange() {return getCurrentAIPackageNode()._targetSearchStartRange;}
    public float getCurrentTargetSearchSphereRadius() {return getCurrentAIPackageNode()._targetSearchSphereRadius;}
    public string getCurrentAIPackageStateName() {return _packageEnd ? "Package End" : getCurrentAIPackageNode()._nodeName;}
    public string getCurrentPackageName() {return getCurrentAIPackage()._name;}
    public string getCurrentAIStateName() {return getCurrentAINode()._nodeName;}
    public Vector3 getCurrentTargetPosition() {return _recentlyAiTargetPosition;}

    private AIPackageNodeData getCurrentAIPackageNode() {return getCurrentAIPackage()._aiPackageNodeData[_currentPackageStateIndex];}
    private AIPackageNodeData getPrevAIPackageNode() {return getCurrentAIPackage()._aiPackageNodeData[_prevPackageStateIndex];}
    private AIPackageBaseData getCurrentAIPackage() {return _aiGraphBaseData._aiPackageData[getCurrentAINode()._packageIndex];}
    private AIPackageBaseData getPrevAIPackage() {return _aiGraphBaseData._aiPackageData[getPrevAINode()._packageIndex];}
    private AIGraphNodeData getCurrentAINode() {return _aiGraphBaseData._aiGraphNodeData[_currentAINodeIndex];}
    private AIGraphNodeData getPrevAINode() {return _aiGraphBaseData._aiGraphNodeData[_prevAINodeIndex];}

#if UNITY_EDITOR
    public AIGraphNodeData getCurrentAINode_Debug() {return getCurrentAINode();}
    public AIPackageNodeData getCurrentAIPackageNode_Debug() {return getCurrentAIPackageNode();}

    public AIGraphBaseData getAIGraphBaseData_Debug() {return _aiGraphBaseData;}
    public AIPackageBaseData getCurrentPackageBaseData_Debug() {return getCurrentAIPackage();}
#endif
}
