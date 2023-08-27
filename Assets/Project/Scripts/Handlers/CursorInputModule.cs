using UnityEngine;
using UnityEngine.EventSystems;

public class CursorInputModule : StandaloneInputModule
{
    public static CursorInputModule Singleton { get; private set; }

    public static GameObject GameObjectsUnderPointer(int pointerId)
    {
        var lastPointer = Singleton.GetLastPointerEventData(pointerId);
        if (lastPointer != null)
            return lastPointer.pointerCurrentRaycast.gameObject;
        return null;
    }

    public static GameObject GameObjectUnderPointer()
    {
        return GameObjectsUnderPointer(kMouseLeftId);
    }

    protected override void Awake()
    {
        Singleton = this;
    }
}
