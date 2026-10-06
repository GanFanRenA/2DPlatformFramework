using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 角色控制器基类。所有 AI / NPC / 炮台控制器继承它。
/// 模块全部可选：Motor、Jumper、Health、Attacker、Dash、Hitbox、GroundCheck、AnimationBridge。
/// 缺失的模块通过 HasXxx 判断，便捷方法内部判空，不会崩溃。
/// </summary>
[DefaultExecutionOrder(100)]   // 保证晚于 Character 执行
public abstract class CharacterControllerBase : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] protected bool _debugLog = false;

    // ---- 目标 ----
    /// <summary>外部可设置。为空时 Start 里自动查找 "Player" tag。</summary>
    public Transform Target { get; set; }
    protected bool HasTarget => Target != null;

    // ---- 模块引用（可能为 null）----
    protected Character Character { get; private set; }
    protected CharacterMotor2D Motor { get; private set; }
    protected CharacterJumper2D Jumper { get; private set; }
    protected CharacterHealth Health { get; private set; }
    protected CharacterAttacker Attacker { get; private set; }
    protected CharacterDash Dash { get; private set; }
    protected HitboxController Hitbox { get; private set; }
    protected GroundChecker GroundCheck { get; private set; }
    protected CharacterAnimationBridge AnimationBridge { get; private set; }
    protected Rigidbody2D Rigidbody { get; private set; }

    // ---- 模块可用性 ----
    protected bool HasMotor => Motor != null && Motor.enabled;
    protected bool HasJumper => Jumper != null && Jumper.enabled;
    protected bool HasHealth => Health != null;
    protected bool HasAttacker => Attacker != null && Attacker.enabled;
    protected bool HasDash => Dash != null && Dash.enabled;
    protected bool HasHitbox => Hitbox != null;
    protected bool HasGroundCheck => GroundCheck != null;

    // ---- 状态 ----
    protected bool IsReady { get; private set; }
    protected bool IsDead => HasHealth && Health.IsDead;
    protected bool IsGrounded => HasJumper && Jumper.IsGrounded;

    // ============================
    // 生命周期
    // ============================

    protected virtual void Awake()
    {
        Character = GetComponent<Character>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        if (Character == null)
        {
            DebugOutputService.RunNullFatal(gameObject, nameof(Character));
            enabled = false;
            return;
        }

        BindModules();

        if (Target == null) Target = FindPlayer();

        if (!OnInitialized())
        {
            enabled = false;
            return;
        }

        IsReady = true;
    }

    protected virtual void Update()
    {
        if (!IsReady) return;

        if (IsDead)
        {
            OnDeathTick();
            return;
        }

        Tick(Time.deltaTime);
    }

    protected virtual void OnDisable()
    {
        OnBeforeDisable();

        SetHitbox(false);
        StopMove();

        IsReady = false;
    }

    // ============================
    // 子类实现点
    // ============================

    /// <summary>每帧主循环。死亡时不会调用。</summary>
    protected abstract void Tick(float dt);

    /// <summary>初始化。返回 false 会禁用组件。默认返回 true。</summary>
    protected virtual bool OnInitialized() => true;

    /// <summary>死亡后每帧调用。默认关闭 Hitbox。</summary>
    protected virtual void OnDeathTick()
    {
        SetHitbox(false);
    }

    /// <summary>OnDisable 前调用，用于子类清理。基类会自动关 Hitbox 和停移动。</summary>
    protected virtual void OnBeforeDisable() { }

    // ============================
    // 便捷方法（内部判空，支持模块缺失）
    // ============================

    /// <summary>设置水平输入。没有 Motor 时静默跳过。</summary>
    protected void Move(float axis)
    {
        if (Motor != null) Motor.SetMoveInput(axis);
    }

    protected void SetVelocity(Vector2 velocity)
    {
        if (Rigidbody != null) Rigidbody.linearVelocity = velocity;
    }

    /// <summary>停止移动。</summary>
    protected void StopMove()
    {
        if (Motor != null) Motor.SetMoveInput(0f);
    }

    /// <summary>请求跳跃。返回是否成功提交请求。</summary>
    protected bool TryJump()
    {
        if (!HasJumper) return false;
        Jumper.RequestJump();
        return true;
    }

    /// <summary>请求冲刺。返回是否成功提交请求。</summary>
    protected bool TryDash()
    {
        if (!HasDash || Motor == null) return false;
        Dash.RequestDash(Motor.Facing);
        return true;
    }

    /// <summary>设置 Hitbox 开关。没有 Hitbox 时静默跳过。</summary>
    protected void SetHitbox(bool active)
    {
        if (Hitbox != null) Hitbox.SetActive(active);
    }

    // ============================
    // 目标工具
    // ============================

    protected float DistanceToTarget()
    {
        if (Target == null) return float.PositiveInfinity;
        return Vector2.Distance(transform.position, Target.position);
    }

    protected float HorizontalDistanceToTarget()
    {
        if (Target == null) return float.PositiveInfinity;
        return Target.position.x - transform.position.x;
    }

    protected float VerticalDistanceToTarget()
    {
        if (Target == null) return float.PositiveInfinity;
        return Target.position.y - transform.position.y;
    }

    /// <summary>目标在左返回 -1，在右返回 1，在死区内返回 0。</summary>
    protected float DirectionToTarget(float deadZone = 0.1f)
    {
        if (Target == null) return 0f;
        float dx = Target.position.x - transform.position.x;
        if (Mathf.Abs(dx) < deadZone) return 0f;
        return Mathf.Sign(dx);
    }

    protected bool IsTargetInRange(float range)
    {
        return DistanceToTarget() <= range;
    }

    protected bool IsTargetInHorizontalRange(float range)
    {
        return Mathf.Abs(HorizontalDistanceToTarget()) <= range;
    }

    // ============================
    // 内部
    // ============================

    private void BindModules()
    {
        Motor = Character.Motor;
        Jumper = Character.Jumper;
        Health = Character.Health;
        Attacker = Character.Attacker;
        Dash = Character.Dash;
        Hitbox = Character.Hitbox;
        GroundCheck = Character.GroundCheck;
        AnimationBridge = Character.AnimationBridge;

        if (_debugLog)
        {
            Debug.Log($"[{GetType().Name}] 模块绑定: " +
                      $"Motor={Motor != null}, Jumper={Jumper != null}, " +
                      $"Health={Health != null}, Attacker={Attacker != null}, " +
                      $"Dash={Dash != null}, Hitbox={Hitbox != null}", this);
        }
    }

    private Transform FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.transform : null;
    }
}