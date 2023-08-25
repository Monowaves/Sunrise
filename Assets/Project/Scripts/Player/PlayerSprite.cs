using System;
using InMotion.Engine;
using UnityEngine;

public class PlayerSprite : MonoBehaviour
{
    public static PlayerSprite Singleton { get; private set;}
    [SerializeField] private MotionExecutor _motionExecutor;

    private void Awake() => _motionExecutor.OnMotionFrame = FrameUpdate;

    private void FrameUpdate()
    {
        if (PlayerBase.Singleton.IsWallSliding) 
            _motionExecutor.Target.flipX = PlayerBase.Singleton.IsTouchingRightWall;
        else
            _motionExecutor.Target.flipX = PlayerBase.Singleton.Facing == PlayerFacing.Left;
    }

    private void Update() 
    {
        _motionExecutor.SetParameter("isWallSliding", PlayerBase.Singleton.IsWallSliding);
        _motionExecutor.SetParameter("isRunning", PlayerBase.Singleton.IsRunning);
        _motionExecutor.SetParameter("isJumping", PlayerBase.Singleton.IsJumping);
        _motionExecutor.SetParameter("isFalling", PlayerBase.Singleton.IsFalling);

        _motionExecutor.SetParameter("isGSPrepare", PlayerBase.Singleton.IsGroundSlamPrepare);
        _motionExecutor.SetParameter("isGSDash", PlayerBase.Singleton.IsGroundSlamDash);
        _motionExecutor.SetParameter("isGSStandUp", PlayerBase.Singleton.IsGroundStandUp);

        _motionExecutor.SetParameter("isSliding", PlayerBase.Singleton.IsSliding);
    }
}
