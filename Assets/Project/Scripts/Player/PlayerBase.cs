using System;
using System.Collections;
using MonoWaves.QoL;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class PlayerBase : MonoBehaviour
{
    public static PlayerBase Singleton { get; private set; }

    [Header("Physics")]
    [SerializeField, Min(0)] private float _gravityScale = 1f;
    [SerializeField, Min(0)] private float _fallGravityScale = 1f;
    [SerializeField, Min(0)] private float _maxGravityAcceleration = 50f;
    [SerializeField, Min(0)] private Vector2 _knockbackForce;
    [field: SerializeField, Min(0)] public float FrictionAmount { get; private set;}
    [SerializeField, Min(0)] private float _maxSlopeAngle = 45f;
    [SerializeField] private PhysicsMaterial2D _material;

    [Header("Checkers")]
    [SerializeField] private BoxChecker _ceilChecker = new();
    [SerializeField] private BoxChecker _groundChecker = new();
    [SerializeField] private RayChecker _slopeChecker = new();

    [field: Header("Components")]
    [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
    [field: SerializeField] public BoxCollider2D BoxCollider { get; private set; }

    [field: Header("Audio")]
    [field: SerializeField] public AudioClip[] Footsteps { get; private set; }
    [field: SerializeField] public AudioClip[] LandingSounds { get; private set; }
    [field: SerializeField] public AudioClip JumpSound { get; private set; }
    [field: SerializeField] public AudioClip GroundSlamSound { get; private set; }
    [field: SerializeField] public AudioClip DamageSound { get; private set; }
    [field: SerializeField] public AudioSource FallNoise { get; private set; }

    [field: Header("Particles")]
    [field: SerializeField] public GameObject FootstepDust { get; private set; }
    [field: SerializeField] public GameObject[] SlidingParticles { get; private set; }
    [field: SerializeField] public GameObject SlamEffect { get; private set; }

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool BlockGravity { get; set; }

    [field: SerializeField, ReadOnly] public bool IsTouchingGround { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsTouchingCeil { get; private set; }

    [field: SerializeField, ReadOnly] public bool IsTouchingLeftWall { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsTouchingRightWall { get; private set; }
    [field: SerializeField, ReadOnly] public Vector2 SlopeNormal { get; set; }
    [field: SerializeField, ReadOnly] public int WallDirection { get; private set; }

    [field: SerializeField, ReadOnly] public float HorizontalAxis { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsMoving { get; private set; }
    [field: SerializeField, ReadOnly] public PlayerFacing Facing { get; private set; }
    [field: SerializeField, ReadOnly] public bool WantToJump { get; private set; }
    [field: SerializeField, ReadOnly] public bool JumpReleased { get; private set; }
    [field: SerializeField, ReadOnly] public bool WantToSlide { get; private set; }
    [field: SerializeField, ReadOnly] public bool SlidePressed { get; private set; }
    [field: SerializeField, ReadOnly] public bool WantToSlam { get; private set; }

    [field: SerializeField, ReadOnly] public bool IsRunning { get; set; }
    [field: SerializeField, ReadOnly] public bool IsFalling { get; set; }
    [field: SerializeField, ReadOnly] public bool IsJumping { get; set; }
    [field: SerializeField, ReadOnly] public bool IsWallSliding { get; set; }
    [field: SerializeField, ReadOnly] public bool IsGroundSlamPrepare { get; set; }
    [field: SerializeField, ReadOnly] public bool IsGroundSlamDash { get; set; }
    [field: SerializeField, ReadOnly] public bool IsGroundStandUp { get; set; }
    [field: SerializeField, ReadOnly] public bool IsSliding { get; set; }
    [field: SerializeField, ReadOnly] public float SlidingMomentum { get; set; }

    [field: SerializeField, ReadOnly] public bool BlockMoveInputs { get; set; }
    [field: SerializeField, ReadOnly] public bool BlockJumpInputs { get; set; }
    [field: SerializeField, ReadOnly] public bool BlockSlamInputs { get; set; }
    [field: SerializeField, ReadOnly] public bool BlockAllInputs { get; set; }

    [field: SerializeField, ReadOnly] public bool DontWriteMoveInputs { get; set; }
    [field: SerializeField, ReadOnly] public bool DontWriteJumpInputs { get; set; }
    [field: SerializeField, ReadOnly] public bool DontWriteAllInputs { get; set; }
    
    [field: SerializeField, ReadOnly] public bool BlockWallChecker { get; set; }

    [field: SerializeField, ReadOnly] public bool DontWriteGroundChecker { get; set; }
    [field: SerializeField, ReadOnly] public bool DontWriteCheckers { get; set; }

    public bool IsTouchingWall => IsTouchingLeftWall || IsTouchingRightWall;
    private Vector2 PlayerCeil => Vector2.up * ((BoxCollider.size.y / 2) + BoxCollider.offset.y);
    private Vector2 Position => transform.position.ToVector2() + Vector2.right * BoxCollider.offset.x;
    
    private BoxChecker _leftWallChecker;
    private BoxChecker _rightWallChecker;

    private RayChecker _leftSlopeChecker;
    private RayChecker _rightSlopeChecker;

    private float _slideTimer;
    private float _fallTimer;

    private void Awake()
    {
        RayChecker leftSlopeChecker = _slopeChecker.Clone();
        leftSlopeChecker.Offset.x = -(BoxCollider.size.x / 2) + 0.05f;
        RayChecker rightSlopeChecker = _slopeChecker.Clone();
        rightSlopeChecker.Offset.x = BoxCollider.size.x / 2 - 0.05f;

        _leftSlopeChecker = leftSlopeChecker;
        _rightSlopeChecker = rightSlopeChecker;

        SetFullSize();
        Singleton = this;
    }

    public void SetHalfSize()
    {
        BoxCollider.offset = PlayerInformation.ColliderHalfOffset;
        BoxCollider.size = PlayerInformation.ColliderHalfSize;

        RegenerateWallCheckers();
    }

    public void SetFullSize()
    {
        BoxCollider.offset = PlayerInformation.ColliderFullOffset;
        BoxCollider.size = PlayerInformation.ColliderFullSize;

        RegenerateWallCheckers();
    }

    public void RegenerateWallCheckers()
    {
        _leftWallChecker = new()
        {
            Offset = new Vector2(-BoxCollider.size.x / 2, BoxCollider.offset.y),
            Size = new Vector2(0.1f, BoxCollider.size.y - 0.25f),
            Mask = ZLayerExtensions.MapLayerMask(),
            GizmosColor = "#0398fc".HexToColor()
        };

        _rightWallChecker = new()
        {
            Offset = new Vector2(BoxCollider.size.x / 2, BoxCollider.offset.y),
            Size = new Vector2(0.1f, BoxCollider.size.y - 0.25f),
            Mask = ZLayerExtensions.MapLayerMask(),
            GizmosColor = "#0398fc".HexToColor()
        };
    }

    private void OnValidate() 
    {
        if (TryGetComponent(out Rigidbody2D rb))
        {
            Rigidbody = rb;
            Rigidbody.freezeRotation = true;
            Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Rigidbody.isKinematic = false;
        }

        if (TryGetComponent(out BoxCollider2D bc))
        {
            BoxCollider = bc;
            BoxCollider.isTrigger = false;
            BoxCollider.sharedMaterial = _material;
            BoxCollider.edgeRadius = 0f;
        }
    }

    private void Update() 
    {
        Inputs();
        Checking();

        if (IsFalling)
            _fallTimer += Time.deltaTime;
        else
            _fallTimer = 0f;

        if (_fallTimer > 1.5f)
            FallNoise.volume = (_fallTimer - 1.5f) / 15;
        else
            FallNoise.volume = 0f;
    }

    private void Checking()
    {
        if (DontWriteCheckers) return;

        if (!DontWriteGroundChecker) IsTouchingGround = Physics2D.OverlapBox(Position + _groundChecker.Offset, _groundChecker.Size, 0f, _groundChecker.Mask);
        IsTouchingCeil = Physics2D.OverlapBox(Position + PlayerCeil, _ceilChecker.Size, 0f, _ceilChecker.Mask);

        IsTouchingLeftWall = !BlockWallChecker && Physics2D.OverlapBox(Position + _leftWallChecker.Offset, _leftWallChecker.Size, 0f, _leftWallChecker.Mask);
        IsTouchingRightWall = !BlockWallChecker && Physics2D.OverlapBox(Position + _rightWallChecker.Offset, _rightWallChecker.Size, 0f, _rightWallChecker.Mask);

        if (IsTouchingLeftWall) WallDirection = -1;
        else if (IsTouchingRightWall) WallDirection = 1;
        else WallDirection = 0;

        RaycastHit2D leftHitInfo = Physics2D.Raycast
        (
            origin: Position + _leftSlopeChecker.Offset, 
            direction: _leftSlopeChecker.Direction,  
            _leftSlopeChecker.Distance, 
            _leftSlopeChecker.Mask
        );

        RaycastHit2D rightHitInfo = Physics2D.Raycast
        (
            origin: Position + _rightSlopeChecker.Offset, 
            direction: _rightSlopeChecker.Direction,  
            _rightSlopeChecker.Distance, 
            _rightSlopeChecker.Mask
        );

        Vector2 targetNormal = Facing == PlayerFacing.Left ? leftHitInfo.normal : rightHitInfo.normal;

        if (Vector2.Angle(Vector2.up, targetNormal).IsInRange(-_maxSlopeAngle, _maxSlopeAngle)) 
            SlopeNormal = targetNormal;
        else
            SlopeNormal = Vector2.up;
    }

    private void Inputs()
    {
        if (DontWriteAllInputs) return;

        if (BlockAllInputs)
        {
            HorizontalAxis = 0;
            IsMoving = false;
            WantToJump = false;
            JumpReleased = false;
            WantToSlide = false;
            SlidePressed = false;
            WantToSlam = false;

            return;
        }

        if (!DontWriteMoveInputs) HorizontalAxis = BlockMoveInputs ? 0 : Keyboard.AxisFrom(KeyCode.A, KeyCode.D);

        IsMoving = HorizontalAxis != 0;
        
        if (HorizontalAxis < 0)
            Facing = PlayerFacing.Left;
        else if (HorizontalAxis > 0)
            Facing = PlayerFacing.Right;   

        if (!DontWriteJumpInputs)
        {
            if (BlockJumpInputs)
            {
                WantToJump = false;
                JumpReleased = false;
            }
            else
            {
                WantToJump = Keyboard.IsPressed(KeyCode.Space);
                JumpReleased = Keyboard.IsReleased(KeyCode.Space);
            }
        }

        WantToSlide = _slideTimer > 0 || Keyboard.IsHolding(KeyCode.LeftShift);
        SlidePressed = Keyboard.IsPressed(KeyCode.LeftShift);

        if (SlidePressed) _slideTimer = 0.1f;
        else _slideTimer -= Time.deltaTime;

        WantToSlam = !BlockSlamInputs && Keyboard.IsPressed(KeyCode.LeftControl);
    }

    private void FixedUpdate()
    {
        Gravity();
    }

    private void Gravity()
    {
        if (BlockGravity)
        {
            Rigidbody.gravityScale = 0;
            return;
        }

        if (IsSloped())
        {
            Rigidbody.gravityScale = 0;
        }
        else
        {
            if (Rigidbody.velocity.y < 0)
                Rigidbody.gravityScale = _fallGravityScale;
            else
                Rigidbody.gravityScale = _gravityScale;
        }

        if (Rigidbody.velocity.y < -_maxGravityAcceleration && !IsGroundSlamDash)
        {
            Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, -_maxGravityAcceleration);
        }
    }

    public bool IsSloped()
    {
        if (!IsTouchingGround) return false;

        return SlopeNormal != Vector2.up;
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = _groundChecker.GizmosColor;
        Gizmos.DrawWireCube(Position + _groundChecker.Offset, _groundChecker.Size);
        Gizmos.color = _ceilChecker.GizmosColor;
        Gizmos.DrawWireCube(Position + PlayerCeil, _ceilChecker.Size);

        if (_leftSlopeChecker != null)
        {
            Gizmos.color = _leftSlopeChecker.GizmosColor;
            Gizmos.DrawLine(Position + _leftSlopeChecker.Offset, _leftSlopeChecker.Direction * _leftSlopeChecker.Distance + Position + _leftSlopeChecker.Offset);
        }

        if (_rightSlopeChecker != null)
        {
            Gizmos.color = _rightSlopeChecker.GizmosColor;
            Gizmos.DrawLine(Position + _rightSlopeChecker.Offset, _rightSlopeChecker.Direction * _rightSlopeChecker.Distance + Position + _rightSlopeChecker.Offset);
        }

        if (_leftWallChecker != null)
        {
            Gizmos.color = _leftWallChecker.GizmosColor;
            Gizmos.DrawWireCube(Position + _leftWallChecker.Offset, _leftWallChecker.Size);
        }

        if (_rightWallChecker != null)
        {
            Gizmos.color = _rightWallChecker.GizmosColor;
            Gizmos.DrawWireCube(Position + _rightWallChecker.Offset, _rightWallChecker.Size);
        }
    }

    public void Move(float duration, float direction = 0, bool stopOnceGrounded = false)
    {
        StopCoroutine(nameof(CO_Move));
        StartCoroutine(nameof(CO_Move), new MoveSettings(duration, direction, stopOnceGrounded));
    }

    private struct MoveSettings
    {
        public float Duration;
        public float Direction;
        public bool StopOnceGrounded;

        public MoveSettings(float duration, float direction, bool stop)
        {
            Duration = duration;
            Direction = direction;
            StopOnceGrounded = stop;
        }
    }

    private IEnumerator CO_Move(MoveSettings settings)
    {
        if (settings.Direction != 0)
        {
            DontWriteMoveInputs = true;
            BlockJumpInputs = true;
            HorizontalAxis = settings.Direction;

            float elapsed = 0f;
            while (elapsed < settings.Duration)
            {
                if (IsTouchingWall || (settings.StopOnceGrounded && IsTouchingGround)) break;

                elapsed += Time.deltaTime;
                yield return null;
            }

            DontWriteMoveInputs = false;
            BlockJumpInputs = false;
        }
    }

    public void Knockback(float direction)
    {
        StartCoroutine(nameof(CO_Knockback), direction);
    }

    private IEnumerator CO_Knockback(float direction)
    {
        if ((direction == 1 && IsTouchingRightWall) || (direction == -1 && IsTouchingLeftWall)) yield break;

        BlockAllInputs = true;
        Rigidbody.velocity = Vector2.zero;

        Rigidbody.AddForce(Vector2.up * _knockbackForce.y, ForceMode2D.Impulse);

        float remain = 1f;
        while (remain > 0)
        {
            remain -= Time.deltaTime * 3;

            Rigidbody.AddForce(60f * remain * Time.deltaTime * new Vector2(_knockbackForce.x * direction, 0), ForceMode2D.Impulse);

            yield return null;
        }
        
        yield return new WaitForSecondsRealtime(0.05f);

        BlockAllInputs = false;
    }
}

[Serializable]
public class BoxChecker
{   
    public Vector2 Offset;
    public Vector2 Size;
    public LayerMask Mask;
    public Color GizmosColor = Color.white;
}

[Serializable]
public class RayChecker
{
    public Vector2 Offset;
    public Vector2 Direction;
    public float Distance;
    public LayerMask Mask;
    public Color GizmosColor = Color.white;
}

public enum PlayerFacing
{
    Left,
    Right
}
