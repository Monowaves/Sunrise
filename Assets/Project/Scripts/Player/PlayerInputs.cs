using System;
using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs Singleton { get; private set; }

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsMoving { get; private set; }
    [field: SerializeField, ReadOnly] public float HorizontalAxis { get; private set; }
    [field: SerializeField, ReadOnly] public bool WantToJump { get; private set; }

    private void Awake() => Singleton = this;

    public Action OnJumpDown { get; set; }
    public Action OnJumpUp { get; set; }
    
    private void Update() 
    {
        HorizontalAxis = Keyboard.AxisFrom(KeyCode.A, KeyCode.D);
        IsMoving = HorizontalAxis != 0;
        WantToJump = Keyboard.IsPressed(KeyCode.Space);

        if (WantToJump) OnJumpDown?.Invoke();

        if (Keyboard.IsReleased(KeyCode.Space)) OnJumpUp?.Invoke();
    }
}
