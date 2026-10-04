using UnityEngine;
/// <summary>
/// 该代码为角色的移动组件，负责处理角色的水平移动、跳跃，不直接操作表现层
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour, ICharacterAnimatorSource
{
    /// <summary>当前朝向：1 = 右，-1 = 左</summary>
    public float Facing { get; private set; } = 1f;

    //是否在地面
    public bool IsGrounded { get; private set; }

    //是否可以跳跃
    public bool CanJump =>
        config != null &&
        jumpCooldownTimer <= 0f &&
        jumpBufferTimer > 0f &&
        coyoteTimer > 0f;

    // ---- ICharacterAnimatorSource ---- 关于动画接口的相关属性赋值
    public bool IsMoving => Mathf.Abs(rb.linearVelocity.x) > 0.1f;
    public float HorizontalSpeed => rb.linearVelocity.x;
    public float VerticalSpeed => rb.linearVelocity.y;

    [SerializeField] private Rigidbody2D rb;
    private CharacterConfig config;
    private float moveInput;
    private float jumpCooldownTimer;//跳跃冷却: 每次跳跃后需要等待一段时间才能再次跳跃
    private float jumpBufferTimer;//预跳跃: 在按下跳跃键后的一小段时间内仍然可以触发跳跃
    private float coyoteTimer;//土狼时间: 滞空后短暂时间依然可以跳跃

    public void Initialize(CharacterConfig config)
    {
        this.config = config;
        ResetJumpTimers();
    }

    public void SetGrounded(bool grounded) => IsGrounded = grounded;

    //设置移动方向
    public void SetMoveInput(float axis) => moveInput = Mathf.Clamp(axis, -1f, 1f);

    //请求跳跃
    public void RequestJump()
    {
        if (config == null) return;
        jumpBufferTimer = config.jumpBufferTime;
    }

    //尝试执行跳跃
    public bool TryPerformJump()
    {
        if (!CanJump) return false;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, config.jumpForce);
        jumpCooldownTimer = config.jumpCooldown;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        return true;
    }

    //每帧移动逻辑
    public void MoveTick(float deltaTime)
    {
        if (config == null) return;

        TickTimers(deltaTime);
        ApplyHorizontal(deltaTime);
        UpdateFacing();
    }

    //计时器更新
    private void TickTimers(float dt)
    {
        if (jumpCooldownTimer > 0f) jumpCooldownTimer -= dt;
        if (jumpBufferTimer > 0f) jumpBufferTimer -= dt;

        if (IsGrounded)
            coyoteTimer = config.coyoteTime;
        else if (coyoteTimer > 0f)
            coyoteTimer -= dt;
    }
    //应用水平移动
    private void ApplyHorizontal(float dt)
    {
        float targetSpeed = moveInput * config.maxRunSpeed;
        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x, targetSpeed, config.runAcceleration * dt);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    //更新朝向  
    private void UpdateFacing()
    {
        if (Mathf.Abs(moveInput) < 0.01f) return;
        Facing = Mathf.Sign(moveInput);
    }

    //重置跳跃计时器
    public void ResetJumpTimers()
    {
        jumpCooldownTimer = 0f;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
    }

    //禁用时重置状态
    private void OnDisable()
    {
        moveInput = 0f;
        ResetJumpTimers();
    }
}