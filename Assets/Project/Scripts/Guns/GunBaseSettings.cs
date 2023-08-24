using UnityEngine;

[CreateAssetMenu(menuName = "Gun Settings")]
public class GunBaseSettings : ScriptableObject
{
    [field: Header("Important")]
    [field: SerializeField] public string Type { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }

    [field: Header("Properties")]
    [field: SerializeField, Min(0)] public float Damage { get; private set; } = 75f;
    [field: SerializeField, Min(1)] public int MaxAmmo { get; private set; } = 12;
    [field: SerializeField, Min(0)] public float ReloadTime { get; private set; } = 0.5f;
    [field: SerializeField, Min(0)] public float SpreadAngle { get; private set; }
    [field: SerializeField, Min(0)] public float ShotCountdown { get; private set; }
    [field: SerializeField] public bool AllowHolding { get; private set; }
    [field: SerializeField] public Vector2 ShootPoint { get; private set; }
    [field: SerializeField, Min(0)] public float ShakeAmplitude { get; private set; } = 0.5f;

    [field: Header("Audio")]
    [field: SerializeField] public AudioClip ShootSound { get; private set; }
    [field: SerializeField] public AudioClip EmptySound { get; private set; }
    [field: SerializeField] public AudioClip ReloadSound { get; private set; }
}
