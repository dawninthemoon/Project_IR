using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageSpawnCharacterActiveType
{
    Spawn = 0,
    PointActivated,
} 

[System.Serializable]
public class StagePointCharacterSpawnData
{
    public string   _characterKey = "";
    public string   _uniqueKey = "";
    public string   _uniqueGroupKey = "";
    public string   _startAction = "";
    public bool     _flip = false;
    public bool     _hideWhenDeactive = false;
    public SearchIdentifier _searchIdentifier = SearchIdentifier.Enemy;
    public StageSpawnCharacterActiveType _activeType = StageSpawnCharacterActiveType.Spawn;
    public Vector3  _localPosition = Vector3.zero;
}

[System.Serializable]
public class StagePointData
{
    public string       _pointName = "";
    public Vector3      _stagePoint = Vector3.zero;
    public float        _maxLimitedDistance = 0f;
    public float        _cameraZoomSize = 0f;
    public float        _cameraZoomSpeed = 4f;
    public bool         _lerpCameraZoom = false;
    public bool         _lockCameraInBound = false;
    public bool         _cameraBoundToTrigger = false;
    public string[]     _onEnterSequencerPath = new string[0];
    public string[]     _onExitSequencerPath = new string[0];

    public StagePointCharacterSpawnData[] _characterSpawnData = null;

    public bool                     _useTriggerBound = false;
    public SearchIdentifier         _targetSearchIdentifier = SearchIdentifier.Enemy;
    public float                    _triggerWidth = 1f;
    public float                    _triggerHeight = 1f;
    public Vector3                  _triggerOffset = Vector3.zero;

    public StagePointData(Vector3 point) {_stagePoint = point;}
}

[System.Serializable]
public class MiniStageListItem
{
    public Vector3                  _localStagePosition;
    public MiniStageData            _data;

    public SearchIdentifier         _overrideTargetSearchIdentifier = SearchIdentifier.Enemy;

    public float                    _overrideTriggerWidth = 0f;
    public float                    _overrideTriggerHeight = 0f;
    public Vector3                  _overrideTriggerOffset = Vector3.zero;
}

[System.Serializable]
public class MarkerItem
{
    public string       _name;
    public Vector3      _position;
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Object/Stage Data", order = 3)]
public class StageData : ScriptableObject
{
    public string           _stageName;
    public GameObject       _backgroundPrefabPath = null;
    public bool             _isMiniStage = false;

    public List<StagePointData>         _stagePointData = new List<StagePointData>();
    public List<MiniStageListItem>      _miniStageData = new List<MiniStageListItem>();
    public List<MarkerItem>             _markerData = new List<MarkerItem>();
    public List<MovementTrackData>      _trackData = new List<MovementTrackData>();

    public MarkerItem findMarker(string markerName)
    {
        foreach(var item in _markerData)
        {
            if(item._name == markerName)
                return item;
        }

        return null;
    }
}
