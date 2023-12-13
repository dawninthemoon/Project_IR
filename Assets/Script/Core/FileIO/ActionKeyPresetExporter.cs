using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;
using UnityEngine;
using ICSharpCode.WpfDesign.XamlDom;

public static class ActionKeyPresetDataLoader
{
    private static Dictionary<string, KeyCode> XBoxKeyCodeBind = new Dictionary<string, KeyCode>
    {
        {"A", KeyCode.JoystickButton0},
        {"B", KeyCode.JoystickButton1},
        {"X", KeyCode.JoystickButton2},
        {"Y", KeyCode.JoystickButton3},
        {"LB", KeyCode.JoystickButton4},
        {"RB", KeyCode.JoystickButton5},
        {"View", KeyCode.JoystickButton6},
        {"Menu", KeyCode.JoystickButton7},
        {"LClick", KeyCode.JoystickButton8},
        {"RClick", KeyCode.JoystickButton9},
    };

    private static Dictionary<string, KeyCode> PSKeyCodeBind = new Dictionary<string, KeyCode>
    {
        {"X", KeyCode.JoystickButton1},
        {"O", KeyCode.JoystickButton2},
        {"Sq", KeyCode.JoystickButton0},
        {"Tr", KeyCode.JoystickButton3},
        {"L1", KeyCode.JoystickButton4},
        {"R1", KeyCode.JoystickButton5},
        {"Touchpad", KeyCode.JoystickButton13},
        {"Option", KeyCode.JoystickButton9},
        {"LClick", KeyCode.JoystickButton10},
        {"RClick", KeyCode.JoystickButton11},
    };

    private static Dictionary<string, string> XBoxAxisBind = new Dictionary<string, string>
    {
        {"LStickHorizontal","Horizontal"},
        {"LStickVertical","Vertical"},
        {"RStickHorizontal","XBoxRightHorizontal"},
        {"RStickVertical","XBoxRightVertical"},
        {"LTrigger","XBoxLeftTrigger"},
        {"RTrigger","XBoxRightTrigger"},
    };

    private static Dictionary<string, string> XBoxAxisButtonBind = new Dictionary<string, string>
    {
        {"DPADLeft","XBoxDPadVertical false"},
        {"DPADRight","XBoxDPadVertical true"},
        {"DPADUp","XBoxDPadHorizontal false"},
        {"DPADDown","XBoxDPadHorizontal true"},
    };

    private static Dictionary<string, string> PSAxisBind = new Dictionary<string, string>
    {
        {"LStickHorizontal","Horizontal"},
        {"LStickVertical","Vertical"},
        {"RStickHorizontal","PSRightHorizontal"},
        {"RStickVertical","PSRightVertical"},
        {"LTrigger","PSLeftTrigger"},
        {"RTrigger","PSRightTrigger"},
    };

    private static Dictionary<string, string> PSAxisButtonBind = new Dictionary<string, string>
    {
        {"DPADLeft","PSDPadVertical false"},
        {"DPADRight","PSDPadVertical true"},
        {"DPADUp","PSDPadHorizontal false"},
        {"DPADDown","PSDPadHorizontal true"},
    };

    public static ActionKeyPresetData[] readFromXML(string path)
    {
        PositionXmlDocument xmlDoc = new PositionXmlDocument();
        try
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(path,readerSettings))
            {
                xmlDoc.Load(reader);
            }
        }
        catch(System.Exception ex)
        {
            DebugUtil.assert(false,"xml load exception : {0}",ex.Message);
            return null;
        }
        
        if(xmlDoc.HasChildNodes == false)
        {
            DebugUtil.assert(false,"xml is empty");
            return null;
        }

        List<ActionKeyPresetData> presetDataList = new List<ActionKeyPresetData>();

        XmlNodeList projectileNodes = xmlDoc.FirstChild.ChildNodes;
        for(int nodeIndex = 0; nodeIndex < projectileNodes.Count; ++nodeIndex)
        {
            ActionKeyPresetData presetData = readPresetData(projectileNodes[nodeIndex]);
            if(presetData == null)
                return null;

            presetDataList.Add(presetData);
        }

        return presetDataList.ToArray();
    }

    private static ActionKeyPresetData readPresetData(XmlNode node)
    {
        ActionKeyPresetData presetData = new ActionKeyPresetData();
        presetData._actionKeyName = node.Name;

        XmlAttributeCollection buffDataNodes = node.Attributes;

        for(int i = 0; i < buffDataNodes.Count; ++i)
        {
            string attrName = buffDataNodes[i].Name;
            string attrValue = buffDataNodes[i].Value;

            if(attrName == "PressType")
                presetData._pressType = (ActionKeyPressType)System.Enum.Parse(typeof(ActionKeyPressType), attrValue);
            else if(attrName == "MultiInputType")
                presetData._multiInputType = (ActionKeyMultiInputType)System.Enum.Parse(typeof(ActionKeyMultiInputType), attrValue);
            else if(attrName == "Threshold")
                presetData._multiInputThreshold = XMLScriptConverter.valueToFloatExtend(attrValue);
            else if(attrName == "Key_KM")
            {
                string[] keyList = attrValue.Split(' ');
                readKeyboardKeyList(keyList);

                int index = (int)ControllerEx.ControllerType.KeyboardMouse;
                presetData._keys[index] = keyList;
                presetData._keyCount[index] = keyList.Length;
            }
            else if(attrName == "Key_XBOX")
            {
                string[] keyList = attrValue.Split(' ');
                readXBOXKeyList(keyList);

                int index = (int)ControllerEx.ControllerType.XboxController;
                presetData._keys[index] = keyList;
                presetData._keyCount[index] = keyList.Length;
            }
            else if(attrName == "Key_PS")
            {
                string[] keyList = attrValue.Split(' ');
                readPSKeyList(keyList);

                int index = (int)ControllerEx.ControllerType.PSController;
                presetData._keys[index] = keyList;
                presetData._keyCount[index] = keyList.Length;
            }
            else
            {
                DebugUtil.assert(false, "invalid attribute name from buffInfo: {0}",attrName);
                continue;
            }

        }


        return presetData;
    }

    private static void readPSKeyList(string[] keyListString)
    {
        for(int i = 0; i < keyListString.Length; ++i)
        {
            if(PSAxisBind.ContainsKey(keyListString[i]))
                ControllerEx.Instance().addXboxBind(keyListString[i],PSAxisBind[keyListString[i]]);
            else if(PSKeyCodeBind.ContainsKey(keyListString[i]))
                ControllerEx.Instance().addXboxBind(keyListString[i],PSKeyCodeBind[keyListString[i]]);
            else if(PSAxisButtonBind.ContainsKey(keyListString[i]))
            {
                string[] split = PSAxisButtonBind[keyListString[i]].Split(' ');
                ControllerEx.Instance().addXboxBind(keyListString[i],split[0],bool.Parse(split[1]));
            }
            else
            {
                DebugUtil.assert(false,"invalid PS key name: {0}",keyListString[i]);
                return;
            }
        }
    }

    private static void readXBOXKeyList(string[] keyListString)
    {
        for(int i = 0; i < keyListString.Length; ++i)
        {
            if(XBoxAxisBind.ContainsKey(keyListString[i]))
                ControllerEx.Instance().addXboxBind(keyListString[i],XBoxAxisBind[keyListString[i]]);
            else if(XBoxKeyCodeBind.ContainsKey(keyListString[i]))
                ControllerEx.Instance().addXboxBind(keyListString[i],XBoxKeyCodeBind[keyListString[i]]);
            else if(XBoxAxisButtonBind.ContainsKey(keyListString[i]))
            {
                string[] split = XBoxAxisButtonBind[keyListString[i]].Split(' ');
                ControllerEx.Instance().addXboxBind(keyListString[i],split[0],bool.Parse(split[1]));
            }
            else
            {
                DebugUtil.assert(false,"invalid XBOX key name: {0}",keyListString[i]);
                return;
            }
        }
    }

    private static void readKeyboardKeyList(string[] keyListString)
    {
        for(int i = 0; i < keyListString.Length; ++i)
        {
            ControllerEx.Instance().addKeyboardBind(keyListString[i],(KeyCode)System.Enum.Parse(typeof(KeyCode), keyListString[i]));
        }
    }

}
