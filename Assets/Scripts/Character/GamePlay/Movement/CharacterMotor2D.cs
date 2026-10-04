using UnityEngine;

/// <summary>
/// 角色的水平移动组件，只负责水平速度与朝向，不处理跳跃、着地与表现
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMotor2D : MonoBehaviour
{
    /// <summary>当前朝向：1 = 右，-1 = 左</summary>
    public float Facing { get; private set; } = 1f;

    /// <summary>当前水平速度（只读）</summary>
    public float HorizontalSpeed => _rb != null ? _rb.linearVelocity.x : 0f;

    [SerializeField] private Rigidbody2D _rb;

    private CharacterConfig _config;
    private float _moveInput;
    private bool _ready;

    public void Initialize(CharacterConfig config)
    {
        _config = config;
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();

        if (_rb == null || _config == null) return;

        _moveInput = 0f;
        _ready = true;
    }

    /// <summary>由输入层调用，设置水平输入轴（-1 ~ 1）</summary>
    public void SetMoveInput(float axis) => _moveInput = Mathf.Clamp(axis, -1f, 1f);

    /// <summary>由 Character 每帧驱动</summary>
    public void MoveTick(float deltaTime)
    {
        if (!_ready) return;

        ApplyHorizontal(deltaTime);
        UpdateFacing();
    }

    private void ApplyHorizontal(float dt)
    {
        float targetSpeed = _moveInput * _config.maxRunSpeed;
        float newSpeed = Mathf.MoveTowards(
            _rb.linearVelocity.x, targetSpeed, _config.runAcceleration * dt);

        _rb.linearVelocity = new Vector2(newSpeed, _rb.linearVelocity.y);
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(_moveInput) < 0.01f) return;
        Facing = Mathf.Sign(_moveInput);
    }

    private void OnDisable()
    {
        _moveInput = 0f;
        _ready = false;
    }
}