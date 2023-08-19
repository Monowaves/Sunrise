using InMotion.Tools.RuntimeScripts;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private MotionExecutor _motionExecutor;

    private void Awake() 
    {
        _motionExecutor.OnMotionFrame = MotionUpdate;
    }

    private void Update() 
    {
        _motionExecutor.SetParameter("isMoving", PlayerBase.Singleton.IsMoving);
    }

    private void MotionUpdate()
    {
        _motionExecutor.Target.flipX = PlayerBase.Singleton.Facing == PlayerFacing.Left;
    }
}
