using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return backgroundRenderTexture; } }
    [SerializeField] private RenderTexture backgroundRenderTexture;

    private static int backgroundLayer;
    public override int layerMasks => backgroundLayer;
    public override string layerName => "Background";

    public void Awake()
    {
        backgroundLayer = (1 << LayerMask.NameToLayer("Background"));
        backgroundRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        backgroundRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
