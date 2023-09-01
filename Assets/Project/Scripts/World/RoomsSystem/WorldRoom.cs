using System.Collections.Generic;
using MonoWaves.QoL;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PolygonCollider2D))]
public class WorldRoom : MonoBehaviour
{
    public static WorldRoom Singleton;
    public PolygonCollider2D Bounds { get; private set; }

    [field: SerializeField] public CompositeCollider2D Collider { get; private set; }
    [field: SerializeField] public GameObject Tilemap { get; private set; }
    [field: SerializeField] public Passage[] Passages { get; private set; }

    [Header("Bounds Creation")]
    [SerializeField] private Vector2 _minOffset;
    [SerializeField] private Vector2 _maxOffset;

    private void Awake() => Singleton = this;

    private void OnValidate() 
    {
        if (TryGetComponent(out PolygonCollider2D bounds))
        {
            Bounds = bounds;
            Bounds.isTrigger = true;
        }
    }

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

        Tilemap = new("Tilemap")
        {
            layer = LayerMask.NameToLayer(Const.MAP)
        };
        Tilemap.transform.SetParent(gridGameObject.transform);
        Tilemap.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = Tilemap.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingLayerName = Const.MAP;

        Rigidbody2D rigidbody = Tilemap.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Static;

        Collider = Tilemap.AddComponent<CompositeCollider2D>();
    
        TilemapCollider2D tilemapCollider = Tilemap.AddComponent<TilemapCollider2D>();
        tilemapCollider.usedByComposite = true;
        tilemapCollider.useDelaunayMesh = true;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    [ContextMenu("Auto bounds")]
    public void AutoBounds()
    {   
        if (!Collider)
        {
            Debug.LogWarning("Cannot create bounds without collider! Setup tilemap first, then use auto bounding");
            return;
        }

        Vector2 min = Collider.bounds.min;
        Vector2 max = Collider.bounds.max;

        List<Vector2> newBounds = new()
        {
            min + _minOffset,
            new Vector2(min.x + _minOffset.x, max.y + _maxOffset.y),
            max + _maxOffset,
            new Vector2(max.x + _maxOffset.x, min.y + _minOffset.y),
        };

        Bounds.points = newBounds.ToArray();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }
}
