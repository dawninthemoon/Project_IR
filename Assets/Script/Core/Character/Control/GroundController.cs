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
    } = 5f;
    #endregion

    #region Ground Check
    private bool _isGrounded;
    public float _groundOffset = 0.1f;
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
        Vector2 point = transform.position + Vector3.down * _groundOffset;
        Vector2 size = new Vector2(transform.localScale.x, transform.localScale.y);
        if (Physics2D.OverlapBox(point, size, 0f, _filter.NoFilter(), _adjustGrounds) > 0f)
        {
            _isGrounded = true;
            _surfacePosition = Physics2D.ClosestPoint(transform.position, _adjustGrounds[0]);
        }
        else {
            _isGrounded = false;
        }
    }
    
    private void CalculateVelocity() {
        VerticalVelocity += Gravity * GravityScale * Time.deltaTime;
        if (_isGrounded && VerticalVelocity < 0f)
        {
            float floorHeight = 0.7f;
            VerticalVelocity = 0f;
            transform.position = new Vector3(transform.position.x, _surfacePosition.y + floorHeight, transform.position.z);
        }

        Vector3 moveAmount = new Vector3(0f, VerticalVelocity, 0f);
        transform.Translate(moveAmount * Time.deltaTime);
    }
}