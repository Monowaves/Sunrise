using MonoWaves.QoL;
using UnityEngine;

public class PlayerWallJump : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _wallJumpUpForce;
    [SerializeField, Min(0)] private float _wallJumpForwardForce;
    [SerializeField, Min(0)] private float _wallSlidingSpeed;
    [SerializeField, Min(0)] private float _jumpCutMultiplier;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsJumping { get; private set; }

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private bool _isTouchingWall => PlayerBase.Singleton.IsTouchingLeftWall || PlayerBase.Singleton.IsTouchingRightWall;
    private bool _isJumpRequested;
    private bool _wasOnWallLastFrame;

    private void Update() 
    {
        if (PlayerBase.Singleton.WantToJump) _isJumpRequested = true;

        if (PlayerBase.Singleton.JumpReleased) JumpCut();

        if (IsJumping && _rb.velocity.y < 0) IsJumping = false;

        if (_isTouchingWall)
        {
            if (!PlayerBase.Singleton.IsTouchingGround)
            {
                if (_isJumpRequested)
                {
                    Jump();
                    PlayerBase.Singleton.BlockMoveInputs = false;
                }
                else
                {
                    _rb.velocity = new Vector2(_rb.velocity.x, -_wallSlidingSpeed * Time.deltaTime * 50);
                    PlayerBase.Singleton.BlockMoveInputs = true;
                }
            }
            else
            {
                if (_isJumpRequested)
                    Jump();
                
                if (PlayerBase.Singleton.BlockMoveInputs)
                    PlayerBase.Singleton.BlockMoveInputs = false;
            }
        }
        else
        {
            if (_isJumpRequested) _isJumpRequested = false;
        }

        if (!_isTouchingWall && _wasOnWallLastFrame)
        {
            PlayerBase.Singleton.BlockMoveInputs = false;
        }

        _wasOnWallLastFrame = _isTouchingWall;
    }

    private void Jump()
    {
        float direction = PlayerBase.Singleton.IsTouchingLeftWall ? 1 : -1;

        Vector2 upForce = Vector2.up * _wallJumpUpForce;
        Vector2 forwardForce = _wallJumpForwardForce * direction * Vector2.right;
        
        _rb.velocity = upForce + forwardForce;

        IsJumping = true;
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
