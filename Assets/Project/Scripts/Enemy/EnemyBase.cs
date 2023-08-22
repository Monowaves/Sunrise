using System.Collections;
using InMotion.Engine;
using MonoWaves.QoL;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [field: Header("Components")]
    [field: SerializeField] public Rigidbody2D Rigidbody { get; private set; }
    [field: SerializeField] public BoxCollider2D Collider { get; private set; }
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; private set; }
    [field: SerializeField] public MotionExecutor MotionExecutor { get; private set; }

    [field: Header("Base")]
    [field: SerializeField] public Vector2 HitboxSize { get; private set; } = Vector2.one;
    [field: SerializeField] public Vector2 TriggerZone { get; private set; } = new Vector2(25f, 12f);
    [field: SerializeField] public float StartHealth { get; private set; } = 75f;
    [field: SerializeField] public float ContactDamage { get; private set; } = 5f;
    
    [field: Header("Audio")]
    [field: SerializeField] public AudioClip HitSound { get; private set; }
    [field: SerializeField] public AudioClip DeathSound { get; private set; }

    [field: Header("Particles")]
    [field: SerializeField] public GameObject HitEffect { get; private set; }
    [field: SerializeField] public GameObject DeathEffect { get; private set; }

    protected virtual IEnumerator EnemyBehaviour() => null;
    protected virtual void OnUpdate() { }

    protected Vector2 DirectionToPlayer => (PlayerBase.Singleton.transform.position - transform.position).normalized;
    protected bool IsTouchingGround { get; private set; }
    protected bool IsTriggered { get; private set; }
    protected float Health { get; private set; }

    private void Awake() 
    {
        Health = StartHealth;
    }

    private void Start() 
    {
        StartCoroutine(nameof(EnemyBehaviour));
    }

    private void Update() 
    {
        OnUpdate();

        Vector2 position = transform.position;

        IsTouchingGround = Physics2D.OverlapBox(position + Vector2.down * (HitboxSize.y / 2), new Vector2(HitboxSize.x - 0.1f, 0.15f), 0f, ZLayerExtensions.MapLayerMask());
        IsTriggered = Physics2D.OverlapBox(position, TriggerZone, 0f, ZLayerExtensions.PlayerLayerMask());
        
        SpriteRenderer.flipX = GetFlipX();
    }

    private void OnValidate() 
    {
        if (Collider != null)
        {
            Collider.size = HitboxSize;
        }
    }

    protected virtual bool GetFlipX() => DirectionToPlayer.x < 0f;

    [ContextMenu("Setup Enemy")]
    private void SetupEnemy()
    {
        ClearGameObject();

        gameObject.layer = LayerMask.NameToLayer(Const.ENEMY);

        Rigidbody = gameObject.AddComponent<Rigidbody2D>();

        Rigidbody.isKinematic = false;
        Rigidbody.freezeRotation = true;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        Rigidbody.drag = 1f;
        Rigidbody.mass = 2f;

        Collider = gameObject.AddComponent<BoxCollider2D>();

        Collider.isTrigger = false;

        GameObject sprite = new("Sprite");
        sprite.transform.SetParent(transform);
        sprite.transform.localPosition = Vector2.zero;

        SpriteRenderer = sprite.AddComponent<SpriteRenderer>();
    }

    private void ClearGameObject()
    {
        foreach (var component in gameObject.GetComponents<Component>())
        {
            if (component != this && component != transform) DestroyImmediate(component);
        }

        foreach (Transform child in gameObject.transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.TryGetComponent(out PlayerHealth player))
        {
            float direction = (player.transform.position - transform.position).x < 0 ? -1 : 1;

            player.Hit(ContactDamage);
            PlayerBase.Singleton.Knockback(direction);
        }
    }

    public void Hit(float damage)
    {
        float clampedDamage = damage.ClampMinimum(0f);
        Health -= clampedDamage;

        if (HitSound) HitSound.Play(AudioOptions.HalfVolumeWithVariation);
        if (HitEffect) HitEffect.Spawn(transform.position);

        OnHealthChanged();
    }

    public void Heal(float amount)
    {
        float clampedAmount = amount.ClampMinimum(0f);

        Health += clampedAmount;
        OnHealthChanged();
    }

    private void OnHealthChanged()
    {
        if (Health <= 0)
        {
            if (DeathSound) DeathSound.Play(AudioOptions.HalfVolumeWithVariation);
            if (DeathEffect) DeathEffect.Spawn(transform.position);

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, TriggerZone);
    }
}
