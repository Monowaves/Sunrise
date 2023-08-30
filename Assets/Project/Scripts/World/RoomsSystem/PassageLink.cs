using MonoWaves.QoL;
using UnityEngine;

[CreateAssetMenu(menuName = "Passage Link")]
public class PassageLink : ScriptableObject
{
    [field: SerializeField] public SceneField[] Linked { get; private set; } = new SceneField[2];

    private void OnValidate() 
    {
        if (Linked.Length != 2)
        {
            Debug.LogWarning("Иди нахуй");
            Linked = new SceneField[2];
        }
    }
}
