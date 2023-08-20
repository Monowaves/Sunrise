using MonoWaves.QoL;
using UnityEngine;

public class PlayerWallJump : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _wallJumpUpForce;
    [SerializeField, Min(0)] private float _wallJumpForwardForce;
    [SerializeField, Min(0)] private float _wallSlidingSpeed;
    [SerializeField, Min(0)] private float _jumpCutMultiplier;
    [SerializeField, Min(0)] private float _jumpBufferTime;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsJumping { get; private set; }
    [field: SerializeField, ReadOnly] public float LastJumpTime { get; private set; }

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private bool _isTouchingWall => PlayerBase.Singleton.IsTouchingLeftWall || PlayerBase.Singleton.IsTouchingRightWall;
    private bool _wasOnWallLastFrame;

    private void Update() 
    {
        LastJumpTime -= Time.deltaTime;

        if (PlayerBase.Singleton.WantToJump) 
        {
            LastJumpTime = _jumpBufferTime;
        }

        if (PlayerBase.Singleton.JumpReleased) JumpCut();

        if (IsJumping && _rb.velocity.y < 0) 
        {
            IsJumping = false;
            PlayerBase.Singleton.IsJumping = false;
        }

        if (_isTouchingWall)
        {
            if (!PlayerBase.Singleton.IsTouchingGround)
            {
                if (LastJumpTime > 0)
                {
                    PlayerBase.Singleton.IsWallSliding = false;

                    Jump();
                    PlayerBase.Singleton.BlockMoveInputs = false;
                }
                else
                {
                    PlayerBase.Singleton.IsWallSliding = true;

                    _rb.velocity = new Vector2(_rb.velocity.x, -_wallSlidingSpeed * Time.deltaTime * 50);
                    PlayerBase.Singleton.BlockMoveInputs = true;
                }
            }
            else
            {
                if (LastJumpTime > 0)
                    Jump();
                
                if (PlayerBase.Singleton.BlockMoveInputs)
                    PlayerBase.Singleton.BlockMoveInputs = false;
            }
        }
        else
        {
            if (PlayerBase.Singleton.IsWallSliding) PlayerBase.Singleton.IsWallSliding = false;
        }

        if (!_isTouchingWall && _wasOnWallLastFrame)
        {
            PlayerBase.Singleton.BlockMoveInputs = false;
        }

        _wasOnWallLastFrame = _isTouchingWall;
    }

    private void Jump()
    {
        Vector2 upForce = Vector2.up * _wallJumpUpForce;
        Vector2 forwardForce = _wallJumpForwardForce * -PlayerBase.Singleton.WallDirection * Vector2.right;
        
        _rb.velocity = upForce + forwardForce;

        IsJumping = true;
        PlayerBase.Singleton.IsJumping = true;
    }

    private void JumpCut()
    {
        if (IsJumping)
        {
            float direction = PlayerBase.Singleton.IsTouchingLeftWall ? 1 : -1;
            _rb.AddForce((1 - _jumpCutMultiplier) * direction * _rb.velocity * Vector2.right, ForceMode2D.Impulse);
        }
    }
}
