using MonoWaves.QoL;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Vector2 minMaxX;
    public GameObject[] enemyPrefab;

    private void Awake() 
    {
        InvokeRepeating(nameof(SpanEnemy), 0f, 0.85f);
    }

    void SpanEnemy()
    {
        Instantiate(enemyPrefab.GetRandomValue(), new Vector2(Random.Range(minMaxX.x, minMaxX.y), 30f), Quaternion.identity);
    }
}
