using MonoWaves.QoL;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Passage : MonoBehaviour
{
    [field: SerializeField] public PassageLink Link { get; private set; }

    [field: Space(9)]
    [field: SerializeField] public Vector2 SpawnOffset { get; private set; }

    public Vector2 ExitPosition => transform.position.ToVector2() + SpawnOffset;

    private void OnValidate() 
    {
        if (TryGetComponent(out BoxCollider2D boxCollider))
        {
            boxCollider.isTrigger = true;
        }
    }

    private void Start() 
    {
        gameObject.layer = LayerMask.NameToLayer("Passage");
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(ExitPosition, new Vector2(1, 2));
    }
}
