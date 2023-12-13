using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Core.Tokens;

public class HPSphereUI
{
    private GameObject _hpSpriteObject;
    private SpriteRenderer _hpSpriteRenderer;

    private GameObject _bpGaugeSpriteObject;
    private SpriteRenderer _bpGaugeSpriteRenderer;

    private GameObject _bpAnimationSpriteObject;
    private SpriteRenderer _bpAnimationSpriteRenderer;

    private AnimationPlayer _bpAnimation = new AnimationPlayer();
    private AnimationCustomPreset _bpFullAnimation;
    private AnimationCustomPreset _bpFireAnimation;
    private AnimationCustomPreset _bpFadeAnimation;

    private float _prevBattlePoint = 0f;
    private bool _activeSelf = false;

    private Vector3 _shakeOffset = Vector3.zero;

    public void createSpriteRenderObject()
    {
        _hpSpriteObject = new GameObject("SpriteObject");
        _hpSpriteObject.transform.position = Vector3.zero;
        _hpSpriteObject.layer = LayerMask.NameToLayer("Interface");

        _hpSpriteRenderer = _hpSpriteObject.AddComponent<SpriteRenderer>();
        _hpSpriteRenderer.material = Material.Instantiate(ResourceContainerEx.Instance().GetMaterial("Material/Material_SpriteDefaultWithPixelSnap"));
        _hpSpriteRenderer.sortingOrder = 8;

        _bpGaugeSpriteObject = new GameObject("BPGauge");
        _bpGaugeSpriteObject.transform.position = Vector3.zero;
        _bpGaugeSpriteObject.transform.SetParent(_hpSpriteObject.transform);
        _bpGaugeSpriteObject.layer = LayerMask.NameToLayer("Interface");

        _bpGaugeSpriteRenderer = _bpGaugeSpriteObject.AddComponent<SpriteRenderer>();
        _bpGaugeSpriteRenderer.material = Material.Instantiate(ResourceContainerEx.Instance().GetMaterial("Material/Material_SpriteDefaultWithPixelSnap"));
        _bpGaugeSpriteRenderer.sortingOrder = 10;

        _bpAnimationSpriteObject = new GameObject("BPAnimation");
        _bpAnimationSpriteObject.transform.position = Vector3.zero;
        _bpAnimationSpriteObject.transform.SetParent(_hpSpriteObject.transform);
        _bpAnimationSpriteObject.layer = LayerMask.NameToLayer("Interface");

        _bpAnimationSpriteRenderer = _bpAnimationSpriteObject.AddComponent<SpriteRenderer>();
        _bpAnimationSpriteRenderer.material = Material.Instantiate(ResourceContainerEx.Instance().GetMaterial("Material/Material_SpriteDefaultWithPixelSnap"));
        _bpAnimationSpriteRenderer.sortingOrder = 9;

        _bpAnimation.initialize();
        _bpFullAnimation = ResourceContainerEx.Instance().GetScriptableObjects("Sprites/UI/HPSphere/HPBPFull")[0] as AnimationCustomPreset;
        _bpFireAnimation = ResourceContainerEx.Instance().GetScriptableObjects("Sprites/UI/HPSphere/BPFire")[0] as AnimationCustomPreset;
        _bpFadeAnimation = ResourceContainerEx.Instance().GetScriptableObjects("Sprites/UI/HPSphere/BPFade")[0] as AnimationCustomPreset;

        _hpSpriteRenderer.sprite = ResourceContainerEx.Instance().GetSprite("Sprites/UI/HPSphere/HP/hpsphere_full");
        _bpGaugeSpriteRenderer.sprite = ResourceContainerEx.Instance().GetSprite("Sprites/UI/HPSphere/BP/hpcross");
    }

    public void initialize(Vector3 position)
    {
        if(_hpSpriteObject == null)
            createSpriteRenderObject();
        
        _hpSpriteObject.transform.position = position;
        _hpSpriteObject.transform.localScale = Vector3.one;
        _hpSpriteObject.SetActive(true);

        _bpGaugeSpriteObject.transform.localScale = Vector3.zero;
        _bpGaugeSpriteObject.SetActive(true);

        _bpAnimationSpriteObject.SetActive(false);

        _activeSelf = true;

    }

    public void setActive(bool value)
    {
        _hpSpriteObject.SetActive(value);
        _bpGaugeSpriteObject.SetActive(value);
        _bpAnimationSpriteObject.SetActive(value);

        _activeSelf = value;
    }

    public void release()
    {
        setActive(false);
    }

    public void progress(Vector3 followPosition, float deltaTime)
    {
        _shakeOffset = MathEx.damp(_shakeOffset,followPosition,7f,deltaTime);
        _hpSpriteObject.transform.position += _shakeOffset;

        if(_bpAnimationSpriteObject.activeInHierarchy)
        {
            _bpAnimation.progress(deltaTime,null);
            if(_bpAnimation.isEnd() == false)
                _bpAnimationSpriteRenderer.sprite = _bpAnimation.getCurrentSprite();
        }
    }

    public void setPosition(Vector3 position)
    {
        _hpSpriteObject.transform.position = position;
    }

    public Vector3 getPosition() 
    {
        return _hpSpriteObject.transform.position;
    }

    public void updateGauge(float percentage)
    {
        if(percentage <= 0f)
        {
            _hpSpriteObject.SetActive(false);
        }
        else
        {
            if(percentage > 1f)
                percentage = 1f;

            _hpSpriteObject.SetActive(_activeSelf);
            _hpSpriteObject.transform.localScale = Vector3.one * percentage;
        }
    }

    public void updateBPGauge(float percentage)
    {
        if(percentage <= 0f)
        {
            _bpGaugeSpriteObject.SetActive(false);
            if(_bpAnimation.isEnd())
                _bpAnimationSpriteObject.SetActive(false);
        }
        else
        {
            if(percentage > 1f)
                percentage = 1f;

            if(_bpAnimation.isEnd())
                _bpGaugeSpriteObject.SetActive(_activeSelf);
            _bpAnimationSpriteObject.SetActive(_activeSelf);
            _bpGaugeSpriteObject.transform.localScale = Vector3.one * percentage;
        }

        if(percentage == 1f && _prevBattlePoint == 1f && _bpAnimation.isEnd())
        {   
            _bpGaugeSpriteObject.SetActive(_activeSelf);
            _bpAnimation.changeAnimationByCustomPreset("Sprites/UI/HPSphere/HPBPFull",_bpFullAnimation);
        }
        else if(_prevBattlePoint == 1f && percentage < 1f)
        {
            _bpGaugeSpriteObject.SetActive(_activeSelf);
            _bpAnimation.changeAnimationByCustomPreset("Sprites/UI/HPSphere/BPFade",_bpFadeAnimation);
        }

        if(_prevBattlePoint < percentage && percentage == 1f)
        {
            _bpGaugeSpriteObject.SetActive(false);
            _bpAnimation.changeAnimationByCustomPreset("Sprites/UI/HPSphere/BPFire",_bpFireAnimation);
        }

        _prevBattlePoint = percentage;
    }
}
