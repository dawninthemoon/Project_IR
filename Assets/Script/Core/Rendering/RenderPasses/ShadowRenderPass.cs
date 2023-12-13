using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ShadowRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return shadowRenderTexture; } }
    [SerializeField] private RenderTexture shadowRenderTexture;

    private static int shadowLayer;
    public override int layerMasks => shadowLayer;
    public override string layerName => "ShadowMap";

    public void Awake()
    {
        shadowLayer = (1 << LayerMask.NameToLayer("ShadowMap"));
        shadowRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.Shadowmap, 1);
        shadowRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
