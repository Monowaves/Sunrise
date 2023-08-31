using MonoWaves.QoL;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class WorldRoom : MonoBehaviour
{
    public static WorldRoom Singleton;

    [field: SerializeField] public Passage[] Passages { get; private set; }

    private void Awake() => Singleton = this;

    [ContextMenu("Detect Passages")]
    public void DetectPassages()
    {   
        Passages = FindObjectsOfType<Passage>(true);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    [ContextMenu("Setup")]
    public void Setup()
    {   
        GameObject gridGameObject = new("Grid");
        gridGameObject.transform.SetParent(transform);
        gridGameObject.AddComponent<Grid>();

        GameObject tilemapGameObject = new("Tilemap")
        {
            layer = LayerMask.NameToLayer(Const.MAP)
        };
        tilemapGameObject.transform.SetParent(gridGameObject.transform);
        tilemapGameObject.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = tilemapGameObject.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingLayerName = Const.MAP;

        Rigidbody2D rigidbody = tilemapGameObject.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Static;

        tilemapGameObject.AddComponent<CompositeCollider2D>();
    
        TilemapCollider2D tilemapCollider = tilemapGameObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.usedByComposite = true;
        tilemapCollider.useDelaunayMesh = true;
    }
}
