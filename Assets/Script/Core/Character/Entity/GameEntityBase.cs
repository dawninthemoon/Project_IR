using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


#if UNITY_EDITOR
using UnityEditor;
#endif


public delegate void DeadEventDelegate(GameEntityBase collisionData);

public class GameEntityBase : SequencerObjectBase
{

    public static float         _defaultFriction = 4f;


    public string               actionGraphPath = "Assets\\Data\\ActionGraph\\ActionGraphTest.xml";
    public string               aiGraphPath = "Assets\\Data\\AIGraph\\CommonEnemyAI.xml";
    public string               statusInfoName = "CommonPlayerStatus";


    public DebugTextManager     debugTextManager;
    public bool                 _actionDebug = false;
    public bool                 _statusDebug = false;
    public bool                 _aiDebug = false;
    public bool                 _animationDebug = false;
    public bool                 _soundDebug = false;
    public bool                 _keepAliveEntity = false;
    

    
    private ActionGraph         _actionGraph;
    private AIGraph             _aiGraph;
    private DanmakuGraph        _danmakuGraph;
    private StatusInfo          _statusInfo;
    private StatusGraphicInterface _graphicInterface;
    private SequencerGraphProcessManager _sequencerProcessManager;

    private CollisionInfo       _collisionInfo;
    private CharacterInfoData   _characterInfo;

    private DeadEventDelegate   _deadEventDelegate = null;


    private MovementControl     _movementControl = new MovementControl();

    private FlipState           _flipState = new FlipState();
    private Quaternion          _spriteRotation = Quaternion.identity;

    private DefenceType         _currentDefenceType = DefenceType.Empty;

    private PhysicsBodyEx       _physicsBody = new PhysicsBodyEx();
    private GroundController    _groundController = new GroundController();


    private Vector3             _currentVelocity = Vector3.zero;
    private Vector3             _recentlyAttackPoint = Vector3.zero;
    private Vector3             _defenceDirection = Vector3.zero;


    private int[]               _currentActionBuffList = null;

    private Color               _debugColor = Color.red;

    private GameEntityBase      _currentTarget;

    private float               _headUpOffset = 0f;
    private float               _characterLifeTime = 0f;
    private float               _directionInputRate = 1f;

    private float               _leftHP = 0f;

    private bool                _updateDirection = true;
    private bool                _updateFlipState = true;

    private bool                _initializeFromCharacter = false;
    private bool                _activeSelf = true;
    private bool                _blockAI = false;
    private bool                _blockInput = false;
    private bool                _isActionChangeFrame = false;

    private DirectionType       _currentDirectionType = DirectionType.AlwaysRight;
    private RotationType        _currentRotationType = RotationType.AlwaysRight;


    private Quaternion          _actionStartRotation = Quaternion.identity;
    private Quaternion          _angleBaseRotation = Quaternion.identity;

    private List<TimelineEffectItem> _enabledLaserEffectItems = new List<TimelineEffectItem>();

#if UNITY_EDITOR
    [HideInInspector] public bool _blockAIByEditor = false;
    [HideInInspector] public List<ActionGraphNodeData> _actionGraphChangeLog = new List<ActionGraphNodeData>();
    [HideInInspector] public List<AIGraphNodeData> _aiGraphChangeLog = new List<AIGraphNodeData>();

    public struct AIPackageChangeLogItem
    {
        public AIPackageNodeData _nodeData;
        public string _packageName;
    };

    [HideInInspector] public List<AIPackageChangeLogItem> _aiPackageChangeLog = new List<AIPackageChangeLogItem>();
#endif

    public override void assign()
    {
        base.assign();

        AddAction(MessageTitles.entity_setTarget,(msg)=>{
            _currentTarget = msg.data as GameEntityBase;
        });

        AddAction(MessageTitles.game_teleportTarget,(msg)=>{
            Transform targetTransform = (Transform)msg.data;
            updatePosition(targetTransform.position);
        });

        
        _actionGraph = new ActionGraph();
        _actionGraph.assign();

        _aiGraph = new AIGraph();
        _aiGraph.assign();

        _danmakuGraph = new DanmakuGraph();
        _statusInfo = new StatusInfo();
        _graphicInterface = new StatusGraphicInterface();
        _sequencerProcessManager = new SequencerGraphProcessManager(this);

        createSpriteRenderObject();
    }

    public virtual void initializeCharacter(CharacterInfoData characterInfo, Vector3 direction)
    {
        _initializeFromCharacter = true;
        _activeSelf = true;
        _characterInfo = characterInfo;

        base.initialize();
        initializeObject();

        detachChildObject();
        setParentObject(null);

        setDirection(direction);

#if UNITY_EDITOR
        _blockAIByEditor = false;
#endif
        _blockAI = false;
        _blockInput = false;

        _currentVelocity = Vector3.zero;
        _currentTarget = null;

        _characterLifeTime = 0f;
        _leftHP = 0f;
        _deadEventDelegate = null;

        _updateDirection = true;
        _updateFlipState = true;
        setKeepAliveEntity(false);
        
        gameObject.name = characterInfo._displayName;
        actionGraphPath = characterInfo._actionGraphPath;
        aiGraphPath = characterInfo._aiGraphPath;
        statusInfoName = characterInfo._statusName;
        _headUpOffset = characterInfo._headUpOffset;

        _enabledLaserEffectItems.Clear();

        _physicsBody.initialize(false,0f,10f);

        _groundController.Initialize(new Vector2(characterInfo._characterWidth, characterInfo._characterHeight));

        _movementControl.initialize();
        _actionGraph.initialize(ResourceContainerEx.Instance().GetActionGraph(characterInfo._actionGraphPath));
        _aiGraph.initialize(this, _actionGraph, ResourceContainerEx.Instance().GetAIGraph(characterInfo._aiGraphPath));

        _actionGraph.initializeCustomValue(_aiGraph.getCustomValueData());

        _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
        _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());

        _danmakuGraph.initialize(this);

        float headUpOffset = _actionGraph.getCurrentHeadUpOffset();
        if(headUpOffset >= 0f)
            _headUpOffset = headUpOffset;

        _statusInfo.initialize(this,characterInfo._statusName);
        _graphicInterface.initialize(this,_statusInfo,new Vector3(0f, _headUpOffset, 0f), true);

        applyActionBuffList(_actionGraph.getDefaultBuffList());
        _currentDirectionType = _actionGraph.getDirectionType();
        _currentRotationType = _actionGraph.getCurrentRotationType();

        CollisionInfoData data = new CollisionInfoData(characterInfo._characterWidth * 2f,characterInfo._characterHeight * 2f,CollisionType.Character);
        _collisionInfo = new CollisionInfo(data);
        CollisionManager.Instance().registerObject(_collisionInfo, this);
        
        _collisionInfo.setActiveCollision(_actionGraph.isActiveCollision());

        _spriteRenderer.enabled = true;
        _spriteRenderer.sprite = _actionGraph.getCurrentSprite(_actionGraph.getCurrentRotationType() != RotationType.AlwaysRight ? (_spriteRotation * _actionStartRotation).eulerAngles.z : MathEx.directionToAngle(_direction));
        _spriteRenderer.flipX = false;
        _spriteRenderer.flipY = false;
        

        initializeActionValue();


#if UNITY_EDITOR
        _actionGraphChangeLog.Clear();
        _aiGraphChangeLog.Clear();
        _aiPackageChangeLog.Clear();

        addActionChangeLog(_actionGraph.getCurrentAction_Debug());
        addAIChangeLog(_aiGraph.getCurrentAINode_Debug());

        AIPackageChangeLogItem packageChangeLogItem;
        packageChangeLogItem._nodeData = _aiGraph.getCurrentAIPackageNode_Debug();
        packageChangeLogItem._packageName = _aiGraph.getCurrentPackageName();
        addAIPackageChangeLog(packageChangeLogItem);

        _actionDebug = false;
        _aiDebug = false;
        _statusDebug = false;
        _animationDebug = false;
#endif
    }

#if UNITY_EDITOR

    private static int _changeLogCount = 8;
    public void addActionChangeLog(ActionGraphNodeData data)
    {
        if(_actionGraphChangeLog.Count >= _changeLogCount)
            _actionGraphChangeLog.RemoveAt(0);
        
        _actionGraphChangeLog.Add(data);
    }

    public void addAIChangeLog(AIGraphNodeData data)
    {
        if(_aiGraphChangeLog.Count >= _changeLogCount)
            _aiGraphChangeLog.RemoveAt(0);
        
        _aiGraphChangeLog.Add(data);
    }

    public void addAIPackageChangeLog(AIPackageChangeLogItem data)
    {
        if(_aiPackageChangeLog.Count >= _changeLogCount)
            _aiPackageChangeLog.RemoveAt(0);
        
        _aiPackageChangeLog.Add(data);
    }
#endif

    public override void initialize()
    {
        if(_initializeFromCharacter)
            return;
        _characterLifeTime = 0f;
        _leftHP = 0f;

        base.initialize();

        _enabledLaserEffectItems.Clear();

        _physicsBody.initialize(false,0f,10f);
        
        _movementControl.initialize();
        _actionGraph.initialize(ResourceContainerEx.Instance().GetActionGraph(actionGraphPath));
        _aiGraph.initialize(this, _actionGraph, ResourceContainerEx.Instance().GetAIGraph(aiGraphPath));

        _actionGraph.initializeCustomValue(_aiGraph.getCustomValueData());

        _sequencerProcessManager.clearSequencerGraphProcessManager();

        _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
        _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());

        _danmakuGraph.initialize(this);
        _statusInfo.initialize(this,statusInfoName);

        _deadEventDelegate = null;

        _updateDirection = true;
        _updateFlipState = true;
        setKeepAliveEntity(false);

        applyActionBuffList(_actionGraph.getDefaultBuffList());

        _headUpOffset = _actionGraph.getCurrentHeadUpOffset();

        CollisionInfoData data = new CollisionInfoData(0.1f,0.1f, CollisionType.Character);
        _collisionInfo = new CollisionInfo(data);

        _collisionInfo.setActiveCollision(_actionGraph.isActiveCollision());

        _graphicInterface.initialize(this,_statusInfo,new Vector3(0f, _collisionInfo.getBoundBox().getHeightHalf(), 0f), true);

        CollisionManager.Instance().registerObject(_collisionInfo, this);

        initializeActionValue();
    }

    private void initializeActionValue()
    {
        _currentDefenceType = _actionGraph.getCurrentDefenceType();
        _currentVelocity = Vector3.zero;
    }
    
    public override void progress(float deltaTime)
    {
        base.progress(deltaTime);
        if(_isActionChangeFrame)
            _isActionChangeFrame = false;

        _characterLifeTime += deltaTime;

        if(_sequencerProcessManager != null)
            _sequencerProcessManager.progress(deltaTime);

        _statusInfo.updateStatus(deltaTime);
        _statusInfo.updateActionConditionData(this);

        _leftHP = _statusInfo.getCurrentStatus("HP");

        _danmakuGraph.process(deltaTime);

        if(_actionGraph != null)
            updateConditionData();

#if UNITY_EDITOR
        if(_activeSelf && (_blockAI == false && _blockAIByEditor == false) && _aiGraph != null)
#else
        if(_activeSelf && _blockAI == false && _aiGraph != null)
#endif
        {
            _aiGraph.updateConditionData();
            
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_TargetDistance, getDistance(_currentTarget));
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_TargetExists, _currentTarget != null);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_ArrivedTarget, _aiGraph.isAIArrivedTarget());
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_CurrentPackageEnd, _aiGraph.isCurrentPackageEnd());
            _actionGraph.setActionConditionData_TargetFrameTag(_currentTarget == null ? null : _currentTarget.getCurrentFrameTagList());
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_PackageStateExecutedTime, _aiGraph.getCurrentPackageExecutedTime());
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_GraphStateExecutedTime, _aiGraph.getCurrentGraphExecutedTime());

            _aiGraph.progress(deltaTime,this);
#if UNITY_EDITOR
            if(_aiGraph.isAIStateChanged())
                addAIChangeLog(_aiGraph.getCurrentAINode_Debug());

            if(_aiGraph.isAIPackageStateChanged())
            {
                AIPackageChangeLogItem packageChangeLogItem;
                packageChangeLogItem._nodeData = _aiGraph.getCurrentAIPackageNode_Debug();
                packageChangeLogItem._packageName = _aiGraph.getCurrentPackageName();
                addAIPackageChangeLog(packageChangeLogItem);
            }
#endif
        }
        else
        {
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_TargetDistance, 0f);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_TargetExists, false);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_ArrivedTarget, false);
            _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.AI_CurrentPackageEnd, false);
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_PackageStateExecutedTime, 0f);
            _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.AI_GraphStateExecutedTime, 0f);
        }

        if(_actionGraph != null && _activeSelf)
        {
            string prevActionName = _actionGraph.getCurrentActionName();

            _physicsBody.progress(deltaTime);

            //action,movementGraph 바뀌는 시점
            if(_actionGraph.progress(deltaTime) == true)
            {
                _isActionChangeFrame = true;

                _currentDefenceType = _actionGraph.getCurrentDefenceType();
                _currentDirectionType = _actionGraph.getDirectionType();

                _collisionInfo.setActiveCollision(_actionGraph.isActiveCollision());

                if(_currentRotationType == RotationType.Torque && _currentRotationType != _actionGraph.getCurrentRotationType())
                    _physicsBody.setTorque(0f);

                _currentRotationType = _actionGraph.getCurrentRotationType();
#if UNITY_EDITOR
                addActionChangeLog(_actionGraph.getCurrentAction_Debug());
#endif
                if(_actionGraph.isActionLoop() == false)
                {
                    applyActionBuffList();
                    _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
                    _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());

                    _updateDirection = true;
                    _updateFlipState = true;

                    updateDirection();
                    updateRotation();

                    _angleBaseRotation = _spriteRotation;
                    _actionStartRotation = _spriteRotation;
                    _actionStartRotation = Quaternion.Inverse(_actionStartRotation);
                }

                float headUpOffset = _actionGraph.getCurrentHeadUpOffset();
                if(headUpOffset >= 0f)
                    _headUpOffset = headUpOffset;
                else if(_characterInfo != null)
                    _headUpOffset = _characterInfo._headUpOffset;
                else
                    _headUpOffset = 0f;

                _graphicInterface.setInterfaceOffset(Vector3.up * _headUpOffset);


                if(_actionGraph.checkCurrentActionFlag(ActionFlags.ClearPush))
                    _currentVelocity = Vector3.zero;
            }

            updateDirection();
            if(_updateFlipState)
            {
                updateFlipState(_actionGraph.getCurrentFlipType());
                _updateFlipState = _actionGraph.getCurrentFlipTypeUpdateOnce() == false;
            }

            //animation 바뀌는 시점
            _actionGraph.updateAnimation(deltaTime, this);
            _movementControl?.progress(deltaTime, _direction * _directionInputRate);
            
            updatePhysics(deltaTime);

            _spriteRenderer.transform.localRotation = _actionGraph.getCurrentAnimationRotation();

            Vector3 outScale = Vector3.one;
            _actionGraph.getCurrentAnimationScale(out outScale);
            _spriteRenderer.transform.localScale = outScale;

            updateRotation();
            _actionGraph.getCurrentAnimationTranslation(out _localSpritePosition);

            _spriteRenderer.sprite = _actionGraph.getCurrentSprite(_actionGraph.getCurrentRotationType() != RotationType.AlwaysRight ? (_spriteRotation * _actionStartRotation).eulerAngles.z : MathEx.directionToAngle(_direction));

        }
        _spriteRenderer.flipX = _flipState.xFlip;
        _spriteRenderer.flipY = _flipState.yFlip;

        if(_statusInfo.isDead())
            _deadEventDelegate?.Invoke(this);

        _deadEventDelegate = null;
        laserEffectCheck();

        _collisionInfo.updateCollisionInfo(transform.position);

        updateDebug();
    }

    public override void afterProgress(float deltaTime)
    {
        base.afterProgress(deltaTime);

        //todo: observer
        _graphicInterface.updatePosition();
        _graphicInterface.updateGague();

        resetState();

        if(_statusDebug == true || GameEditorMaster._instance._statusDebugAll)
        {
            _statusInfo.updateDebugTextXXX(debugTextManager);
        }
        
        CollisionManager.Instance().collisionRequest(_collisionInfo,this,gameEntityCollisionEvent,collisionEndEvent);
    }

    public override void deactive()
    {
        _graphicInterface.release();
        base.deactive();
    }

    public override void dispose(bool disposeFromMaster)
    {
        CollisionManager.Instance().deregisterObject(_collisionInfo.getCollisionInfoData(),this);
        _aiGraph.release();
        _actionGraph.release();
        _danmakuGraph.release();

        base.dispose(disposeFromMaster);
    }

    private void updateDebug()
    {
        if(_actionDebug == true || GameEditorMaster._instance._actionDebugAll)
        {
            debugTextManager.updateDebugText("Action","Action: " + getCurrentActionName());
            debugTextManager.updateDebugText("Defence","Defence: " + getDefenceType());

            string frameTag = "";
            HashSet<string> frameTagList = _actionGraph.getCurrentFrameTagList();
            foreach(var item in frameTagList)
            {
                frameTag += item + ", ";
            }

            debugTextManager.updateDebugText("FrameTag","FrameTag: " + frameTag);
        }
    
        if(getDefenceAngle() != 0f)
            GizmoHelper.instance.drawArc(transform.position,0.8f,getDefenceAngle(),_defenceDirection,Color.cyan,0f);

        debugTextManager.updatePosition(new Vector3(0f, _collisionInfo.getBoundBox().getBottom() - transform.position.y, 0f));

        if(_actionDebug == true || GameEditorMaster._instance._actionDebugAll)
        {
            GizmoHelper.instance.drawLine(transform.position, transform.position + _direction * 1.2f,Color.magenta);
            GizmoHelper.instance.drawLine(transform.position, transform.position + ControllerEx.Instance().getJoystickAxisR(transform.position) * 0.5f,Color.cyan);

            _collisionInfo.drawCollosionArea(_debugColor,1f);
        }

        if(_aiDebug == true || GameEditorMaster._instance._aiDebugAll)
        {
            if(_aiGraph != null && _aiGraph.isValid())
            {
                debugTextManager.updateDebugText("AIState","AIState: " + _aiGraph.getCurrentAIStateName());
                debugTextManager.updateDebugText("AIPackage","   AIPackage: " + _aiGraph.getCurrentPackageName());
                debugTextManager.updateDebugText("AI","   AIPackageState: " + getCurrentAIPackageStateName());
            }

            if(_aiGraph.hasTargetPosition() == true)
            {
                Color targetColor = _aiGraph.isAIArrivedTarget() ? Color.green : Color.red;
                GizmoHelper.instance.drawCircle(_aiGraph.getCurrentTargetPosition(),0.1f,18,targetColor);
                GizmoHelper.instance.drawLine(_aiGraph.getCurrentTargetPosition(), transform.position, targetColor);
            }

            bool hasTarget = _currentTarget != null;
            switch(_aiGraph.getCurrentTargetSearchType())
            {
                case TargetSearchType.Near:
                    GizmoHelper.instance.drawCircle(transform.position,_aiGraph.getCurrentTargetSearchRange(),18,hasTarget ? Color.green : Color.red);
                break;
                case TargetSearchType.NearDirection:
                case TargetSearchType.NearMousePointDirection:
                {
                    Vector3 direction = Vector3.right;
                    if(_aiGraph.getCurrentTargetSearchType() == TargetSearchType.NearDirection)
                        direction = getDirection();
                    else if(_aiGraph.getCurrentTargetSearchType() == TargetSearchType.NearMousePointDirection)
                        direction = getDirectionFromType(DirectionType.MousePoint);

                    GizmoHelper.instance.drawCircle(transform.position,_aiGraph.getCurrentTargetSearchRange(),18,hasTarget ? Color.green : Color.red);
                    GizmoHelper.instance.drawCircle(transform.position + direction * _aiGraph.getCurrentTargetSearchStartRange(),_aiGraph.getCurrentTargetSearchSphereRadius(),18,hasTarget ? Color.green : Color.red);

                    GizmoHelper.instance.drawLine(transform.position + direction * _aiGraph.getCurrentTargetSearchStartRange(), transform.position + direction * _aiGraph.getCurrentTargetSearchRange(), hasTarget ? Color.green : Color.red);
                    GizmoHelper.instance.drawCircle(transform.position + direction * _aiGraph.getCurrentTargetSearchRange(),_aiGraph.getCurrentTargetSearchSphereRadius(),18,hasTarget ? Color.green : Color.red);
                }
                break;
                case TargetSearchType.NearHorizontal:
                case TargetSearchType.NearHorizontalDirection:
                {
                    float width = _aiGraph.getCurrentTargetSearchRange();
                    float up = _aiGraph.getCurrentHorizontalTargetSearchRangeUp();
                    float down = _aiGraph.getCurrentHorizontalTargetSearchRangeDown();
                    Vector3 direction = getDirection();

                    float height = up + down;
                    Vector3 centerOffset = new Vector3(width * 0.5f * (direction.x < 0f ? -1f : 1f), height * 0.5f - down);

                    GizmoHelper.instance.drawRectangle(transform.position + centerOffset, new Vector3(width,height) * 0.5f, hasTarget ? Color.green : Color.red);
                }
                break;
            }

            Dictionary<string, float> customValueDictionary = _actionGraph.getCustomValueDictionary();
            foreach(var item in customValueDictionary)
            {
                debugTextManager.updateDebugText(item.Key,"   " + item.Key + ": " + item.Value);
            }

            if(_currentTarget != null)
            {
                GizmoHelper.instance.drawLine(_currentTarget.transform.position, transform.position, Color.cyan);
            }
        }

        if(_animationDebug || GameEditorMaster._instance._animationDebugAll)
        {
            debugTextManager.updateDebugText("Animation","Animation: " + _actionGraph.getCurrentAnimationName());
        }

        _debugColor = Color.red;
    }

    private void gameEntityCollisionEvent(CollisionSuccessData data)
    {
        _debugColor = Color.green;
    }

    private void collisionEndEvent()
    {

    }

    public void resetState()
    {
        _attackState = AttackState.Default;
        _defenceState = DefenceState.Default;
    }

    public void blockAI(bool value)
    {
        _blockAI = value;

        if(_blockAI)
        {
            _aiGraph.claerAIGraph();
            _aiGraph.setDefaultAINode(this);
        }
    }

#if UNITY_EDITOR
    public void blockAI_Debug(bool value)
    {
        _blockAIByEditor = value;
        setAction(_actionGraph.getActionGraphBaseData_Debug()._defaultActionIndex);
        _aiGraph.claerAIGraph();
        _aiGraph.setDefaultAINode(this);
    }

    public void initializeCharacter_Debug()
    {
        base.initialize();
        initializeObject();

        detachChildObject();
        setParentObject(null);

        setDirection(Vector3.right);

        _blockAIByEditor = false;
        _blockAI = false;
        _blockInput = false;

        _currentVelocity = Vector3.zero;
        _currentTarget = null;

        _characterLifeTime = 0f;
        _leftHP = 0f;
        _deadEventDelegate = null;

        _updateDirection = true;
        _updateFlipState = true;
        setKeepAliveEntity(false);

        _enabledLaserEffectItems.Clear();

        _physicsBody.initialize(false,0f,10f);

        _sequencerProcessManager.clearSequencerGraphProcessManager();

        _movementControl.initialize();
        _actionGraph.initialize(ResourceContainerEx.Instance().GetActionGraph(actionGraphPath));
        _aiGraph.initialize(this, _actionGraph, ResourceContainerEx.Instance().GetAIGraph(aiGraphPath));

        _actionGraph.initializeCustomValue(_aiGraph.getCustomValueData());

        _movementControl.changeMovement(this,_actionGraph.getCurrentMovement());
        _movementControl.setMoveScale(_actionGraph.getCurrentMoveScale());

        _danmakuGraph.initialize(this);

        _statusInfo.initialize(this,_characterInfo._statusName);
        _graphicInterface.initialize(this,_statusInfo,new Vector3(0f, _headUpOffset, 0f), true);

        applyActionBuffList(_actionGraph.getDefaultBuffList());

        _spriteRenderer.enabled = true;
        _spriteRenderer.sprite = _actionGraph.getCurrentSprite(_actionGraph.getCurrentRotationType() != RotationType.AlwaysRight ? (_spriteRotation * _actionStartRotation).eulerAngles.z : MathEx.directionToAngle(_direction));
        _spriteRenderer.flipX = false;
        _spriteRenderer.flipY = false;

        initializeActionValue();

        addActionChangeLog(_actionGraph.getCurrentAction_Debug());
        addAIChangeLog(_aiGraph.getCurrentAINode_Debug());

        AIPackageChangeLogItem packageChangeLogItem;
        packageChangeLogItem._nodeData = _aiGraph.getCurrentAIPackageNode_Debug();
        packageChangeLogItem._packageName = _aiGraph.getCurrentPackageName();
        addAIPackageChangeLog(packageChangeLogItem);

        _actionDebug = false;
        _aiDebug = false;
        _statusDebug = false;
        _animationDebug = false;
    }
#endif

    public void laserEffectCheck()
    {
        for(int index = 0; index < _enabledLaserEffectItems.Count; ++index)
        {
            if(_enabledLaserEffectItems[index].isActivated() && _enabledLaserEffectItems[index]._spawnOwner == this)
                continue;
            
            _enabledLaserEffectItems.RemoveAt(index);
            --index;
        }

        if(_actionGraph.checkCurrentActionFlag(ActionFlags.LaserEffect) == false)
        {
            for(int index = 0; index < _enabledLaserEffectItems.Count; ++index)
            {
                _enabledLaserEffectItems[index].release();
            }
            _enabledLaserEffectItems.Clear();
        }

    }

    public void updateStatusConditionData(string targetName, float value)
    {
        _actionGraph.setActionConditionData_Status(targetName,value);
    }

    public void updateConditionData()
    {
        Vector3 input = ControllerEx.Instance().GetJoystickAxis();
        Vector3 inputDirection = ControllerEx.Instance().getJoystickAxisR(transform.position);

        float angleBetweenStick = MathEx.clampDegree(Vector3.SignedAngle(input, inputDirection,Vector3.forward));
        float angleDirection = MathEx.clampDegree(Vector3.SignedAngle(Vector3.right, _direction, Vector3.forward));
        float angleDirectionToStick = MathEx.clampDegree(Vector3.Angle(getFlipState().xFlip ? -Vector3.right : Vector3.right, inputDirection));

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Test, MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_Dash, Input.GetKey(KeyCode.Space));
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleBetweenStick, angleBetweenStick);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleDirection, angleDirection);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_AngleFlipDirectionToStick, angleDirectionToStick);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsXFlip, _flipState.xFlip);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsYFlip, _flipState.yFlip);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_CurrentFrame, _actionGraph.getCurrentFrame());
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsCatcher, hasChildObject());
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Action_IsCatchTarget, hasParentObject());
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Action_ActionExecutedTime, _actionGraph.getActionExecutedTime());

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_AttackCharge, Input.GetMouseButton(0));
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_AttackBlood, Input.GetKey(KeyCode.R));
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_Guard, Input.GetMouseButton(1));
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_CanInput, _blockInput == false);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Input_MoveInput, ControllerEx.Instance().GetJoystickAxis().x != 0f);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Guarded, _attackState == AttackState.AttackGuarded);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Success, _attackState == AttackState.AttackSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Parried, _attackState == AttackState.AttackParried);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_Evaded, _attackState == AttackState.AttackEvade);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_GuardBreak, _attackState == AttackState.AttackGuardBreak);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_GuardBreakFail, _attackState == AttackState.AttackGuardBreakFail);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Attack_CatchTarget, _attackState == AttackState.AttackCatch);

        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Crash, _defenceState == DefenceState.DefenceCrash);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Success, _defenceState == DefenceState.DefenceSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Parry, _defenceState == DefenceState.ParrySuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Hit, _defenceState == DefenceState.Hit);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Evade, _defenceState == DefenceState.EvadeSuccess);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_GuardBroken, _defenceState == DefenceState.GuardBroken);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_GuardBreakFail, _defenceState == DefenceState.GuardBreakFail);
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Defence_Catched, _defenceState == DefenceState.Catched);

        // if(_actionGraph.getActionConditionData_Bool(ConditionNodeUpdateType.Entity_Dead) && _statusInfo.isDead() == false)
        //     DebugUtil.assert(false, "이미 죽었는데 다시 살아났습니다. 통보 요망");
            
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Entity_Dead, _statusInfo.isDead());
        _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Entity_Kill, false);
        _actionGraph.setActionConditionData_Float(ConditionNodeUpdateType.Entity_LifeTime, _characterLifeTime);

    }

    public override void release(bool disposeFromMaster)
    {
        base.release(disposeFromMaster);

        if(disposeFromMaster == false)
            _movementControl?.release();
        
        _aiGraph.release();
        _actionGraph.release();
        _danmakuGraph.release();
    }

    public void updateFlipState(FlipType flipType)
    {
        _flipState = getCurrentFlipState(flipType);
    }

    private FlipState getCurrentFlipState(FlipType flipType)
    {
        FlipState currentFlipState = _actionGraph.getCurrentFlipState();
        FlipState flipState = new FlipState();

        if(_direction.sqrMagnitude <= 0.01f)
            return _flipState;

        switch(flipType)
        {
            case FlipType.Direction:
                if(MathEx.abs(_direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = _direction.x < 0;
                if(MathEx.abs(_direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = _direction.y < 0;

                break;
            case FlipType.MousePoint:
            {
                Vector3 direction = ControllerEx.Instance().getJoystickAxisR(transform.position);
                if(MathEx.abs(direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = direction.x < 0;
                if(MathEx.abs(direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = direction.y < 0;
            }
            break;
            case FlipType.MoveDirection:
            {
                Vector3 direction = getMovementControl().getMoveDirection();
                if(MathEx.abs(direction.x) != 0f && currentFlipState.xFlip == true)
                    flipState.xFlip = direction.x < 0;
                if(MathEx.abs(direction.y) != 0f && currentFlipState.yFlip == true)
                    flipState.yFlip = direction.y < 0;
            }
            break;
            case FlipType.Keep:
                flipState.xFlip = _spriteRenderer.flipX;
                flipState.yFlip = _spriteRenderer.flipY;
                break;
        }

        DebugUtil.assert((int)FlipType.Count == 5, "flip type count error");

        return flipState;
    }

    public void clearActionBuffList()
    {
        _statusInfo.clearBuff();
    }

    public void deleteActionBuffList(int[] buffList)
    {
        if(buffList == null)
            return;

        for(int i = 0; i < buffList.Length; ++i)
        {
            _statusInfo.deleteBuff(buffList[i]);
        }
    }

    public void applyActionBuffList(int[] buffList)
    {
        if(buffList == null)
            return;

        for(int i = 0; i < buffList.Length; ++i)
        {
            _statusInfo.applyBuff(buffList[i]);

            if(_statusInfo.checkBuffApplyStatus(buffList[i],"HP"))
            {
                bool isDead = checkDeadBeforeApplyBuff(GlobalTimer.Instance().getSclaedDeltaTime(),buffList[i]);
                _actionGraph.setActionConditionData_Bool(ConditionNodeUpdateType.Entity_Dead,isDead);
            }
        }

    }

    public bool predictionDead()
    {
        return _leftHP <= 0f;
    }

    public bool checkDeadBeforeApplyBuff(float deltaTime, int targetBuff)
    {
        _leftHP += _statusInfo.buffValuePrediction(deltaTime, targetBuff);
        return _leftHP <= 0f;
    }

    private void applyActionBuffList()
    {
        if(_currentActionBuffList != null)
        {
            for(int i = 0; i < _currentActionBuffList.Length; ++i)
            {
                _statusInfo.deleteBuff(_currentActionBuffList[i]);
            }
        }

        _currentActionBuffList = _actionGraph.getCurrentBuffList();

        applyActionBuffList(_currentActionBuffList);
    }

    private void updatePhysics(float deltaTime)
    {
        transform.position += _currentVelocity * deltaTime;
        _currentVelocity = MathEx.convergence0(_currentVelocity, _defaultFriction * deltaTime);
    }

    //todo : input manager 만들어서 거기서 moveiNput 가져오게 만들기
    private void updateDirection()
    {
        if(_updateDirection == false)
            return;

        DirectionType directionType = DirectionType.AlwaysRight;
        DefenceDirectionType defenceDirectionType = DefenceDirectionType.Direction;

        if(_actionGraph != null)
        {
            directionType = _currentDirectionType;
            defenceDirectionType = _actionGraph.getDefenceDirectionType();
        }

        _direction = getDirectionFromType(directionType);

        switch(defenceDirectionType)
        {
            case DefenceDirectionType.Direction:
                _defenceDirection = _direction;
                break;
            case DefenceDirectionType.MousePoint:
                _defenceDirection = ControllerEx.Instance().getJoystickAxisR(transform.position);
                break;
        }

        _updateDirection = _actionGraph.getCurrentDirectionUpdateOnce() == false;
    }

    public Vector3 getDirectionFromType(DirectionType directionType)
    {
        Vector3 direction = _direction;
        _directionInputRate = 1f;

        switch(directionType)
        {
            case DirectionType.AlwaysRight:
                direction = Vector3.right;
                break;
            case DirectionType.AlwaysLeft:
                direction = Vector3.left;
                break;
            case DirectionType.AlwaysUp:
                direction = Vector3.up;
                break;
            case DirectionType.AlwaysDown:
                direction = Vector3.down;
                break;
            case DirectionType.Keep:
                break;
            case DirectionType.MoveInput:
            case DirectionType.MoveInputHorizontal:
            {
                Vector3 input = ControllerEx.Instance().GetJoystickAxis();
                if(MathEx.equals(input.sqrMagnitude,0f,float.Epsilon) == false )
                {
                    direction = input;
                    _directionInputRate = input.magnitude;
                    direction.Normalize();
                }
                else
                {
                    _directionInputRate = 0f;
                    break;
                }

                if(directionType == DirectionType.MoveInputHorizontal)
                {
                    direction.y = 0f;
                    _directionInputRate = direction.magnitude;
                    direction.Normalize();
                }
                
            }
            break;
            case DirectionType.MousePoint:
                direction = ControllerEx.Instance().getJoystickAxisR(transform.position);
                break;
            case DirectionType.AttackedPoint:
                direction = (_recentlyAttackPoint - transform.position).normalized;
                break;
            case DirectionType.AI:
                direction = _aiGraph.getRecentlyAIDirection();
                break;
            case DirectionType.AIHorizontal:
                direction = _aiGraph.getRecentlyAIDirection();
                direction.y = 0f;
                direction.Normalize();
                break;
            case DirectionType.AITarget:
                if(_currentTarget != null && _currentTarget.isValid())
                    direction = (_currentTarget.transform.position - transform.position).normalized;
                break;
            case DirectionType.MoveDirection:
                direction = getMovementControl().getMoveDirection();
                break;
            case DirectionType.Count:
                DebugUtil.assert(false, "invalid direction type : {0}",_currentDirectionType);
                break;
        }

        // if(direction.sqrMagnitude == 0f)
        //     direction = Vector3.right;

        direction = Quaternion.Euler(0f,0f,_actionGraph.getCurrentDirectionAngle()) * direction;

        return direction;
    }

    private void updateRotation()
    {
        RotationType rotationType = _currentRotationType;
        if(_actionGraph != null)
        {
            rotationType = _currentRotationType;
        }

        float targetRotation = 0f;
        switch(rotationType)
        {
            case RotationType.AlwaysRight:
                targetRotation = 0f;
                break;
            case RotationType.Direction:
                targetRotation = MathEx.directionToAngle(_direction);
                break;
            case RotationType.MousePoint:
                targetRotation = MathEx.directionToAngle( ControllerEx.Instance().getJoystickAxisR(transform.position));
                break;
            case RotationType.MoveDirection:
                targetRotation = MathEx.directionToAngle(getMovementControl().getMoveDirection());
                break;
            case RotationType.Keep:
                break;
            case RotationType.Torque:
                targetRotation = _spriteRotation.eulerAngles.z + _physicsBody.getCurrentTorqueValue() * GlobalTimer.Instance().getSclaedDeltaTime();
                break;
    
        }
        DebugUtil.assert((int)RotationType.Count == 6, "check this");

        if(_actionGraph != null && _actionGraph.isRotateBySpeed())
        {
            if (MathEx.equals(_spriteRotation.eulerAngles.z, targetRotation, float.Epsilon) == false)
            {
                float angle = Mathf.MoveTowardsAngle(_spriteRotation.eulerAngles.z, targetRotation, _actionGraph.getCurrentRotateSpeed() * GlobalTimer.Instance().getSclaedDeltaTime());
                _spriteRotation = Quaternion.Euler(0f,0f,angle);
            }
        }
        else
        {
            _spriteRotation = Quaternion.Euler(0f,0f, targetRotation);
        }

        float zRotation = _spriteRotation.eulerAngles.z;
        if(rotationType != RotationType.AlwaysRight)
            zRotation -= (getFlipState().xFlip ? -180f : 0f);

        _spriteObject.transform.localRotation *= Quaternion.Euler(0f,0f,zRotation);
    }

    public void setDirectionType(DirectionType directionType)
    {
        _currentDirectionType = directionType;
        updateDirection();
    }

    public MovementGraphPresetData getMovementGraphPresetDataFromActionIndex(int actionIndex)
    {
        if(actionIndex == -1)
            return null;
            
        return _actionGraph.getMovementGraphPresetByIndex(actionIndex);
    }

    public float getAnimationPlayTimeFromActionIndex(int actionIndex)
    {
        if(actionIndex == -1)
            return 0f;
            
        return _actionGraph.getAnimationPlayTimeByIndex(actionIndex);
    }

    public void setGraphicInterfaceActive(bool value)
    {
        _graphicInterface?.setActive(value);
    }

    public GroundController getGroundController() {return _groundController;}

    public void addTorque(float torque) {_physicsBody.addTorque(torque);}
    public void setTorque(float torque) {_physicsBody.setTorque(torque);}

    public bool isMoving() {return _movementControl.isMoving();}
    public bool isValid() {return _movementControl != null && _actionGraph != null && _spriteRenderer != null && gameObject.activeInHierarchy;}

    public void setSpriteRotation(Quaternion rotation) {_spriteObject.transform.rotation = rotation;}

    public void addDanmaku(string path, Vector3 offset, bool useFlip) {_danmakuGraph.addDanmakuGraph(path, transform.position, offset, useFlip, _searchIdentifier);}
    
    public void addDeadEvent(DeadEventDelegate deadEvent) {_deadEventDelegate += deadEvent;} 
    public void deleteDeadEvent(DeadEventDelegate deadEvent) {_deadEventDelegate -= deadEvent;}

    public bool isDead() {return _statusInfo.isDead();}

    public void setActionCondition_Bool(ConditionNodeUpdateType updateType, bool value) {_actionGraph.setActionConditionData_Bool(updateType, value);}

    public void executeCustomAIEvent(string eventName) {_aiGraph.executeCustomAIEvent(eventName);}

    public void addLaserEffect(TimelineEffectItem laserEffect) {_enabledLaserEffectItems.Add(laserEffect);}

    public SequencerGraphProcessor startSequencer(string sequencerKey, GameEntityBase targetEntity, bool includePlayer) {return _sequencerProcessManager.startSequencerClean(sequencerKey,targetEntity,includePlayer);}

    public void addSequencerSignal(string signal) {_sequencerProcessManager.addSequencerSignal(signal);}

    public void setActiveSelf(bool active, bool hideGraphics) 
    {
        _activeSelf = active;
        if(hideGraphics)
            _spriteRenderer.enabled = _activeSelf;
        else
            _spriteRenderer.enabled = true;
    }

    public CommonMaterial getCharacterMaterial()
    {
        CommonMaterial commonMaterial = _actionGraph.getCurrentMaterial();
        if(commonMaterial == CommonMaterial.Count)
            return _characterInfo._defaultMaterial;
        
        
        return commonMaterial;
    }

    public void setKeepAliveEntity(bool value) {_keepAliveEntity = value;}
 
    public bool isKeepAliveEntity() {return _keepAliveEntity;}
    public bool isActionChangeFrame() {return _isActionChangeFrame;}
    public bool isActiveSelf() {return _activeSelf;}

    public int getActionIndex(string actionName) {return _actionGraph.getActionIndex(actionName);}

    public void blockInput(bool value) {_blockInput = value; _actionGraph.blockInput(value);}
    public float getHeadUpOffset() {return _headUpOffset;}

    public float getCustomValue(string customValueName) {return _actionGraph.getCustomValue(customValueName);}
    public void setCustomValue(string customValueName, float value) {_actionGraph.setCustomValue(customValueName, value);}
    public void addCustomValue(string customValueName, float value) {_actionGraph.addCustomValue(customValueName, value);}

    public AttackType getCurrentIgnoreAttackType() {return _actionGraph.getCurrentIgnoreAttackType();}
    public StatusInfo getStatusInfo() { return _statusInfo; }
    public float getStatusPercentage(string targetName) {return _statusInfo.getCurrentStatusPercentage(targetName);}

    public void executeAIEvent(AIChildEventType eventType) {_aiGraph.executeAIEvent(eventType);}
    public bool processActionCondition(ActionGraphConditionCompareData compareData) {return _actionGraph.processActionCondition(compareData);}

    public HashSet<string> getCurrentFrameTagList() {return _actionGraph.getCurrentFrameTagList();}

    public void addVelocity(Vector3 velocity) {_currentVelocity += velocity;}
    public void setVelocity(Vector3 velocity) {_currentVelocity = velocity;}

    public bool checkCurrentActionFlag(ActionFlags flag) {return _actionGraph.checkCurrentActionFlag(flag);}

    public bool applyFrameTag(string tag) {return _actionGraph.applyFrameTag(tag);}
    public void deleteFrameTag(string tag) {_actionGraph.deleteFrameTag(tag);}
    public bool checkFrameTag(string tag) {return _actionGraph.checkFrameTag(tag);}

    public bool isAIGraphValid() {return _aiGraph != null && _aiGraph.isValid();}
    public TargetSearchType getCurrentTargetSearchType() {return _aiGraph.getCurrentTargetSearchType();}
    public SearchIdentifier getCurrentSearchIdentifier() {return _aiGraph.getCurrentSearchIdentifier();}
    public float getCurrentTargetSearchRange() {return _aiGraph.getCurrentTargetSearchRange();}
    public float getCurrentTargetSearchStartRange() {return _aiGraph.getCurrentTargetSearchStartRange();}
    public float getCurrentTargetSearchSphereRadius() {return _aiGraph.getCurrentTargetSearchSphereRadius();}

    public Vector3 getCurrentDefenceDirection() {return _defenceDirection;}

    public void setAIDirectionRotateProcess(float time, float angle) {_aiGraph.setRotateProcess(time,angle);}

    public void terminateAIPackage() {_aiGraph.terminatePackage();}
    public void setAIState(int index) {_aiGraph.changeAIPackageStateOther(index);}

    public void setAiDirection(float angle) {_aiGraph.setAIDirection(angle);}
    public void setAiDirection(Vector3 direction) {_aiGraph.setAIDirection(direction);}
    public Vector3 getAiDirection() {return _aiGraph.getRecentlyAIDirection();}

    public void setAnimationSpeed(float speed) {_actionGraph.setAnimationSpeed(speed);}

    public void setAttackPoint(Vector3 attackPoint) {_recentlyAttackPoint = attackPoint;}

    public void setDefenceType(DefenceType defenceType) {_currentDefenceType = defenceType;}

    public void changeAnimationByPath(string path) {_actionGraph.changeAnimationByCustomPreset(path);}

    public void setDummyAction() {_actionGraph.changeActionOther(_actionGraph.getDummyActionIndex());}
    public void setAction(int index) {_actionGraph.changeActionOther(index);}
    public void setAction(string nodeName) {_actionGraph.changeActionOther(_actionGraph.getActionIndex(nodeName));}
    
    public void setDefaultAction()
    {
        _actionGraph.setDefaultActionOther();
    }


    public void setTargetEntity(GameEntityBase target) {_currentTarget = target;}
    public GameEntityBase getCurrentTargetEntity() {return _currentTarget;}

    public Vector3 getAttackPoint() {return _recentlyAttackPoint;}

    public FlipState getFlipState() {return _flipState;}

    public CollisionInfo getCollisionInfo() {return _collisionInfo;}
    public string getCurrentAIPackageStateName() {return _aiGraph.getCurrentAIPackageStateName();}
    public string getCurrentActionName() {return _actionGraph == null ? "" : _actionGraph.getCurrentActionName();}
    public float getDefenceAngle() {return _actionGraph.getCurrentDefenceAngle();}
    public DefenceType getDefenceType() {return _currentDefenceType;}
    public MoveValuePerFrameFromTimeDesc getMoveValuePerFrameFromTimeDesc(){return _actionGraph.getMoveValuePerFrameFromTimeDesc();}
    public MovementGraph getCurrentMovementGraph(){return _actionGraph.getCurrentMovementGraph();}
    public MovementGraphPresetData getCurrentMovementGraphPreset() {return _actionGraph.getCurrentMovementGraphPreset();}
    public MovementBase getCurrentMovement() {return _movementControl.getCurrentMovement();}
    public MovementControl getMovementControl(){return _movementControl;}

    public ActionGraph getActionGraph() {return _actionGraph;}
#if UNITY_EDITOR
    public AIGraph getAIGraph_Debug() {return _aiGraph;}
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameEntityBase))]
public class GameEntityBaseEditor : Editor
{
    protected GameEntityBase control;
    private GUIStyle buttonStyle;

    private Vector2 _actionListScroll = Vector2.zero;
    private string _actionListSearchString = "";
    private string[] _actionListSearchStringArray = null;

    public void OnEnable()
    {
        control = (GameEntityBase)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
            GUI.FocusControl("");

        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
        }

        
        ActionGraphBaseData actionBaseData = control.getActionGraph().getActionGraphBaseData_Debug();
        if(actionBaseData == null)
            return;
        
        EditorGUILayout.BeginHorizontal();
        {
            Color guiColor = GUI.color;
            GUI.color = control._blockAIByEditor ? Color.red : guiColor;
            if( control._blockAIByEditor ? GUILayout.Button("Enable AI") : GUILayout.Button("Block AI"))
                control.blockAI_Debug(!control._blockAIByEditor);
            GUI.color = guiColor;

            if(GUILayout.Button("Refresh"))
            {
                control.initializeCharacter_Debug();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Action List");
        string searchString = _actionListSearchString;

        _actionListScroll = EditorGUILayout.BeginScrollView(_actionListScroll,"box",GUILayout.Height(200f));
        {
            _actionListSearchString = EditorGUILayout.TextField("Search",_actionListSearchString);

            if(_actionListSearchString != searchString)
                _actionListSearchStringArray = _actionListSearchString.ToLower().Split(' ');

            for(int index = 0; index < actionBaseData._actionNodeCount; ++index)
            {
                if(_actionListSearchString != "")
                {
                    string lowerString = actionBaseData._actionNodeData[index]._nodeName.ToLower();
                    bool contains = false;
                    foreach(var targetString in _actionListSearchStringArray)
                    {
                        contains = lowerString.Contains(targetString);
                        if(contains)
                            break;
                    }

                    if(contains == false)
                        continue;
                }

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button(actionBaseData._actionNodeData[index]._nodeName, buttonStyle))
                    control.setAction(index);

                if(GUILayout.Button("Open",GUILayout.Width(50f)))
                    FileDebugger.OpenFileWithCursor(actionBaseData._fullPath,actionBaseData._actionNodeData[index]._lineNumber);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();


        EditorGUILayout.Space(10f);

        GUILayout.Label("State Execution Log");

        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("Action");
            for(int index = control._actionGraphChangeLog.Count - 1; index >= 0; --index)
            {
                EditorGUILayout.BeginHorizontal();

                ActionGraph actionGraph = control.getActionGraph();

                if(GUILayout.Button(control._actionGraphChangeLog[index]._nodeName,buttonStyle,GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f)))
                    FileDebugger.OpenFileWithCursor(actionGraph.getActionGraphBaseData_Debug()._fullPath,control._actionGraphChangeLog[index]._lineNumber);


                if (control._actionGraphChangeLog[index]._isDummyAction == false)
                {
                    int targetAnimationIndex = control._actionGraphChangeLog[index]._animationInfoIndex;
                    AnimationPlayDataInfo[] playDataInfoArray = control.getActionGraph().getActionGraphBaseData_Debug()._animationPlayData[targetAnimationIndex];

                    if (playDataInfoArray != null)
                    {
                        // float widthInterval = (EditorGUIUtility.currentViewWidth * 0.5f) * (1f / playDataInfoArray.Length);
                        // float width = widthInterval;
                        for (int i = 0; i < playDataInfoArray.Length; ++i)
                        {
                            AnimationPlayDataInfo animationPlayDataInfo = playDataInfoArray[i];

                            if (GUILayout.Button(animationPlayDataInfo._path, buttonStyle))
                                PingTarget(animationPlayDataInfo._customPreset);
                        }

                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10f);

        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("AIGraph");
            for(int index = control._aiGraphChangeLog.Count - 1; index >= 0; --index)
            {
                if(GUILayout.Button(control._aiGraphChangeLog[index]._nodeName,buttonStyle))
                    FileDebugger.OpenFileWithCursor(control.getAIGraph_Debug().getAIGraphBaseData_Debug()._fullPath,control._aiGraphChangeLog[index]._lineNumber);
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10f);

        EditorGUILayout.BeginVertical("box");
        {
            GUILayout.Label("AIPackage");
            for(int index = control._aiPackageChangeLog.Count - 1; index >= 0; --index)
            {
                if(GUILayout.Button(control._aiPackageChangeLog[index]._packageName + ": " + control._aiPackageChangeLog[index]._nodeData._nodeName,buttonStyle))
                    FileDebugger.OpenFileWithCursor(control.getAIGraph_Debug().getCurrentPackageBaseData_Debug()._fullPath,control._aiPackageChangeLog[index]._nodeData._lineNumber);
            }
        }
        EditorGUILayout.EndVertical();

    }

    private void PingTarget(ScriptableObject obj)
    {
        EditorGUIUtility.PingObject(obj);
        EditorUtility.FocusProjectWindow();
    }
}
#endif
