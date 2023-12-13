
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AnimationCustomPreset", menuName = "Scriptable Object/Animation Custom Preset", order = int.MaxValue)]
public class AnimationCustomPreset : ScriptableObject
{
    public AnimationCustomPresetData _animationCustomPresetData;
    public string _translationPresetName = "";
    public string _rotationPresetName="";
    public string _scalePresetName = "";
}

#if UNITY_EDITOR

[CustomEditor(typeof(AnimationCustomPreset))]
public class AnimationCustomPresetEditor : Editor
{
    AnimationCustomPreset controll;

    private AnimationPlayer _animationPlayer = new AnimationPlayer();

    private float _fps = 0f;
    private bool _isLoaded = false;
    private bool _update = false;
    private bool _playSecond = false;

    private double previousTime;

    public void Update()
    {
        double currentTime = EditorApplication.timeSinceStartup;

        if (previousTime == 0)
        {
            previousTime = currentTime;
            return;
        }

        double deltaTime = currentTime - previousTime;
        previousTime = currentTime;

        if(_playSecond)
        {
            deltaTime = 0.02f;
        }

        if((_isLoaded && _update) || _playSecond)
        {
            _animationPlayer.progress((float)deltaTime, null);
            Repaint();

            _playSecond = false;
        }

        EditorApplication.update -= Update;
    }

	void OnEnable()
    {
        controll = (AnimationCustomPreset)target;
    }

    public override void OnInspectorGUI()
    {
		base.OnInspectorGUI();

        GUILayout.Space(10f);

        if(controll._animationCustomPresetData != null)
            GUILayout.Label("Total Duration : " + controll._animationCustomPresetData.getTotalDuration());

        EditorGUILayout.BeginHorizontal();

        _fps = EditorGUILayout.FloatField(_fps);
        if(GUILayout.Button("Set Duration From FPS"))
        {
            float perFrame = 1f / _fps;
            for(int index = 0; index < controll._animationCustomPresetData._duration.Length; ++index)
            {
                controll._animationCustomPresetData._duration[index] = perFrame;
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(30f);

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("TestPlay");
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Play Begin", GUILayout.Width(100f)))
        {
            _animationPlayer.initialize();
            _animationPlayer.changeAnimation(loadAnimation());

            _isLoaded = true;
            _update = true;

            Repaint();
        }

        GUI.enabled = _isLoaded;

        if(GUILayout.Button(">", GUILayout.Width(30f)))
        {
            _update = false;
            _playSecond = true;

            Repaint();
        }

        Color currentColor = GUI.color;
        GUI.color = _isLoaded == false ? currentColor : (_update ? Color.green : Color.red);
        if(GUILayout.Button(_update ? "Playing" : "Paused"))
        {
            _update = !_update;
            Repaint();
        }

        GUI.color = currentColor;
        GUI.enabled = true;

        GUILayout.EndHorizontal();

        if(_isLoaded)
        {
            EditorApplication.update += Update;
        }

        Texture2D currentTexture = _isLoaded  ? _animationPlayer.getCurrentSprite().texture : null;

        if(_isLoaded)
        {
            GUILayout.Label("Animation Time : (" + _animationPlayer.getCurrentAnimationTime() + " : " + _animationPlayer.getCurrentAnimationDuration() + ")");
            GUILayout.Label("Index : (" + (_animationPlayer.getCurrentIndex() + 1) + " : " + _animationPlayer.getEndIndex() + ")");
        }

        if(currentTexture != null)
        {
            Rect rect = GUILayoutUtility.GetRect(currentTexture.width, currentTexture.height);
            Vector3 translation = Vector3.zero;
            _animationPlayer.getCurrentAnimationTranslation(out translation);
            rect.center += new Vector2(translation.x, translation.y) * 100f;

            Vector3 scale = Vector3.one;
            _animationPlayer.getCurrentAnimationScale(out scale);

            EditorGUIUtility.ScaleAroundPivot(scale,rect.center);
            EditorGUIUtility.RotateAroundPivot(_animationPlayer.getCurrentAnimationRotation().eulerAngles.z, rect.center);
            
            GUI.DrawTexture(rect, currentTexture,ScaleMode.ScaleToFit);
            EditorGUIUtility.RotateAroundPivot(-_animationPlayer.getCurrentAnimationRotation().eulerAngles.z, rect.center);
            EditorGUIUtility.ScaleAroundPivot(Vector3.one,rect.center);
        }



        EditorGUILayout.EndVertical();
    }

    public AnimationPlayDataInfo loadAnimation()
    {
        AnimationCustomPreset animationCustomPreset = controll;

        AnimationPlayDataInfo playData = new AnimationPlayDataInfo();
        playData._actionTime = controll._animationCustomPresetData.getTotalDuration();
        playData._customPresetData = animationCustomPreset._animationCustomPresetData;
        playData._customPreset = animationCustomPreset;
        playData._path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(controll));

        if(animationCustomPreset._rotationPresetName != "")
        {
            AnimationRotationPreset rotationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationRotationPreset") as AnimationRotationPreset;
            playData._rotationPresetData = rotationPreset.getPresetData(animationCustomPreset._rotationPresetName);
        }
        
        if(animationCustomPreset._scalePresetName != "")
        {
            AnimationScalePreset scalePreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationScalePreset") as AnimationScalePreset;
            playData._scalePresetData = scalePreset.getPresetData(animationCustomPreset._scalePresetName);
        }

        if (animationCustomPreset._translationPresetName != "")
        {
            AnimationTranslationPreset translationPreset = ResourceContainerEx.Instance().GetScriptableObject("Preset\\AnimationTranslationPreset") as AnimationTranslationPreset;
            playData._translationPresetData = translationPreset.getPresetData(animationCustomPreset._translationPresetName);
        }

        return playData;
    }
}


#endif
