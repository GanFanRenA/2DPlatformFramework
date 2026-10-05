using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 角色核心组件。作为可选模块注册表：挂了就初始化，没挂就跳过。
/// 不强制任何模块，缺配置只禁用对应模块，不影响整个角色。
/// </summary>
[DefaultExecutionOrder(-100)]
public class Character : MonoBehaviour, ICharacterAnimatorSource
{
    [Header("Config")]
    [SerializeField] private CharacterConfig config;

    // ---- 可选模块（自动查找，不强制拖引用）----
    public CharacterMotor2D Motor { get; private set; }
    public CharacterJumper2D Jumper { get; private set; }
    public CharacterHealth Health { get; private set; }
    public CharacterAttacker Attacker { get; private set; }
    public CharacterDash Dash { get; private set; }
    public CharacterAnimationBridge AnimationBridge { get; private set; }
    public GroundChecker GroundCheck { get; private set; }
    public HitboxController Hitbox { get; private set; }

    // ---- ICharacterAnimatorSource（缺失模块返回安全默认值）----
    public float Facing => Motor != null ? Motor.Facing : 1f;
    public bool IsGrounded => Jumper != null && Jumper.IsGrounded;
    public float HorizontalSpeed => Motor != null ? Motor.HorizontalSpeed : 0f;
    public float VerticalSpeed => Jumper != null ? Jumper.VerticalSpeed : 0f;
    public bool IsMoving => Mathf.Abs(HorizontalSpeed) > 0.1f;

    private Rigidbody2D _rb;

    private void Awake()
    {
        // 自动查找所有可选模块
        Motor = GetComponent<CharacterMotor2D>();
        Jumper = GetComponent<CharacterJumper2D>();
        Health = GetComponent<CharacterHealth>();
        Attacker = GetComponent<CharacterAttacker>();
        Dash = GetComponent<CharacterDash>();
        AnimationBridge = GetComponent<CharacterAnimationBridge>();
        GroundCheck = GetComponentInChildren<GroundChecker>(true);
        Hitbox = GetComponentInChildren<HitboxController>(true);

        _rb = GetComponent<Rigidbody2D>();

        Initialize();
    }

    private void Initialize()
    {
        // config 为 null 时，所有需要 config 的模块会自我禁用，Character 本身不报错
        MovementConfig movementConfig = config != null ? config.Movement : null;
        JumpConfig jumpConfig = config != null ? config.Jump : null;
        AttackConfig attackConfig = config != null ? config.Attack : null;
        HealthConfig healthConfig = config != null ? config.Health : null;
        DashConfig dashConfig = config != null ? config.Dash : null;

        // 每个模块单独初始化，互不影响
        Motor?.Initialize(movementConfig);
        Jumper?.Initialize(jumpConfig);
        Health?.Initialize(healthConfig, _rb);
        Attacker?.Initialize(attackConfig);
        Dash?.Initialize(dashConfig, _rb);
        Hitbox?.Initialize(attackConfig);

        // 动画桥需要知道所有模块，可空
        if (AnimationBridge != null)
        {
            AnimationBridge.Initialize(this, Jumper, Attacker, Health, Dash, movementConfig);
        }
    }

    private void FixedUpdate()
    {
        // 地面检测 → 跳跃组件
        if (GroundCheck != null && Jumper != null)
            Jumper.SetGrounded(GroundCheck.IsGrounded);

        // 计时器与冲刺
        Jumper?.TickTimers(Time.fixedDeltaTime);
        Dash?.Tick(Time.fixedDeltaTime);

        // 输入锁定：任何模块要求锁，就锁
        bool locked =
            (Health != null && Health.IsDead) ||
            (Attacker != null && Attacker.IsAttacking) ||
            (Dash != null && Dash.IsDashing);

        if (Motor != null) Motor.InputLocked = locked;
        if (Jumper != null) Jumper.InputLocked = locked;

        // 未锁定时执行移动与跳跃
        if (!locked)
        {
            Jumper?.TryPerformJump();
            Motor?.MoveTick(Time.fixedDeltaTime);
        }
    }
}