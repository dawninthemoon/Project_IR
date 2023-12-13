using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveDepthRenderPass : AkaneRenderPass
{
    public override int layerMasks => perspectiveLayer;
    private static int perspectiveLayer;
    public override string layerName => "Perspective";

    public override RenderTexture RenderTexture { get { return perspectiveDepthRenderTexture; } }
    [SerializeField] private RenderTexture perspectiveDepthRenderTexture;
    public void Awake()
    {
        perspectiveLayer = (1 << LayerMask.NameToLayer(layerName));
        perspectiveDepthRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.Depth, 1);
        perspectiveDepthRenderTexture.filterMode = FilterMode.Point;
    }

    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.orthographic = false;
        renderCamera.fieldOfView = AkaneRenderPipeline.FieldOfView;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
        renderCamera.orthographic = true;
    }
}
