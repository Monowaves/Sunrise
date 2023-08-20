using MonoWaves.QoL;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _jumpForce;
    [SerializeField, Range(0, 1)] private float _jumpCutMultiplier;
    [SerializeField, Min(0)] private float _jumpBufferTime;
    [SerializeField, Min(0)] private float _coyoteTime;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsJumping { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsJumpReleased { get; private set; }
    [field: SerializeField, ReadOnly] public float LastGroundedTime { get; private set; }
    [field: SerializeField, ReadOnly] public float LastJumpTime { get; private set; }
    
    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private void Update() 
    {
        LastGroundedTime -= Time.deltaTime;
        LastJumpTime -= Time.deltaTime;

        if (PlayerBase.Singleton.WantToJump) OnJumpDown();
        if (PlayerBase.Singleton.JumpReleased) OnJumpUp();

        if (PlayerBase.Singleton.IsTouchingGround)
        {
            LastGroundedTime = _coyoteTime;

            if (PlayerBase.Singleton.IsFalling)
            {
                PlayerBase.Singleton.IsFalling = false;
            }
        }

        if ( _rb.velocity.y < 0)
        {
            if (!PlayerBase.Singleton.IsTouchingGround && !PlayerBase.Singleton.IsFalling)
                PlayerBase.Singleton.IsFalling = true;

            if (IsJumping)
            {
                IsJumping = false;
                PlayerBase.Singleton.IsJumping = false;
            }
        }

        if (LastGroundedTime > 0 && LastJumpTime > 0 && !IsJumping)
        {
            Jump();
        }
    }

    private void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

        LastGroundedTime = 0;
        LastJumpTime = 0;

        IsJumping = true;
        IsJumpReleased = false;

        PlayerBase.Singleton.IsJumping = true;
    }

    private void OnJumpDown()
    {
        LastJumpTime = _jumpBufferTime;
    }

    private void OnJumpUp()
    {
        if (_rb.velocity.y > 0 && IsJumping)
        {
            _rb.AddForce(Vector2.down * _rb.velocity * (1 - _jumpCutMultiplier), ForceMode2D.Impulse);
        }

        IsJumpReleased = true;
        LastJumpTime = 0;
    }
}
