using MonoWaves.QoL;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Pickupable : MonoBehaviour
{
    [SerializeField] private LayerMask _attractionLayerMask;

    public virtual void OnPlayerPickup() { }
    public virtual void OnEnemyPickup(EnemyBase enemy) { }

    [SerializeField] private Collider2D _collider;
    [SerializeField] private Rigidbody2D _rigidbody;

    private bool _isAttracted;

    private void OnValidate() 
    {
        if (TryGetComponent(out _rigidbody))
        {
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        TryGetComponent(out _collider);
    }

    private void Update() 
    {
        Collider2D other = Physics2D.OverlapCircle(transform.position, 5f, _attractionLayerMask);

        _isAttracted = other != null;
        _collider.isTrigger = _isAttracted;

        if (_isAttracted)
        {
            _rigidbody.gravityScale = 0f;
            _rigidbody.MovePosition(Vector3.MoveTowards(transform.position, other.transform.position, Time.deltaTime * 50f));
        }
        else
        {
            _rigidbody.gravityScale = 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent(out PlayerBase _))
        {
            OnPlayerPickup();
            return;
        }

        if (other.TryGetComponent(out EnemyBase enemy))
        {
            OnEnemyPickup(enemy);
        }
    }
}
