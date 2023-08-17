using UnityEngine;
using MonoWaves.QoL;
using System;

public class PlayerRun : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _speed;
    [SerializeField, Min(0)] private float _acceleration;
    [SerializeField, Min(0)] private float _decceleration;
    [SerializeField, Range(0, 2)] private float _velocityPower;

    [Header("Info")]
    [SerializeField, ReadOnly] private float _inputAxis;

    private Rigidbody2D _rb => PlayerPhysics.Singleton.Rigidbody;

    private void Update() 
    {
        _inputAxis = Keyboard.AxisFrom(KeyCode.A, KeyCode.D);
    }

    private void FixedUpdate() 
    {
        float moveVector = _inputAxis * _speed;
        float vectorDifference = moveVector - _rb.velocity.x;
        float accelerationRate = Mathf.Abs(moveVector) > 0.01f ? _acceleration : _decceleration;
        float targetVector = Mathf.Pow(Mathf.Abs(vectorDifference) * accelerationRate, _velocityPower) * Math.Sign(vectorDifference);

        _rb.AddForce(targetVector * Vector2.right);
    }
}
