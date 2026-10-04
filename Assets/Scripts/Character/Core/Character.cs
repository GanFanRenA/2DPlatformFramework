using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 该代码为所有角色的核心组件，负责初始化角色的各个子组件并作为根访问类，不进行具体操作
/// </summary>
public class Character : MonoBehaviour, ICharacterAnimatorSource
{
    [Header("Character Components")]
    [SerializeField] private CharacterConfig config;
    [SerializeField] private CharacterMotor2D motor;
    [SerializeField] private CharacterJumper2D jumper;
    [SerializeField] private CharacterAnimator animator;
    [SerializeField] private GroundChecker groundCheck;

    public CharacterMotor2D Motor => motor;
    public CharacterJumper2D Jumper => jumper;
    public CharacterAnimator Animator => animator;

    // ---- ICharacterAnimatorSource ----
    public float Facing => motor != null ? motor.Facing : 1f;
    public bool IsGrounded => jumper != null && jumper.IsGrounded;
    public bool IsMoving => motor != null && Mathf.Abs(motor.HorizontalSpeed) > 0.1f;
    public float VerticalSpeed => jumper != null ? jumper.VerticalSpeed : 0f;

    private bool _initialized;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        bool ok = true;

        if (config == null) { DebugOutputService.RunNullFatal(gameObject, nameof(config)); ok = false; }
        if (motor == null) { DebugOutputService.RunNullFatal(gameObject, nameof(motor)); ok = false; }
        if (jumper == null) { DebugOutputService.RunNullFatal(gameObject, nameof(jumper)); ok = false; }
        if (groundCheck == null) { DebugOutputService.RunNullFatal(gameObject, nameof(groundCheck)); ok = false; }
        if (animator == null) { DebugOutputService.RunNullFatal(gameObject, nameof(animator)); ok = false; }

        if (!ok)
        {
            enabled = false;
            return;
        }

        motor.Initialize(config);
        jumper.Initialize(config);
        animator.Initialize(this);   // 关键：把 Character 自己作为动画数据源

        _initialized = true;
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;

        // 顺序很重要：
        // 1. 从地面检测写入着地状态
        // 2. 更新计时器（冷却 / 预输入 / 土狼）
        // 3. 尝试跳跃
        // 4. 应用水平移动
        jumper.SetGrounded(groundCheck.IsGrounded);
        jumper.TickTimers(Time.fixedDeltaTime);
        jumper.TryPerformJump();
        motor.MoveTick(Time.fixedDeltaTime);
    }
}