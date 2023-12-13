using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PostProcessProfileApplyType
{
    BaseBlend,
    Additional,
}

public class PostProcessProfileControl
{   
    private class ProfileBlender
    {
        public PostProcessProfile _sourceLayer;
        public float _blendTime = 0f;
        private float _timer = 0f;

        public bool blend(ref PostProcessProfileData postProcessProfileData, float deltaTime, bool reverse)
        {
            _timer += deltaTime;
            float blendRate = _timer * (1f / _blendTime);
            blendRate = MathEx.clamp01f(blendRate);

            if(reverse)
                blendRate = 1f - blendRate;

            postProcessProfileData.blend(_sourceLayer,blendRate);
            return isEnd();
        }

        public bool isEnd()
        {
            return _timer >= _blendTime;
        }

        public void setProfileData(PostProcessProfile profile, float blendTime)
        {
            _sourceLayer = profile;
            _blendTime = blendTime;
            _timer = 0f;
        }
    }
    
    private SimplePool<ProfileBlender>  _profileBlenderPool = new SimplePool<ProfileBlender>();
    private List<ProfileBlender>        _baseBlendingProfileList = new List<ProfileBlender>();
    private ProfileBlender              _additionalEffectProfile = new ProfileBlender();

    private PostProcessProfileData      _resultData = new PostProcessProfileData();

    private Material                    _targetMaterial;

    private bool                        _isBlending = false;

    static public Material getPostProcessMaterial(bool editMode)
    {
        GameObject targetGameObject = GameObject.FindGameObjectWithTag("ScreenResultMesh");
        if(targetGameObject == null)
            return null;

        MeshRenderer targetMeshRenderer = targetGameObject.GetComponent<MeshRenderer>();
        if(targetMeshRenderer == null)
            return null;

        if(editMode)
        {
            List<Material> sharedMaterial = new List<Material>();
            targetMeshRenderer.GetSharedMaterials(sharedMaterial);
            return sharedMaterial[0];
        }
        else
        {
            return targetMeshRenderer.material;
        }
    }


    public void updateMaterial(bool editMode)
    {
        _targetMaterial = getPostProcessMaterial(editMode);
    }

    public void processBlend(float deltaTime)
    {
        if(_baseBlendingProfileList.Count == 0)
            return;

        if(_isBlending == false && _additionalEffectProfile.isEnd())
            return;

        _resultData.copy(_baseBlendingProfileList[0]._sourceLayer);
        int resultIndex = 0;
        for(int baseBlendIndex = 1; baseBlendIndex < _baseBlendingProfileList.Count; ++ baseBlendIndex)
        {
            if(_baseBlendingProfileList[baseBlendIndex].blend(ref _resultData, deltaTime, false))
                resultIndex = baseBlendIndex;
        }

        for(int index = 0; index < resultIndex; ++index)
        {
            _baseBlendingProfileList.RemoveAt(0);
        }

        if(_additionalEffectProfile.isEnd() == false)
            _additionalEffectProfile.blend(ref _resultData, deltaTime, true);

        if(_isBlending)
            _resultData.syncValueToMaterial(_targetMaterial);

        _isBlending = _baseBlendingProfileList.Count > 1 || _additionalEffectProfile.isEnd() == false;
    }

    public void setAdditionalEffectProfile(PostProcessProfile profile, float blendTime)
    {
        _additionalEffectProfile.setProfileData(profile,blendTime);
        _isBlending = true;
    }

    public void addBaseBlendProfile(PostProcessProfile profile, float blendTime)
    {
        ProfileBlender blender = _profileBlenderPool.dequeue();
        blender.setProfileData(profile,blendTime);

        _baseBlendingProfileList.Add(blender);
        _isBlending = true;
    }
}
