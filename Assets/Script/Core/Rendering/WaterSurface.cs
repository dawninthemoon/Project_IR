using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaterSurface : MonoBehaviour
{
    public float _scrollSpeed = 1f;
    private Material _renderMaterial = null;
    private MeshRenderer _meshRenderer;
    private float _offset = 0;

    public void initialize()
    {
        if(AkaneRenderPipeline._instance == null)
            return;

        RenderTexture resultTexture = AkaneRenderPipeline._instance.getResultTexture();
        if(resultTexture == null)
            return;

        _meshRenderer = GetComponent<MeshRenderer>();
        _renderMaterial = _meshRenderer.sharedMaterial;
        if(_renderMaterial == null)
            return;

        _renderMaterial.SetTexture("_WaterReflectionsTex",resultTexture);

        _offset = 0f;
    }

    void Start()
    {
        initialize();
    }

    [ExecuteAlways]
    void LateUpdate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false && _renderMaterial == null)
        {
            initialize();
        }

        if(Application.isPlaying == false)
            return;
#endif

        _offset += Time.deltaTime * _scrollSpeed;
        if (_offset > 1.0f)
          _offset -= 1.0f;

        _renderMaterial.SetTextureOffset("_Displacement", new Vector2(_offset, 0));
    }


    
}
