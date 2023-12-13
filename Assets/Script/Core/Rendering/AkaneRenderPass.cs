using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AkaneRenderPass : ScriptableObject
{
    public abstract int layerMasks { get; }
    public abstract string layerName { get; }
    public abstract RenderTexture RenderTexture { get; }
    public abstract void Draw(Camera renderCamera);
}