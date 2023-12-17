using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public enum EffectUpdateType
{
    ScaledDeltaTime,
    NoneScaledDeltaTime,
}

public enum EffectType
{
    SpriteEffect,
    TimelineEffect,
    ParticleEffect,
    TrailEffect,
    AnimationEffect,
}

public class EffectRequestData : MessageData
{
    public string _effectPath;
    public AnimationCustomPreset _animationCustomPreset;

    public float _startFrame;
    public float _endFrame;
    public float _framePerSecond;

    public float _angle;

    public bool _followDirection;
    public bool _usePhysics;
    public bool _useFlip;
    public bool _castShadow;
    public bool _dependentAction;

    public string _attackDataName;

    public EffectType _effectType;
    public EffectUpdateType _updateType = EffectUpdateType.ScaledDeltaTime;

    public Vector3 _position;
    public Vector3 _scale;
    public Quaternion _rotation;

    public ObjectBase _executeEntity = null;
    public Transform _parentTransform = null;
    public Animator _timelineAnimator = null;

    public PhysicsBodyDescription _physicsBodyDesc = new PhysicsBodyDescription(null);

    public float _lifeTime = 0f;
    public float _trailWidth = 0f;
    public Material _trailMaterial = null;
    public Vector3[] _trailPositionData = null;

    public void createPresetAnimationRequestData(string path)
    {
        _effectPath = path;
        _animationCustomPreset = ResourceContainerEx.Instance().GetAnimationCustomPreset(path);
        if(_animationCustomPreset == null)
            return;
        
        _effectType = EffectType.SpriteEffect;
    }

    public void clearRequestData()
    {
        _effectPath = "";
        _startFrame = 0f;
        _endFrame = -1f;
        _framePerSecond = 0f;
        _angle = 0f;
        _followDirection = false;
        _usePhysics = false;
        _useFlip = false;
        _castShadow = false;
        _dependentAction = false;
        _effectType = EffectType.SpriteEffect;
        _updateType = EffectUpdateType.ScaledDeltaTime;
        _position = Vector3.zero;
        _scale = Vector3.one;
        _rotation = Quaternion.identity;
        _parentTransform = null;
        _timelineAnimator = null;
        _physicsBodyDesc.clearPhysicsBody();
        _lifeTime = 0f;
        _trailWidth = 0f;
        _trailMaterial = null;
        _trailPositionData = null;
        _executeEntity = null;
        _animationCustomPreset = null;
    }
}  

public abstract class EffectItemBase
{
    public  ObjectBase             _spawnOwner = null;
    public EffectUpdateType        _effectUpdateType = EffectUpdateType.NoneScaledDeltaTime;
    public EffectType              _effectType = EffectType.SpriteEffect;
    public string                  _effectPath = "";

    protected bool                 _stopEffect = false;

    public abstract void    initialize(EffectRequestData effectData);
    public abstract bool    progress(float deltaTime);
    public abstract void    release();

    public abstract bool    isValid();

    public virtual bool     isActivated() {return true;}

    public virtual void     stopEffect() {_stopEffect = true;}

}

public class EffectItem : EffectItemBase
{
    private AnimationPlayer         _animationPlayer = new AnimationPlayer();
    private AnimationPlayDataInfo   _animationPlayData = new AnimationPlayDataInfo();

    private PhysicsBodyEx           _physicsBody = new PhysicsBodyEx();

    private SpriteRenderer          _spriteRenderer;
    private Transform               _parentTransform = null;

    private Vector3                 _localPosition;
    private Quaternion              _rotation;
    private bool                    _usePhysics = false;
    private bool                    _useFlip = false;

    public void createItem()
    {
        GameObject gameObject = new GameObject("Effect");
        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        gameObject.SetActive(false);

        _effectType = EffectType.SpriteEffect;
    }

    public override void initialize(EffectRequestData effectData)
    {
        _effectPath = effectData._effectPath;
        _effectUpdateType = effectData._updateType;

        _animationPlayData._path = effectData._effectPath;
        _animationPlayData._startFrame = effectData._startFrame;
        _animationPlayData._endFrame = effectData._endFrame;
        _animationPlayData._framePerSec = effectData._framePerSecond;

        _animationPlayData._customPresetData = null;
        _animationPlayData._frameEventData = null;
        _animationPlayData._timeEventData = null;
        _animationPlayData._actionTime = -1f;
        _animationPlayData._frameEventDataCount = 0;
        _animationPlayData._timeEventDataCount = 0;
        _animationPlayData._hasMovementGraph = false;
        _animationPlayData._isLoop = false;
        _animationPlayData._animationLoopCount = 0;
        _animationPlayData._flipState = new FlipState{xFlip = false, yFlip = false};

        _stopEffect = false;
        _useFlip = effectData._useFlip;
        _rotation = effectData._rotation;

        if(effectData._animationCustomPreset != null)
        {
            _animationPlayData._customPresetData = effectData._animationCustomPreset._animationCustomPresetData;
            if(effectData._animationCustomPreset._rotationPresetName != "")
            {
                AnimationRotationPreset rotationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationRotationPreset") as AnimationRotationPreset;
                _animationPlayData._rotationPresetData = rotationPreset.getPresetData(effectData._animationCustomPreset._rotationPresetName);
            }
            
            if(effectData._animationCustomPreset._scalePresetName != "")
            {
                AnimationScalePreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationScalePreset") as AnimationScalePreset;
                _animationPlayData._scalePresetData = scalePreset.getPresetData(effectData._animationCustomPreset._scalePresetName);
            }

            if(effectData._animationCustomPreset._translationPresetName != "")
            {
                AnimationTranslationPreset translationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationTranslationPreset") as AnimationTranslationPreset;
                _animationPlayData._translationPresetData = translationPreset.getPresetData(effectData._animationCustomPreset._translationPresetName);
            }
        }

        _animationPlayer.initialize();
        if(effectData._animationCustomPreset != null)
            _animationPlayer.changeAnimationByCustomPreset(_animationPlayData._path,effectData._animationCustomPreset);
        else
            _animationPlayer.changeAnimation(_animationPlayData);

        _parentTransform = effectData._parentTransform;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            _parentTransform = null;

        _spriteRenderer.gameObject.layer = effectData._castShadow ? LayerMask.NameToLayer("Character") : LayerMask.NameToLayer("EffectEtc");

        _spriteRenderer.transform.position = effectData._position;
        _spriteRenderer.transform.localRotation = Quaternion.Euler(0f,0f,effectData._angle);
        _spriteRenderer.transform.localScale = effectData._scale;
        _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();
        _spriteRenderer.gameObject.SetActive(true);
        _spriteRenderer.flipX = _useFlip;

        _localPosition = _spriteRenderer.transform.position;
        if(_parentTransform != null)
            _localPosition = _spriteRenderer.transform.position - _parentTransform.position;

        _physicsBody.initialize(effectData._physicsBodyDesc);
        _usePhysics = effectData._usePhysics;

    }

    public override bool progress(float deltaTime)
    {
        if(_stopEffect)
            return true;

        bool isEnd = _animationPlayer.progress(deltaTime,null);

        _spriteRenderer.sprite = _animationPlayer.getCurrentSprite();
        _spriteRenderer.transform.localRotation *= _animationPlayer.getAnimationRotationPerFrame();

        Vector3 outScale = Vector3.one;
        if(_animationPlayer.getCurrentAnimationScale(out outScale))
            _spriteRenderer.transform.localScale = outScale;

        switch(_effectUpdateType)
        {
            case EffectUpdateType.ScaledDeltaTime:
            break;
            case EffectUpdateType.NoneScaledDeltaTime:
            deltaTime = Time.deltaTime;
            break;
        }

        if(_usePhysics)
        {
            _physicsBody.progress(deltaTime);

            Vector3 velocity = _physicsBody.getCurrentVelocity();
            float torque = _physicsBody.getCurrentTorqueValue();

            if(_useFlip)
            {
                torque *= -1f;
                velocity.x *= -1f;
            }

            _localPosition += (velocity * deltaTime);
            _spriteRenderer.transform.localRotation *= Quaternion.Euler(0f,0f,torque * deltaTime);
        }

        Vector3 translationPreset = Vector3.zero;
        _animationPlayer.getCurrentAnimationTranslation(out translationPreset);

        Vector3 worldPosition = _localPosition;
        if(_parentTransform != null)
            worldPosition = _parentTransform.position + _localPosition;

        _spriteRenderer.transform.position = worldPosition + translationPreset;

        return isEnd;
    }

    public override void release()
    {
        _spriteRenderer.gameObject.transform.SetParent(null);
        _spriteRenderer.gameObject.SetActive(false);
    }

    public override bool isValid()
    {
        return _spriteRenderer != null;
    }

    public override bool isActivated() {return _spriteRenderer.gameObject.activeInHierarchy;}
}

public class AnimationEffectItem : EffectItemBase
{
    private GameObject              _effectObject;
    private Animation               _animationComponent;

    public ObjectBase               _executeObject;
    public bool                     _followDirection;
    public Transform                _parentTransform = null;

    private float                   _playSpeed = 1f;


    public void createItem(string prefabPath)
    {
        GameObject effectPrefab = ResourceContainerEx.Instance().GetPrefab(prefabPath);
        if(effectPrefab == null)
        {
            DebugUtil.assert(false, "잘못된 타임라인 이펙트 경로 입니다. 오타가 있지는 않나요? : {0}", prefabPath);
            return;
        }

        _effectObject = GameObject.Instantiate(effectPrefab);
        _animationComponent = _effectObject.GetComponent<Animation>();

        if(_animationComponent == null)
        {
            DebugUtil.assert(false, "Animation이 이펙트 프리팹에 존재하지 않습니다. 애니메이션 이펙트가 맞나요? : {0}", prefabPath);
            return;
        }

        _effectType = EffectType.AnimationEffect;
        release();
    }

    public override void initialize(EffectRequestData effectData)
    {
        _executeObject = effectData._executeEntity;
        _followDirection = effectData._followDirection;

        _effectPath = effectData._effectPath;
        _effectUpdateType = effectData._updateType;

        _effectObject.transform.position = effectData._position;
        _effectObject.transform.rotation = _executeObject ? Quaternion.Euler(0f,0f,MathEx.directionToAngle(_executeObject.getDirection())) : effectData._rotation;

        _parentTransform = effectData._parentTransform;

        _stopEffect = false;

        if(effectData._lifeTime != 0f)
            _playSpeed = 1.0f / effectData._lifeTime;
        else
            _playSpeed = 1f;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            _parentTransform = null;

        if(_parentTransform != null)
            _effectObject.transform.SetParent(_parentTransform);

        // if(_timelineEffectControl != null && _timelineEffectControl._isLaserEffect && _executeObject is GameEntityBase)
        //     (_executeObject as GameEntityBase).addLaserEffect(this);

        _effectObject.SetActive(true);
        _animationComponent.Stop();
        _animationComponent.Play(PlayMode.StopAll);
    }

    public override bool progress(float deltaTime)
    {
        if(isValid() == false )
            return false;

        if(_stopEffect)
            return true;

        switch(_effectUpdateType)
        {
            case EffectUpdateType.ScaledDeltaTime:
            break;
            case EffectUpdateType.NoneScaledDeltaTime:
            deltaTime = Time.deltaTime;
            break;
        }

        if(_followDirection)
            _effectObject.transform.rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(_executeObject.getDirection()));

        return _animationComponent.isPlaying == false;
    }

    public override void release()
    {
        _animationComponent.Stop();
        disableEffect();
    }

    public void disableEffect()
    {
        _effectObject.transform.SetParent(null);
        _effectObject.SetActive(false);
    }

    public override bool isValid()
    {
        return _effectObject != null && _animationComponent != null;
    }

    public override bool isActivated() {return _effectObject.activeInHierarchy;}
}

public class ParticleEffectItem : EffectItemBase
{
    private GameObject              _effectObject;
    private ParticleSystem[]        _allParticleSystems;

    public ObjectBase               _executeObject;
    public bool                     _followDirection;
    private bool                    _dependentAction = false;
    public Transform                _parentTransform = null;

    public float                    _lifeTime = 0f;


    public void createItem(string prefabPath)
    {
        GameObject effectPrefab = ResourceContainerEx.Instance().GetPrefab(prefabPath);
        if(effectPrefab == null)
        {
            DebugUtil.assert(false, "잘못된 타임라인 이펙트 경로 입니다. 오타가 있지는 않나요? : {0}", prefabPath);
            return;
        }

        _effectObject = GameObject.Instantiate(effectPrefab);
        ParticleSystem[] myParticles = _effectObject.GetComponents<ParticleSystem>();
        ParticleSystem[] chidrensParticles = _effectObject.GetComponentsInChildren<ParticleSystem>();

        _allParticleSystems = new ParticleSystem[myParticles.Length + chidrensParticles.Length];
        myParticles.CopyTo(_allParticleSystems,0);
        chidrensParticles.CopyTo(_allParticleSystems, myParticles.Length);

        if(_allParticleSystems == null || _allParticleSystems.Length == 0)
        {
            DebugUtil.assert(false, "ParticleSystem이 이펙트 프리팹에 존재하지 않습니다. 파티클 이펙트가 맞나요? : {0}", prefabPath);
            return;
        }

        _effectType = EffectType.ParticleEffect;
        release();
    }

    public override void initialize(EffectRequestData effectData)
    {
        _executeObject = effectData._executeEntity;
        _followDirection = effectData._followDirection;

        _effectPath = effectData._effectPath;
        _effectUpdateType = effectData._updateType;

        _effectObject.transform.position = effectData._position;
        _effectObject.transform.rotation = effectData._rotation;

        _parentTransform = effectData._parentTransform;
        _dependentAction = effectData._dependentAction;

        _lifeTime = effectData._lifeTime;

        _stopEffect = false;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            _parentTransform = null;

        if(_parentTransform != null)
            _effectObject.transform.SetParent(_parentTransform);

        _effectObject.SetActive(true);
        foreach(var item in _allParticleSystems)
        {
            item.Stop();
            item.Play();
        }
    }

    public bool isAllParticleEnd()
    {
        for(int index = 0; index < _allParticleSystems.Length; ++index)
        {
            if(_allParticleSystems[index].IsAlive())
                return false;
        }

        return true;
    }

    public override bool progress(float deltaTime)
    {
        if(isValid() == false)
            return false;

        switch(_effectUpdateType)
        {
            case EffectUpdateType.ScaledDeltaTime:
            break;
            case EffectUpdateType.NoneScaledDeltaTime:
            deltaTime = Time.deltaTime;
            break;
        }

        if(_lifeTime > 0f)
        {
            _lifeTime -= deltaTime;
            if(_lifeTime <= 0f)
                stopAllParticleSystem();
        }

        if(_dependentAction && _executeObject is GameEntityBase && (_executeObject as GameEntityBase).isActionChangeFrame())
        {
            _dependentAction = false;
            stopAllParticleSystem();
        }
         

        if(_followDirection)
            _effectObject.transform.rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(_executeObject.getDirection()));

        return isAllParticleEnd();
    }

    public void stopAllParticleSystem()
    {
        foreach(var item in _allParticleSystems)
        {
            item.Stop();
        }
    }

    public override void release()
    {
        stopAllParticleSystem();
        disableEffect();
    }

    public void disableEffect()
    {
        _effectObject?.transform.SetParent(null);
        _effectObject?.SetActive(false);
    }

    public override bool isValid()
    {
        return _effectObject != null && _allParticleSystems != null && _allParticleSystems.Length > 0;
    }

    public override bool isActivated() {return _effectObject.activeInHierarchy;}

    public override void stopEffect()
    {
        base.stopEffect();

        foreach(var item in _allParticleSystems)
        {
            item.Stop();
        }
    }
}

public class TrailEffectItem : EffectItemBase
{
    private GameObject              _effectObject;
    private TrailEffectControl      _trailEffectControl;

    public Transform                _parentTransform = null;

    private float _lifeTime = 0f;

    public void createItem(string prefabPath)
    {
        GameObject effectPrefab = ResourceContainerEx.Instance().GetPrefab(prefabPath);
        if(effectPrefab == null)
        {
            DebugUtil.assert(false, "잘못된 타임라인 이펙트 경로 입니다. 오타가 있지는 않나요? : {0}", prefabPath);
            return;
        }

        _effectObject = GameObject.Instantiate(effectPrefab);
        _trailEffectControl = _effectObject.GetComponent<TrailEffectControl>();

        if(_trailEffectControl == null)
        {
            DebugUtil.assert(false, "trail Effect Control이 프리펩에 존재하지 않습니다. 트레일 이펙트가 맞나요? : {0}", prefabPath);
            return;
        }

        _effectType = EffectType.TrailEffect;
        release();
    }

    public override void initialize(EffectRequestData effectData)
    {
        _effectPath = "Trail";
        _effectUpdateType = effectData._updateType;

        _effectObject.transform.position = effectData._position;
        _effectObject.transform.rotation = effectData._rotation;

        _parentTransform = effectData._parentTransform;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            _parentTransform = null;

        if(_parentTransform != null)
            _effectObject.transform.SetParent(_parentTransform);

        TrailEffectDescription trailEffectDesc;
        trailEffectDesc._sortingLayerName = "Effect";
        trailEffectDesc._sortingOrder = 0;
        trailEffectDesc._textureMode = LineTextureMode.Stretch;
        trailEffectDesc._time = effectData._lifeTime;
        trailEffectDesc._width = effectData._trailWidth;
        trailEffectDesc._layerName = "EffectEtc";

        _trailEffectControl.setMaterial(effectData._trailMaterial);
        _trailEffectControl.setDescription(ref trailEffectDesc);
        _trailEffectControl.setPositions(effectData._trailPositionData, effectData._parentTransform);
        _effectObject.SetActive(true);
        
        _lifeTime = effectData._lifeTime;
    }

    public override bool progress(float deltaTime)
    {
        if(isValid() == false)
            return false;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            return true;

        if(_parentTransform != null)
            _trailEffectControl.updatePositions();

        switch(_effectUpdateType)
        {
            case EffectUpdateType.ScaledDeltaTime:
            break;
            case EffectUpdateType.NoneScaledDeltaTime:
            deltaTime = Time.deltaTime;
            break;
        }

        _lifeTime -= deltaTime;
        return _lifeTime <= 0f;
    }

    public override void release()
    {
        _effectObject.transform.SetParent(null);
        _effectObject.SetActive(false);
    }

    public override bool isValid()
    {
        return _effectObject != null && _trailEffectControl != null;
    }
}

public class TimelineEffectItem : EffectItemBase
{
    private GameObject              _effectObject;
    private PlayableDirector        _playableDirector;
    private TimelineEffectControl   _timelineEffectControl;

    public ObjectBase               _executeObject;
    public bool                     _followDirection;

    public Transform                _parentTransform = null;

    private float _playSpeed = 1f;


    public void createItem(string prefabPath)
    {
        GameObject effectPrefab = ResourceContainerEx.Instance().GetPrefab(prefabPath);
        if(effectPrefab == null)
        {
            DebugUtil.assert(false, "잘못된 타임라인 이펙트 경로 입니다. 오타가 있지는 않나요? : {0}", prefabPath);
            return;
        }

        _effectObject = GameObject.Instantiate(effectPrefab);
        _playableDirector = _effectObject.GetComponent<PlayableDirector>();
        _timelineEffectControl = _effectObject.GetComponent<TimelineEffectControl>();

        if(_playableDirector == null)
        {
            DebugUtil.assert(false, "playable director가 이펙트 프리팹에 존재하지 않습니다. 타임라인 이펙트가 맞나요? : {0}", prefabPath);
            return;
        }

        _effectType = EffectType.TimelineEffect;
        release();
    }

    public override void initialize(EffectRequestData effectData)
    {
        _executeObject = effectData._executeEntity;
        _followDirection = effectData._followDirection;

        _effectPath = effectData._effectPath;
        _effectUpdateType = effectData._updateType;

        _effectObject.transform.position = effectData._position;
        _effectObject.transform.rotation = _executeObject ? Quaternion.Euler(0f,0f,MathEx.directionToAngle(_executeObject.getDirection())) : effectData._rotation;

        _parentTransform = effectData._parentTransform;

        _stopEffect = false;

        if(effectData._lifeTime != 0f)
            _playSpeed = 1.0f / effectData._lifeTime;
        else
            _playSpeed = 1f;

        if(_parentTransform != null && _parentTransform.gameObject.activeInHierarchy == false)
            _parentTransform = null;

        if(_parentTransform != null)
            _effectObject.transform.SetParent(_parentTransform);

        if(_timelineEffectControl != null && _timelineEffectControl._isCharacterMaterialEffect)
            _timelineEffectControl.setCharacterAnimator(effectData._timelineAnimator);
        
        if(_timelineEffectControl != null && _timelineEffectControl._isLaserEffect && _executeObject is GameEntityBase)
        {
            _timelineEffectControl.setAttackData(effectData._attackDataName);
            (_executeObject as GameEntityBase).addLaserEffect(this);
        }

        _effectObject.SetActive(true);
        _playableDirector.Stop();
        _playableDirector.Play();
    }

    public override bool progress(float deltaTime)
    {
        if(isValid() == false || _playableDirector.playableGraph.IsValid() == false)
            return false;

        if(_stopEffect)
            return true;

        switch(_effectUpdateType)
        {
            case EffectUpdateType.ScaledDeltaTime:
            break;
            case EffectUpdateType.NoneScaledDeltaTime:
            deltaTime = Time.deltaTime;
            break;
        }

        if(_followDirection)
            _effectObject.transform.rotation = Quaternion.Euler(0f,0f,MathEx.directionToAngle(_executeObject.getDirection()));

        _playableDirector.playableGraph.Evaluate(_playSpeed * deltaTime);

        return _playableDirector.state != PlayState.Playing;
    }

    public override void release()
    {
        _playableDirector.Stop();
        disableEffect();
    }

    public void disableEffect()
    {
        _effectObject.transform.SetParent(null);
        _effectObject.SetActive(false);

        _timelineEffectControl?.deleteCharacterAnimatorBinding();

        if(_timelineEffectControl != null && _timelineEffectControl._isCharacterMaterialEffect)
            _timelineEffectControl.setCharacterAnimator(null);
        
        if(_timelineEffectControl != null && _timelineEffectControl._isOutlineEffect)
        {
            Material currentMaterial = _executeObject?.getCurrentMaterial();
            if(currentMaterial != null)
            {
                currentMaterial.SetFloat("_OutlineAlpha",0f);
            }
        }
    }

    public override bool isValid()
    {
        return _effectObject != null && _playableDirector != null;
    }

    public override bool isActivated() {return _effectObject.activeInHierarchy;}
}


public class EffectManager : ManagerBase
{
    public static EffectManager _instance;

    private List<EffectItemBase> _processingItems = new List<EffectItemBase>();

    private SimplePool<EffectItem> _effectItemPool = new SimplePool<EffectItem>();
    private Dictionary<string, SimplePool<TimelineEffectItem>> _timelineEffectPool = new Dictionary<string, SimplePool<TimelineEffectItem>>();
    private Dictionary<string, SimplePool<AnimationEffectItem>> _animationEffectPool = new Dictionary<string, SimplePool<AnimationEffectItem>>();
    private Dictionary<string, SimplePool<TrailEffectItem>> _trailEffectPool = new Dictionary<string, SimplePool<TrailEffectItem>>();
    private Dictionary<string, SimplePool<ParticleEffectItem>> _particleEffectPool = new Dictionary<string, SimplePool<ParticleEffectItem>>();

    public override void assign()
    {
        _instance = this;
        
        base.assign();
        CacheUniqueID("EffectManager");
        RegisterRequest();

        AddAction(MessageTitles.effect_spawnEffect,receiveEffectRequest);
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

        for(int i = 0; i < _processingItems.Count;)
        {
            float targetDeltaTime = _processingItems[i]._effectUpdateType == EffectUpdateType.ScaledDeltaTime ? deltaTime : Time.deltaTime;
            if(_processingItems[i].isActivated() == false || _processingItems[i].progress(targetDeltaTime) == true)
            {
                _processingItems[i].release();
                returnEffectItemToQueue(_processingItems[i]);
                _processingItems.RemoveAt(i);

                continue;
            }
            
            ++i;
        }
    }

    private void returnEffectItemToQueue(EffectItemBase item)
    {
        switch(item._effectType)
        {
            case EffectType.SpriteEffect:
                _effectItemPool.enqueue(item as EffectItem);
            break;
            case EffectType.TimelineEffect:
                _timelineEffectPool[item._effectPath].enqueue(item as TimelineEffectItem);
            break;
            case EffectType.TrailEffect:
                _trailEffectPool[item._effectPath].enqueue(item as TrailEffectItem);
            break;
            case EffectType.ParticleEffect:
                _particleEffectPool[item._effectPath].enqueue(item as ParticleEffectItem);
            break;
            case EffectType.AnimationEffect:
                _animationEffectPool[item._effectPath].enqueue(item as AnimationEffectItem);
            break;
        }
        
    }

    public EffectItemBase createEffect(EffectRequestData requestData)
    {
        EffectItemBase itemBase = null;

        if(requestData._effectType == EffectType.SpriteEffect)
        {
            EffectItem item = _effectItemPool.dequeue();
            if(item.isValid() == false)
                item.createItem();

            item.initialize(requestData);
            itemBase = item;
        }
        else if(requestData._effectType == EffectType.TimelineEffect)
        {
            if(_timelineEffectPool.ContainsKey(requestData._effectPath) == false)
                _timelineEffectPool.Add(requestData._effectPath, new SimplePool<TimelineEffectItem>());

            TimelineEffectItem item = _timelineEffectPool[requestData._effectPath].dequeue();
            if(item.isValid() == false)
                item.createItem(requestData._effectPath);

            item.initialize(requestData);
            itemBase = item;
        }
        else if(requestData._effectType == EffectType.TrailEffect)
        {
            if(_trailEffectPool.ContainsKey("Trail") == false)
                _trailEffectPool.Add("Trail", new SimplePool<TrailEffectItem>());

            TrailEffectItem item = _trailEffectPool["Trail"].dequeue();
            if(item.isValid() == false)
                item.createItem("Prefab/Effect/TrailEffectBase");

            item.initialize(requestData);
            itemBase = item;
        }
        else if(requestData._effectType == EffectType.ParticleEffect)
        {
            if(_particleEffectPool.ContainsKey(requestData._effectPath) == false)
                _particleEffectPool.Add(requestData._effectPath, new SimplePool<ParticleEffectItem>());

            ParticleEffectItem item = _particleEffectPool[requestData._effectPath].dequeue();
            if(item.isValid() == false)
                item.createItem(requestData._effectPath);

            item.initialize(requestData);
            itemBase = item;
        }
        else if(requestData._effectType == EffectType.AnimationEffect)
        {
            if(_animationEffectPool.ContainsKey(requestData._effectPath) == false)
                _animationEffectPool.Add(requestData._effectPath, new SimplePool<AnimationEffectItem>());

            AnimationEffectItem item = _animationEffectPool[requestData._effectPath].dequeue();
            if(item.isValid() == false)
                item.createItem(requestData._effectPath);

            item.initialize(requestData);
            itemBase = item;
        }
        
        if(itemBase == null)
        {
            DebugUtil.assert(false, "이펙트 데이터가 없습니다. 통보 요망");
            return null;
        }

        itemBase._spawnOwner = requestData._executeEntity;
        _processingItems.Add(itemBase);

        return itemBase;
    }

    public void receiveEffectRequest(Message msg)
    {
        createEffect(MessageDataPooling.CastData<EffectRequestData>(msg.data));
    }
}