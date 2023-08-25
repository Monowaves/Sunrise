using UnityEngine;

public class HealthShard : Pickupable
{
    [Header("Properties")]
    [SerializeField] private float _healAmount = 5f;

    public override void OnPlayerPickup()
    {
        PlayerHealth.Singleton.Heal(_healAmount);
        Destroy(gameObject);
    }
}
