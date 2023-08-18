using System;
using MonoWaves.QoL;
using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    public static PlayerChecker Singleton { get; private set; }

    [Header("Properties")]
    [SerializeField] private BoxChecker _groundChecker;
    [SerializeField] private BoxChecker _wallLeftChecker;
    [SerializeField] private BoxChecker _wallRightChecker;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsTouchingGround { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsTouchingLeftWall { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsTouchingRightWall { get; private set; }

    private void Awake() => Singleton = this;

    private void Update() 
    {
        if (_groundChecker == null) throw new ArgumentException("Ground checker is null");
        Vector2 position = transform.position;

        IsTouchingGround = Physics2D.OverlapBox(position + _groundChecker.Offset, _groundChecker.Size, 0f, _groundChecker.Mask);

        IsTouchingLeftWall = Physics2D.OverlapBox(position + _wallLeftChecker.Offset, _wallLeftChecker.Size, 0f, _wallLeftChecker.Mask);
        IsTouchingRightWall = Physics2D.OverlapBox(position + _wallRightChecker.Offset, _wallRightChecker.Size, 0f, _wallRightChecker.Mask);
    }

    private void OnDrawGizmosSelected() 
    {
        Vector2 position = transform.position;

        Gizmos.color = _groundChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _groundChecker.Offset, _groundChecker.Size);

        Gizmos.color = _wallLeftChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _wallLeftChecker.Offset, _wallLeftChecker.Size);

        Gizmos.color = _wallRightChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _wallRightChecker.Offset, _wallRightChecker.Size);
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
