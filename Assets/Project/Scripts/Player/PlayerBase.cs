using System;
using MonoWaves.QoL;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class PlayerBase : MonoBehaviour
{
    public static PlayerBase Singleton { get; private set; }

    [Header("Physics")]
    [SerializeField, Min(0)] private float _gravityScale = 1f;
    [SerializeField, Min(0)] private float _fallGravityScale = 1f;
    [field: SerializeField, Min(0)] public float FrictionAmount { get; private set;}
    [SerializeField, Min(0)] private float _maxSlopeAngle = 45f;
    [SerializeField] private PhysicsMaterial2D _material;

    [Header("Checkers")]
    [SerializeField] private BoxChecker _groundChecker = new();
    [SerializeField] private BoxChecker _wallLeftChecker = new();
    [SerializeField] private BoxChecker _wallRightChecker = new();
    [SerializeField] private RayChecker _leftSlopeChecker = new();
    [SerializeField] private RayChecker _rightSlopeChecker = new();

    [field: Header("Components")]
    [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool BlockGravity { get; set; }

    [field: SerializeField, ReadOnly] public bool IsTouchingGround { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsTouchingLeftWall { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsTouchingRightWall { get; private set; }
    [field: SerializeField, ReadOnly] public Vector2 SlopeNormal { get; set; }
    [field: SerializeField, ReadOnly] public int WallDirection { get; private set; }

    [field: SerializeField, ReadOnly] public float HorizontalAxis { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsMoving { get; private set; }
    [field: SerializeField, ReadOnly] public PlayerFacing Facing { get; private set; }
    [field: SerializeField, ReadOnly] public bool WantToJump { get; private set; }
    [field: SerializeField, ReadOnly] public bool JumpReleased { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsShifting { get; private set; }
    [field: SerializeField, ReadOnly] public bool ShiftPressed { get; private set; }
    [field: SerializeField, ReadOnly] public bool CtrlPressed { get; private set; }
    [field: SerializeField, ReadOnly] public bool BlockMoveInputs { get; set; }

    [field: SerializeField, ReadOnly] public bool IsRunning { get; set; }
    [field: SerializeField, ReadOnly] public bool IsFalling { get; set; }
    [field: SerializeField, ReadOnly] public bool IsJumping { get; set; }
    [field: SerializeField, ReadOnly] public bool IsWallJumping { get; set; }
    [field: SerializeField, ReadOnly] public bool IsWallSliding { get; set; }

    private void Awake() => Singleton = this;

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
            bc.isTrigger = false;
            bc.sharedMaterial = _material;
            bc.offset = Vector2.up * -0.075f;
            bc.size = new Vector2(0.9f, 1.85f);
            bc.edgeRadius = 0f;
        }
    }

    private void Update() 
    {
        Inputs();
        Checking();
    }

    private void Checking()
    {
        Vector2 position = transform.position;

        IsTouchingGround = Physics2D.OverlapBox(position + _groundChecker.Offset, _groundChecker.Size, 0f, _groundChecker.Mask);

        IsTouchingLeftWall = Physics2D.OverlapBox(position + _wallLeftChecker.Offset, _wallLeftChecker.Size, 0f, _wallLeftChecker.Mask);
        IsTouchingRightWall = Physics2D.OverlapBox(position + _wallRightChecker.Offset, _wallRightChecker.Size, 0f, _wallRightChecker.Mask);

        if (IsTouchingLeftWall) WallDirection = -1;
        else if (IsTouchingRightWall) WallDirection = 1;
        else WallDirection = 0;

        RaycastHit2D leftHitInfo = Physics2D.Raycast
        (
            origin: position + _leftSlopeChecker.Offset, 
            direction: _leftSlopeChecker.Direction,  
            _leftSlopeChecker.Distance, 
            _leftSlopeChecker.Mask
        );

        RaycastHit2D rightHitInfo = Physics2D.Raycast
        (
            origin: position + _rightSlopeChecker.Offset, 
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
        HorizontalAxis = BlockMoveInputs ? 0 : Keyboard.AxisFrom(KeyCode.A, KeyCode.D);

        IsMoving = HorizontalAxis != 0;
        
        if (HorizontalAxis < 0)
            Facing = PlayerFacing.Left;
        else if (HorizontalAxis > 0)
            Facing = PlayerFacing.Right;   

        WantToJump = Keyboard.IsPressed(KeyCode.Space);
        JumpReleased = Keyboard.IsReleased(KeyCode.Space);

        IsShifting = Keyboard.IsHolding(KeyCode.LeftShift);
        ShiftPressed = Keyboard.IsPressed(KeyCode.LeftShift);

        CtrlPressed = Keyboard.IsPressed(KeyCode.LeftControl);
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
    }

    public bool IsSloped()
    {
        if (!IsTouchingGround) return false;

        return SlopeNormal != Vector2.up;
    }

    private void OnDrawGizmosSelected() 
    {
        Vector2 position = transform.position;

        Gizmos.color = _groundChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _groundChecker.Offset, _groundChecker.Size);

        Gizmos.color = _wallLeftChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _wallLeftChecker.Offset, _wallLeftChecker.Size);

        Gizmos.color = _wallRightChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _wallRightChecker.Offset, _wallRightChecker.Size);;

        Gizmos.color = _leftSlopeChecker.Color;
        Gizmos.DrawLine(position + _leftSlopeChecker.Offset, _leftSlopeChecker.Direction * _leftSlopeChecker.Distance + position + _leftSlopeChecker.Offset);

        Gizmos.color = _rightSlopeChecker.Color;
        Gizmos.DrawLine(position + _rightSlopeChecker.Offset, _rightSlopeChecker.Direction * _rightSlopeChecker.Distance + position + _rightSlopeChecker.Offset);
    }
}

[Serializable]
public class BoxChecker
{
    [field: SerializeField] public Vector2 Offset { get; private set; }
    [field: SerializeField] public Vector2 Size { get; private set; }
    [field: SerializeField] public LayerMask Mask { get; private set; }
    [field: SerializeField] public Color GizmosColor { get; private set; } = Color.white;
}

[Serializable]
public class RayChecker
{
    public Vector2 Offset;
    public Vector2 Direction;
    public float Distance;
    public LayerMask Mask;
    public Color Color = Color.white;
}

public enum PlayerFacing
{
    Left,
    Right
}
