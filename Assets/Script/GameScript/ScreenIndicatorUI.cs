using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenIndicatorUI : MonoBehaviour
{
    public static ScreenIndicatorUI _instance;

    public float _magnitude = 1f;
    public Sprite _indicatorSprite;

    public string _targetSortingLayer;
    public int _targetSortingOrder;

    public SceneCharacterManager _characterManager;

    private Dictionary<CharacterEntityBase, SpriteRenderer> _enabledCharacters = new Dictionary<CharacterEntityBase, SpriteRenderer>();
    private Queue<SpriteRenderer> _indicatorPool = new Queue<SpriteRenderer>();
    private Vector2 _cameraSizeHalf;

    private Vector3[,] _screenSectors = new Vector3[4,2];

    private bool _active = true;

    public void Awake()
    {
        _instance = this;
    }

    public void Start()
    {        
        updateCameraSize();

        _active = true;

        _characterManager.addCharacterEnableDelegate(addCharacter);
        _characterManager.addCharacterDisableDelegate(disableCharacter);
    }

    public void Update()
    {
        updateCameraSize();

        Vector3 center = Camera.main.transform.position;
        caclculateScreenSectors(center);

        foreach(var item in _enabledCharacters)
        {
            if(item.Key.isActiveSelf() == false)
            {
                item.Value.gameObject.SetActive(false);
                continue;
            }
            
            bool isInCamera = isInCameraSector(center, item.Key.transform.position);
            item.Value.gameObject.SetActive(_active && isInCamera == false);

            if(isInCamera)
                continue;

            Vector3 sectorPosition = getSectorPosition(center,item.Key.transform.position);
            Vector3 direction = item.Key.transform.position - center;

            item.Value.transform.position = sectorPosition;
            item.Value.transform.rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(direction));
        }
    }

    public void setActive(bool value)
    {
        _active = value;
    }

    public void updateCameraSize()
    {
        float orthoSize = Camera.main.orthographicSize;
        float aspectRatio = Camera.main.aspect;
        float height = orthoSize * 2f;
        float width = height * aspectRatio;

        _cameraSizeHalf.x = width * 0.5f;
        _cameraSizeHalf.y = height * 0.5f;
    }

    public SpriteRenderer getIndicator()
    {
        if(_indicatorPool.Count <= 0)
            createIndicator();

        SpriteRenderer spriteRenderer = _indicatorPool.Dequeue();
        return spriteRenderer;
    }

    public void returnToPool(SpriteRenderer spriteRenderer)
    {
        _indicatorPool.Enqueue(spriteRenderer);
    }

    public void createIndicator()
    {
        GameObject screenIndicator = new GameObject("ScreenIndicator");
        SpriteRenderer spriteRenderer = screenIndicator.AddComponent<SpriteRenderer>();

        screenIndicator.layer = gameObject.layer;
        spriteRenderer.sprite = _indicatorSprite;
        spriteRenderer.sortingLayerName = _targetSortingLayer;
        spriteRenderer.sortingOrder = _targetSortingOrder;

        screenIndicator.SetActive(false);

        _indicatorPool.Enqueue(spriteRenderer);
    }

    public void addCharacter(CharacterEntityBase character)
    {
        SpriteRenderer spriteRenderer = getIndicator();

        Vector3 center = Camera.main.transform.position;
        caclculateScreenSectors(center);
        if(isInCameraSector(center, spriteRenderer.transform.position) == false)
        {
            spriteRenderer.gameObject.SetActive(true);
            Vector3 sectorPosition = getSectorPosition(center,character.transform.position);
            spriteRenderer.transform.position = sectorPosition;
        }

        _enabledCharacters.Add(character,spriteRenderer);
    }

    public void disableCharacter(CharacterEntityBase character)
    {
        _enabledCharacters[character].gameObject.SetActive(false);
        returnToPool(_enabledCharacters[character]);
        _enabledCharacters.Remove(character);
    }

    public bool isInCameraSector(Vector3 center, Vector3 point)
    {
        return (-_cameraSizeHalf.x + center.x <= point.x && _cameraSizeHalf.x + center.x >= point.x) && (-_cameraSizeHalf.y + center.y <= point.y && _cameraSizeHalf.y + center.y >= point.y);
    }

    public Vector3 getSectorPosition(Vector3 center, Vector3 targetPosition)
    {
        for(int index = 0; index < 4; ++index)
        {
            Vector2 outintersectionPosition = Vector2.zero;
            if(MathEx.findLineIntersection(_screenSectors[index,0],_screenSectors[index,1],center,targetPosition,out outintersectionPosition) == false)
                continue;
            
            return new Vector3(outintersectionPosition.x,outintersectionPosition.y,0f);
        }

        return center;
    }

    public void caclculateScreenSectors(Vector3 center)
    {
        Vector2 cameraSize = _cameraSizeHalf * _magnitude;

        //left
        _screenSectors[0,0] = new Vector3(-cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[0,1] = new Vector3(-cameraSize.x, -cameraSize.y, 0f) + center;

        //right
        _screenSectors[1,0] = new Vector3(cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[1,1] = new Vector3(cameraSize.x, -cameraSize.y, 0f) + center;

        //up
        _screenSectors[2,0] = new Vector3(-cameraSize.x, cameraSize.y, 0f) + center;
        _screenSectors[2,1] = new Vector3(cameraSize.x, cameraSize.y, 0f) + center;

        //down
        _screenSectors[3,0] = new Vector3(-cameraSize.x, -cameraSize.y, 0f) + center;
        _screenSectors[3,1] = new Vector3(cameraSize.x, -cameraSize.y, 0f) + center;
    }
}
