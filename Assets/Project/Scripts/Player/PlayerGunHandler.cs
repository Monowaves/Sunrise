using UnityEngine;
using MonoWaves.QoL;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerGunHandler : MonoBehaviour
{
    [SerializeField] private Transform _gunHolder;
    [SerializeField] private Transform _gun;
    private SpriteRenderer _gunSpriteRenderer;

    private Vector2 _gunOrigin;

    private List<GunBaseSettings> _registerdGuns;
    private GunBaseSettings _current;
    private int _currentIndex;
    private GunBase _currentComponent;

    private readonly Dictionary<string, (int ammo, float value)> _savedAmmoCount = new();

    private void Awake() 
    {
        _gunSpriteRenderer = _gun.GetComponent<SpriteRenderer>();
        _registerdGuns = Resources.LoadAll<GunBaseSettings>("Guns").ToList();

        PickGun(_registerdGuns.GetRandomValue());

        _gunOrigin = _gun.localPosition;
    }

    private void DropGun()
    {
        if (_current != null)
        {
            if (!_savedAmmoCount.ContainsKey(_current.Name))
            {
                _savedAmmoCount.Add(_current.Name, (_currentComponent.RemainingAmmo, AmmoBar.Singleton.AmmoSlider.value));
            }
            else
            {
                _savedAmmoCount[_current.Name] = (_currentComponent.RemainingAmmo, AmmoBar.Singleton.AmmoSlider.value);
            }
        }


        foreach (Transform point in _gun)
        {
            Destroy(point.gameObject);
        }

        foreach (var gun in _gun.GetComponents<GunBase>())
        {
            Destroy(gun);
        }

        _gunSpriteRenderer.sprite = null;
    }

    private void PickGun(GunBaseSettings settings)
    {
        DropGun();

        _gunSpriteRenderer.sprite = settings.Sprite;
        _currentComponent = _gun.gameObject.AddComponent(Type.GetType(settings.Type)) as GunBase;

        _currentComponent.Settings = settings;
        if (_savedAmmoCount.ContainsKey(settings.Name)) _currentComponent.DefaultValues = _savedAmmoCount[settings.Name];

        _current = settings;
        _currentIndex = _registerdGuns.IndexOf(_current);
    } 

    private void Update() 
    {
        if (Keyboard.IsPressed(KeyCode.E))
        {
            PickGun(_registerdGuns.GetNext(_currentIndex));
        }
        else if (Keyboard.IsPressed(KeyCode.Q))
        {
            PickGun(_registerdGuns.GetPrevious(_currentIndex));
        }

        _gunHolder.localScale = new Vector3
        (
            PlayerBase.Singleton.Facing == PlayerFacing.Left ? -1 : 1, 
            1, 
            1
        );

        Vector2 cursorWorld = PlayerCamera.Camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 directionToCursor = cursorWorld - transform.position.ToVector2();
        float angle = Mathf.Atan2(directionToCursor.y, directionToCursor.x * _gunHolder.localScale.x) * Mathf.Rad2Deg;
        float distance = directionToCursor.x * _gunHolder.localScale.x;

        Vector2 targetLocalPosition = new Vector2
        (
            _gunOrigin.x + distance * 0.05f,
            _gunOrigin.y + 0.05f - _current.ShootPoint.y
        )   .ClampX(0, 1);

        _gun.SetLocalPositionAndRotation
        (
            Vector2.Lerp(_gun.localPosition, targetLocalPosition, Time.deltaTime * 3), 
            Quaternion.AngleAxis(angle, Vector3.forward)
        );

        _gun.localScale = new Vector3(1, angle < -90 || angle > 90 ? -1 : 1, 1);
    }
}
