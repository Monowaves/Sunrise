using UnityEngine;

public class Defaults : DontDestroyOnLoadBehaviour
{
    public static Material SpriteLit { get; private set; }
    public static GameObject HealthShard { get; private set; }

    protected override void Initialize()
    {
        SpriteLit = Resources.Load<Material>("Defaults/SpriteLit");
        HealthShard = Resources.Load<GameObject>("Defaults/HealthShard");
    }
}
