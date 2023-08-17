using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class PlayerPhysics : MonoBehaviour
{
    public static PlayerPhysics Singleton { get; private set; }

    [Header("Properties")]
    [SerializeField, Min(0)] private float _gravityScale = 1f;
    [field: SerializeField, Min(0)] public float FrictionAmount { get; private set;}
    [SerializeField] private PhysicsMaterial2D _material;

    [field: Header("Components")]
    [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }

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
            Rigidbody.gravityScale = _gravityScale;
        }

        if (TryGetComponent(out BoxCollider2D bc))
        {
            bc.isTrigger = false;
            bc.sharedMaterial = _material;
            bc.size = new Vector2(0.8f, 1.6f);
            bc.edgeRadius = 0.1f;
        }
    }
}
