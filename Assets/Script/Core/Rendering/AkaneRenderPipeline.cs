using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class AkaneRenderPipeline : MonoBehaviour
{
    public static AkaneRenderPipeline _instance;

    [SerializeField] private Camera internalCamera;
    [SerializeField] private List<AkaneRenderPass> renderPasses;
    [SerializeField] private float fieldOfView;
    static public float FieldOfView;

    private CombinePass _resultPass;
    private void initializeRenderResources()
    {
        _instance = this;

        renderPasses = new List<AkaneRenderPass>();

        BackgroundRenderPass backgroundPass = ScriptableObject.CreateInstance<BackgroundRenderPass>();
        backgroundPass.Awake();
        CharacterRenderPass characterPass = ScriptableObject.CreateInstance<CharacterRenderPass>();
        characterPass.Awake();
        ShadowRenderPass shadowPass = ScriptableObject.CreateInstance<ShadowRenderPass>();
        shadowPass.Awake();
        EffectRenderPass effectPass = ScriptableObject.CreateInstance<EffectRenderPass>();
        effectPass.Awake();

        InterfaceRenderPass interfacePass = ScriptableObject.CreateInstance<InterfaceRenderPass>();
        interfacePass.Awake();

        PerspectiveRenderPass perspectivePass = ScriptableObject.CreateInstance<PerspectiveRenderPass>();
        perspectivePass.Awake();

        PerspectiveDepthRenderPass perspectiveDepthPass = ScriptableObject.CreateInstance<PerspectiveDepthRenderPass>();
        perspectiveDepthPass.Awake();

        CombinePass combinePass = CombinePass.CreateInstance(backgroundPass, characterPass, shadowPass, perspectivePass, interfacePass, perspectiveDepthPass);
        combinePass.Awake();

        EmptyRenderPass emptyPass = ScriptableObject.CreateInstance<EmptyRenderPass>();
        emptyPass.Awake();

        combinePass.AddPass(backgroundPass);
        combinePass.AddPass(characterPass);
        combinePass.AddPass(shadowPass);
        combinePass.AddPass(effectPass);

        renderPasses.Add(backgroundPass);
        renderPasses.Add(perspectivePass);
        renderPasses.Add(perspectiveDepthPass);
        renderPasses.Add(characterPass);
        renderPasses.Add(shadowPass);
        renderPasses.Add(effectPass);
        renderPasses.Add(combinePass);
        renderPasses.Add(interfacePass);
        renderPasses.Add(combinePass);
        renderPasses.Add(emptyPass);

        _resultPass = combinePass;
    }

    private void Awake()
    {
        initializeRenderResources();
    }

    [ExecuteAlways]
    private void LateUpdate()
    {
        FieldOfView = fieldOfView;
        if (Application.isPlaying == true)
        {
            internalDraw();
        }
        else
        {
            editor_internalDraw();
        }
    }

    private void OnApplicationQuit()
    {
        ReleaseResources();
    }
    void ReleaseResources()
    {
        renderPasses = null;
    }

    private void editor_internalDraw()
    {
        if (renderPasses == null)
        {
            initializeRenderResources();
        }

        internalDraw();
    }
    private void internalDraw()
    {
        for (int i = 0; i < renderPasses.Count; i++)
        {
            var renderPass = renderPasses[i];

            renderPass.Draw(internalCamera);
        }
    }

    public RenderTexture getResultTexture()
    {
        return _resultPass.RenderTexture;
    }
}
