using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVAnimation : MonoBehaviour
{
    [System.Serializable]
    public class UVAnimationItem
    {
        public string _name;
        public Vector2 _direction;
        public float _speed;
        public SpriteRenderer _targetRenderer;

        public Vector2 _currentUV = Vector2.zero;

        public void updateUVAnimation(float deltaTime)
        {
            if(_targetRenderer == null)
                return;

            _currentUV += _direction * _speed * deltaTime;

            if(MathEx.abs(_currentUV.x) >= 1f)
                _currentUV.x += _currentUV.x > 0f ? -1f : 1f;
            
            if(MathEx.abs(_currentUV.y) >= 1f)
                _currentUV.x += _currentUV.x > 0f ? -1f : 1f;

            _targetRenderer.material.SetFloat("_UVX",_currentUV.x);
            _targetRenderer.material.SetFloat("_UVY",_currentUV.y);
        }
    }

    public List<UVAnimationItem> _uvAnimationItems = new List<UVAnimationItem>();

    void Update()
    {
        if(MasterManager.instance != null && MasterManager.instance.isGameUpdate() == false)
            return;

        foreach(var item in _uvAnimationItems)
        {
            item.updateUVAnimation(GlobalTimer.Instance().getSclaedDeltaTime());
        }
    }
}
