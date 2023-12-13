using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage _instance;

    public GameObject _toastMessageParent;
    public Text _toastText;
    public Image _backgroundImage;

    public float _fadeTime = 0.3f;

    private float _showTime = 1f;

    private float _showTimer = 0f;
    private float _fadeTimer = 0f;
    private float _colorAlpha = 0f;
    private bool _isShow = false;

    public void Awake()
    {
        _instance = this;
        _toastMessageParent.SetActive(false);
    }

    void Update()
    {
        if(_isShow == false)
            return;

        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();

        if(_showTime <= _showTimer)
        {
            _fadeTimer += deltaTime;
            float rate = MathEx.clamp01f(_fadeTimer * (1f / _fadeTime));

            Color imageColor = _backgroundImage.color;
            imageColor.a = 1f - rate;
            _backgroundImage.color = imageColor;

            Color textColor = _toastText.color;
            textColor.a = _colorAlpha * (1f - rate);
            _toastText.color = textColor;

            if(rate >= 1f)
            {
                _isShow = false;
                _toastMessageParent.SetActive(false);
            }
        }
        else
        {
            _showTimer += deltaTime;
        }
    }

    public void ShowToastMessage(string text, float time, Color textColor)
    {
        _toastText.text = text;
        _toastText.color = textColor;
        _colorAlpha = textColor.a;

        Color imageColor = _backgroundImage.color;
        imageColor.a = 1f;

        _backgroundImage.color = imageColor;

        _showTime = time;
        _showTimer = 0f;
        _fadeTimer = 0f;

        _isShow = true;

        _toastMessageParent.SetActive(true);
    }
}
