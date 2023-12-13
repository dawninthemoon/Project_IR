using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionKeyInputData
{
    public int          _presetDataIndex;
    public float        _thresholdTime = 0f;

    public bool         _consumed = false;

    public byte[]       _isValid = new byte[1];



    public ActionKeyInputData(int index)
    {
        _presetDataIndex = index;
    }
}

public class ActionKeyInputManager : Singleton<ActionKeyInputManager>
{
    private Dictionary<string, ActionKeyInputData> _actionKeyInputData;
    private ActionKeyPresetData[]           _actionKeyPresetData;
    private int                             _actinoKeyInputDataCount;
    private bool                            _isValid = false;


    public bool isValid()
    {
        return _isValid;
    }

    public void setPresetData(ActionKeyPresetData[] presetData) 
    {
        _actionKeyPresetData = presetData;
        createKeyInputData(_actionKeyPresetData);

        _isValid = _actionKeyPresetData != null;

        ControllerEx.Instance().CreateKeys();
    }

    private void createKeyInputData(ActionKeyPresetData[] presetData)
    {
        _actinoKeyInputDataCount = presetData.Length;
        _actionKeyInputData = new Dictionary<string, ActionKeyInputData>();
        for(int i = 0; i < _actinoKeyInputDataCount; ++i)
        {
            _actionKeyInputData.Add(presetData[i]._actionKeyName, new ActionKeyInputData(i));
        }
    }

    public void progress(float deltaTime)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"action key preset data is null");
            return;
        }

        ControllerEx.Instance().UpdateKeyState();

        foreach(ActionKeyInputData item in _actionKeyInputData.Values)
        {
            bool check = false;

            ActionKeyPresetData presetData = _actionKeyPresetData[item._presetDataIndex];
            switch(presetData._multiInputType)
            {
                case ActionKeyMultiInputType.Single:
                    check = singleInputCheck(presetData);
                    break;
                case ActionKeyMultiInputType.SameTime:
                    check = sameTimeInputCheck(deltaTime,item,presetData);
                    break;
            }

            if(check == false && item._consumed)
                item._consumed = false;
            item._isValid[0] = item._consumed ? (byte)0 : (check ? (byte)1 : (byte)0);
        }
    }

    public bool keyCheck(string keyName)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"action key preset data is null");
            return false;
        }

        if(_actionKeyInputData.ContainsKey(keyName) == false)
        {
            DebugUtil.assert(false,"target key is not exists: {0}",keyName);
            return false;
        }

        bool check = _actionKeyInputData[keyName]._isValid[0] == (byte)1;
        return check;
    }

    public byte[] actionKeyCheck(string keyName)
    {
        if(isValid() == false)
        {
            DebugUtil.assert(false,"action key preset data is null");
            return null;
        }

        if(_actionKeyInputData.ContainsKey(keyName) == false)
        {
            DebugUtil.assert(false,"target key is not exists: {0}",keyName);
            return null;
        }

        return _actionKeyInputData[keyName]._isValid;
    }

    private bool sameTimeInputCheck(float deltaTime, ActionKeyInputData inputData, ActionKeyPresetData presetData)
    {
        inputData._thresholdTime += deltaTime;

        switch(presetData._pressType)
        {
            case ActionKeyPressType.KeyDown:
                DebugUtil.assert(false,"error");
                return false;
            case ActionKeyPressType.KeyPressed:
            {
                if(inputData._thresholdTime >= presetData._multiInputThreshold)
                    break;

                for(int i = 0; i < presetData._keyCount[(int)ControllerEx.Instance().getCurrentControllerType()]; ++i)
                {
                    if(ControllerEx.Instance().KeyPress(getKeyCode(i,presetData)) == false)
                        return false;
                }

                inputData._thresholdTime = presetData._multiInputThreshold;

                Debug.Log("222");
                return true;
            }
            case ActionKeyPressType.KeyUp:
                DebugUtil.assert(false,"error");
                return false;
        }

        for(int i = 0; i < presetData._keyCount[(int)ControllerEx.Instance().getCurrentControllerType()]; ++i)
        {
            if(ControllerEx.Instance().KeyPress(getKeyCode(i,presetData)) == true)
                return false;
        }

        inputData._thresholdTime = 0f;

        return false;
    }

    private bool singleInputCheck(ActionKeyPresetData presetData)
    {
        switch(presetData._pressType)
        {
            case ActionKeyPressType.KeyDown:
                return ControllerEx.Instance().KeyDown(getKeyCode(0,presetData));
            case ActionKeyPressType.KeyPressed:
                return ControllerEx.Instance().KeyPress(getKeyCode(0,presetData));
            case ActionKeyPressType.KeyUp:
                return ControllerEx.Instance().KeyUp(getKeyCode(0,presetData));
        }

        DebugUtil.assert(false,"invalid pressType: {0}",presetData._pressType.ToString());
        return false;
    }

    private string getKeyCode(int index, ActionKeyPresetData presetData)
    {
        return presetData.getKey((int)ControllerEx.Instance().getCurrentControllerType(),index);
    }


}





public class ControllerEx : Singleton<ControllerEx>
{
    public enum ControllerType
    {
        KeyboardMouse = 0,
        XboxController,
        PSController
    };

    public enum KeyType
    {
        Button = 0,
        Axis,
        TwoSideAxisButton,
    };

    public enum KeyState
    {
        Down = 0,
        Press,
        Up,
        None
    };

    public class Key
    {
        public KeyType type;
        public KeyCode code;
        public KeyState state;

        public string axisName = "";
        public float axis = 0f;

        public bool side = false;

        public void ChangeState(KeyState s)
        {
            state = s;
        }

        public Key(){}

        public Key(KeyCode c)
        {
            type = KeyType.Button;
            code = c;
            state = KeyState.None;
        }

        public Key(string c)
        {
            type = KeyType.Axis;
            axisName = c;
            axis = 0f;
            state = KeyState.None;
        }

        public Key(string an, bool s) // false == left
        {
            type = KeyType.TwoSideAxisButton;
            axisName = an;
            side = s;
            axis = 0f;
            state = KeyState.None;
        }
    }

    public Dictionary<string,Key> keyBindList;// = new Dictionary<string, Key>();
    public ControllerType controller = ControllerType.KeyboardMouse;
    public Vector2 centerAxis;
    public Camera mainView;

    private Dictionary<string,Key> _keyboardBind = new Dictionary<string, Key>();
    private Dictionary<string,Key> _xboxBind = new Dictionary<string, Key>();
    private Dictionary<string,Key> _psBind = new Dictionary<string, Key>();
    private Dictionary<string,bool> _axisButtonCheck = new Dictionary<string, bool>();

    private Sprite[] _xboxKeys;
    private Sprite[] _psKeys;

    private string _controllerLists = "";

    private float _axisPushStr = 0.4f;
    private float _deviceCheckTimer = 1f;

    private Vector3 _joystickRAxis = Vector3.right;

    private static readonly KeyCode[] keyCodes = System.Enum.GetValues(typeof(KeyCode))
                                                 .Cast<KeyCode>()
                                                 .Where(k => ((int)k < (int)KeyCode.Mouse2))
                                                 .ToArray();

    public bool KeyDown(string key) {return KeyCheck(key) == KeyState.Down;}
    public bool KeyPress(string key) {return KeyCheck(key) == KeyState.Press;}
    public bool KeyUp(string key) {return KeyCheck(key) == KeyState.Up;}

    public ControllerType getCurrentControllerType() {return controller;}

    public bool _keyBreak = false;

    public KeyState KeyCheck(string key)
    {
        if(keyBindList.ContainsKey(key))
        {
            return keyBindList[key].state;
        }
        else
        {
            //Debug.Log("Key does not exists");
            return KeyState.None;
        }
    }

    public void addKeyboardBind(string name, KeyCode keyCode)
    {
        if(_keyboardBind.ContainsKey(name) == true)
            return;
        _keyboardBind.Add(name,new Key(keyCode));
    }

    public void addXboxBind(string name, KeyCode keyCode)
    {
        if(_xboxBind.ContainsKey(name) == true)
            return;
        _xboxBind.Add(name,new Key(keyCode));
    }

    public void addXboxBind(string name, string keyCode)
    {
        if(_xboxBind.ContainsKey(name) == true)
            return;
        _xboxBind.Add(name,new Key(keyCode));
    }

    public void addXboxBind(string name, string keyCode, bool side)
    {
        if(_xboxBind.ContainsKey(name) == true)
            return;
        _xboxBind.Add(name,new Key(keyCode,side));
    }

    public void addPSBind(string name, KeyCode keyCode)
    {
        if(_psBind.ContainsKey(name) == true)
            return;
        _psBind.Add(name,new Key(keyCode));
    }

    public void addPSBind(string name, string keyCode, bool side)
    {
        if(_psBind.ContainsKey(name) == true)
            return;
        _psBind.Add(name,new Key(keyCode,side));
    }
    
    public void CreateKeyBindDic()
    {
        _keyboardBind.Add("MainAttack",new Key(KeyCode.W));
        _keyboardBind.Add("DriveAttack",new Key(KeyCode.Mouse1));
        _keyboardBind.Add("WeaponChange",new Key(KeyCode.R));
        _keyboardBind.Add("Cancel",new Key(KeyCode.Tab));
        _keyboardBind.Add("Left",new Key(KeyCode.LeftArrow));
        _keyboardBind.Add("Right",new Key(KeyCode.RightArrow));
        _keyboardBind.Add("Up",new Key(KeyCode.UpArrow));
        _keyboardBind.Add("Down",new Key(KeyCode.DownArrow));
        _keyboardBind.Add("Option",new Key(KeyCode.Escape));

        _xboxBind.Add("MainAttack",new Key(KeyCode.JoystickButton0));
        _xboxBind.Add("DriveAttack",new Key("XBoxRightTrigger"));
        _xboxBind.Add("WeaponChange",new Key(KeyCode.JoystickButton2));
        _xboxBind.Add("Cancel",new Key(KeyCode.JoystickButton1));

        _psBind.Add("MainAttack",new Key(KeyCode.JoystickButton1));
        _psBind.Add("DriveAttack",new Key("PSRightTrigger"));
        _psBind.Add("WeaponChange",new Key(KeyCode.JoystickButton0));
        _psBind.Add("Cancel",new Key(KeyCode.JoystickButton2));
        
        SaveKeyBindInfo();
    }

    public void LoadKeyBindInfo()
    {
        // var data = IOManager.ReadiniFile("keyMapping.ini");
        // if(data == null)
        // {
        //     CreateKeyBindDic();
        //     return;
        // }

        // foreach(var block in data)
        // {
        //     ControllerType type = block.Key == "km" ? ControllerType.KeyboardMouse : 
        //                         (block.Key == "xbox" ? ControllerType.XboxController : 
        //                         (block.Key == "ps" ? ControllerType.PSController : ControllerType.PSController));
        //     if(block.Value == null)
        //         continue;
            
        //     foreach(var keyData in block.Value)
        //     {
        //         Key key = new Key();
        //         var property = keyData.data.Split(',');
        //         var keyType = (KeyType)int.Parse(property[0]);
        //         key.type = keyType;

        //         if(keyType == KeyType.Button)
        //         {
        //             key.code = (KeyCode)int.Parse(property[1]);
        //         }
        //         else if(keyType == KeyType.Axis)
        //         {
        //             key.axisName = property[2];
        //         }
        //         else if(keyType == KeyType.TwoSideAxisButton)
        //         {
        //             key.axisName = property[2];
        //             key.side = property[3] == "0";
        //         }

        //         BindLoadedKey(keyData.title,type,key);
        //     }
        // }

        // data = null;
    }

    public void SaveKeyBindInfo()
    {
        List<string> keys = new List<string>();
        keys.Add("[km]");
        foreach(var key in _keyboardBind)
        {
            keys.Add(key.Key + "=" + (int)key.Value.type + "," +
                                    (int)key.Value.code + "," +
                                    key.Value.axisName + "," + 
                                    (key.Value.side ? "0" : "1"));
        }
        keys.Add("[xbox]");
        foreach(var key in _xboxBind)
        {
            keys.Add(key.Key + "=" + (int)key.Value.type + "," +
                                    (int)key.Value.code + "," +
                                    key.Value.axisName + "," + 
                                    (key.Value.side ? "0" : "1"));
        }
        keys.Add("[ps]");
        foreach(var key in _psBind)
        {
            keys.Add(key.Key + "=" + (int)key.Value.type + "," +
                                    (int)key.Value.code + "," +
                                    key.Value.axisName + "," + 
                                    (key.Value.side ? "0" : "1"));
        }

        Debug.Log("Save");
    }

    public void BindLoadedKey(string keyName,ControllerType ct, Key key)
    {
        if(ct == ControllerType.KeyboardMouse)
        {
            _keyboardBind[keyName] = key;
        }
        else if((ct == ControllerType.XboxController))
        {
            _xboxBind[keyName] = key;
        }
        else if((ct == ControllerType.PSController))
        {
            _psBind[keyName] = key;
        }
    }

    public void DeleteBindInfo(Key key, ControllerType con)
    {
        string n = "";
        foreach(var item in keyBindList)
        {
            if(item.Value.axisName == key.axisName && item.Value.code == key.code && item.Value.type == key.type && item.Value.side == key.side)
            {
                n = item.Key;
                break;
            }
        }

        if(n == "")
        {
            Debug.Log("wath fucfucfucfuck");
            return;
        }

        keyBindList[n] = null;

        if(con == ControllerType.KeyboardMouse)
            _keyboardBind.Remove(n);
        if(con == ControllerType.XboxController)
            _xboxBind.Remove(n);
        if(con == ControllerType.PSController)
            _psBind.Remove(n);
    
        Debug.Log("bind key delete : " + n);
    }

    public void KeyBind(string keyName,ControllerType ct, Key key,Key prevKey)
    {
        //_keyBreak = true;

        var n = FindOverlapKey(key);

        if(n != null)
        {
            Debug.Log(n);
            Debug.Log(keyName);
        }
        
        // if(keyBindList.ContainsKey(keyName))
        // {
            key.state = KeyState.Press;
            keyBindList[keyName] = key;

            if(ct == ControllerType.KeyboardMouse)
            {
                _keyboardBind[keyName] = key;
            }
            else if(ct == ControllerType.XboxController)
            {
                _xboxBind[keyName] = key;
            }
            else if(ct == ControllerType.PSController)
            {
                _psBind[keyName] = key;
            }

        if(n != null && n != keyName)
        {
            prevKey.state = KeyState.Press;
            keyBindList[n] = prevKey;

            if(ct == ControllerType.KeyboardMouse)
            {
                _keyboardBind[n] = prevKey;
            }
            else if(ct == ControllerType.XboxController)
            {
                _xboxBind[n] = prevKey;
            }
            else if(ct == ControllerType.PSController)
            {
                _psBind[n] = prevKey;
            }

        }
        // }
        // else
        // {
        //     Debug.Log("KeyName Error : " + keyName);
        // }
    }

    public string FindOverlapKey(Key key)
    {
        foreach(var item in keyBindList)
        {
            if(item.Value.axisName == key.axisName && item.Value.code == key.code && item.Value.type == key.type && item.Value.side == key.side)
            {
                return item.Key;
            }
        }

        return null;
    }

    public void CreateKeys()
    {
        InputDeviceCheck();
        InputDevieKeyBind();
    }

    public void InputDevieKeyBind()
    {
        switch(controller)
        {
            case ControllerType.KeyboardMouse:
            keyBindList = _keyboardBind;
            break;
            case ControllerType.XboxController:
            keyBindList = _xboxBind;
            break;
            case ControllerType.PSController:
            keyBindList = _psBind;
            break;
        }
    }

    public void CheckCurrentInputDevice()
    {
        if(Input.anyKey)
        {
            bool joy = false;
            for(int i = 0; i < 14; ++i)
            {
                if(Input.GetKey(KeyCode.JoystickButton0 + i))
                {
                    joy = true;
                    break;
                }
            }

            if(controller != ControllerType.KeyboardMouse && !joy)
            {
                controller = ControllerType.KeyboardMouse;
                InputDevieKeyBind();
            }
            else if(controller == ControllerType.KeyboardMouse && joy)
            {
                _controllerLists = "";
                InputDeviceCheck();
                InputDevieKeyBind();
            }
            
        }
    }

    public void InputDeviceCheck()
    {
        string[] con = Input.GetJoystickNames();
        if(con != null && con.Length > 0 && con[0] != _controllerLists)
        {
            ControllerType type = ControllerType.KeyboardMouse;

            if(con[0].Contains("Xbox"))
                type = ControllerType.XboxController;
            else
                type = ControllerType.PSController;

            _controllerLists = con[0];

            controller = type;
        }
    }

    public void BindKeys(string n, KeyCode k)
    {
        keyBindList[n].code = k;
        keyBindList[n].state = KeyState.None;
    }

    public void UpdateKeyState()
    {
        _deviceCheckTimer -= Time.deltaTime;
        if(_deviceCheckTimer <= 0f)
        {
            var con = controller;
            InputDeviceCheck();
            if(con != controller)
                InputDevieKeyBind();
            
            _deviceCheckTimer = 1f;
        }

        CheckCurrentInputDevice();

        if(_keyBreak)
        {
            return;
        }

        foreach(var key in keyBindList)
        {
            var k = key.Value;

            if(k == null)
                continue;
            
            if(k.type == KeyType.Button)
            {
                bool down = Input.GetKey(k.code);
                
                if(down && k.state == KeyState.Down)
                    k.state = KeyState.Press;
                else if(down && k.state == KeyState.None)
                    k.state = KeyState.Down;
                else if(!down && (k.state == KeyState.Press || k.state == KeyState.Down))
                    k.state = KeyState.Up;
                else if(k.state == KeyState.Up)
                    k.state = KeyState.None;
            }
            else if(k.type == KeyType.Axis)
            {
                k.axis = Input.GetAxis(k.axisName);
                bool down = k.axis >= _axisPushStr;

                if(down && k.state == KeyState.Down)
                    k.state = KeyState.Press;
                else if(down && k.state == KeyState.None)
                    k.state = KeyState.Down;
                else if(!down && (k.state == KeyState.Press || k.state == KeyState.Down))
                    k.state = KeyState.Up;
                else if(k.state == KeyState.Up)
                    k.state = KeyState.None;
            }
            else if(k.type == KeyType.TwoSideAxisButton)
            {
                k.axis = Input.GetAxis(k.axisName);
                bool down = k.side ? k.axis > 0f : (k.axis < 0f);

                if(down && k.state == KeyState.Down)
                    k.state = KeyState.Press;
                else if(down && k.state == KeyState.None)
                    k.state = KeyState.Down;
                else if(!down && (k.state == KeyState.Press || k.state == KeyState.Down))
                    k.state = KeyState.Up;
                else if(k.state == KeyState.Up)
                    k.state = KeyState.None;
            }
        }

        centerAxis = controller == ControllerType.KeyboardMouse ? GetScreenCenterAxis() : GetJoystickAxis();
    }

    public Key GetBindedKeyInfo(string key, ControllerType type)
    {
        if(type == ControllerType.KeyboardMouse)
        {
            if(_keyboardBind.ContainsKey(key))
                return _keyboardBind[key];
        }
        else if(type == ControllerType.XboxController)
        {
            if(_xboxBind.ContainsKey(key))
                return _xboxBind[key];
        }
        else if(type == ControllerType.PSController)
        {
            if(_psBind.ContainsKey(key))
                return _psBind[key];
        }

        return null;
    }

    public Key GetCurrentKeyInput()
    {
        if(controller == ControllerType.KeyboardMouse)
            return GetCurrentKeyboardMouseInput();
        else if(controller == ControllerType.XboxController)
            return GetCurrentXboxControllerInput();
        else if(controller == ControllerType.PSController)
            return GetCurrentPSContollerInput();
        
        return null;
    }

    public string GetKeyboardString(string type)
    {
        var s = _keyboardBind[type].code.ToString();
        s = s == "Mouse0" ? "LMB" : (s == "Mouse1" ? "RMB" : s);

        return s;
    }

    public Sprite GetXboxGraphic(string s)
    {
        return GetXboxGraphic(_xboxBind[s]);
    }

    public Sprite GetPSGraphic(string s)
    {
        return GetPSGraphic(_xboxBind[s]);
    }

    public Sprite GetXboxGraphic(ControllerEx.Key key)
    {
        if(key.type == ControllerEx.KeyType.Button)
        {
            if(key.code == KeyCode.JoystickButton0)
                return _xboxKeys[6];
            else if(key.code == KeyCode.JoystickButton1)
                return _xboxKeys[7];
            else if(key.code == KeyCode.JoystickButton2)
                return _xboxKeys[8];
            else if(key.code == KeyCode.JoystickButton3)
                return _xboxKeys[9];
            else if(key.code == KeyCode.JoystickButton4)
                return _xboxKeys[11];
            else if(key.code == KeyCode.JoystickButton5)
                return _xboxKeys[12];
            else if(key.code == KeyCode.JoystickButton6)
                return _xboxKeys[15];
            else if(key.code == KeyCode.JoystickButton7)
                return _xboxKeys[14];
            else if(key.code == KeyCode.JoystickButton8)
                return _xboxKeys[0];
            else if(key.code == KeyCode.JoystickButton9)
                return _xboxKeys[1];
        }
        else if(key.type == ControllerEx.KeyType.Axis)
        {
            if(key.axisName == "XBoxLeftTrigger")
                return _xboxKeys[10];
            else if(key.axisName == "XBoxRightTrigger")
            {
                return _xboxKeys[13];
            }
        }
        else if(key.type == ControllerEx.KeyType.TwoSideAxisButton)
        {
            if(key.axisName == "XBoxDPadHorizontal")
                return _xboxKeys[key.side ? 5 : 4];
            else if(key.axisName == "XBoxDPadVertical")
                return _xboxKeys[key.side ? 2 : 3];
        }

        return null;
    }
    public Sprite GetPSGraphic(ControllerEx.Key key)
    {
        if(key.type == ControllerEx.KeyType.Button)
        {
            if(key.code == KeyCode.JoystickButton0)
                return _psKeys[8];
            else if(key.code == KeyCode.JoystickButton1)
                return _psKeys[6];
            else if(key.code == KeyCode.JoystickButton2)
                return _psKeys[7];
            else if(key.code == KeyCode.JoystickButton3)
                return _psKeys[9];
            else if(key.code == KeyCode.JoystickButton4)
                return _psKeys[10];
            else if(key.code == KeyCode.JoystickButton5)
                return _psKeys[11];
            else if(key.code == KeyCode.JoystickButton8)
                return _psKeys[14];
            else if(key.code == KeyCode.JoystickButton9)
                return _psKeys[15];
            else if(key.code == KeyCode.JoystickButton10)
                return _psKeys[0];
            else if(key.code == KeyCode.JoystickButton11)
                return _psKeys[1];
        }
        else if(key.type == ControllerEx.KeyType.Axis)
        {
            if(key.axisName == "PSLeftTrigger")
                return _psKeys[12];
            else if(key.axisName == "PSRightTrigger")
                return _psKeys[13];
        }
        else if(key.type == ControllerEx.KeyType.TwoSideAxisButton)
        {
            if(key.axisName == "PSDPadHorizontal")
                return _psKeys[key.side ? 5 : 4];
            else if(key.axisName == "PSDPadVertical")
                return _psKeys[key.side ? 2 : 3];
        }

        return null;
    }

    public Key GetCurrentKeyboardMouseInput()
    {
        if (Input.anyKeyDown)
        {
            for (int i = 0; i < keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    Key key = new Key(keyCodes[i]);
                    return key;
                }
            }
        }
        
        return null;
    }

    public Key GetCurrentXboxControllerInput()
    {
        if (Input.anyKeyDown)
        {
            if(Input.GetKeyDown(KeyCode.JoystickButton0)) { return new Key(KeyCode.JoystickButton0);}
            if(Input.GetKeyDown(KeyCode.JoystickButton1)) { return new Key(KeyCode.JoystickButton1);}
            if(Input.GetKeyDown(KeyCode.JoystickButton2)) { return new Key(KeyCode.JoystickButton2);}
            if(Input.GetKeyDown(KeyCode.JoystickButton3)) { return new Key(KeyCode.JoystickButton3);}
            if(Input.GetKeyDown(KeyCode.JoystickButton4)) { return new Key(KeyCode.JoystickButton4);}
            if(Input.GetKeyDown(KeyCode.JoystickButton5)) { return new Key(KeyCode.JoystickButton5);}
            if(Input.GetKeyDown(KeyCode.JoystickButton6)) { return new Key(KeyCode.JoystickButton6);}
            if(Input.GetKeyDown(KeyCode.JoystickButton7)) { return new Key(KeyCode.JoystickButton7);}
            if(Input.GetKeyDown(KeyCode.JoystickButton8)) { return new Key(KeyCode.JoystickButton8);}
            if(Input.GetKeyDown(KeyCode.JoystickButton9)) { return new Key(KeyCode.JoystickButton9);}
        }

        if(Input.GetAxis("XBoxDPadHorizontal") > 0f) {return new Key("XBoxDPadHorizontal",true);}
        if(Input.GetAxis("XBoxDPadHorizontal") < 0f) {return new Key("XBoxDPadHorizontal",false);}
        if(Input.GetAxis("XBoxDPadVertical") > 0f) {return new Key("XBoxDPadVertical",true);}
        if(Input.GetAxis("XBoxDPadVertical") < 0f) {return new Key("XBoxDPadVertical",false);}
        if(Input.GetAxis("XBoxLeftTrigger") > _axisPushStr) {return new Key("XBoxLeftTrigger");}
        if(Input.GetAxis("XBoxRightTrigger") > _axisPushStr) {return new Key("XBoxRightTrigger");}

        return null;
    }

    public Key GetCurrentPSContollerInput()
    {
        if (Input.anyKeyDown)
        {
            if(Input.GetKeyDown(KeyCode.JoystickButton0)) { return new Key(KeyCode.JoystickButton0);}
            if(Input.GetKeyDown(KeyCode.JoystickButton1)) { return new Key(KeyCode.JoystickButton1);}
            if(Input.GetKeyDown(KeyCode.JoystickButton2)) { return new Key(KeyCode.JoystickButton2);}
            if(Input.GetKeyDown(KeyCode.JoystickButton3)) { return new Key(KeyCode.JoystickButton3);}
            if(Input.GetKeyDown(KeyCode.JoystickButton4)) { return new Key(KeyCode.JoystickButton4);}
            if(Input.GetKeyDown(KeyCode.JoystickButton5)) { return new Key(KeyCode.JoystickButton5);}
            if(Input.GetKeyDown(KeyCode.JoystickButton8)) { return new Key(KeyCode.JoystickButton8);}
            if(Input.GetKeyDown(KeyCode.JoystickButton9)) { return new Key(KeyCode.JoystickButton9);}
            if(Input.GetKeyDown(KeyCode.JoystickButton10)) { return new Key(KeyCode.JoystickButton10);}
            if(Input.GetKeyDown(KeyCode.JoystickButton11)) { return new Key(KeyCode.JoystickButton11);}
        }

        if(Input.GetAxis("PSDPadHorizontal") > 0f) {return new Key("PSDPadHorizontal",true);}
        if(Input.GetAxis("PSDPadHorizontal") < 0f) {return new Key("PSDPadHorizontal",false);}
        if(Input.GetAxis("PSDPadVertical") > 0f) {return new Key("PSDPadVertical",true);}
        if(Input.GetAxis("PSDPadVertical") < 0f) {return new Key("PSDPadVertical",false);}
        if(Input.GetAxis("PSLeftTrigger") > _axisPushStr) {return new Key("PSLeftTrigger");}
        if(Input.GetAxis("PSRightTrigger") > _axisPushStr) {return new Key("PSRightTrigger");}

        return null;
    }

    public Vector2 GetWorldScreenCenterAxis(Vector3 c)
    {
        Vector2 pos = Input.mousePosition;
        Vector2 center = mainView.WorldToScreenPoint(c);

        return (pos - center).normalized;
    }

    public Vector2 GetScreenCenterAxis()
    {
        Vector2 pos = Input.mousePosition;
        Vector2 center = new Vector2(Screen.width / 2f,Screen.height / 2f);

        return (pos - center).normalized;
    }

    public float GetCenterDistance()
    {
        Vector2 pos = Input.mousePosition;
        Vector2 center = new Vector2(Screen.width / 2f,Screen.height / 2f);

        return Vector2.Distance(pos,center);
    }

    public Vector2 GetJoystickAxis()
    {
        var axis = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
        if(MathEx.abs(axis.magnitude) >= _axisPushStr)
            return axis.normalized;
        else
            return Vector2.zero;
    }

    public Vector3 getJoystickAxisR(Vector3 position)
    {
        Vector3 axis = Vector2.zero;
        if(controller == ControllerType.XboxController)
            axis = new Vector3(Input.GetAxis("XBoxRightHorizontal"),Input.GetAxis("XBoxRightVertical"));
        else if(controller == ControllerType.PSController)
            axis = new Vector3(Input.GetAxis("PSRightHorizontal"),Input.GetAxis("PSRightVertical"));
        else if(controller == ControllerType.KeyboardMouse)
            axis = (MathEx.deleteZ(Camera.main.ScreenToWorldPoint(Input.mousePosition)) - position).normalized;

        if(MathEx.abs(axis.magnitude) >= _axisPushStr)
        {
            _joystickRAxis = axis.normalized;
            return axis.normalized;
        }
        else
            return _joystickRAxis;
    }

    public void SetMainViewCamera(Camera cam)
    {
        mainView = cam;
    }
}