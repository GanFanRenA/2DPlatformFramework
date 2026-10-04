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
    [SerializeField] private CharacterAnimationBridge animationBridge;
    [SerializeField] private GroundChecker groundCheck;

    public CharacterMotor2D Motor => motor;
    public CharacterJumper2D Jumper => jumper;
    public CharacterAnimationBridge AnimationBridge => animationBridge;

    // ---- ICharacterAnimatorSource ----
    public float Facing => motor != null ? motor.Facing : 1f;
    public bool IsGrounded => jumper != null && jumper.IsGrounded;
    public float HorizontalSpeed => motor != null ? motor.HorizontalSpeed : 0f;
    public float VerticalSpeed => jumper != null ? jumper.VerticalSpeed : 0f;
    public bool IsMoving => Mathf.Abs(HorizontalSpeed) > 0.1f;

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
        if (animationBridge == null) { DebugOutputService.RunNullFatal(gameObject, nameof(animationBridge)); ok = false; }

        if (!ok)
        {
            enabled = false;
            return;
        }

        motor.Initialize(config);
        jumper.Initialize(config);
        animationBridge.Initialize(this, jumper);

        _initialized = true;
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;

        jumper.SetGrounded(groundCheck.IsGrounded);
        jumper.TickTimers(Time.fixedDeltaTime);
        jumper.TryPerformJump();
        motor.MoveTick(Time.fixedDeltaTime);
    }
}