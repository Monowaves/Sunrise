using MonoWaves.QoL;
using UnityEngine;

public class GunBase : MonoBehaviour
{
    public GunBaseSettings Settings;
    public (int ammo, float value) DefaultValues;

    private Transform _holder;
    private SpriteRenderer _sr;

    private Transform _shootPoint;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsCountdown { get; private set; }
    [field: SerializeField, ReadOnly] public bool IsReloading { get; private set; }
    [field: SerializeField, ReadOnly] public int RemainingAmmo { get; private set; }

    private void Start() 
    {
        _sr = GetComponent<SpriteRenderer>();
        _holder = transform.parent;

        _shootPoint = new GameObject("Shoot Point").transform;
        _shootPoint.SetParent(transform);
        _shootPoint.localPosition = Settings.ShootPoint;
        
        if (DefaultValues == (0, 0))
        {
            Reload();
            return;
        }

        RemainingAmmo = DefaultValues.ammo;
        AmmoBar.Singleton.SetMaxAmmo(Settings.MaxAmmo, DefaultValues.value);
    }

    private void Update() 
    {
        bool shotRequested = Settings.AllowHolding ? Mouse.IsHolding(MouseCode.Left) : Mouse.IsPressed(MouseCode.Left);

        if (shotRequested)
        {
            if (!IsCountdown && !IsReloading)
            {
                float randomAngle = Random.Range(transform.localRotation.z - Settings.SpreadAngle / 2, transform.localRotation.z + Settings.SpreadAngle / 2);
                Vector2 direction = transform.right.RandomInCone(Settings.SpreadAngle) * _holder.localScale.x;

                RaycastHit2D hit = Physics2D.Raycast(_shootPoint.position, direction, 1000, LayerMask.GetMask(Const.ENEMY, Const.MAP));

                MakeLine(_shootPoint.position, hit.point);

                if (hit.transform.TryGetComponent(out EnemyBase enemy))
                {
                    enemy.Hit(Settings.Damage);
                }

                IsCountdown = true;
                Invoke(nameof(ResetCanShoot), Settings.ShotCountdown);

                ShootEffects();

                RemainingAmmo--;
                AmmoBar.Singleton.SetAmmoCount(RemainingAmmo);

                if (RemainingAmmo == 0)
                {
                    Settings.ReloadSound.Play(AudioOptions.HalfVolume);
                    IsReloading = true;
                    Invoke(nameof(Reload), Settings.ReloadTime);
                }
            }
        }

        if (Mouse.IsPressed(MouseCode.Left) && IsReloading)
        {
            Settings.EmptySound.Play(AudioOptions.HalfVolume);
        }
    }

    private void ShootEffects()
    {
        Settings.ShootSound.Play(new() { Volume = 0.3f });
        PlayerCamera.Singleton.Shake(Settings.ShakeAmplitude);
    }

    private void MakeLine(Vector2 startPoint,Vector2 endPoint)
    {
        (Vector2, Color) start = (startPoint, new Color32(255, 239, 127, 255));
        (Vector2, Color) end = (endPoint, new Color32(255, 62, 0, 0));

        FadingLine newLine = LineSystem.DrawFading2D(start, end, 0.1f);
        newLine.Fade(0.2f);
    }

    private void ResetCanShoot() => IsCountdown = false;

    private void Reload()
    {
        RemainingAmmo = Settings.MaxAmmo;
        AmmoBar.Singleton.SetMaxAmmo(Settings.MaxAmmo);

        IsReloading = false;
    }
}
