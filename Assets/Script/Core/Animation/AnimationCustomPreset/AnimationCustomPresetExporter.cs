#if UNITY_EDITOR

using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UnityEngine;
using UnityEditor;

public class AnimationCustomPresetExporter : AssetPostprocessor
{
    private const string kYAMLExtension = ".yaml";
    private const string kCustomPresetKeyword = "animation_";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            if (Path.GetExtension(assetPath) != kYAMLExtension || assetPath.Contains(kCustomPresetKeyword) == false)
                continue;

            TextAsset yamlAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (yamlAsset != null)
                saveAsScriptableObject(assetPath, yamlAsset.text);
        }
    }

    public static void saveAsScriptableObject(string filePath, string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        AnimationCustomPresetData animationCustomPresetData = null;

        try
        {
            animationCustomPresetData = deserializer.Deserialize<AnimationCustomPresetData>(new StringReader(yaml));
        }
        catch(YamlException exception)
        {
            DebugUtil.assert(false, "YAML Deserialize Exception!! : {0}", exception.Message);
            return;
        }

        AnimationCustomPreset customPreset = ScriptableObject.CreateInstance<AnimationCustomPreset>();
        customPreset._animationCustomPresetData = animationCustomPresetData;

        string scriptableObjectPath = filePath.Replace(kYAMLExtension, ".asset");
        ScriptableObject existingScriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(scriptableObjectPath);

        if(existingScriptableObject != null)
        {
            EditorUtility.CopySerialized(customPreset, existingScriptableObject);
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(customPreset, scriptableObjectPath);
            AssetDatabase.SaveAssets();
        }

        AssetDatabase.DeleteAsset(filePath);
    }
}

#endif