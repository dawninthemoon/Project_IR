using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenDirector : MonoBehaviour
{
    [System.Serializable]
    public class ScreenFader
    {
        private enum FadeState
        {
            None,
            FadeIn,
            FadeOut,
        }

        public SpriteRenderer   _faderSpriteRenderer = null;

        private FadeState       _state = FadeState.None;
        private bool            _isEnd = false;
        private float           _lambda = 0f;
        private float           _alpha = 0f;
        private float           _targetAlpha = 0f;

        public void initialize()
        {
            _state = FadeState.None;
            _faderSpriteRenderer.gameObject.SetActive(false);
            _isEnd = false;
            _lambda = 0f;
            _alpha = 0f;
            _targetAlpha = 0f;

            updateAlpha();
        }

        public void updateAlpha()
        {
            Color color = _faderSpriteRenderer.color;
            color.a = _alpha;
            _faderSpriteRenderer.color = color;
        }

        public void fadeIn(float lambda)
        {
            _state = FadeState.FadeIn;
            _isEnd = false;
            _lambda = lambda;
            _alpha = 0f;
            _targetAlpha = 1f;

            _faderSpriteRenderer.gameObject.SetActive(true);
        }

        public void fadeOut(float lambda)
        {
            _state = FadeState.FadeOut;
            _isEnd = false;
            _lambda = lambda;
            _alpha = 1f;
            _targetAlpha = 0f;

            _faderSpriteRenderer.gameObject.SetActive(true);
        }

        public void clear()
        {
            _state = FadeState.None;
            _isEnd = true;
            _lambda = 0f;
            _alpha = 0f;
            _targetAlpha = 0f;

            _faderSpriteRenderer.gameObject.SetActive(false);
        }

        private bool isDampingEnd()
        {
            return MathEx.equals(_alpha,_targetAlpha,0.01f);
        }

        public void progress(float deltaTime)
        {
            if(_isEnd)
                return;

            _alpha = MathEx.damp(_alpha, _targetAlpha, _lambda, deltaTime);
            if(isDampingEnd())
            {
                _alpha = _targetAlpha;
                _isEnd = true;

                if(_state == FadeState.FadeOut)
                    _faderSpriteRenderer.gameObject.SetActive(false);
            }

            updateAlpha();
        }
    }


    static public ScreenDirector _instance = null;

    public ScreenFader _screenFader = new ScreenFader();
    public GameObject _mainHudParent;

    public void Awake()
    {
        _instance = this;
    }

    public void Update()
    {
        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();
        progress(deltaTime);
    }

    public void initialize()
    {
        _screenFader.initialize();
    }

    void progress(float deltaTime)
    {
        _screenFader.progress(deltaTime);
    }

    public void ScreenFadeIn(float lambda)
    {
        _screenFader.fadeIn(lambda);
    }

    public void ScreenFadeOut(float lambda)
    {
        _screenFader.fadeOut(lambda);
    }

    public void setActiveMainHud(bool active)
    {
        UIRepeater.Instance().updateUIRepeater();
        _mainHudParent?.SetActive(active);
    }
}
