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


    private Rigidbody2D _rb => PlayerPhysics.Singleton.Rigidbody;

    private bool _isTouchingWall => PlayerChecker.Singleton.IsTouchingLeftWall || PlayerChecker.Singleton.IsTouchingRightWall;
    private bool _isJumpRequested;
    

    private void Update() 
    {
        if (PlayerInputs.Singleton.WantToJump) _isJumpRequested = true;

        if (IsJumping && _rb.velocity.y < 0) IsJumping = false;

        if (PlayerInputs.Singleton.JumpReleased) JumpCut();

        if (!PlayerChecker.Singleton.IsTouchingGround && _isTouchingWall)
        {
            if (_isJumpRequested)
                Jump();
            else
            {
                _rb.velocity = new Vector2(_rb.velocity.x, -_wallSlidingSpeed * Time.deltaTime * 50);
                PlayerInputs.Singleton.BlockMoveInputs = true;
            }
        }
        else if (PlayerChecker.Singleton.IsTouchingGround && _isTouchingWall)
        {
            if (_isJumpRequested)
                Jump();
        }
        else
        {
            if (PlayerInputs.Singleton.BlockMoveInputs) PlayerInputs.Singleton.BlockMoveInputs = false;
            if (_isJumpRequested) _isJumpRequested = false;
        }
    }

    private void Jump()
    {
        float direction = PlayerChecker.Singleton.IsTouchingLeftWall ? 1 : -1;

        Vector2 upForce = Vector2.up * _wallJumpUpForce;
        Vector2 forwardForce = _wallJumpForwardForce * direction * Vector2.right;
        
        _rb.velocity = upForce + forwardForce;

        IsJumping = true;
    }

    private void JumpCut()
    {
        if (IsJumping)
        {
            float direction = PlayerChecker.Singleton.IsTouchingLeftWall ? 1 : -1;
            _rb.AddForce((1 - _jumpCutMultiplier) * direction * _rb.velocity * Vector2.right, ForceMode2D.Impulse);
        }
    }
}
