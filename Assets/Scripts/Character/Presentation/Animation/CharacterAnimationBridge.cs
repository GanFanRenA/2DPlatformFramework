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

    private ICharacterAnimatorSource _source;
    private CharacterJumper2D _jumper;
    private bool _ready;

    public void Initialize(ICharacterAnimatorSource source, CharacterJumper2D jumper)
    {
        _source = source;
        _jumper = jumper;

        bool ok = true;
        if (_source == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_source)); ok = false; }
        if (_jumper == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_jumper)); ok = false; }
        if (_animator == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_animator)); ok = false; }
        if (_spriteRenderer == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_spriteRenderer)); ok = false; }

        if (!ok)
        {
            enabled = false;
            return;
        }

        // 订阅跳跃事件，发生时写 Trigger
        _jumper.JumpPerformed += OnJumpPerformed;

        _ready = true;
    }

    private void OnDisable()
    {
        if (_jumper != null)
            _jumper.JumpPerformed -= OnJumpPerformed;

        _ready = false;
    }

    private void LateUpdate()
    {
        if (!_ready) return;

        // ---- 只写事实参数 ----
        float maxSpeed = 1f;   // 归一化用，稍后从 Config 注入
        if (_source != null && maxSpeed > 0f)
        {
            float normalized = Mathf.Clamp01(Mathf.Abs(_source.HorizontalSpeed) / maxSpeed);
            _animator.SetFloat(AnimationParams.Speed, normalized, 0.1f, Time.deltaTime);
        }

        _animator.SetFloat(AnimationParams.VerticalVelocity, _source.VerticalSpeed);
        _animator.SetBool(AnimationParams.IsGrounded, _source.IsGrounded);

        // ---- 朝向只影响渲染 ----
        if (Mathf.Abs(_source.Facing) > 0.01f)
            _spriteRenderer.flipX = _source.Facing < 0f;
    }

    private void OnJumpPerformed()
    {
        if (_animator != null)
            _animator.SetTrigger(AnimationParams.Jump);
    }
}