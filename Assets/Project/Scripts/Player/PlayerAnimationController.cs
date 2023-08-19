using InMotion.Tools.RuntimeScripts;
using MonoWaves.QoL;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private MotionExecutor _motionExecutor;

    private void Update() 
    {
        _motionExecutor.Target.flipX = PlayerBase.Singleton.Facing == PlayerFacing.Left;
    }
}
