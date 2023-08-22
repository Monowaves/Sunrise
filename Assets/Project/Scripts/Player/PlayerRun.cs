using UnityEngine;
using System;
using MonoWaves.QoL;

public class PlayerRun : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _speed;
    [SerializeField, Min(0)] private float _acceleration;
    [SerializeField, Min(0)] private float _decceleration;
    [SerializeField, Range(0, 2)] private float _velocityPower;

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private Vector2 _moveDirection;

    private void Update()
    {
        PlayerBase.Singleton.IsRunning = PlayerBase.Singleton.IsTouchingGround && PlayerBase.Singleton.IsMoving;
    }

    private void FixedUpdate()
    {
        Vector2 defaultDirection = Vector2.right;
        _moveDirection = PlayerBase.Singleton.IsSloped() ? ZVector2Math.ProjectOnPlane(defaultDirection, PlayerBase.Singleton.SlopeNormal) : defaultDirection;

        ApplyRun();
        ApplyFriction();
    }

    private void ApplyRun()
    {
        float moveVector = PlayerBase.Singleton.HorizontalAxis * _speed;
        float vectorDifference = moveVector - _rb.velocity.x;
        float accelerationRate = PlayerBase.Singleton.IsMoving ? _acceleration : _decceleration;
        float targetVector = Mathf.Pow(Mathf.Abs(vectorDifference) * accelerationRate, _velocityPower) * Math.Sign(vectorDifference);

        _rb.AddForce(targetVector * _moveDirection);
    }

    private void ApplyFriction()
    {
        if (PlayerBase.Singleton.IsTouchingGround && PlayerBase.Singleton.IsMoving)
        {
            float applyAmount = Mathf.Min(Mathf.Abs(_rb.velocity.x), PlayerBase.Singleton.FrictionAmount);
            applyAmount *= Mathf.Sign(_rb.velocity.x);
            _rb.AddForce(_moveDirection * -applyAmount, ForceMode2D.Impulse);
        }
    }

    public void Footstep()
    {
        AudioSystem.Play(PlayerBase.Singleton.Footsteps, AudioOptions.HalfVolumeWithVariation);

        Vector3 position = new Vector2
        (
            transform.position.x,
            transform.position.y - PlayerBase.Singleton.BoxCollider.size.y / 2
        );

        Instantiate(PlayerBase.Singleton.FootstepDust, position, Quaternion.Euler(0, PlayerBase.Singleton.Facing == PlayerFacing.Left ? 180 : 0, 0));
    }
}
