using MonoWaves.QoL;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs Singleton { get; private set; }

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public float HorizontalAxis { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsMoving { get; private set; }
    [field: SerializeField, ReadOnly] public PlayerFacing Facing { get; private set; }
    [field: SerializeField, ReadOnly] public bool WantToJump { get; private set; }
    [field: SerializeField, ReadOnly] public bool JumpReleased { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsShifting { get; private set; }
    [field: SerializeField, ReadOnly] public bool ShiftPressed { get; private set; }
    [field: SerializeField, ReadOnly] public bool CtrlPressed { get; private set; }
    [field: SerializeField, ReadOnly] public bool BlockMoveInputs { get; set; }

    private void Awake() => Singleton = this;
    
    private void Update() 
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
}

public enum PlayerFacing
{
    Left,
    Right
}
