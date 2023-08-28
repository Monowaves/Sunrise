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

        GunPreview.Singleton.SetPreview(Settings.Sprite, Settings.Rarity);
        GunInfo.Singleton.SetInfo(Settings);
        
        if (DefaultValues == (0, 0))
        {
            Reload();
            return;
        }

        RemainingAmmo = DefaultValues.ammo;
        AmmoBar.Singleton.SetMaxAmmo(Settings.Capacity, DefaultValues.value);
        GameCursor.SetReload(false);
    }

    private void Update() 
    {
        bool shotRequested = Settings.AllowHolding ? Mouse.IsHolding(MouseCode.Left) : Mouse.IsPressed(MouseCode.Left);

        if (shotRequested && !IsCountdown && !IsReloading && !GameCursor.IsHovering)
        {
            float randomAngle = Random.Range(transform.localRotation.z - Settings.SpreadAngle / 2, transform.localRotation.z + Settings.SpreadAngle / 2);
            Vector2 direction = transform.right.RandomInCone(Settings.SpreadAngle) * _holder.localScale.x;

            RaycastHit2D hit = Physics2D.Raycast(_shootPoint.position, direction, 750, LayerMask.GetMask(Const.ENEMY, Const.MAP));
            Vector2 hitPoint = !hit ? direction * 200 + transform.position.ToVector2() : hit.point;

            IsCountdown = true;
            Invoke(nameof(ResetCanShoot), Settings.ShotCountdown);

            ShootEffects(hitPoint);

            RemainingAmmo--;
            AmmoBar.Singleton.SetAmmoCount(RemainingAmmo);

            if (RemainingAmmo == 0)
            {
                Settings.ReloadSound.Play(AudioOptions.HalfVolume);

                IsReloading = true;
                GameCursor.SetReload(true);

                Invoke(nameof(Reload), Settings.ReloadTime);
            }

            if (!hit) return;

            if (hit.transform.TryGetComponent(out EnemyBase enemy))
            {
                enemy.Hit(Settings.Damage, hit.point, Vector2.one * 0.5f);
            }
        }

        if (Mouse.IsPressed(MouseCode.Left) && IsReloading)
        {
            Settings.EmptySound.Play(AudioOptions.HalfVolume);
        }
    }

    private void ShootEffects(Vector2 hitPoint)
    {
        MakeLine(_shootPoint.position, hitPoint);
        Settings.ShootSound.Play(new() { Volume = 0.3f });
        PlayerCamera.Singleton.Shake(Settings.ShakeAmplitude);
        GameCursor.PlayShoot();
    }

    private void MakeLine(Vector2 startPoint,Vector2 endPoint)
    {
        (Vector2, Color) start = (startPoint, new Color32(255, 239, 127, 255));
        (Vector2, Color) end = (endPoint, new Color32(255, 62, 0, 0));

        FadingLine newLine = LineSystem.DrawFading2D(start, end, 0.1f, Defaults.SpriteLit);
        newLine.Fade(0.2f);
    }

    private void ResetCanShoot() => IsCountdown = false;

    private void Reload()
    {
        RemainingAmmo = Settings.Capacity;
        AmmoBar.Singleton.SetMaxAmmo(Settings.Capacity);

        IsReloading = false;
        GameCursor.SetReload(false);
    }
}
