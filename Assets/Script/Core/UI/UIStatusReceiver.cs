using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStatusReceiver : MonoBehaviour
{
    public enum ReceiveUIType
    {
        ImageFill,
        Text,
    }

    public enum ReceiveValueType
    {
        Percentage,
        RealValue,
    }

    public ReceiveUIType _receiveUIType;
    public ReceiveValueType _receiveValueType;

    public string _targetEntityUniqueKey;
    public string _targetStatusName;
    
    private Image _targetImage;
    private Text _targetText;

    private bool _isValid = false;

    private void Awake()
    {
        switch (_receiveUIType)
        {
            case ReceiveUIType.ImageFill:
                _targetImage = GetComponent<Image>();
                break;
            case ReceiveUIType.Text:
                _targetText = GetComponent<Text>();
                break;
        }

        _isValid = _targetImage != null || _targetText != null;
    }

    private void Start()
    {
        UIRepeater.Instance().registerUIReceiver(_targetEntityUniqueKey, this);
    }

    public void updateUI(StatusInfo targetStatusInfo)
    {
        if (_isValid == false)
            return;

        float value = getValueFromStatus(targetStatusInfo);
        switch (_receiveUIType)
        {
            case ReceiveUIType.ImageFill:
                _targetImage.fillAmount = value;
                break;
            case ReceiveUIType.Text:
                _targetText.text = value.ToString();
                break;
        }
    }

    private float getValueFromStatus(StatusInfo statusInfo)
    {
        switch (_receiveValueType)
        {
            case ReceiveValueType.Percentage:
                return statusInfo.getCurrentStatusPercentage(_targetStatusName);
            case ReceiveValueType.RealValue:
                return statusInfo.getCurrentStatus(_targetStatusName);
        }

        return 0f;
    }

}
