using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHairUI : MonoBehaviour
{
    public static CrossHairUI _instance;

    public Sprite _guidLineSprite;
    public GameObject _gagueObject;

    public Transform _targetPosition;
    public GameEntityBase _targetEntity;

    public int _guidLineCount = 1;

    public float _lineObjectRatio = 0.7f;
    public float _maxDistance = .25f;

    public string _gagueTargetStatusName = "DashPoint";
    public int _gagueStackCount = 4;
    public float _gagueStackGague;
    public float _gagueStartOffset = 0.05f;
    public float _gagueStackGap = 0.5f;

    private List<GameObject> _guidLineObjects = new List<GameObject>();
    private List<SpriteRenderer> _gagueObjects = new List<SpriteRenderer>();

    public void Awake()
    {
        _instance = this;
        createGuidLineObjects(_guidLineCount);
        createGagueObjects();
    }

    public void createGuidLineObjects(int count)
    {
        for(int index = 0; index < count; ++index)
        {
            GameObject guidLineObject = new GameObject("CrossHairGuid");
            guidLineObject.AddComponent<SpriteRenderer>().sprite = _guidLineSprite;

            _guidLineObjects.Add(guidLineObject);
        }
    }

    public void createGagueObjects()
    {
        for(int index = 0; index < _gagueStackCount; ++index)
        {
            GameObject guidLineObject = Instantiate(_gagueObject);
            guidLineObject.transform.SetParent(this.transform);
            guidLineObject.transform.localPosition = Vector3.right * (_gagueStackGap * (float)(index + 1)) + Vector3.right * _gagueStartOffset;
            SpriteRenderer spriteRenderer = guidLineObject.GetComponent<SpriteRenderer>();

            _gagueObjects.Add(spriteRenderer);
        }
    }

    // public void updateGague()
    // {
    //     float percentage = _targetEntity.getStatusPercentage(_gagueTargetStatusName) * (float)_gagueStackCount;
    //     for(int index = 0; index < _gagueStackCount; ++index)
    //     {
    //         float gague = MathEx.clamp01f(percentage - (float)index);
    //         _gagueObjects[index].material.SetFloat("_Gague",gague);
    //     }
    // }

    public void Update()
    {
        if (_targetPosition == null || _targetEntity == null || _targetEntity.isDead())
        {
            _targetPosition = null;
            _targetEntity = null;
            return;
        }
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMousePosition.z = 0f;

        Vector3 toMouseVector =  worldMousePosition - _targetPosition.position;
        Quaternion rotation = Quaternion.Euler(0f, 0f, MathEx.directionToAngle(toMouseVector.normalized));


        for (int index = 0; index < _guidLineObjects.Count; ++index)
        {
            float ratio = (_lineObjectRatio) * (float)(index + 1);
            Vector3 newPosition = Vector3.Lerp(worldMousePosition, _targetPosition.position, ratio);

            // 새로 추가된 코드: 거리 제한 로직
            _guidLineObjects[index].transform.position = worldMousePosition - Vector3.ClampMagnitude(worldMousePosition - newPosition, _maxDistance * (float)(index + 1));
            _guidLineObjects[index].transform.rotation = rotation;
        }

        transform.position = worldMousePosition;
        transform.rotation = rotation;
        
        float percentage = _targetEntity.getStatusPercentage(_gagueTargetStatusName) * (float)_gagueStackCount;
        for(int index = 0; index < _gagueStackCount; ++index)
        {
            float gague = MathEx.clamp01f(percentage - (float)index);
            _gagueObjects[index].material.SetFloat("_Gague",gague);
            _gagueObjects[index].transform.position = _targetPosition.position + toMouseVector*_gagueStartOffset + Vector3.ClampMagnitude(toMouseVector*_gagueStartOffset, _maxDistance * (float)(index + 1) * _gagueStackGap);
        }
    }

    public void setActive(bool value)
    {
        gameObject.SetActive(value);

        for(int index = 0; index < _guidLineObjects.Count; ++index)
        {
            _guidLineObjects[index].SetActive(value);
        }

        Update();
    }

    public void setTarget(GameEntityBase target)
    {
        _targetPosition = target.transform;
        _targetEntity = target;

        Update();
    }
}