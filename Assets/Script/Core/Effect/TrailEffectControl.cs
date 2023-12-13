using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TrailEffectDescription
{
    public float _time;
    public float _width;
    public LineTextureMode _textureMode;
    public string _layerName;
    public string _sortingLayerName;
    public int _sortingOrder;
}

public class TrailEffectControl : MonoBehaviour
{
    public LineRenderer _lineRenderer;
    public LineRenderer _placeholder;

    private Transform   _targetTransform;
    private List<Vector3>   _positionList = new List<Vector3>();

    private int _positionArrayCount;

    public void Start()
    {
    }

    public void setPositions(Vector3[] positionArray, Transform targetTransform)
    {
        _targetTransform = targetTransform;

        copyArrayToList(positionArray);

        _lineRenderer.positionCount = positionArray.Length;
        _lineRenderer.SetPositions(positionArray);

        _placeholder.positionCount = positionArray.Length;
        _placeholder.SetPositions(positionArray);
        updatePositions();
    }

    private void copyArrayToList(Vector3[] positionArray)
    {
        _positionArrayCount = positionArray.Length;

        if(_positionList.Count == 0)
            _positionList.AddRange(positionArray);
        else if(_positionList.Count < positionArray.Length)
            _positionList.AddRange(new Vector3[positionArray.Length - _positionList.Count]);

        for(int index = 0; index < positionArray.Length; ++index)
        {
            _positionList[index] = positionArray[index];
        }
    }

    public void setMaterial(Material material)
    {
        _lineRenderer.material = material;
    }

    public void updatePositions()
    {
        if(_targetTransform == null)
            return;

        for(int i = 0; i < _positionArrayCount; ++i)
        {
            if(i > 0)
                GizmoHelper.instance.drawLine(_positionList[i - 1] + _targetTransform.position,_positionList[i] + _targetTransform.position, Color.red);

            _lineRenderer.SetPosition(i, _positionList[i] + _targetTransform.position);
            _placeholder.SetPosition(i, _positionList[i] + _targetTransform.position);
        }
    }

    public void setDescription(ref TrailEffectDescription desc)
    {
        setDescription(ref desc, _lineRenderer);
        setDescription(ref desc, _placeholder);
    }

    private void setDescription(ref TrailEffectDescription desc, LineRenderer lineRenderer)
    {
        lineRenderer.startWidth = desc._width;
        lineRenderer.endWidth = desc._width;
        lineRenderer.textureMode = desc._textureMode;
        lineRenderer.sortingLayerName = desc._sortingLayerName;
        lineRenderer.sortingOrder = desc._sortingOrder;
        lineRenderer.gameObject.layer = LayerMask.NameToLayer(desc._layerName);
    }
}
