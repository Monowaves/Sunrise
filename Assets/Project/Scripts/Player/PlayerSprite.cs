using InMotion.Engine;
using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public static PlayerSprite Singleton { get; private set; }

    [field: SerializeField] public MotionExecutor MotionExecutor { get; private set; }

    private void Awake() 
    {
        MotionExecutor.OnMotionFrame = FrameUpdate;
        Singleton = this;
    }

    private void FrameUpdate()
    {
        if (PlayerBase.Singleton.IsWallSliding) 
            MotionExecutor.Target.flipX = PlayerBase.Singleton.IsTouchingRightWall;
        else
            MotionExecutor.Target.flipX = PlayerBase.Singleton.Facing == PlayerFacing.Left;
    }

    private void Update() 
    {
        MotionExecutor.SetParameter("isWallSliding", PlayerBase.Singleton.IsWallSliding);
        MotionExecutor.SetParameter("isRunning", PlayerBase.Singleton.IsRunning);
        MotionExecutor.SetParameter("isJumping", PlayerBase.Singleton.IsJumping);
        MotionExecutor.SetParameter("isFalling", PlayerBase.Singleton.IsFalling);

        MotionExecutor.SetParameter("isGSPrepare", PlayerBase.Singleton.IsGroundSlamPrepare);
        MotionExecutor.SetParameter("isGSDash", PlayerBase.Singleton.IsGroundSlamDash);
        MotionExecutor.SetParameter("isGSStandUp", PlayerBase.Singleton.IsGroundStandUp);

        MotionExecutor.SetParameter("isSlidingSlide", PlayerBase.Singleton.IsSliding);
    }
}
