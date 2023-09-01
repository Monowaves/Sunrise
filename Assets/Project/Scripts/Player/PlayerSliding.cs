using MonoWaves.QoL;
using UnityEngine;

public class PlayerSliding : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField, Min(0)] private float _startMomentum = 2f;
    [SerializeField, Min(0)] private float _endMomentum = 0.5f;
    [SerializeField, Min(0)] private float _momentumGain = 3.5f;
    [SerializeField, Min(0)] private float _momentumLose = 5f;

    [field: Header("Info")]
    [field: SerializeField, ReadOnly] public bool IsSliding { get; private set; }
    [field: SerializeField, ReadOnly] public float Momentum { get; private set; }
    [field: SerializeField, ReadOnly] public bool StartedSlidingOnGround { get; private set; }
    [field: SerializeField, ReadOnly] public bool WasSlidingLastFrame { get; private set; }

    private Rigidbody2D _rb => PlayerBase.Singleton.Rigidbody;

    private void Awake() 
    {
        InvokeRepeating(nameof(HandleParticles), 0f, 0.05f);
    }

    private void HandleParticles()
    {
        if (IsSliding)
        {
            if (Momentum > 35)
            {
                Vector2 position = new
                (
                    transform.position.x, 
                    transform.position.y - PlayerInformation.ColliderHalfSize.y / 2 + PlayerInformation.ColliderHalfOffset.y
                );
    
                ZGameObjectExtensions.Spawn(PlayerBase.Singleton.SlidingParticles.GetRandomValue(), position);
            }
        }
    }

    private void Update() 
    {
        if (PlayerBase.Singleton.SlidePressed && PlayerBase.Singleton.IsTouchingGround && !PlayerBase.Singleton.IsTouchingWall && !IsSliding) 
        {
            StartedSlidingOnGround = true;

            PlayerSprite.Singleton.MotionExecutor.AbortParameter("isSlidingStop");
            PlayerSprite.Singleton.MotionExecutor.InvokeParameter("isSlidingAwake", true, false);

            PlayerBase.Singleton.SetHalfSize();
        }
        
        if (PlayerBase.Singleton.WantToSlide)
        {
            if (StartedSlidingOnGround)
            {
                IsSliding = true;

                PlayerBase.Singleton.BlockMoveInputs = true;
                PlayerBase.Singleton.BlockJumpInputs = true;
            }

            if (PlayerBase.Singleton.IsTouchingWall && IsSliding && WasSlidingLastFrame)
            {
                StopSliding();
            }
        }
        else if (IsSliding && (!PlayerBase.Singleton.IsTouchingCeil || PlayerBase.Singleton.IsTouchingWall))
        {
            StopSliding();
        }

        if (IsSliding)
        {
            if (Momentum > _endMomentum) Momentum -= Time.deltaTime * _momentumLose;
        }

        if (!IsSliding && Momentum < _startMomentum) Momentum += Time.deltaTime * _momentumGain;

        PlayerBase.Singleton.IsSliding = IsSliding;
        PlayerBase.Singleton.SlidingMomentum = IsSliding ? Momentum : 0;

        WasSlidingLastFrame = IsSliding;
    }

    private void StopSliding()
    {
        PlayerSprite.Singleton.MotionExecutor.AbortParameter("isSlidingAwake");
        PlayerSprite.Singleton.MotionExecutor.InvokeParameter("isSlidingStop", true, false);

        PlayerBase.Singleton.SetFullSize();

        StartedSlidingOnGround = false;
        IsSliding = false;

        PlayerBase.Singleton.BlockMoveInputs = false;
        PlayerBase.Singleton.BlockJumpInputs = false;
    }

    private void FixedUpdate() 
    {
        if (IsSliding)
        {
            float direction = PlayerBase.Singleton.Facing == PlayerFacing.Left ? -1 : 1;
            Vector2 moveDirection = PlayerBase.Singleton.IsSloped() ? ZVector2Math.ProjectOnPlane(Vector2.right, PlayerBase.Singleton.SlopeNormal) : Vector2.right;
        
            _rb.AddForce(Mathf.Clamp(Momentum, _endMomentum, _startMomentum) * 5 * direction * moveDirection);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.TryGetComponent(out EnemyBase enemy) && IsSliding)    
        {
            enemy.Hit(10, transform.position, Vector2.up * PlayerBase.Singleton.SlidingMomentum / 15);
            Momentum += 15;
            PlayerCamera.Singleton.Shake(1f);
        }
    }
}
