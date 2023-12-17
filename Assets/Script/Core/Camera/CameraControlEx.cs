using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public enum CameraModeType
{
    ArenaMode,
    TargetCenterMode,
    TwoTargetMode,
    PositionMode,
    Count,
};

public abstract class CameraModeBase
{
    public static Vector2Int _screenPixel = new Vector2Int(800, 600);
    
    public static float _pixelPerUnit = 100f;

    protected Vector3 _cameraPosition;
    protected Vector3 _currentTargetPosition;
    protected Transform _currentTarget;
    protected GameEntityBase _targetEntity;


    public abstract CameraModeType getCameraModeType();
    public abstract void initialize(Vector3 position);
    public abstract void progress(float deltaTime, Vector3 targetPosition);
    public abstract void release();

    public void setCurrentTargetEntity(GameEntityBase targetEntity)
    {
        _targetEntity = targetEntity;
    }

    public void setCurrentTarget(Transform obj)
    {
        _currentTarget = obj;
    }

    public void setCurrentTargetPosition(Vector3 targetPosition)
    {
        _currentTargetPosition = targetPosition;
    }

    public Vector3 getCameraPosition()
    {
        return _cameraPosition;
    }
};

public class CameraTwoTargetMode : CameraModeBase
{
    public float _targetPositionRatio = 0.25f;
    public static float _cameraMoveSpeedRate = 7.3f;

    private Vector3 _currentCenterPosition;


    public override CameraModeType getCameraModeType() => CameraModeType.TargetCenterMode;
    public override void initialize(Vector3 position)
    {
        _cameraPosition = position;
        _targetEntity = null;
        updateCameraCenter();
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        updateCameraCenter();
        _cameraPosition = MathEx.damp(_cameraPosition, _currentCenterPosition, _cameraMoveSpeedRate, deltaTime);
        GizmoHelper.instance.drawLine(_currentCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }

    public void updateCameraCenter()
    {
        if(_targetEntity == null || _targetEntity.isDead())
            _currentCenterPosition = _currentTarget.transform.position;
        else
            _currentCenterPosition = _currentTarget.transform.position + (_targetEntity.transform.position - _currentTarget.transform.position) * _targetPositionRatio;
    }
};

public class CameraPositionMode : CameraModeBase
{
    public float _targetPositionRatio = 0.25f;
    public static float _cameraMoveSpeedRate = 8.0f;

    public override CameraModeType getCameraModeType() => CameraModeType.PositionMode;
    public override void initialize(Vector3 position)
    {
        _cameraPosition = position;
        _targetEntity = null;
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        _cameraPosition = Vector3.Lerp(_cameraPosition, _currentTargetPosition, _cameraMoveSpeedRate * deltaTime);
    }

    public override void release()
    {
    }

};

public class CameraArenaMode : CameraModeBase
{
    public static float _cameraCenterRate = 0.1f;
    public static float _cameraMoveSpeedRate = 2f;
    private Vector3 _cameraCenterPosition = Vector3.zero;
    public override CameraModeType getCameraModeType() => CameraModeType.ArenaMode;
    public override void initialize(Vector3 position)
    {
        _cameraCenterPosition = position;

        // if(_currentTarget != null && _currentTarget is GameEntityBase)
        // {
        //     GameEntityBase target = ((GameEntityBase)_currentTarget).getCurrentTargetEntity();
        //     if(target != null && target.isDead() == false)
        //         _cameraCenterPosition = target.transform.position + (_currentTarget.transform.position - target.transform.position) * 0.5f;
        // }

        _cameraPosition = position;
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {            
        Vector3 targetDirection = (targetPosition - _cameraCenterPosition);
        _cameraPosition = Vector3.Lerp(_cameraPosition, _cameraCenterPosition + (targetDirection * _cameraCenterRate), _cameraMoveSpeedRate * deltaTime);
        GizmoHelper.instance.drawLine(_cameraCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }
};

public class CameraTargetCenterMode : CameraModeBase
{
    public static float _cameraMoveSpeedRate = 7.3f;

    private Vector3 _currentCenterPosition;


    public override CameraModeType getCameraModeType() => CameraModeType.TargetCenterMode;
    public override void initialize(Vector3 position)
    {
        _cameraPosition = position;
        _targetEntity = null;
        updateCameraCenter();
    }

    public override void progress(float deltaTime, Vector3 targetPosition)
    {
        updateCameraCenter();
        _cameraPosition = _currentCenterPosition;//MathEx.damp(_cameraPosition, _currentCenterPosition, _cameraMoveSpeedRate, deltaTime);
        GizmoHelper.instance.drawLine(_currentCenterPosition, targetPosition, Color.red);
    }

    public override void release()
    {
    }

    public void updateCameraCenter()
    {
        if((_targetEntity == null || _targetEntity.isDead()) && _currentTarget != null)
            _currentCenterPosition = _currentTarget.transform.position;
    }
};




public class CameraControlEx : Singleton<CameraControlEx>
{
    public float _cameraBoundRate = 0.9f;
    private Camera _currentCamera;
    private Transform _currentTarget;

    private CameraModeBase _currentCameraMode;
    private Vector3 _cameraTargetPosition;

    private CameraModeBase[] _cameraModes;
    private GameEntityBase _cameraTargetEntity;

    private PostProcessProfileControl _postProcessProfileControl;

    private float _mainCamSize;
    private float _currentMainCamSize;
    private float _currentZoomSpeed;
	private float _camWidth;
	private float _camHeight;

    private bool _enableShake = false;
    private float _shakeScale = 0f;
    private float _shakeTime = 0f;
    private float _shakeTimer = 0f;
    private float _shakeSpeed = 0f;

    private Vector3 _cameraPosition = Vector3.zero;

    private Vector3 _shakePosition = Vector3.zero;
    private Vector2 _cameraBoundHalf = Vector2.zero;

    public void initialize()
    {
        _currentCamera = Camera.main;
        _cameraPosition = _currentCamera.transform.position;

        _mainCamSize = _currentCamera.orthographicSize;
        _currentMainCamSize = _mainCamSize;
        _currentZoomSpeed = 4f;

        _camHeight = _mainCamSize;
		_camWidth = _camHeight * ((float)Screen.width / (float)Screen.height);
        _cameraBoundHalf = new Vector2(_camWidth, _camHeight) * _cameraBoundRate;

        _cameraModes = new CameraModeBase[(int)CameraModeType.Count];

        _postProcessProfileControl = new PostProcessProfileControl();
        _postProcessProfileControl.updateMaterial(false);

        setCameraMode(CameraModeType.TargetCenterMode);
    }

    public void progress(float deltaTime)
    {
        updateCameraMode(deltaTime);
        
        _postProcessProfileControl.processBlend(deltaTime);

        if(MathEx.equals(_currentCamera.orthographicSize,_currentMainCamSize,float.Epsilon) == true)
			_currentCamera.orthographicSize = _currentMainCamSize;
		else	
			_currentCamera.orthographicSize = Mathf.Lerp(_currentCamera.orthographicSize,_currentMainCamSize,_currentZoomSpeed * deltaTime);

        if(_enableShake)
        {
            _shakeTimer += deltaTime;
            if(_shakeTimer >= _shakeTime)
                _shakeTimer = _shakeTime;
            
            float factor = _shakeTimer * (1f / _shakeTime);
            float shakeScale = Mathf.Lerp(_shakeScale, 0f, factor);
            if(factor >= 1f)
                _enableShake = false;

            _shakePosition = MathEx.lemniscate(_shakeTimer * _shakeSpeed) * shakeScale;
        }

        GizmoHelper.instance.drawRectangle(_cameraPosition,_cameraBoundHalf,Color.green);
    }

    public void setShake(float scale, float speed, float time)
    {
        _shakeScale = scale;
        _shakeTime = time;
        _shakeSpeed = speed;
        _shakeTimer = 0f;

        _enableShake = true;

        _shakePosition = Vector3.zero;
    }

    public void Zoom(float scale)
	{
		if(_currentCamera.orthographicSize > scale)
        {
			_currentCamera.orthographicSize = _currentMainCamSize + scale;
            _currentZoomSpeed = 4f;
        }
	}

    public void setZoomSize(float zoomSize, float speed)
    {
        _currentMainCamSize = zoomSize;
        _currentZoomSpeed = speed;
    }

    public void setDefaultZoomSize()
    {
        _currentMainCamSize = _mainCamSize;
        _currentZoomSpeed = 4f;
    }

    public void setZoomSizeForce(float zoomSize)
    {
        setZoomSize(zoomSize, 4f);
        _currentCamera.orthographicSize = zoomSize;
    }

    public void clearCamera(Vector3 position)
    {
        _currentCameraMode = null;
        _currentTarget = null;

        setDefaultZoomSize();

        Vector3 currentPosition = position;
        currentPosition.z = -10f;
        _cameraPosition = currentPosition + _shakePosition;

        CameraControlEx.Instance().setCameraMode(CameraModeType.PositionMode);
        CameraControlEx.Instance().setCameraTargetPosition(_cameraPosition);
    }

    public void setCameraMode(CameraModeType mode)
    {
        if(_currentCamera == null )
            return;

        if(_currentCameraMode != null && _currentCameraMode.getCameraModeType() == mode)
            return;

        if(_currentCameraMode != null)
            _currentCameraMode.release();

        int index = (int)mode;
        if(_cameraModes[index] == null)
        {
            switch(mode)
            {
                case CameraModeType.ArenaMode:
                    _cameraModes[index] = new CameraArenaMode();
                break;
                case CameraModeType.TargetCenterMode:
                    _cameraModes[index] = new CameraTargetCenterMode();
                break;
                case CameraModeType.TwoTargetMode:
                    _cameraModes[index] = new CameraTwoTargetMode();
                break;
                case CameraModeType.PositionMode:
                    _cameraModes[index] = new CameraPositionMode();
                break;
                default:
                    DebugUtil.assert(false, "잘못된 카메라 모드입니다. 이게 뭐징");
                break;
            }
        }

        _currentCameraMode = _cameraModes[index];
        _currentCameraMode.setCurrentTarget(_currentTarget);
        _currentCameraMode.setCurrentTargetPosition(_cameraTargetPosition);
        _currentCameraMode.initialize(_cameraPosition);
    }

    public void setCameraTarget(Transform obj)
    {
        _currentTarget = obj;
        _currentCameraMode?.setCurrentTarget(_currentTarget);
        _currentCameraMode?.initialize(_cameraPosition);
    }

    public void setCameraTargetPosition(Vector3 targetPosition)
    {
        _cameraTargetPosition = targetPosition;
        _currentCameraMode?.setCurrentTargetPosition(_cameraTargetPosition);
    }

    private void updateCameraMode(float deltaTime)
    {
        if(_currentCameraMode == null)
            return;

        if(_currentTarget is GameEntityBase)
            _currentCameraMode.setCurrentTargetEntity(_cameraTargetEntity);
        else
            _currentCameraMode.setCurrentTargetEntity(null);

        _currentCameraMode.progress(deltaTime,_currentTarget == null ? _cameraTargetPosition : _cameraPosition);
    }

    public Vector3 getCameraPosition()
    {
        Vector3 currentPosition = _currentCameraMode == null ? _cameraPosition : _currentCameraMode.getCameraPosition();
        currentPosition.z = -10f;
        return currentPosition;
    }

    public void setCameraPosition(Vector3 position)
    {
        _cameraPosition = position + _shakePosition;
    }

    public void SyncPosition()
    {
        if(_currentCamera == null || _currentCameraMode == null)
            return;

        _currentCamera.transform.position = _cameraPosition;
    }

    public bool isCameraTargetObject(ObjectBase targetObject)
    {
        return _currentTarget == targetObject;
    }

    public bool IsInCameraBound(Vector3 position, Vector3 cameraPosition, out Vector3 cameraInPosition)
	{
		var bound = GetCamBounds(cameraPosition);
        cameraInPosition = position;
        
        if(position.x < bound.x)
            cameraInPosition.x = bound.x;
        else if(position.x > bound.y)
            cameraInPosition.x = bound.y;

        if(position.y < bound.z)
            cameraInPosition.y = bound.z;
        else if(position.y > bound.w)
            cameraInPosition.y = bound.w;

		return (position.x >= bound.x && position.x <= bound.y && position.y >= bound.z && position.y <= bound.w);
	}

    public Vector3 getRandomPositionInCamera()
    {
        return new Vector3(Random.Range(-_camWidth * 0.5f,_camWidth * 0.5f),Random.Range(-_camHeight * 0.5f,_camHeight * 0.5f));
    }

    public Vector4 GetCamBounds(in Vector3 position) //x min x max y min y max
	{
		return new Vector4(position.x - _cameraBoundHalf.x, position.x + _cameraBoundHalf.x,
							position.y - _cameraBoundHalf.y, position.y + _cameraBoundHalf.y);
	}

    public PostProcessProfileControl getPostProcessProfileControl()
    {
        return _postProcessProfileControl;
    }
    
    public Camera getCurrentCamera()
    {
        return _currentCamera;
    }
}
