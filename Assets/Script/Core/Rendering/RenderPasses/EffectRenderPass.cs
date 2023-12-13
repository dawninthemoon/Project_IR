using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectRenderPass : AkaneRenderPass
{
    private static int effectLayer;
    private static int shadowScreenLayer;
    public override int layerMasks => effectLayer | shadowScreenLayer;
    public override string layerName => "EffectEtc";
    public override RenderTexture RenderTexture { get { return effectRenderTexture; } }
    [SerializeField] private RenderTexture effectRenderTexture;

    public void Awake()
    {
        effectLayer = (1 << LayerMask.NameToLayer(layerName));

        effectRenderTexture = new RenderTexture(960, 640, 1, RenderTextureFormat.ARGBHalf, 1)
        {
            stencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None,
        };
        
        effectRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera)
    {
      //  renderCamera.clearFlags = CameraClearFlags.Nothing;
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;
       // renderCamera.clearFlags = CameraClearFlags.SolidColor;
    }
}
