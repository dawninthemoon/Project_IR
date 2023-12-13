using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PostProcessProfileData
{
    public float _sunAngle = 0f;
    public float _shadowDistance = 0f;
    public float _shadowDistanceRatio = 0f;

    public Vector2 _screenSize = new Vector2();
    public float _shadowDistanceOffset = 0f;
    public Color _shadowColor = Color.white;

    public float _brightness = 0f;
    public float _saturation = 0f;
    public Color _colorTint = Color.white;
    public Color _backgroundColorTint = Color.white;

    public float _blurSize = 0f;
    public float _multiSampleDistance = 0f;
    public Color _multiSampleColorTintRight = Color.white;
    public Color _multiSampleColorTintLeft = Color.white;

    public float _fogRate = 0f;
    public float _fogStrength = 0f;
    public Color _fogColor = Color.white;

    public bool _pixelSnap = false;

    public void syncValueToMaterial(Material targetMaterial)
    {
        targetMaterial.SetFloat("_SunAngle",_sunAngle);
        targetMaterial.SetFloat("_ShadowDistance",_shadowDistance);
        targetMaterial.SetFloat("_ShadowDistanceRatio",_shadowDistanceRatio);

        targetMaterial.SetVector("_ScreenSize",new Vector4(_screenSize.x,_screenSize.y,0f,0f));
        targetMaterial.SetFloat("_ShadowDistanceOffset",_shadowDistanceOffset);
        targetMaterial.SetColor("_ShadowColor",_shadowColor);

        targetMaterial.SetFloat("_Brightness",_brightness);
        targetMaterial.SetFloat("_Saturation",_saturation);
        targetMaterial.SetColor("_ColorTint",_colorTint);
        targetMaterial.SetColor("_BackgroundColorTint",_backgroundColorTint);
        
        targetMaterial.SetFloat("_BlurSize",_blurSize);
        targetMaterial.SetFloat("_MultiSampleDistance",_multiSampleDistance);
        targetMaterial.SetColor("_MultiSampleColorTintRight",_multiSampleColorTintRight);
        targetMaterial.SetColor("_MultiSampleColorTintLeft",_multiSampleColorTintLeft);

        targetMaterial.SetFloat("_FogRate",_fogRate);
        targetMaterial.SetFloat("_FogStrength",_fogStrength);
        targetMaterial.SetColor("_FogColor",_fogColor);

        targetMaterial.SetFloat("PixelSnap",_pixelSnap ? 1f : 0f);
    }

    public void syncValueFromMaterial(Material targetMaterial)
    {
        _sunAngle = targetMaterial.GetFloat("_SunAngle");
        _shadowDistance = targetMaterial.GetFloat("_ShadowDistance");
        _shadowDistanceRatio = targetMaterial.GetFloat("_ShadowDistanceRatio");

        _screenSize = targetMaterial.GetVector("_ScreenSize");
        _shadowDistanceOffset = targetMaterial.GetFloat("_ShadowDistanceOffset");
        _shadowColor = targetMaterial.GetColor("_ShadowColor");

        _brightness = targetMaterial.GetFloat("_Brightness");
        _saturation = targetMaterial.GetFloat("_Saturation");
        _colorTint = targetMaterial.GetColor("_ColorTint");
        _backgroundColorTint = targetMaterial.GetColor("_BackgroundColorTint");

        _blurSize = targetMaterial.GetFloat("_BlurSize");
        _multiSampleDistance = targetMaterial.GetFloat("_MultiSampleDistance");
        _multiSampleColorTintRight = targetMaterial.GetColor("_MultiSampleColorTintRight");
        _multiSampleColorTintLeft = targetMaterial.GetColor("_MultiSampleColorTintLeft");

        _fogRate = targetMaterial.GetFloat("_FogRate");
        _fogStrength = targetMaterial.GetFloat("_FogStrength");
        _fogColor = targetMaterial.GetColor("_FogColor");

        _pixelSnap = targetMaterial.GetFloat("PixelSnap") == 1f;
    }

    public void blend(PostProcessProfile destination, float ratio)
    {
        _sunAngle                   = Mathf.LerpAngle(_sunAngle, destination._profileData._sunAngle, ratio);
        _shadowDistance             = Mathf.Lerp(_shadowDistance, destination._profileData._shadowDistance, ratio);
        _shadowDistanceRatio        = Mathf.Lerp(_shadowDistanceRatio, destination._profileData._shadowDistanceRatio, ratio);
        _screenSize                 = Vector2.Lerp(_screenSize, destination._profileData._screenSize, ratio);
        _shadowDistanceOffset       = Mathf.Lerp(_shadowDistanceOffset, destination._profileData._shadowDistanceOffset, ratio);
        _shadowColor                = Color.Lerp(_shadowColor, destination._profileData._shadowColor, ratio);
        _brightness                 = Mathf.Lerp(_brightness, destination._profileData._brightness, ratio);
        _saturation                 = Mathf.Lerp(_saturation, destination._profileData._saturation, ratio);
        _colorTint                  = Color.Lerp(_colorTint, destination._profileData._colorTint, ratio);
        _backgroundColorTint        = Color.Lerp(_backgroundColorTint, destination._profileData._backgroundColorTint, ratio);
        _blurSize                   = Mathf.Lerp(_blurSize, destination._profileData._blurSize, ratio);
        _multiSampleDistance        = Mathf.Lerp(_multiSampleDistance, destination._profileData._multiSampleDistance, ratio);
        _multiSampleColorTintRight  = Color.Lerp(_multiSampleColorTintRight, destination._profileData._multiSampleColorTintRight, ratio);
        _multiSampleColorTintLeft   = Color.Lerp(_multiSampleColorTintLeft, destination._profileData._multiSampleColorTintLeft, ratio);
        _fogRate                    = Mathf.Lerp(_fogRate, destination._profileData._fogRate, ratio);
        _fogStrength                = Mathf.Lerp(_fogStrength, destination._profileData._fogStrength, ratio);
        _fogColor                   = Color.Lerp(_fogColor, destination._profileData._fogColor, ratio);
        _pixelSnap                  = ratio >= 0.5f ? destination._profileData._pixelSnap : _pixelSnap;
    }

    public void blendCopy(PostProcessProfileData source, PostProcessProfileData destination, float ratio)
    {
        _sunAngle                   = Mathf.LerpAngle(source._sunAngle, destination._sunAngle, ratio);
        _shadowDistance             = Mathf.Lerp(source._shadowDistance, destination._shadowDistance, ratio);
        _shadowDistanceRatio        = Mathf.Lerp(source._shadowDistanceRatio, destination._shadowDistanceRatio, ratio);
        _screenSize                 = Vector2.Lerp(source._screenSize, destination._screenSize, ratio);
        _shadowDistanceOffset       = Mathf.Lerp(source._shadowDistanceOffset, destination._shadowDistanceOffset, ratio);
        _shadowColor                = Color.Lerp(source._shadowColor, destination._shadowColor, ratio);
        _brightness                 = Mathf.Lerp(source._brightness, destination._brightness, ratio);
        _saturation                 = Mathf.Lerp(source._saturation, destination._saturation, ratio);
        _colorTint                  = Color.Lerp(source._colorTint, destination._colorTint, ratio);
        _backgroundColorTint        = Color.Lerp(source._backgroundColorTint, destination._backgroundColorTint, ratio);
        _blurSize                   = Mathf.Lerp(source._blurSize, destination._blurSize, ratio);
        _multiSampleDistance        = Mathf.Lerp(source._multiSampleDistance, destination._multiSampleDistance, ratio);
        _multiSampleColorTintRight  = Color.Lerp(source._multiSampleColorTintRight, destination._multiSampleColorTintRight, ratio);
        _multiSampleColorTintLeft   = Color.Lerp(source._multiSampleColorTintLeft, destination._multiSampleColorTintLeft, ratio);
        _fogRate                    = Mathf.Lerp(source._fogRate, destination._fogRate, ratio);
        _fogStrength                = Mathf.Lerp(source._fogStrength, destination._fogStrength, ratio);
        _fogColor                   = Color.Lerp(source._fogColor, destination._fogColor, ratio);
        _pixelSnap                  = ratio >= 0.5f ? destination._pixelSnap : source._pixelSnap;
    }

    public void copy(PostProcessProfile profile)
    {
        _sunAngle                   = profile._profileData._sunAngle;
        _shadowDistance             = profile._profileData._shadowDistance ;
        _shadowDistanceRatio        = profile._profileData._shadowDistanceRatio;
        _screenSize                 = profile._profileData._screenSize;
        _shadowDistanceOffset       = profile._profileData._shadowDistanceOffset;
        _shadowColor                = profile._profileData._shadowColor;
        _brightness                 = profile._profileData._brightness;
        _saturation                 = profile._profileData._saturation;
        _colorTint                  = profile._profileData._colorTint;
        _backgroundColorTint        = profile._profileData._backgroundColorTint;
        _blurSize                   = profile._profileData._blurSize;
        _multiSampleDistance        = profile._profileData._multiSampleDistance;
        _multiSampleColorTintRight  = profile._profileData._multiSampleColorTintRight;
        _multiSampleColorTintLeft   = profile._profileData._multiSampleColorTintLeft;
        _fogRate                    = profile._profileData._fogRate;
        _fogStrength                = profile._profileData._fogStrength;
        _fogColor                   = profile._profileData._fogColor;
        _pixelSnap                  = profile._profileData._pixelSnap;
    }
}

[CreateAssetMenu(fileName = "PostProcessProfile", menuName = "Scriptable Object/PostProcessProfile", order = 1)]
public class PostProcessProfile : ScriptableObject
{
    public PostProcessProfileData _profileData = new PostProcessProfileData();
}


#if UNITY_EDITOR

[CustomEditor(typeof(PostProcessProfile))]
public class PostProcessProfileEditor : Editor
{
    private PostProcessProfile controll;
    private bool _editMode = false;
    private Material _targetMaterial = null;

    void OnEnable()
    {
        controll = (PostProcessProfile)target;
    }

    public override void OnInspectorGUI()
    {
        bool isChange = false;
        isChange |= floatSlider("Sun Angle", ref controll._profileData._sunAngle, 0f, 360f);
        isChange |= floatSlider("Shadow Distance", ref controll._profileData._shadowDistance, 0.1f, 3f);
        isChange |= floatSlider("Shadow Distance Ratio", ref controll._profileData._shadowDistanceRatio, 0f, 10f);

        GUILayout.Space(20f);
        isChange |= vector2Field("Screen Size",ref controll._profileData._screenSize);
        isChange |= floatSlider("Shadow Distance Offset",ref controll._profileData._shadowDistanceOffset, 0f, 10f);
        isChange |= colorPicker("Shadow Color",ref controll._profileData._shadowColor);

        GUILayout.Space(20f);
        isChange |= floatSlider("Brightness",ref controll._profileData._brightness, 0f, 5f);
        isChange |= floatSlider("Saturation",ref controll._profileData._saturation, 0f, 1f);
        isChange |= colorPicker("Color Tint",ref controll._profileData._colorTint);
        isChange |= colorPicker("Background Color Tint",ref controll._profileData._backgroundColorTint);

        GUILayout.Space(20f);
        isChange |= floatSlider("Blur Size",ref controll._profileData._blurSize, 0f, 2f);
        isChange |= floatSlider("MultiSample Distance",ref controll._profileData._multiSampleDistance, 0f, 5f);
        isChange |= colorPicker("MultiSample Color Tint Right",ref controll._profileData._multiSampleColorTintRight);
        isChange |= colorPicker("MultiSample Color Tint Left",ref controll._profileData._multiSampleColorTintLeft);

        GUILayout.Space(20f);
        isChange |= floatSlider("Fog Rate",ref controll._profileData._fogRate, 0f, 1f);
        isChange |= floatSlider("Fog Strength",ref controll._profileData._fogStrength, 0f, 1f);
        isChange |= colorPicker("Fog Color",ref controll._profileData._fogColor);

        if(isChange)
        {
            syncValueToMaterial();
            EditorUtility.SetDirty(controll);
        }

        GUILayout.Space(20f);

        Color guiColor = GUI.color;
        GUI.color = _editMode ? Color.green : Color.red;
        if(GUILayout.Button(_editMode ? "Exit Edit Mode" :"Edit PostProcess"))
        {
            _editMode = !_editMode;
            syncValueToMaterial();
        }
        GUI.color = guiColor;

        if(GUILayout.Button("Get Value From Material"))
        {
            controll._profileData.syncValueFromMaterial(PostProcessProfileControl.getPostProcessMaterial(true));
            EditorUtility.SetDirty(controll);
        }
    }

    public bool floatSlider(string title, ref float value, float min, float max)
    {
        float beforeValue = EditorGUILayout.Slider(title, value,min,max);
        if(beforeValue != value)
        {
            value = beforeValue;
            return true;
        }

        return false;
    }

    public bool vector2Field(string title, ref Vector2 value)
    {
        Vector2 afterValue = EditorGUILayout.Vector2Field(title,value);
        if(value != afterValue)
        {
            value = afterValue;
            return true;
        }

        return false;
    }

    public bool colorPicker(string title, ref Color color)
    {
        Color beforeColor = EditorGUILayout.ColorField(title, color);
        if(MathEx.equals(beforeColor,color,0f) == false)
        {
            color = beforeColor;
            return true;
        }

        return false;
    }

    public bool materialToggle(string title, ref float toggle)
    {
        bool realToggle = toggle == 1f;
        bool beforeToggle = EditorGUILayout.Toggle(title,realToggle);
        if(realToggle != beforeToggle)
        {
            toggle = beforeToggle ? 1f : 0f;
            return true;
        }

        return false;
    }

    public void syncValueToMaterial()
    {
        if(_editMode == false)
            return;

        if(_targetMaterial == null)
        {
            GameObject targetGameObject = GameObject.FindGameObjectWithTag("ScreenResultMesh");
            if(targetGameObject == null)
                return;

            MeshRenderer targetMeshRenderer = targetGameObject.GetComponent<MeshRenderer>();
            if(targetMeshRenderer == null)
                return;

            List<Material> sharedMaterial = new List<Material>();
            targetMeshRenderer.GetSharedMaterials(sharedMaterial);
            _targetMaterial = sharedMaterial[0];
        }

        controll._profileData.syncValueToMaterial(_targetMaterial);
    }
}

#endif