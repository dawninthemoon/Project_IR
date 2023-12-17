using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameEditorMaster : MonoBehaviour
{
    public static GameEditorMaster _instance;
    public bool _actionDebugAll = false;
    public bool _aiDebugAll = false;
    public bool _statusDebugAll = false;
    public bool _animationDebugAll = false;
    public bool _soundDebugAll = false;

    public float _debugCameraMoveSpeed = 2f;
    public float _debugCameraMoveSpeedMultiflier = 4f;

    public Slider   _timeMagnitudeSlider;
    public Text     _timeMagnitudeText;

    public Text     _debugCharacterNameText;
    public Toggle   _actionDebugToggle;
    public Toggle   _aiDebugToggle;
    public Toggle   _statusDebugToggle;
    public Toggle   _animationDebugToggle;
    public Toggle   _soundDebugToggle;

    public RectTransform _stageSelectorContent;
    public GameObject _stageSelectorButtonItemPrefab;

    private static string _hotKey_EditorOnOff = "EditorHotKey_EditorOnOff";
    private static string _hotKey_UpdateFrame = "EditorHotKey_UpdateFrame";
    private static string _hotKey_StageSelectorOnOff = "EditorHotKey_StageSelectorOnOff";

    private List<EditorWindowBase> _currentWindows = new List<EditorWindowBase>();
    private GameObject _editorParent;
    private GameObject _stageSelectorParent;
    private EditorWindowBase _currentFocusWindow;
    private bool _activeEditor = false;

    private GameEntityBase _currentlySelectedEntity;

    private void Awake()
    {
        _instance = this;

        fillStageSelectorItem();
    }

    private void Start()
    {
        _editorParent = transform.Find("Editors").gameObject;
        _stageSelectorParent = transform.Find("StageSelector").gameObject;
    }

    private void Update()
    {
        updateHotkeysAlways();

        if(_activeEditor == false)
            return;

        updateHotkeysEditor();

        _currentFocusWindow?.updateHotKey();

        foreach(EditorWindowBase window in _currentWindows)
        {
            if(window == _currentFocusWindow)
                continue;

            window.updateHotKey();
        }

        _currentFocusWindow?.mainUpdate(Time.deltaTime);

        foreach(EditorWindowBase window in _currentWindows)
        {
            if(window == _currentFocusWindow)
                continue;

            window.mainUpdate(Time.deltaTime);
        }

        float debugCameraMoveSpeed = _debugCameraMoveSpeed;
        if(Input.GetKey(KeyCode.LeftShift))
            debugCameraMoveSpeed *= _debugCameraMoveSpeedMultiflier;

        if(Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 position = Camera.main.transform.position;
            Camera.main.transform.position = position + Vector3.up * debugCameraMoveSpeed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 position = Camera.main.transform.position;
            Camera.main.transform.position = position + Vector3.down * debugCameraMoveSpeed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 position = Camera.main.transform.position;
            Camera.main.transform.position = position + Vector3.left * debugCameraMoveSpeed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 position = Camera.main.transform.position;
            Camera.main.transform.position = position + Vector3.right * debugCameraMoveSpeed * Time.deltaTime;
        }

#if UNITY_EDITOR
        updateCharacterPick();
#endif
    }

    void updateHotkeysAlways()
    {
        if(ActionKeyInputManager.Instance().keyCheck(_hotKey_EditorOnOff))
        {
            if(_activeEditor)
                editorOff();
            else
                editorOn();
        }
        else if(ActionKeyInputManager.Instance().keyCheck(_hotKey_UpdateFrame))
        {
            updateFrame();
        }
        else if(ActionKeyInputManager.Instance().keyCheck(_hotKey_StageSelectorOnOff))
        {
            stageSelectorOnOff();
        }
    }

    void updateHotkeysEditor()
    {

    }

    void stageSelectorOnOff()
    {
        bool active = !_stageSelectorParent.activeInHierarchy;
        _stageSelectorParent?.SetActive(active);
        Cursor.visible = active;
    }

    void fillStageSelectorItem()
    {
        StageData[] stageData = ResourceContainerEx.Instance().GetStageDataAll("StageData/");
        if(stageData == null)
            return;

        Vector2 sizeDelta = _stageSelectorContent.sizeDelta;
        sizeDelta.y = 50f + 45f * (float)stageData.Length;
        _stageSelectorContent.sizeDelta = sizeDelta;

        int offsetIndex = 0;

        for(int index = 0; index < stageData.Length; ++index)
        {
            if(stageData[index]._isMiniStage)
            {
                ++offsetIndex;
                continue;
            }

            GameObject stageButton = Instantiate(_stageSelectorButtonItemPrefab);
            RectTransform rectTransform = stageButton.GetComponent<RectTransform>();
            rectTransform.SetParent(_stageSelectorContent,false);
            rectTransform.anchoredPosition = new Vector3(0f,-(40f + 45f * (float)(index - offsetIndex)));

            int indexPointer = index;
            stageButton.GetComponentInChildren<Text>().text = stageData[index]._stageName;
            stageButton.GetComponent<Button>().onClick.AddListener(()=>{
                if(_activeEditor)
                    editorOff();

                StageProcessor.Instance().stopStage();
                StageProcessor.Instance().startStage(stageData[indexPointer],Vector3.zero);
                stageSelectorOnOff();
                
            });
        }
    }
#if UNITY_EDITOR
    public void updateCharacterPick()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) == false)
            return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = MathEx.deleteZ(mousePosition);
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        SceneCharacterManager sceneCharacterManager = SceneCharacterManager._managerInstance as SceneCharacterManager;
        var characters = sceneCharacterManager.getCurrentlyEnabledCharacters();

        //bool targetFinded = false;

        foreach(var character in characters.Values)
        {
            BoundBox boundBox = character.getCollisionInfo().getBoundBox();
            if(boundBox.intersection(mousePosition))
            {
                Selection.activeGameObject = character.gameObject;
                EditorGUIUtility.PingObject(character.gameObject);

                _debugCharacterNameText.text = character.gameObject.name;
                _currentlySelectedEntity = null;
                setTargetDebugSwitch();

                _currentlySelectedEntity = character;
                setTargetDebugSwitch();

                //targetFinded = true;
                break;
            }
        }
    }
#endif
    public void setMainWindow(EditorWindowBase window)
    {
        _currentFocusWindow = window;
    }

    private void updateFrame()
    {
        editorOn();
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_updateFrame,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);
    }

    public void updateTimeMagnitude()
    {
        GlobalTimer.Instance().setDebugTimeRatio(_timeMagnitudeSlider.value);
        _timeMagnitudeText.text = "Magnitude: " + _timeMagnitudeSlider.value.ToString();
    }

    public void setActionDebug() {_actionDebugAll = !_actionDebugAll;}
    public void setAiDebug() {_aiDebugAll = !_aiDebugAll;}
    public void setStatusDebug() {_statusDebugAll = !_statusDebugAll;}
    public void setAnimationDebug() {_animationDebugAll = !_animationDebugAll;}
    public void setSoundDebug() {_soundDebugAll = !_soundDebugAll;}

    public void setTargetDebugSwitch(bool clear = false)
    {
        _actionDebugToggle.isOn = clear ? false : (_currentlySelectedEntity != null ? _currentlySelectedEntity._actionDebug : false);
        _aiDebugToggle.isOn = clear ? false : (_currentlySelectedEntity != null ? _currentlySelectedEntity._aiDebug : false);
        _statusDebugToggle.isOn = clear ? false : (_currentlySelectedEntity != null ? _currentlySelectedEntity._statusDebug : false);
        _animationDebugToggle.isOn = clear ? false : (_currentlySelectedEntity != null ? _currentlySelectedEntity._animationDebug : false);
        _soundDebugToggle.isOn = clear ? false : (_currentlySelectedEntity != null ? _currentlySelectedEntity._soundDebug : false);
    }


    public void setTargetActionDebug() {if(_currentlySelectedEntity==null) return; _currentlySelectedEntity._actionDebug = _actionDebugToggle.isOn;}
    public void setTargetAiDebug() {if(_currentlySelectedEntity==null) return; _currentlySelectedEntity._aiDebug = _aiDebugToggle.isOn;}
    public void setTargetStatusDebug() {if(_currentlySelectedEntity==null) return; _currentlySelectedEntity._statusDebug =  _statusDebugToggle.isOn;}
    public void setTargetAnimationDebug() {if(_currentlySelectedEntity==null) return; _currentlySelectedEntity._animationDebug = _animationDebugToggle.isOn;}
    public void setTargetSoundDebug() {if(_currentlySelectedEntity==null) return; _currentlySelectedEntity._soundDebug = _soundDebugToggle.isOn;}

    public bool isActionDebug() {return _actionDebugAll;}
    public bool isAiDebug() {return _aiDebugAll;}
    public bool isStatusDebug() {return _statusDebugAll;}
    public bool isAnimationDebug() {return _animationDebugAll;}
    public bool isSoundDebug() {return _soundDebugAll;}

    private void editorOn()
    {
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_pauseUpdate,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);

        _activeEditor = true;
        _editorParent?.SetActive(true);

        Cursor.visible = true;
    }

    private void editorOff()
    {
        var msg = MessagePool.GetMessage();
        msg.Set(MessageTitles.system_playUpdate,0,null,null);
        MasterManager.instance.SendMessageDirectInMaster(msg);

        _activeEditor = false;
        _editorParent?.SetActive(false);

        Cursor.visible = false;
    }
}
