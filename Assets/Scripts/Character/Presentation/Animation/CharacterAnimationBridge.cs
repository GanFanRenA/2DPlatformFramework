using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 角色动画桥接层。只负责把逻辑事实写入 Animator 参数，
/// 状态切换完全由 Animator 状态树决定。
/// </summary>
public class CharacterAnimationBridge : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private MovementConfig _movementConfig;
    private ICharacterAnimatorSource _source;
    private CharacterJumper2D _jumper;
    private CharacterAttacker _attacker;
    private CharacterHealth _health;
    private CharacterDash _dash;
    private bool _ready;

    public void Initialize(ICharacterAnimatorSource source,
                       CharacterJumper2D jumper,
                       CharacterAttacker attacker,
                       CharacterHealth health,
                       CharacterDash dash,
                       MovementConfig movementConfig)
    {
        _source = source;
        _jumper = jumper;
        _attacker = attacker;
        _health = health;
        _dash = dash;
        _movementConfig = movementConfig;

        bool ok = true;
        if (_source == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_source)); ok = false; }
        if (_animator == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_animator)); ok = false; }
        if (_spriteRenderer == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_spriteRenderer)); ok = false; }

        if (!ok)
        {
            enabled = false;
            return;
        }

        // 可选订阅：有就订阅，没有就跳过
        if (_jumper != null) _jumper.JumpPerformed += OnJumpPerformed;
        if (_attacker != null) _attacker.AttackRequested += OnAttackRequested;
        if (_health != null)
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
        }
        if (_dash != null) _dash.DashPerformed += OnDashPerformed;

        _ready = true;
    }

    private void OnDisable()
    {
        if (_jumper != null) _jumper.JumpPerformed -= OnJumpPerformed;
        if (_attacker != null) _attacker.AttackRequested -= OnAttackRequested;
        if (_health != null)
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }
        if (_dash != null) _dash.DashPerformed -= OnDashPerformed;

        _ready = false;
    }

    private void LateUpdate()
    {
        if (!_ready) return;

        // Speed 归一化
        float maxSpeed = _movementConfig != null ? _movementConfig.maxRunSpeed : 1f;
        if (maxSpeed > 0f)
        {
            float normalized = Mathf.Clamp01(Mathf.Abs(_source.HorizontalSpeed) / maxSpeed);
            _animator.SetFloat(AnimationParams.Speed, normalized, 0.1f, Time.deltaTime);
        }

        _animator.SetFloat(AnimationParams.VerticalVelocity, _source.VerticalSpeed);
        _animator.SetBool(AnimationParams.IsGrounded, _source.IsGrounded);

        if (Mathf.Abs(_source.Facing) > 0.01f)
            _spriteRenderer.flipX = _source.Facing < 0f;
    }

    private void OnJumpPerformed()
    {
        if (_animator != null)
            _animator.SetTrigger(AnimationParams.Jump);
    }
    private void OnAttackRequested()
    {
        Debug.Log("[Bridge] OnAttackRequested, 写 Attack Trigger");
        if (_animator != null)
            _animator.SetTrigger(AnimationParams.Attack);
    }
    private void OnDamaged(int amount) => _animator.SetTrigger(AnimationParams.Hurt);
    private void OnDied() => _animator.SetBool(AnimationParams.IsDead, true);
    private void OnDashPerformed() => _animator.SetTrigger(AnimationParams.Dash);
}