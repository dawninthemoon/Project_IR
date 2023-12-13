using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicInterface
{
    private StatusGraphicInterfaceData _graphicInterfaceData;
    private Transform       _parentTransform;
    private SpriteRenderer  _gagueBar;
    private SpriteRenderer  _placeHolder;

    private Material        _gagueMaterial;
    
    public void updateGague(float percentage)
    {
        _gagueMaterial.SetFloat("_Gague",percentage);
    }

    public void updatePosition(Vector3 position)
    {
        _parentTransform.position = position;
    }

    public void active()
    {
        _parentTransform.gameObject.SetActive(true);
    }

    public void deactive()
    {
        _parentTransform.gameObject.SetActive(false);
    }

    public bool isValidGraphicinterface()
    {
        return _parentTransform != null;
    }

    public string getTargetInteraceName()
    {
        return _graphicInterfaceData._targetStatus;
    }

    public StatusGraphicInterfaceData getStatusGraphicInterfaceData()
    {
        return _graphicInterfaceData;
    }

    public void initialize(StatusGraphicInterfaceData data)
    {
        if(isValidGraphicinterface() == false)
        {
            DebugUtil.assert(false, "invalid status graphic interface");
            return;
        }

        _graphicInterfaceData = data;
        _gagueMaterial.SetColor("_Color", data._interfaceColor);
    }

    public void createGraphicInterface(Vector3 initialPosition, string sortingLayer, int sortingOrder, LayerMask targetLayer, Sprite placeHolder, Sprite gagueBar)
    {
        _parentTransform = new GameObject("GraphicInterface").transform;
        _gagueBar = new GameObject("GagueBar").AddComponent<SpriteRenderer>();
        _placeHolder = new GameObject("placeHolder").AddComponent<SpriteRenderer>();

        _gagueBar.transform.SetParent(_parentTransform);
        _placeHolder.transform.SetParent(_parentTransform);

        _gagueBar.sprite = gagueBar;
        _placeHolder.sprite = placeHolder;

        _gagueBar.gameObject.layer = targetLayer;
        _placeHolder.gameObject.layer = targetLayer;

        _gagueBar.sortingLayerName = sortingLayer;
        _placeHolder.sortingLayerName = sortingLayer;

        _gagueBar.sortingOrder = sortingOrder;
        _placeHolder.sortingOrder = sortingOrder - 1;

        _gagueMaterial = Material.Instantiate(ResourceContainerEx.Instance().GetMaterial("Material/Material_GagueBar"));
        _gagueBar.material = _gagueMaterial;

        updateGague(1f);
        updatePosition(initialPosition);
    }
}

public class StatusGraphicInterface
{
    private static SimplePool<GraphicInterface> _graphicInterfacePool = new SimplePool<GraphicInterface>();

    private ObjectBase                      _targetObject;
    private StatusInfo                      _targetStatusInfo;
    private StatusInfoData                  _targetStatusInfoData;
    private List<GraphicInterface>          _graphicInterface = new List<GraphicInterface>();

    private Vector3                         _interfaceOffset;

    private float                           _reverseOffset = 1f;

    private Sprite _gagueSprite;
    private Sprite _placeHolderSprite;

    public void initialize(ObjectBase targetObject, StatusInfo statusInfo, Vector3 offset, bool reverseOffset)
    {
        _targetObject = targetObject;
        _targetStatusInfo = statusInfo;
        _targetStatusInfoData = statusInfo.getStatusInfoData();
        _interfaceOffset = offset;
        _reverseOffset = reverseOffset ? -1f : 1f;

        if(_gagueSprite == null)
        {
            _gagueSprite = ResourceContainerEx.Instance().GetSprite("Sprites/UI/StatusTest/GagueSprite");
            _placeHolderSprite = ResourceContainerEx.Instance().GetSprite("Sprites/UI/StatusTest/PlaceHolderSprite"); 
        }

        release();

        if(_targetStatusInfoData._graphicInterfaceData == null)
            return;

        for(int index = _targetStatusInfoData._graphicInterfaceData.Length - 1; index >= 0; --index)
        {
            GraphicInterface graphicInterface = _graphicInterfacePool.dequeue();
            if(graphicInterface.isValidGraphicinterface() == false)
                graphicInterface.createGraphicInterface(Vector3.zero, "UI", 0, LayerMask.NameToLayer("EffectEtc"),_placeHolderSprite,_gagueSprite);

            graphicInterface.initialize(_targetStatusInfoData._graphicInterfaceData[index]);
            graphicInterface.active();
            _graphicInterface.Add(graphicInterface);
        }

        updateGague();
        updatePosition();
    }

    public void setInterfaceOffset(Vector3 value)
    {
        _interfaceOffset = value;
    }

    public void release()
    {
        if(_graphicInterface.Count != 0)
        {
            for(int index = 0; index < _graphicInterface.Count; ++index)
            {
                _graphicInterface[index].deactive();
                _graphicInterfacePool.enqueue(_graphicInterface[index]);
            }

            _graphicInterface.Clear();
        }
    }

    public void updateGague()
    {
        if(_targetStatusInfoData._graphicInterfaceData == null)
            return;

        for(int index = 0; index < _graphicInterface.Count; ++index)
        {
            _graphicInterface[index].updateGague(_targetStatusInfo.getCurrentStatusPercentage(_graphicInterface[index].getTargetInteraceName()));
        }
    }

    public void setActive(bool value)
    {
        foreach(var item in _graphicInterface)
        {
            if(value)
                item.active();
            else
                item.deactive();
        }
    }

    public void updatePosition()
    {
        if(_targetStatusInfoData._graphicInterfaceData == null)
            return;

        Vector3 targetPosition = _targetObject.transform.position + _interfaceOffset * _reverseOffset;

        for(int index = 0; index < _graphicInterface.Count; ++index)
        {
            _graphicInterface[index].updatePosition(targetPosition);
            targetPosition.y += _graphicInterface[index].getStatusGraphicInterfaceData()._horizontalGap * _reverseOffset;
        }
    }
}
