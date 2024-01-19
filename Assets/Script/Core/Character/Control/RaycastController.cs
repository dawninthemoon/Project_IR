using UnityEngine;
using System.Collections;

public class RaycastController {
	protected float _skinWidth = 0.005f;
	[SerializeField] private float _dstBetweenRays = 0.06f;
	protected int _horizontalRayCount;
	protected int _verticalRayCount;

	protected float _horizontalRaySpacing;
	protected float _verticalRaySpacing;
	protected RaycastOrigins raycastOrigins;
    private Vector2 _targetRadius;

	public virtual void Initialize(Vector2 targetRadius) {
        _targetRadius = targetRadius;
        _targetRadius.x -= (_skinWidth * 2f);
		_targetRadius.y -= (_skinWidth * 2f);
		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins(Vector3 position) {

		raycastOrigins.bottomLeft = new Vector2(position.x - _targetRadius.x, position.y - _targetRadius.y + _skinWidth);
		raycastOrigins.bottomRight = new Vector2(position.x + _targetRadius.x, position.y - _targetRadius.y + _skinWidth);
		raycastOrigins.topLeft = new Vector2(position.x - _targetRadius.x, position.y + _targetRadius.y);
		raycastOrigins.topRight = new Vector2(position.x + _targetRadius.x, position.y + _targetRadius.y);
	}

	public void CalculateRaySpacing() {
		float boundsWidth = _targetRadius.x * 2f + (-_skinWidth * 2f);
		float boundsHeight = _targetRadius.y * 2f + (-_skinWidth * 2f);
		
		_horizontalRayCount = Mathf.RoundToInt(boundsHeight / _dstBetweenRays);
		_verticalRayCount = Mathf.RoundToInt(boundsWidth / _dstBetweenRays);
		
		_horizontalRaySpacing = _targetRadius.y * 2f / (_horizontalRayCount - 1);
		_verticalRaySpacing = _targetRadius.x * 2f / (_verticalRayCount - 1);
	}
	
	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}