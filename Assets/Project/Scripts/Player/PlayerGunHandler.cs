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

    private void Awake() 
    {
        _gunSpriteRenderer = _gun.GetComponent<SpriteRenderer>();
        _registerdGuns = Resources.LoadAll<GunBaseSettings>("Guns").ToList();

        _gunOrigin = _gun.localPosition;
        PickGun(_registerdGuns.GetRandomValue());
    }

    private void DropGun()
    {
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
        GunBase newGun = _gun.gameObject.AddComponent(Type.GetType(settings.Type)) as GunBase;

        newGun.Settings = settings;

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
            _gunOrigin.y
        )
            .Clamp(new Vector2
            (
                0, 
                _gunOrigin.y
            ), new Vector2
            (
                1, 
                _gunOrigin.y
            )
        );

        _gun.SetLocalPositionAndRotation
        (
            Vector2.Lerp(_gun.localPosition, targetLocalPosition, Time.deltaTime * 3), 
            Quaternion.AngleAxis(angle, Vector3.forward)
        );

        _gun.localScale = new Vector3(1, angle < -90 || angle > 90 ? -1 : 1, 1);
    }
}
