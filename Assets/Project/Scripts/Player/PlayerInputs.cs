using MonoWaves.QoL;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    public static PlayerInputs Singleton { get; private set; }

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsMoving { get; private set; }
    [field: SerializeField, ReadOnly] public float HorizontalAxis { get; private set; }

    private void Awake() => Singleton = this;
    
    private void Update() 
    {
        HorizontalAxis = Keyboard.AxisFrom(KeyCode.A, KeyCode.D);
        IsMoving = HorizontalAxis != 0;
    }
}
