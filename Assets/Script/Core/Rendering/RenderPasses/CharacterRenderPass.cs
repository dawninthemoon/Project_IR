using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRenderPass : AkaneRenderPass
{
    public override RenderTexture RenderTexture { get { return characterRenderTexture; } }
    [SerializeField] private RenderTexture characterRenderTexture;

    private static int characterLayer;
    public override int layerMasks => characterLayer;
    public override string layerName => "Character";

    public void Awake()
    {
        characterLayer = (1 << LayerMask.NameToLayer(layerName));
        characterRenderTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGBHalf, 1);
        characterRenderTexture.filterMode = FilterMode.Point;
    }
    public override void Draw(Camera renderCamera)
    {
        renderCamera.targetTexture = RenderTexture;
        renderCamera.cullingMask = layerMasks;

        renderCamera.Render();

        renderCamera.cullingMask = 0;

    }
}
