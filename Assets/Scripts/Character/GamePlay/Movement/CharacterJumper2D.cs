using UnityEngine;

/// <summary>
/// 角色的跳跃组件，负责跳跃力、冷却、预输入、土狼时间与着地状态
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterJumper2D : MonoBehaviour
{
    /// <summary>当前是否着地（由 Character 每帧从 GroundChecker 写入）</summary>
    public bool IsGrounded { get; private set; }

    /// <summary>当前垂直速度（只读）</summary>
    public float VerticalSpeed => _rb != null ? _rb.linearVelocity.y : 0f;

    /// <summary>当前是否满足跳跃条件</summary>
    public bool CanJump =>
        _ready &&
        _jumpCooldownTimer <= 0f &&
        _jumpBufferTimer > 0f &&
        (IsGrounded || _coyoteTimer > 0f);

    [SerializeField] private Rigidbody2D _rb;

    private CharacterConfig _config;
    private float _jumpCooldownTimer;   // 跳跃冷却
    private float _jumpBufferTimer;     // 预输入
    private float _coyoteTimer;         // 土狼时间
    private bool _ready;

    public void Initialize(CharacterConfig config)
    {
        _config = config;
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();

        if (_rb == null || _config == null) return;

        ResetJumpTimers();
        _ready = true;
    }

    /// <summary>由 Character 每帧从 GroundChecker 写入</summary>
    public void SetGrounded(bool grounded) => IsGrounded = grounded;

    /// <summary>由输入层调用，请求跳跃</summary>
    public void RequestJump()
    {
        if (!_ready) return;
        _jumpBufferTimer = _config.jumpBufferTime;
    }

    /// <summary>由 Character 每帧调用，尝试执行跳跃</summary>
    public bool TryPerformJump()
    {
        if (!CanJump) return false;

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _config.jumpForce);
        _jumpCooldownTimer = _config.jumpCooldown;
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
        return true;
    }

    /// <summary>由 Character 每帧调用，更新冷却、预输入、土狼时间</summary>
    public void TickTimers(float dt)
    {
        if (!_ready) return;

        if (_jumpCooldownTimer > 0f) _jumpCooldownTimer -= dt;
        if (_jumpBufferTimer > 0f) _jumpBufferTimer -= dt;

        if (IsGrounded)
            _coyoteTimer = _config.coyoteTime;
        else if (_coyoteTimer > 0f)
            _coyoteTimer -= dt;
    }

    public void ResetJumpTimers()
    {
        _jumpCooldownTimer = 0f;
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
    }

    private void OnDisable()
    {
        ResetJumpTimers();
        IsGrounded = false;
        _ready = false;
    }
}