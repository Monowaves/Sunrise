public class Important : DontDestroyOnLoadBehaviour
{
    public static Important Singleton;

    protected override void Initialize()
    {
        Singleton = this;
    }

    protected override void OnFinish()
    {
        Singleton = null;
    }
}
