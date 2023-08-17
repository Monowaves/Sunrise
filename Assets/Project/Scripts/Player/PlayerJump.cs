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
    
    private Rigidbody2D _rb => PlayerPhysics.Singleton.Rigidbody;
    
    private void Start() 
    {
        PlayerInputs.Singleton.OnJumpDown = OnJumpDown;
        PlayerInputs.Singleton.OnJumpUp = OnJumpUp;
    }

    private void Update() 
    {
        LastGroundedTime -= Time.deltaTime;
        LastJumpTime -= Time.deltaTime;

        if (PlayerChecker.Singleton.IsGrounded)
        {
            LastGroundedTime = _coyoteTime;
        }

        if (IsJumping && _rb.velocity.y < 0)
        {
            IsJumping = false;
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
