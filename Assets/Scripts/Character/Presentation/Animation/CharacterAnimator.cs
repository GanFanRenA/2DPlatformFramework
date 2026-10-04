using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 该代码为角色的动画控制组件，负责根据角色状态更新 Animator，不直接处理输入或物理逻辑
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    private static readonly int HashState = Animator.StringToHash("state");

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private ICharacterAnimatorSource source;
    private AnimState currentState = AnimState.Idle;
    private bool _ready;

    public void Initialize(ICharacterAnimatorSource source)
    {
        this.source = source;

        bool ok = true;
        if (source == null) { DebugOutputService.RunNullFatal(gameObject, nameof(source)); ok = false; }
        if (animator == null) { DebugOutputService.RunNullFatal(gameObject, nameof(animator)); ok = false; }
        if (spriteRenderer == null) { DebugOutputService.RunNullFatal(gameObject, nameof(spriteRenderer)); ok = false; }

        _ready = ok;
        if (_ready) ResetState();
    }

    private void LateUpdate()
    {
        if (!_ready) return;

        UpdateFacing();
        UpdateState();
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null || source == null) return;
        spriteRenderer.flipX = source.Facing < 0f;
    }

    private void UpdateState()
    {
        if (animator == null || source == null) return;

        AnimState next = ResolveState();
        if (next == currentState) return;

        currentState = next;
        animator.SetInteger(HashState, (int)currentState);
    }

    private AnimState ResolveState()
    {
        if (!source.IsGrounded)
        {
            return source.VerticalSpeed > 0f ? AnimState.Jumping : AnimState.Falling;
        }
        return source.IsMoving ? AnimState.Moving : AnimState.Idle;
    }

    public void ResetState()
    {
        currentState = AnimState.Idle;
        if (animator != null)
            animator.SetInteger(HashState, (int)AnimState.Idle);
    }

    private void OnDisable() => ResetState();
}