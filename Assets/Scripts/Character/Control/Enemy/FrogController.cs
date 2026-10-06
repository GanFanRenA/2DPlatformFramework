using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 青蛙敌人。以跳跃为攻击方式，Jump 与 Fall 阶段开启 Hitbox。
/// 依赖：Motor、Jumper、Hitbox。不依赖：Attacker、Dash、Health（可选）。
/// </summary>
public class FrogController : CharacterControllerBase
{
    [SerializeField] private FrogConfig _config;

    private FrogState _state;
    private float _stateTimer;
    private bool _leftGround;

    private enum FrogState { Idle, Airborne, Landing }

    protected override bool OnInitialized()
    {
        if (_config == null)
        {
            DebugOutputService.RunNullFatal(gameObject, nameof(_config));
            return false;
        }
        if (!HasMotor || !HasJumper)
        {
            DebugOutputService.RunNullFatal(gameObject, "FrogController 需要 Motor 和 Jumper");
            return false;
        }
        if (!HasHitbox)
        {
            DebugOutputService.RunNullOptional(gameObject, "HitboxController", "青蛙将无法造成伤害");
        }

        EnterIdle();
        return true;
    }

    protected override void Tick(float dt)
    {
        TickStateMachine(dt);
        TickHitbox();
    }

    // ---- 状态机 ----

    private void TickStateMachine(float dt)
    {
        switch (_state)
        {
            case FrogState.Idle:
                StopMove();
                _stateTimer -= dt;
                if (_stateTimer <= 0f && (Target == null || IsTargetInRange(_config.detectRange)))
                    EnterAirborne();
                break;

            case FrogState.Airborne:
                ApplyAirbornePhysics(dt);
                // A grounded value can still be true for one frame after the
                // jump request. Only accept landing after the frog has actually
                // left the ground, then observes ground contact again.
                if (!IsGrounded)
                    _leftGround = true;
                else if (_leftGround && Jumper.VerticalSpeed <= 0f)
                    EnterLanding();
                break;

            case FrogState.Landing:
                StopMove();
                _stateTimer -= dt;
                if (_stateTimer <= 0f) EnterIdle();
                break;
        }
    }

    private void TickHitbox()
    {
        if (!HasHitbox) return;

        bool airborne = _state == FrogState.Airborne && !IsGrounded;
        if (!airborne) { SetHitbox(false); return; }

        bool rising = Jumper.VerticalSpeed > 0f;
        bool allowed = (rising && _config.damageOnRising) ||
                       (!rising && _config.damageOnFalling);
        SetHitbox(allowed);
    }

    // ---- 状态切换 ----

    private void EnterIdle()
    {
        _state = FrogState.Idle;
        _stateTimer = _config.idleDuration;
        if (Motor != null) Motor.InputLocked = false;
        StopMove();
        if (_debugLog) Debug.Log("[Frog] → Idle", this);
    }

    private void EnterAirborne()
    {
        _state = FrogState.Airborne;
        _leftGround = false;

        float dir = DirectionToTarget(_config.deadZone);
        if (Mathf.Abs(dir) < 0.01f && Motor != null)
            dir = Motor.Facing;

        if (Motor != null)
        {
            Motor.SetFacing(dir);
            Motor.InputLocked = true;
        }

        float burstSpeed = _config.horizontalJumpSpeed * 1.15f;
        SetVelocity(new Vector2(dir * burstSpeed, 0f));
        TryJump();

        AnimationBridge?.PunchScale(new Vector3(0.85f, 1.2f, 1f), 0f, 0.06f, 0.04f, 0.15f);

        if (_debugLog) Debug.Log($"[Frog] → Airborne, dir={dir}, vx={dir * _config.horizontalJumpSpeed}", this);
    }

    private void ApplyAirbornePhysics(float dt)
    {
        if (Rigidbody == null) return;

        Vector2 vel = Rigidbody.linearVelocity;

        // ---- 1. 不对称重力 ----
        // 上升阶段：额外加重力 * (riseMultiplier - 1)，让上升更飘
        // 下降阶段：额外加重力 * (fallMultiplier - 1)，让下落更快
        float gravity = Physics2D.gravity.y;
        float extraGravity = 0f;

        if (vel.y > 0f)
            extraGravity = gravity * (_config.riseMultiplier - 1f);
        else if (vel.y < 0f)
            extraGravity = gravity * (_config.fallMultiplier - 1f);

        vel.y += extraGravity * dt;

        // ---- 2. 水平空气阻力 ----
        if (_config.airDrag > 0f)
        {
            vel.x = Mathf.MoveTowards(vel.x, 0f, _config.airDrag * dt);
        }

        // ---- 3. 顶点减速 ----
        // 顶点附近水平速度乘以一个系数，让 Frog 在最高点有"顿一下"的感觉
        if (Mathf.Abs(vel.y) < 1f && _config.apexSpeedMultiplier < 1f)
        {
            vel.x = Mathf.MoveTowards(
                vel.x,
                vel.x * _config.apexSpeedMultiplier,
                dt * 2f);
        }

        Rigidbody.linearVelocity = vel;
    }

    private void EnterLanding()
    {
        _state = FrogState.Landing;
        _stateTimer = _config.landingDuration;
        if (Motor != null) Motor.InputLocked = false;
        StopMove();

        AnimationBridge?.PunchScale(new Vector3(1.3f, 0.75f, 1f), 0f, 0.08f, 0.03f, 0.14f);

        if (_debugLog) Debug.Log("[Frog] → Landing", this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_config == null) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _config.detectRange);
    }
#endif
}
