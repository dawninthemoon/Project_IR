using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinePass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return null; } }
    private Material renderMaterial;

    BackgroundRenderPass backgroundRenderPass;
    CharacterRenderPass characterRenderPass;
    ShadowRenderPass shadowRenderPass;
    PerspectiveRenderPass perspectiveRenderPass;
    PerspectiveDepthRenderPass perspectiveDepthRenderPass;
    InterfaceRenderPass interfaceRenderPass;

    Dictionary<string, AkaneRenderPass> renderPasses = new Dictionary<string, AkaneRenderPass>();

    private static int shadowScreenLayer;
    public override int layerMasks => shadowScreenLayer;
    public override string layerName => "ShadowScreen";

    public void AddPass<T>(T renderPass) where T : AkaneRenderPass
    {
        renderPasses.Add(renderPass.layerName, renderPass);
    }

    public void Awake()
    {
        shadowScreenLayer = (1 << LayerMask.NameToLayer(layerName));

        if (renderMaterial == null)
        {
            var quad = GameObject.FindGameObjectWithTag("ScreenResultMesh");

            var renderer = quad.GetComponent<Renderer>();
            renderMaterial = renderer.sharedMaterial;
        }
    }

    public static CombinePass CreateInstance(BackgroundRenderPass backgroundPass, CharacterRenderPass characterPass, ShadowRenderPass shadowPass, PerspectiveRenderPass perspectivePass, InterfaceRenderPass interfacePass, PerspectiveDepthRenderPass perspectiveDepthPass)
    {
        var pass = ScriptableObject.CreateInstance<CombinePass>();
        pass.backgroundRenderPass = backgroundPass;
        pass.characterRenderPass = characterPass;
        pass.shadowRenderPass = shadowPass;
        pass.perspectiveRenderPass = perspectivePass;
        pass.interfaceRenderPass = interfacePass;
        pass.perspectiveDepthRenderPass = perspectiveDepthPass;
        return pass;
    }
    public override void Draw(Camera renderCamera)
    {
        renderMaterial?.SetTexture("_CharacterTexture", characterRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_MainTex", backgroundRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_ShadowMapTexture", shadowRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_PerspectiveTexture", perspectiveRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_InterfaceTexture", interfaceRenderPass?.RenderTexture);
        renderMaterial?.SetTexture("_PerspectiveDepthTexture", perspectiveDepthRenderPass?.RenderTexture);

        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
