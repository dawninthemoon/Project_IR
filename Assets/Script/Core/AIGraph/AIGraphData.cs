using System.Collections.Generic;

[System.Serializable]
public class AIGraphBaseData
{
    public string                               _name;
    public AIGraphNodeData[]                    _aiGraphNodeData = null;
    public AIPackageBaseData[]                  _aiPackageData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;
    public AIGraphCustomValue[]                 _customValueData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string, AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _aiPackageCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;
    public int                                  _customValueDataCount = -1;

#if UNITY_EDITOR
    public string _fullPath = "";
#endif
}

[System.Serializable]
public class AIGraphNodeData
{
    public AIGraphNodeData()
    {
        _packageIndex = -1;
        _branchIndexStart = 0;
        _branchCount = 0;
        _coolDownTime = 0f;
    }

    public string                       _nodeName;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string,AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public int                          _packageIndex;
    public int                          _branchIndexStart;
    public int                          _branchCount;
    public float                        _coolDownTime;

#if UNITY_EDITOR
    public int _lineNumber = 0;
#endif
}





[System.Serializable]
public class AIPackageBaseData
{
    public string                               _name;
    public AIPackageNodeData[]                  _aiPackageNodeData = null;
    public ActionGraphBranchData[]              _branchData = null;
    public ActionGraphConditionCompareData[]    _conditionCompareData = null;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string,AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public Dictionary<AIPackageEventType, AIChildFrameEventItem> _aiPackageEvents = new Dictionary<AIPackageEventType, AIChildFrameEventItem>();

    public int                                  _defaultAIIndex = -1;

    public int                                  _aiNodeCount = -1;
    public int                                  _branchCount = -1;
    public int                                  _conditionCompareDataCount = -1;

#if UNITY_EDITOR
    public string _fullPath = "";
#endif
}

[System.Serializable]
public class AIPackageNodeData
{
    public AIPackageNodeData()
    {
        _branchIndexStart = 0;
        _branchCount = 0;
    }

    public string                       _nodeName;

    public float                        _updateTime = 1f;
    public TargetSearchType             _targetSearchType = TargetSearchType.None;
    public SearchIdentifier             _searchIdentifier = SearchIdentifier.Count;
    public float                        _targetSearchRange = 999f;
    public float                        _targetSearchStartRange = 0f;
    public float                        _targetSearchSphereRadius = 0f;

    public UnityEngine.Vector3          _targetPosition;
    public float                        _arriveThreshold = 0.1f;
    public bool                         _hasTargetPosition = false;

    public Dictionary<AIChildEventType, AIChildFrameEventItem> _aiEvents = new Dictionary<AIChildEventType, AIChildFrameEventItem>();
    public Dictionary<string,AIChildFrameEventItem> _customAIEvents = new Dictionary<string, AIChildFrameEventItem>();

    public int                          _branchIndexStart;
    public int                          _branchCount;

#if UNITY_EDITOR
    public int _lineNumber = 0;
#endif
}

[System.Serializable]
public class AIGraphCustomValue
{
    public string _name = "";
    public float _customValue = 0f;
}