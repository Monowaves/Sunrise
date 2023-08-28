using MonoWaves.QoL;
using UnityEngine;

[CreateAssetMenu(menuName = "Passage Link")]
public class PassageLink : ScriptableObject
{
    [field: SerializeField] public SceneField[] Linked { get; private set; } = new SceneField[2];
}
