using System.Collections;
using UnityEngine;

public class SlimeOfTesting : EnemyBase
{
    private int _jumpCounter;

    protected override IEnumerator EnemyBehaviour()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.6666f);

            if (IsTouchingGround && IsTriggered) 
            {
                Vector2 jumpForce;

                if (_jumpCounter == 5)
                {
                    jumpForce = new Vector2(45, 55);
                    _jumpCounter = 0;
                }
                else
                {
                    jumpForce = new Vector2(20, 25);
                    _jumpCounter++;
                }

                Rigidbody.AddForce(Vector2.right * DirectionToPlayer * jumpForce.x + Vector2.up * jumpForce.y, ForceMode2D.Impulse);

                if (_jumpCounter == 0)
                {
                    yield return new WaitForSeconds(0.5f);

                    Rigidbody.velocity = Vector2.zero;
                    Rigidbody.AddForce(Vector2.down * 60f, ForceMode2D.Impulse);
                }
            }
        }
    }

    protected override void OnUpdate() 
    {
        MotionExecutor.SetParameter("isJumping", !IsTouchingGround);
    }

    protected override bool GetFlipX() => false;
}
