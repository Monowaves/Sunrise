using System.Collections;
using UnityEngine;

public class PlayerGroundSlam : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _airTime = 0.5f;
    [SerializeField, Min(0)] private float _dashSpeed;
    [SerializeField, Min(0)] private float _slamDamage;
    [SerializeField] private BoxChecker _slamChecker;
    [SerializeField] private GameObject _slamEffect;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsDashing { get; private set;}

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private void Update() 
    {
        bool isInAir = !PlayerBase.Singleton.IsTouchingGround && !PlayerBase.Singleton.IsTouchingRightWall && !PlayerBase.Singleton.IsTouchingLeftWall;

        if (PlayerBase.Singleton.CtrlPressed && isInAir && !IsDashing)
            StartCoroutine(GroundSlam());
    }

    private IEnumerator GroundSlam()
    {
        IsDashing = true;
        PlayerBase.Singleton.BlockMoveInputs = true;
        PlayerBase.Singleton.BlockGravity = true;
        PlayerHealth.Singleton.StartInvincible();

        _rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(_airTime);

        while (!PlayerBase.Singleton.IsTouchingGround)
        {
            _rb.velocity = _dashSpeed * Vector2.down;
            yield return null;
        }

        Vector2 position = transform.position;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position + _slamChecker.Offset, _slamChecker.Size, 0f, _slamChecker.Mask); 

        foreach (var other in colliders)
        {
            if (other.TryGetComponent(out EnemyBase enemy))
            {
                enemy.Hit(_slamDamage);
            }
        }

        Instantiate(_slamEffect, transform.position + Vector3.down, Quaternion.identity);

        IsDashing = false;
        PlayerBase.Singleton.BlockMoveInputs = false;
        PlayerBase.Singleton.BlockGravity = false;
        PlayerHealth.Singleton.StopInvincible();
    }

    private void OnDrawGizmos() 
    {
        Vector2 position = transform.position;

        Gizmos.color = _slamChecker.GizmosColor;
        Gizmos.DrawWireCube(position + _slamChecker.Offset, _slamChecker.Size);
    }
}
