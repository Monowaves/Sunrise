using UnityEngine;

public class Defaults : DontDestroyOnLoadBehaviour
{
    public static Material SpriteLit { get; private set; }
    public static GameObject HealthShard { get; private set; }
    public static AudioClip HitSound { get; private set; }
    public static GameObject Blood { get; private set; }
    
    protected override void Initialize()
    {
        GetDefaults();
    }

    private static void GetDefaults()
    {
        SpriteLit = Get<Material>("Defaults/SpriteLit");
        HealthShard = Get<GameObject>("Defaults/HealthShard");
        HitSound = Get<AudioClip>("Defaults/DefaultHit");
        Blood = Get<GameObject>("Defaults/Blood");
    }

    public static T Get<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    } 
}
