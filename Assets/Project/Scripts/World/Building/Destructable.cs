using System;
using MonoWaves.QoL;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Destructable : MonoBehaviour
{
    [field: SerializeField, ReadOnly] public string GUID { get; private set; }
    [field: SerializeField, ReadOnly] public CompositeCollider2D Collider { get; private set; }

    [field: Header("Properties")]
    [field: SerializeField] public DestructMode DestructMode { get; private set; }
    [field: SerializeField] public float DestructTime { get; private set; }

    [MenuItem("GameObject/Destructable", false, 30)]
    public static void CreateCustomObject()
    {
        GameObject newObj = new("Destructable", typeof(Destructable));
        if (Selection.activeGameObject) newObj.transform.SetParent(Selection.activeGameObject.transform);
        Selection.activeGameObject = newObj;
    }

    private void Reset() 
    {
        GUID = Guid.NewGuid().ToString();
        gameObject.layer = LayerMask.NameToLayer(Const.MAP);

        ClearGameObject();

        gameObject.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingLayerName = Const.MAP;

        Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Static;

        Collider = gameObject.AddComponent<CompositeCollider2D>();
    
        TilemapCollider2D tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
        tilemapCollider.usedByComposite = true;
        tilemapCollider.useDelaunayMesh = true;

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private void ClearGameObject()
    {
        foreach (var component in gameObject.GetComponents<Component>())
        {
            if (component != this && component != transform) DestroyImmediate(component);
        }

        foreach (Transform child in gameObject.transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    public void Destruct()
    {
        Invoke(nameof(DestructEnd), DestructTime);
    }

    private void DestructEnd()
    {
        Destroy(gameObject);
    }
}

public enum DestructMode
{
    Attack,
    GroundSlam,
    Slide
}
