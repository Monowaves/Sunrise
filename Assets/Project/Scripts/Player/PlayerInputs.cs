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

    public bool BlockMoveInputs { get; set; }

    private void Awake() => Singleton = this;
    
    private void Update() 
    {
        HorizontalAxis = BlockMoveInputs ? 0 : Keyboard.AxisFrom(KeyCode.A, KeyCode.D);

        IsMoving = HorizontalAxis != 0;
        Facing = HorizontalAxis == -1 ? PlayerFacing.Left : PlayerFacing.Right;

        WantToJump = Keyboard.IsPressed(KeyCode.Space);
        JumpReleased = Keyboard.IsReleased(KeyCode.Space);
    }
}

public enum PlayerFacing
{
    Left,
    Right
}
