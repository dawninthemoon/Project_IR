using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LetterBox : MonoBehaviour
{
    public static LetterBox _instance;
    public AnimationCurve _animationCurve;
    public float _letterBoxSizeHalf = 37.5f;
    public float _letterBoxTime = 1f;

    public GameObject _letterBoxParent;
    public UnityEngine.UI.Image _upBox;
    public UnityEngine.UI.Image _downBox;

    private bool _isShow = false;
    private bool _update = false;
    private float _time = 0f;


    public void Awake()
    {
        _instance = this;
        _letterBoxParent.SetActive(false);
    }

    public void Update()
    {
        if(_update == false)
            return;

        float deltaTime = GlobalTimer.Instance().getSclaedDeltaTime();
        _time += deltaTime;
        if(_time >= _letterBoxTime)
        {
            _time = _letterBoxTime;
            _update = false;

            if(_isShow == false)
                _letterBoxParent.SetActive(false);
        }

        float positionRate = _animationCurve.Evaluate(_time * (1f / _letterBoxTime));
        if(_isShow)
        {
            Vector2 position = _upBox.rectTransform.anchoredPosition;
            position.y = math.lerp(_letterBoxSizeHalf, -_letterBoxSizeHalf, positionRate);
            _upBox.rectTransform.anchoredPosition = position;

            position = _downBox.rectTransform.anchoredPosition;
            position.y = math.lerp(-_letterBoxSizeHalf, _letterBoxSizeHalf, positionRate);
            _downBox.rectTransform.anchoredPosition = position;
        }
        else
        {
            Vector2 position = _upBox.rectTransform.anchoredPosition;
            position.y = math.lerp(-_letterBoxSizeHalf, _letterBoxSizeHalf, positionRate);
            _upBox.rectTransform.anchoredPosition = position;

            position = _downBox.rectTransform.anchoredPosition;
            position.y = math.lerp(_letterBoxSizeHalf, -_letterBoxSizeHalf, positionRate);
            _downBox.rectTransform.anchoredPosition = position;
        }
    }

    public void Show()
    {
        _isShow = true;
        _update = true;
        _time = 0f;

        Vector2 position = _upBox.rectTransform.anchoredPosition;
        position.y = _letterBoxSizeHalf;
        _upBox.rectTransform.anchoredPosition = position;

        position = _downBox.rectTransform.anchoredPosition;
        position.y = -_letterBoxSizeHalf;
        _downBox.rectTransform.anchoredPosition = position;

        _letterBoxParent.SetActive(true);
    }

    public void Hide()
    {
        _isShow = false;
        _update = true;
        _time = 0f;

        Vector2 position = _upBox.rectTransform.anchoredPosition;
        position.y = -_letterBoxSizeHalf;
        _upBox.rectTransform.anchoredPosition = position;

        position = _downBox.rectTransform.anchoredPosition;
        position.y = _letterBoxSizeHalf;
        _downBox.rectTransform.anchoredPosition = position;

        _letterBoxParent.SetActive(true);
    }

    public void clear()
    {
        _isShow = false;
        _update = false;
        _time = 0f;

        _letterBoxParent.SetActive(false);
    }
}
