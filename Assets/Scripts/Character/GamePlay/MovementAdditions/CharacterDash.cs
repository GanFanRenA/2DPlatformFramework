using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 角色冲刺模块。冲刺期间接管水平速度，禁止移动与跳跃输入。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterDash : MonoBehaviour
{
    public bool IsDashing { get; private set; }
    public bool IsOnCooldown => _cooldownTimer > 0f;

    public event System.Action DashPerformed;
    public event System.Action DashEnded;

    private DashConfig _config;
    private Rigidbody2D _rb;

    private float _dashTimer;
    private float _cooldownTimer;
    private float _dashDirection;
    private bool _ready;

    public void Initialize(DashConfig config, Rigidbody2D rb)
    {
        _config = config;
        _rb = rb;

        if (_config == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_config)); enabled = false; return; }
        if (_rb == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_rb)); enabled = false; return; }

        _ready = true;
    }

    /// <summary>由输入层调用。facing 为当前朝向</summary>
    public void RequestDash(float facing)
    {
        if (!_ready) return;
        if (IsDashing) return;
        if (_cooldownTimer > 0f) return;

        _dashDirection = facing >= 0f ? 1f : -1f;
        IsDashing = true;
        _dashTimer = _config.dashDuration;
        _cooldownTimer = _config.dashCooldown;

        // 立即设置速度，避免下一帧才生效
        _rb.linearVelocity = new Vector2(_dashDirection * _config.dashSpeed, 0f);

        DashPerformed?.Invoke();
    }

    /// <summary>由 Character 每帧驱动</summary>
    public void Tick(float dt)
    {
        if (!_ready) return;

        if (_cooldownTimer > 0f) _cooldownTimer -= dt;

        if (!IsDashing) return;

        _dashTimer -= dt;

        // 冲刺期间保持速度
        _rb.linearVelocity = new Vector2(_dashDirection * _config.dashSpeed, 0f);

        if (_dashTimer <= 0f)
        {
            IsDashing = false;
            DashEnded?.Invoke();
        }
    }

    private void OnDisable()
    {
        IsDashing = false;
        _dashTimer = 0f;
        _cooldownTimer = 0f;
        _ready = false;
    }
}