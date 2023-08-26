using System.Collections;
using MonoWaves.QoL;
using UnityEngine;

public class BlindSlime : EnemyBase
{
    private bool _wasOnGroundLastFrame;

    protected override IEnumerator EnemyBehaviour()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (IsTouchingGround)
                Rigidbody.AddForce(20 * ZRandom.Or(-1, 1) * Vector2.right + Vector2.up * 25, ForceMode2D.Impulse);
        }
    }

    protected override void OnUpdate()
    {
        MotionExecutor.SetParameter("isJumping", !IsTouchingGround && Rigidbody.velocity.y > 0);
        MotionExecutor.SetParameter("isFalling", !IsTouchingGround && Rigidbody.velocity.y < 0);

        if (!_wasOnGroundLastFrame && IsTouchingGround)
            MotionExecutor.InvokeParameter("isLanding", true, false);

        _wasOnGroundLastFrame = IsTouchingGround;
    }

    protected override bool GetFlipX() => false;
}
