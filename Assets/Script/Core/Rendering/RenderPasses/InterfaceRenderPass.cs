using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return interfaceRenderTexture; } }
    [SerializeField] private RenderTexture interfaceRenderTexture;

    private static int interfaceLayer;
    public override int layerMasks => interfaceLayer;
    public override string layerName => "Interface";

    public void Awake()
    {
        interfaceLayer = (1 << LayerMask.NameToLayer(layerName));
        interfaceRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        interfaceRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
    }
}
