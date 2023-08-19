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

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private void Update() 
    {
        bool isInAir = !PlayerBase.Singleton.IsTouchingGround && !PlayerBase.Singleton.IsTouchingRightWall && !PlayerBase.Singleton.IsTouchingLeftWall;

        if (PlayerBase.Singleton.CtrlPressed && isInAir && !IsDashing)
            StartCoroutine(GroundSlam());
    }

    private IEnumerator GroundSlam()
    {
        IsDashing = true;
        PlayerBase.Singleton.BlockMoveInputs = true;
        PlayerBase.Singleton.BlockGravity = true;

        _rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(_airTime);

        while (!PlayerBase.Singleton.IsTouchingGround)
        {
            _rb.velocity = _dashSpeed * Vector2.down;
            yield return null;
        }

        IsDashing = false;
        PlayerBase.Singleton.BlockMoveInputs = false;
        PlayerBase.Singleton.BlockGravity = false;
    }
}
