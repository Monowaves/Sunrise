using InMotion.Tools.RuntimeScripts;
using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public static PlayerSprite Singleton { get; private set;}
    [SerializeField] private MotionExecutor _motionExecutor;

    private void Awake() 
    {
        _motionExecutor.OnMotionFrame = MotionUpdate;
    }

    private void Update() 
    {
        _motionExecutor.SetParameter("isRunning", PlayerBase.Singleton.IsRunning);
        _motionExecutor.SetParameter("isJumping", PlayerBase.Singleton.IsJumping);
    }

    private void MotionUpdate()
    {
        _motionExecutor.Target.flipX = PlayerBase.Singleton.Facing == PlayerFacing.Left;
    }
}
