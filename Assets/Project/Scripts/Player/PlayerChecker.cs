using System;
using MonoWaves.QoL;
using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    public static PlayerChecker Singleton { get; private set; }

    [Header("Properties")]
    [SerializeField] private BoxChecker _groundChecker;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsGrounded { get; private set; }

    private void Awake() => Singleton = this;

    private void Update() 
    {
        if (_groundChecker == null) throw new ArgumentException("Ground checker is null");
        Vector2 position = transform.position;

        IsGrounded = Physics2D.OverlapBox(position + _groundChecker.Offset, _groundChecker.Size, 0f, _groundChecker.Mask);
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = _groundChecker.GizmosColor;
        Vector2 position = transform.position;

        Gizmos.DrawWireCube(position + _groundChecker.Offset, _groundChecker.Size);
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
