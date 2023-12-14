using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour {
    #region Move Variables
    private static readonly float Gravity = -9.81f;
    public float VerticalVelocity 
    {
        get;
        set;
    }
    public float MoveSpeed 
    {
        get;
        set;
    } = 5f;
    public float GravityScale 
    {
        get;
        set;
    } = 0.1f;
    #endregion

    #region Ground Check
    public bool IsGrounded {
        get;
        private set;
    }
    public float _characterRadius = 0.15f;
    private Vector2 _surfacePosition;
    private ContactFilter2D _filter;
    #endregion
    Collider2D[] _adjustGrounds = new Collider2D[1];

    public void Progress()
    {
        GroundCheckProgress();
        CalculateVelocity();
    }

    private void GroundCheckProgress()
    {
        Vector2 point = transform.position + Vector3.down * _characterRadius;
        Vector2 size = new Vector2(_characterRadius, _characterRadius);
        if (Physics2D.OverlapBox(point, size, 0f, _filter.NoFilter(), _adjustGrounds) > 0f)
        {
            IsGrounded = true;
            _surfacePosition = Physics2D.ClosestPoint(transform.position, _adjustGrounds[0]);
        }
        else {
            IsGrounded = false;
        }
    }
    
    private void CalculateVelocity() {
        VerticalVelocity += Gravity * GravityScale * Time.deltaTime;
        if (IsGrounded && VerticalVelocity < 0f)
        {
            //float floorHeight = 1f;
            VerticalVelocity = 0f;
            transform.position = new Vector3(transform.position.x, _surfacePosition.y + _characterRadius, transform.position.z);
        }
    }
}