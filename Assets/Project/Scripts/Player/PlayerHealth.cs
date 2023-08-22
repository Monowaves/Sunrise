using MonoWaves.QoL;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Singleton { get; private set; }

    [Header("Properties")]
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private float _startHealth = 100f;
    [SerializeField] private float _invincibleTime = 0.5f;
    
    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public float Health { get; private set; }
    [field: SerializeField, ReadOnly] public bool Invincible { get; private set; }

    private void Awake() 
    {
        Health = _startHealth;
        StopInvincible();
        OnHealthChanged();

        Singleton = this;
    }

    public void Hit(float damage)
    {
        if (Invincible) return;

        float clampedDamage = damage.ClampMinimum(0f);

        Health -= clampedDamage;
        OnHealthChanged();

        StartInvincible();
        Invoke(nameof(StopInvincible), _invincibleTime);
    }

    public void Heal(float amount)
    {
        float clampedAmount = amount.ClampMinimum(0f);

        Health += clampedAmount;
        OnHealthChanged();
    }

    private void OnHealthChanged()
    {
        _healthText.text = $"Health: {Health}";
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
