using System.Collections;
using MonoWaves.QoL;
using UnityEngine;

public class PlayerGroundSlam : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _airTime = 0.5f;
    [SerializeField, Min(0)] private float _dashSpeed;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsDashing { get; private set;}

    private Rigidbody2D _rb => PlayerPhysics.Singleton.Rigidbody;

    private void Update() 
    {
        bool isInAir = !PlayerChecker.Singleton.IsTouchingGround && !PlayerChecker.Singleton.IsTouchingRightWall && !PlayerChecker.Singleton.IsTouchingLeftWall;

        if (PlayerInputs.Singleton.CtrlPressed && isInAir && !IsDashing)
            StartCoroutine(GroundSlam());
    }

    private IEnumerator GroundSlam()
    {
        IsDashing = true;
        PlayerInputs.Singleton.BlockMoveInputs = true;
        PlayerPhysics.Singleton.BlockGravity = true;

        _rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(_airTime);

        while (!PlayerChecker.Singleton.IsTouchingGround)
        {
            _rb.velocity = _dashSpeed * 125 * Time.deltaTime * Vector2.down;
            yield return null;
        }

        IsDashing = false;
        PlayerInputs.Singleton.BlockMoveInputs = false;
        PlayerPhysics.Singleton.BlockGravity = false;
    }
}
