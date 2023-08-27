using InMotion.Engine;
using MonoWaves.QoL;
using UnityEngine;
using UnityEngine.UI;

public class GameCursor : DontDestroyOnLoadBehaviour
{
    public static GameCursor Singleton { get; private set; }

    [SerializeField] private float _size;
    [SerializeField] private MotionExecutor _motionExecutor;

    public static bool IsHovering { get; private set; }

    protected override void Initialize()
    {
        Singleton = this;
        Application.focusChanged += (bool arg) => Cursor.visible = false;

        InvokeRepeating(nameof(ApplyPosition), 0, 0.002f);
    }

    private void OnValidate() 
    {
        ApplySize();
    }

    private void Update() 
    {
        ApplySize();

#if UNITY_EDITOR
        Playmode();
#endif

        GameObject pointed = CursorInputModule.GameObjectUnderPointer();
        IsHovering = CanBeHovered(pointed);

        _motionExecutor.SetParameter("isHovering", IsHovering);
    }

    private void ApplyPosition()
    {
        Vector2 cursorCurrent = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cursorCurrent;
    }

    private void ApplySize()
    {
        transform.localScale = _size * Camera.main.orthographicSize * Vector3.one / 10;
    }

    private void Playmode()
    {
        var isInside = Input.mousePosition.ToVector2().IsInRange(Vector2.zero, new Vector2(Screen.width, Screen.height));

        Cursor.visible = !isInside;
    }

    private bool CanBeHovered(GameObject obj)
    {
        if (!obj) return false;

        return obj.GetComponent<Selectable>();
    }

    public static void PlayShoot()
    {
        Singleton._motionExecutor.InvokeParameter("isShooting", true, false);
    }
}
