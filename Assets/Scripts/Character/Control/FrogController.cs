using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 青蛙敌人控制器。以跳跃为攻击方式，Jump 与 Fall 阶段开启 Hitbox。
/// 不依赖 PlayerInput，不依赖 Animator 的 Attack 状态。
/// </summary>
[RequireComponent(typeof(Character))]
public class FrogController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private FrogConfig _config;

    [Header("调试")]
    [SerializeField] private bool _debugLog = false;

    /// <summary>外部可以通过这个属性设置目标，否则自动查找 "Player" tag</summary>
    public Transform Target { get; set; }

    private Character _character;
    private CharacterMotor2D _motor;
    private CharacterJumper2D _jumper;
    private CharacterHealth _health;
    private HitboxController _hitbox;

    private FrogState _state = FrogState.Idle;
    private float _stateTimer;
    private bool _ready;

    private enum FrogState
    {
        Idle,       // 等待，静止
        Windup,     // 前摇，朝目标转向并加速
        Airborne,   // 空中（上升 + 下落），开启 Hitbox
        Landing,    // 落地硬直，关闭 Hitbox
    }

    // ---- 生命周期 ----

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 由外部或 Start 调用。也允许 Character 在 Initialize 后主动调用以提前初始化。
    /// </summary>
    public void Initialize()
    {
        if (_config == null)
        {
            DebugOutputService.RunNullFatal(gameObject, nameof(_config));
            enabled = false;
            return;
        }

        if (_character == null)
        {
            DebugOutputService.RunNullFatal(gameObject, nameof(_character));
            enabled = false;
            return;
        }

        _motor = _character.Motor;
        _jumper = _character.Jumper;
        _health = _character.Health;
        _hitbox = _character.Hitbox;

        if (_motor == null)
        {
            DebugOutputService.RunNullFatal(gameObject, "CharacterMotor2D");
            enabled = false;
            return;
        }
        if (_jumper == null)
        {
            DebugOutputService.RunNullFatal(gameObject, "CharacterJumper2D");
            enabled = false;
            return;
        }
        if (_hitbox == null)
        {
            DebugOutputService.RunNullOptional(gameObject, "HitboxController", "青蛙将无法造成伤害");
        }

        // 自动查找玩家
        if (Target == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) Target = playerObj.transform;
        }

        EnterIdle();
        _ready = true;

        if (_debugLog) Debug.Log("[Frog] 初始化完成", this);
    }

    private void Update()
    {
        if (!_ready) return;

        // 死亡后停止一切行为
        if (_health != null && _health.IsDead)
        {
            if (_hitbox != null) _hitbox.SetActive(false);
            return;
        }

        TickStateMachine(Time.deltaTime);
        TickHitbox();
    }

    private void OnDisable()
    {
        if (_hitbox != null) _hitbox.SetActive(false);
        if (_motor != null) _motor.SetMoveInput(0f);
        _ready = false;
    }

    // ---- 状态机 ----

    private void TickStateMachine(float dt)
    {
        switch (_state)
        {
            case FrogState.Idle:
                _motor.SetMoveInput(0f);
                _stateTimer -= dt;
                if (_stateTimer <= 0f && IsTargetInRange())
                    EnterWindup();
                break;

            case FrogState.Windup:
                _stateTimer -= dt;
                if (_stateTimer <= 0f)
                    EnterAirborne();
                break;

            case FrogState.Airborne:
                // 空中不改输入，让角色保持起跳时的水平速度
                if (_jumper.IsGrounded)
                    EnterLanding();
                break;

            case FrogState.Landing:
                _motor.SetMoveInput(0f);
                _stateTimer -= dt;
                if (_stateTimer <= 0f)
                    EnterIdle();
                break;
        }
    }

    private void EnterIdle()
    {
        _state = FrogState.Idle;
        _stateTimer = _config.idleDuration;
        if (_debugLog) Debug.Log("[Frog] → Idle", this);
    }

    private void EnterWindup()
    {
        _state = FrogState.Windup;
        _stateTimer = _config.windupDuration;

        // 朝目标方向加速（让起跳时有水平速度）
        float dir = ComputeHorizontalDirection();
        _motor.SetMoveInput(dir);

        if (_debugLog) Debug.Log($"[Frog] → Windup, dir = {dir}", this);
    }

    private void EnterAirborne()
    {
        _state = FrogState.Airborne;

        // 起跳方向由 Windup 阶段已设置的输入决定
        // 如果需要空中控制，在起跳瞬间再乘一下 airControl
        float dir = ComputeHorizontalDirection();
        _motor.SetMoveInput(dir * _config.airControl);

        _jumper.RequestJump();

        if (_debugLog) Debug.Log($"[Frog] → Airborne, dir = {dir}", this);
    }

    private void EnterLanding()
    {
        _state = FrogState.Landing;
        _stateTimer = _config.landingDuration;
        _motor.SetMoveInput(0f);

        if (_debugLog) Debug.Log("[Frog] → Landing", this);
    }

    // ---- Hitbox 控制 ----

    private void TickHitbox()
    {
        if (_hitbox == null) return;

        // 只有在空中（跳跃上升或下落）时才开启
        bool airborne = _state == FrogState.Airborne && !_jumper.IsGrounded;

        if (!airborne)
        {
            _hitbox.SetActive(false);
            return;
        }

        // 根据上升 / 下落阶段和配置，决定是否开启
        bool rising = _jumper.VerticalSpeed > 0f;
        bool allowed = (rising && _config.damageOnRising) ||
                       (!rising && _config.damageOnFalling);

        _hitbox.SetActive(allowed);
    }

    // ---- 辅助 ----

    private float ComputeHorizontalDirection()
    {
        if (Target == null) return 0f;

        float dist = Target.position.x - transform.position.x;
        if (Mathf.Abs(dist) <= _config.deadZone) return 0f;

        return Mathf.Sign(dist);
    }

    private bool IsTargetInRange()
    {
        if (Target == null) return true;   // 没找到玩家就盲跳，制造存在感

        float dist = Vector2.Distance(transform.position, Target.position);
        return dist <= _config.detectRange;
    }

    // ---- 编辑期 Gizmo ----

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_config == null) return;

        // 检测范围
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _config.detectRange);

        // 死区（水平）
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.5f);
        Vector3 left = transform.position + Vector3.left * _config.deadZone;
        Vector3 right = transform.position + Vector3.right * _config.deadZone;
        Gizmos.DrawLine(left, right);
    }
#endif
}