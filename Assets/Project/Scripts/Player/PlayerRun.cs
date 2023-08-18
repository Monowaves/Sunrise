using UnityEngine;
using System;

public class PlayerRun : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _speed;
    [SerializeField, Min(0)] private float _acceleration;
    [SerializeField, Min(0)] private float _decceleration;
    [SerializeField, Range(0, 2)] private float _velocityPower;

    private Rigidbody2D _rb => PlayerPhysics.Singleton.Rigidbody;

    private void FixedUpdate()
    {
        ApplyRun();
        ApplyFriction();
    }

    private void ApplyRun()
    {
        float moveVector = PlayerInputs.Singleton.HorizontalAxis * _speed;
        float vectorDifference = moveVector - _rb.velocity.x;
        float accelerationRate = PlayerInputs.Singleton.IsMoving ? _acceleration : _decceleration;
        float targetVector = Mathf.Pow(Mathf.Abs(vectorDifference) * accelerationRate, _velocityPower) * Math.Sign(vectorDifference);

        _rb.AddForce(targetVector * Vector2.right);
    }

    private void ApplyFriction()
    {
        if (PlayerChecker.Singleton.IsTouchingGround && PlayerInputs.Singleton.IsMoving)
        {
            float applyAmount = Mathf.Min(Mathf.Abs(_rb.velocity.x), PlayerPhysics.Singleton.FrictionAmount);
            applyAmount *= Mathf.Sign(_rb.velocity.x);
            _rb.AddForce(Vector2.right * -applyAmount, ForceMode2D.Impulse);
        }
    }
}
