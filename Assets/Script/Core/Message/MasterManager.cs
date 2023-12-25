using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MasterManager : MessageHub<ManagerBase>
{
    public static MasterManager     instance;

    public List<ManagerBase>        managers;

    private bool                    _update = true;

    private bool                    _frameUpdate = false;

    private float                   _updateStopTimer = 0f;

    private float                   _timeScaleTimer = 0f;
    private float                   _targetTimeScale = 0f;

    private float                   _timeScaleBlendTimer = 0f;
    private float                   _timeScaleBlendFactor = 0f;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        _unknownMessageProcess = (msg)=>
        {
            MessagePool.ReturnMessage(msg);
        };
        AddAction(MessageTitles.system_registerRequest,(msg)=>
        {
            RegisterReceiver(msg);
        });
        AddAction(MessageTitles.system_deregisterRequest,(msg)=>
        {
            DeregisteReceiver(msg);
        });

        AddAction(MessageTitles.system_pauseUpdate,(msg)=>
        {
            _update = false;
        });
        AddAction(MessageTitles.system_playUpdate,(msg)=>
        {
            _update = true;
        });
        AddAction(MessageTitles.system_updateFrame,(msg)=>
        {
            _frameUpdate = true;
        });
        AddAction(MessageTitles.entity_stopUpdate, (msg)=>{
            StopUpdateSecond(MessageDataPooling.CastData<FloatData>(msg.data).value);
        });
        AddAction(MessageTitles.entity_setTimeScale, (msg)=>{
            Vector3 timeScaleData = MessageDataPooling.CastData<Vector3Data>(msg.data).value;
            setTimeScaleSecond(timeScaleData.x,timeScaleData.y,timeScaleData.z);
        });

        StaticDataLoader.loadStaticData();

        foreach(var m in managers)
        {
            if(m == null)
                continue;

            m.assign();
            RegisterReceiver(m);
        }
        foreach(var m in managers)
        {
            if(m == null)
                continue;
                
            m.initialize();
        }

        FMODAudioManager.Instance().initialize();
        CameraControlEx.Instance().initialize();
        DanmakuManager.Instance().initialize();
        UIRepeater.Instance().clear();

        StageProcessor.Instance().setTargetCameraControl(CameraControlEx.Instance());
    }
    public void Start()
    {
        ReceiveMessageProcessing();

        Cursor.visible = false;
    }
    public void Update()
    {
        if(Input.GetKey(KeyCode.F1))
        {
            Screen.SetResolution(960,640,FullScreenMode.Windowed);
        }
        else if(Input.GetKey(KeyCode.F2))
        {
            Screen.SetResolution(1440,960,FullScreenMode.Windowed);
        }
        else if(Input.GetKey(KeyCode.F3))
        {
            Screen.SetResolution(2160,1440,FullScreenMode.Windowed);
        }
        else if(Input.GetKey(KeyCode.F4))
        {
            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height,FullScreenMode.FullScreenWindow);
        }

        float deltaTimeMultiflier = 1f;
        if(Input.GetKey(KeyCode.LeftBracket))
            deltaTimeMultiflier = 10f;

        float deltaTime = Time.deltaTime * deltaTimeMultiflier;
        ActionKeyInputManager.Instance().progress(deltaTime);
        GlobalTimer.Instance().setUpdateProcessing(true);

        if(_update == false && _frameUpdate == false)
        {
            GlobalTimer.Instance().setUpdateProcessing(false);
            ReceiveMessageProcessing();
            return;
        }
        
        if(_frameUpdate)
        {
            deltaTime = 0.01666f * 2f;
            _frameUpdate = false;
        }

        if(_updateStopTimer > 0f)
        {
            _updateStopTimer -= deltaTime;
            return;
        }

        if(_timeScaleTimer > 0f)
        {
            _timeScaleTimer -= deltaTime;
            deltaTime *= _targetTimeScale;
        }
        else if(_timeScaleBlendTimer > 0f)
        {
            _timeScaleBlendTimer -= deltaTime;
            deltaTime *= Mathf.Lerp(1f, _targetTimeScale, (_timeScaleBlendTimer / _timeScaleBlendFactor));
        }

        GlobalTimer.Instance().setScaledDeltaTime(deltaTime);
        GlobalTimer.Instance().updateGlobalTime(deltaTime);

        deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        DanmakuManager.Instance().process(deltaTime);

        ManagersUpdate(deltaTime);
        ManagersAfterUpdate(deltaTime);
        ManagersSendMessageProcessing();
        SendMessageProcessing();
        CallReceiveMessageProcessing();
        ManagersReceiveMessageProcessing();
        ReceiveMessageProcessing();

        UIRepeater.Instance().updateUIRepeater();
        HPSphereUIManager.Instance().progress(deltaTime);
        TalkBalloonManager.Instance().updateTalkBalloonManager(deltaTime);
        CameraControlEx.Instance().progress(deltaTime);

        StageProcessor.Instance().processStage(deltaTime);
        
        FMODAudioManager.Instance().updateAudio();
        CameraControlEx.Instance().SyncPosition();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            StageProcessor.Instance().stopStage(true);
            ScreenDirector._instance._screenFader.clear();
            LetterBox._instance.clear();
        }
    }

    public void LateUpdate()
    {
        CollisionManager.Instance().collisionUpdate();
        WeightRandomManager.Instance().updateRandom();
    }

    public void FixedUpdate()
    {
        ManagersFixedUpdate(Time.fixedDeltaTime);
    }

    public void StopUpdateSecond(float time)
    {
        if(_updateStopTimer < time)
            _updateStopTimer = time;
    }

    public bool isGameUpdate() {return _update;}

    public void setTimeScaleSecond(float timeScale, float time, float blendTime)
    {
        _targetTimeScale = timeScale;
        _timeScaleTimer = time;
        _timeScaleBlendFactor = blendTime;
        _timeScaleBlendTimer = blendTime;
    }

    public void ManagersUpdate(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.progress(deltaTime);
        }
    }
    public void ManagersAfterUpdate(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.afterProgress(deltaTime);
        }
    }
    public void ManagersFixedUpdate(float deltaTime)
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.fixedProgress(deltaTime);
        }
    }
    public void ManagersSendMessageProcessing()
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.SendMessageProcessing();
        }
    }
    public void ManagersReceiveMessageProcessing()
    {
        foreach(var receiver in _receivers.Values)
        {
            receiver.CallReceiveMessageProcessing();
        }
    }
    public void SendMessageDirectInMaster(ushort title, int target, System.Object data)
    {
        var msg = MessagePack(title,target,data);
        SendMessageDirectInMaster(msg);
    }
    public void SendMessageDirectInMaster(Message msg)
    {
        HandleMessage(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void SendMessageDirectInMasterQuick(ushort title, int target, System.Object data)
    {
        var msg = MessagePack(title,target,data);
        SendMessageDirectInMasterQuick(msg);
    }
    public void SendMessageDirectInMasterQuick(Message msg)
    {
        HandleMessageQuick(msg);
#if UNITY_EDITOR
        Debug_AddSendedQueue(msg);
#endif
    }
    public void HandleMessageQuick(Message msg)
    {
        if(IsInReceivers(msg.target) && _receivers[msg.target].CanHandleMessage(msg))
        {
#if UNITY_EDITOR
            _receivers[msg.target].Debug_AddReceivedQueue(msg);
#endif
            _receivers[msg.target].MessageProcessing(msg);
            MessagePool.ReturnMessage(msg);
        }
        else if((msg.target == 0 || GetUniqueID() == msg.target) && CanHandleMessage(msg))
        {
#if UNITY_EDITOR
            Debug_AddReceivedQueue(msg);
#endif
            MessageProcessing(msg);
            MessagePool.ReturnMessage(msg);
        }
        else
        {
            bool find = false;
            foreach(var other in _receivers.Values)
            {            
                if(other.IsInReceivers(msg.target) && other.GetReciever(msg.target).CanHandleMessage(msg))
                {
#if UNITY_EDITOR
                    other.GetReciever(msg.target).Debug_AddReceivedQueue(msg);
#endif
                    Debug.Log("End");
                    other.GetReciever(msg.target).MessageProcessing(msg);
                    MessagePool.ReturnMessage(msg);
                    find = true;
                    break;
                }
            }
            if(!find)
                _unknownMessageProcess(msg);
        }
    }
    public override void HandleMessage(Message msg)
    {
        if(IsInReceivers(msg.target))
        {
            _receivers[msg.target].ReceiveMessage(msg);
        }
        else if(msg.target == 0 || GetUniqueID() == msg.target)
        {
            ReceiveMessage(msg);
        }
        else
        {
            bool find = false;
            foreach(var other in _receivers.Values)
            {
                if(other.IsInReceivers(msg.target))
                {
                    other.GetReciever(msg.target).ReceiveMessage(msg);
                    find = true;
                    break;
                }
            }
            if(!find)
                _unknownMessageProcess(msg);
        }
    }
    private void RegisterReceiver(Message msg)
    {
        if(msg.target == GetUniqueID() || msg.target == 0)
        {
            var manager = (ManagerBase)msg.sender;
            RegisterReceiver(manager);
        }
        else if(IsInReceivers(msg.target))
        {
            _receivers[msg.target].RegisterReceiver((ObjectBase)msg.sender);
        }
    }
    private void DeregisteReceiver(Message msg)
    {
        if(msg.target == GetUniqueID() || msg.target == 0)
        {
            var manager = (int)msg.data;
            DeregisteReceiver(manager);
        }
        else if(IsInReceivers(msg.target))
        {
            var target = (int)msg.data;
            _receivers[msg.target].DeregisteReceiver(target);
        }
    }
    protected virtual void OnDestroy()
    {
        dispose(true);
        ClearUniqueIDCache();
    }
}
