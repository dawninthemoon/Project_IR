using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : Singleton<CameraControl> {

	public Camera mainCam{get{return _main;}}
	public Vector3 position{get{return _position;}}

	private ObjectBase _followTarget = null;
	private float _followSpeed = 10f;

	private Transform _tp;
	private Camera _main;


	private Vector3 _position;
	private Vector3 _velocity;
	
	private Vector3 _targetPosition;

	private float _mainCamSize;
	private float camWidth;
	private float camHeight;

	private bool _isDelayed = false;

	public void assign()
	{
		_main = Camera.main;
		_tp = _main.transform;
		_position = _tp.position;

		camHeight = _main.orthographicSize;
		camWidth = camHeight * ((float)Screen.width / (float)Screen.height);

		_mainCamSize = _main.orthographicSize;
	}

	public Vector4 GetCamBounds() //x min x max y min y max
	{
		return new Vector4(_position.x - camWidth, _position.x + camWidth,
							_position.y - camHeight, _position.y + camHeight);
	}

	public bool IsInCamera(Vector3 pos)
	{
		var bound = GetCamBounds();
		return (pos.x >= bound.x && pos.x <= bound.y && pos.y >= bound.z && pos.y <= bound.w);
	}

	public void SetTarget(ObjectBase target)
	{
		_followTarget = target;

	}

	public void SetSpeed(float value)
	{
		_followSpeed = value;
	}


	public void initialize()
	{

	}

	public Vector3 ScreenToWorldMouse()
	{
		Vector3 pos = _main.ScreenToWorldPoint(Input.mousePosition);
		pos.z = 0f;
		return pos;
	}

	public void progress(float deltaTime)
	{

		if(_followTarget != null)
		{
			if(_isDelayed == false)
				updateTargetPosition(_followTarget.transform);

			Follow(deltaTime);

			
		}


		if(MathEx.equals(_main.orthographicSize,_mainCamSize,float.Epsilon) == true)
			_main.orthographicSize = _mainCamSize;
		else	
			_main.orthographicSize = Mathf.Lerp(_main.orthographicSize,_mainCamSize,4f * deltaTime);
	}

	public void Zoom(float scale)
	{
		if(_main.orthographicSize > scale)
			_main.orthographicSize = scale;
	}

	public void setDelay(bool value)
	{
		_isDelayed = value;
	}

	public void release()
	{

	}

	Vector3 targetPos;

	float cameraAngle = 0f;

	private void updateTargetPosition(Transform targetTransform)
	{
		_targetPosition = targetTransform.position;
	}

	public void Follow(float deltaTime)
	{
		Vector3 pos = (Vector2)_position;
		Vector3 followPos = _targetPosition;

		Vector3 direction = _followTarget.getDirection();
		float angle = Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;

		float angleDist = MathEx.abs(cameraAngle - angle);
		cameraAngle = Mathf.LerpAngle(cameraAngle, angle, 480f * deltaTime / angleDist);

		targetPos = followPos;// + /*_followTarget.direction.normalized*/ MathEx.angleToDirection(cameraAngle * Mathf.Deg2Rad) * 0.65f;

		float dist = Vector2.Distance(targetPos,pos);
		_velocity = (targetPos - pos).normalized * dist * _followSpeed;

		_position += _velocity * deltaTime;
		_position.z = -10f;
	}

	public void SyncPosition()
	{
		
		_tp.SetPositionAndRotation(_position, _tp.rotation);
	}
}
