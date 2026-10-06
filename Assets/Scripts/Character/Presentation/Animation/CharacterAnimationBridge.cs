using UnityEngine;
using System.Collections;
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

    private Coroutine _stretchRoutine;
    private Transform _scaleTarget;
    private Vector3 _baseScale = Vector3.one;
    private bool _hasBaseScale;

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
        if (_spriteRenderer != null)
        {
            // Cache the animated visual transform once. Scale animation must not
            // modify the character root, which owns facing and physics.
            _scaleTarget = _spriteRenderer.transform;
            _baseScale = _scaleTarget.localScale;
            _hasBaseScale = true;
        }
        if (_dash != null) _dash.DashPerformed += OnDashPerformed;

        _ready = true;
    }

    public void SetStretch(Vector3 scale, float duration = 0.1f)
    {
        if (_scaleTarget == null) return;
        if (!gameObject.activeInHierarchy) return;

        // 打断上一次过渡，避免叠加
        if (_stretchRoutine != null)
        {
            StopCoroutine(_stretchRoutine);
            _stretchRoutine = null;
        }

        _stretchRoutine = StartCoroutine(StretchRoutine(scale, duration));
    }

    public void ResetStretch(float duration = 0.1f)
    {
        if (!_hasBaseScale) return;
        SetStretch(_baseScale, duration);
    }

    public void PunchScale(Vector3 punchScale,
                       float punchDelay = 0f,
                       float punchDuration = 0.06f,
                       float returnDelay = 0.04f,
                       float returnDuration = 0.15f)
    {
        if (_scaleTarget == null) return;
        if (!gameObject.activeInHierarchy) return;

        if (_stretchRoutine != null)
        {
            StopCoroutine(_stretchRoutine);
            _stretchRoutine = null;
        }

        _stretchRoutine = StartCoroutine(PunchRoutine(
            punchScale, punchDelay, punchDuration, returnDelay, returnDuration));
    }

    private IEnumerator PunchRoutine(Vector3 punchScale,
                                 float punchDelay, float punchDuration,
                                 float returnDelay, float returnDuration)
    {
        var t = _scaleTarget;

        // 1. 等待
        if (punchDelay > 0f) yield return new WaitForSeconds(punchDelay);

        // 2. 变形
        Vector3 start = t.localScale;
        yield return LerpScale(t, start, punchScale, punchDuration);

        // 3. 回弹前等待
        if (returnDelay > 0f) yield return new WaitForSeconds(returnDelay);

        // 4. 回弹到基准
        // Read the actual scale at the end of the punch phase. This keeps the
        // return continuous if another scale operation or animator update ran.
        Vector3 returnFrom = t.localScale;
        Vector3 baseScale = _hasBaseScale ? _baseScale : Vector3.one;
        yield return LerpScale(t, returnFrom, baseScale, returnDuration);

        _stretchRoutine = null;
    }

    private IEnumerator LerpScale(Transform t, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f) { t.localScale = to; yield break; }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - k) * (1f - k);
            t.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }
        t.localScale = to;
    }

    private IEnumerator StretchRoutine(Vector3 target, float duration)
    {
        var t = _scaleTarget;
        Vector3 start = t.localScale;

        if (duration <= 0f)
        {
            t.localScale = target;
            _stretchRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - k) * (1f - k);
            t.localScale = Vector3.LerpUnclamped(start, target, eased);
            Debug.Log($"[Stretch] 当前 scale = {t.localScale}");
            yield return null;
        }

        t.localScale = target;

        _stretchRoutine = null;
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
        if (_stretchRoutine != null)
        {
            StopCoroutine(_stretchRoutine);
            _stretchRoutine = null;
        }
        if (_scaleTarget != null && _hasBaseScale)
            _scaleTarget.localScale = _baseScale;

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
