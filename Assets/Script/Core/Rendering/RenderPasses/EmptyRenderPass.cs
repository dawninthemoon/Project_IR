using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyRenderPass : AkaneRenderPass
{
    public override int layerMasks => 0;

    public override string layerName => "somelayer";

    public override RenderTexture RenderTexture { get { return emptyTexture; } }
    private RenderTexture emptyTexture;

    public void Awake()
    {
        emptyTexture = new RenderTexture(1024, 1024, 1);
    }

    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
        return;
    }
}
