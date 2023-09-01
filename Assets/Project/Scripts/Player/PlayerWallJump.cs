using System;
using MonoWaves.QoL;
using UnityEngine;

public class PlayerWallJump : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private Vector2 _jumpForce;
    [SerializeField, Min(0)] private float _jumpDuration;
    [SerializeField, Min(0)] private float _jumpTime;
    [SerializeField, Min(0)] private float _slidingSpeed;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsSliding { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsJumping { get; private set; }

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;
    
    private float _jumpTimer;
    private float _direction;

    private void Update() 
    {
        WallSlide();
        WallJump();
    }

    private void WallSlide()
    {
        if (PlayerBase.Singleton.IsTouchingGround || !PlayerBase.Singleton.IsTouchingWall) 
        {
            SetSliding(false);
            return;
        }

        SetSliding(true);
        _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y.ClampMinimum(-_slidingSpeed));
    }

    private void WallJump()
    {
        if (!IsSliding)
        {
            _jumpTimer -= Time.deltaTime;

            return;
        }

        SetJumping(false);
        _direction = PlayerBase.Singleton.Facing == PlayerFacing.Left ? 1 : -1;

        _jumpTimer = _jumpTime;

        CancelInvoke(nameof(StopJump));

        if (PlayerBase.Singleton.WantToJump && _jumpTimer > 0f)
        {
            SetJumping(true);
            _rb.velocity = new Vector2(_direction * _jumpForce.x, _jumpForce.y);

            _jumpTimer = 0f;
            Invoke(nameof(StopJump), _jumpDuration);

            PlayerBase.Singleton.JumpSound.Play(AudioOptions.HalfVolumeWithVariation);
        }
    }

    private void SetSliding(bool value)
    {
        if (IsSliding == value) return;

        PlayerBase.Singleton.BlockMoveInputs = value;
        PlayerBase.Singleton.IsWallSliding = value;
        IsSliding = value;
    }

    private void SetJumping(bool value)
    {
        if (IsJumping == value) return;

        IsJumping = value;
    }

    private void StopJump() => SetJumping(false);
}
