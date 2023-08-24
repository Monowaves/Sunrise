using System.Collections;
using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Singleton { get; private set; }

    [Header("Properties")]
    [SerializeField] private Slider _healthBar;
    [SerializeField] private float _startHealth = 100f;
    [SerializeField] private float _invincibleTime = 0.5f;
    [SerializeField] private float _slowDownMinimum = 0.1f;
    [SerializeField] private float _slowDownTime = 0.25f;
    [SerializeField] private float _slowDownReduce = 5f;
    [SerializeField] private float _slowDownGain = 8f;
    
    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public float Health { get; private set; }
    [field: SerializeField, ReadOnly] public bool Invincible { get; private set; }

    private void Awake() 
    {
        Singleton = this;

        _healthBar.minValue = 0;
        _healthBar.maxValue = _startHealth;
        
        Health = _startHealth;
        StopInvincible();
        OnHealthChanged();
    }

    private void Update() 
    {
        _healthBar.value = Mathf.Lerp(_healthBar.value, Health, Time.deltaTime * 10);
    }

    public void Hit(float damage, Vector3 source)
    {
        if (Invincible) return;

        float direction = (transform.position - source).x < 0 ? -1 : 1;
        PlayerBase.Singleton.Knockback(direction);

        Damage(damage);

        StartCoroutine(nameof(HitEffects));

        StartInvincible();
        Invoke(nameof(StopInvincible), _invincibleTime);
    }

    private IEnumerator HitEffects()
    {
        PlayerCamera.Singleton.Shake(0.85f);

        float remaining = 1f;
        while (remaining > _slowDownMinimum)
        {
            remaining -= Time.deltaTime * _slowDownReduce;
            Time.timeScale = Mathf.Clamp(remaining, _slowDownMinimum, 1f);

            yield return null;
        }

        yield return new WaitForSecondsRealtime(_slowDownTime);

        float elapsed = _slowDownMinimum;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime * _slowDownGain;
            Time.timeScale = Mathf.Clamp01(elapsed);

            yield return null;
        }

        Time.timeScale = 1f;
    }

    public void Damage(float damage)
    {
        float clampedDamage = damage.ClampMinimum(0f);

        Health -= clampedDamage;
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void StartInvincible()
    {
        Invincible = true;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(Const.PLAYER), LayerMask.NameToLayer(Const.ENEMY));
    }

    public void StopInvincible()
    {
        Invincible = false;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(Const.PLAYER), LayerMask.NameToLayer(Const.ENEMY), false);
    }
}
