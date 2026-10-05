using UnityEngine;

/// <summary>
/// 攻击状态的 StateMachineBehaviour。
/// 在指定 normalizedTime 区间内启用 Hitbox，退出时通知 Attacker。
/// </summary>
public class AttackHitboxBehaviour : StateMachineBehaviour
{
    [SerializeField] private float _hitStart = 0.3f;
    [SerializeField] private float _hitEnd = 0.6f;

    private HitboxController _hitbox;
    private CharacterAttacker _attacker;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("[SMB] Attack 进入");
        var character = animator.GetComponentInParent<Character>();
        if (character != null)
        {
            _hitbox = character.GetComponentInChildren<HitboxController>(true);
            _attacker = character.GetComponentInChildren<CharacterAttacker>(true);
        }

        if (_hitbox != null) _hitbox.SetActive(false);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_hitbox == null) return;

        float t = stateInfo.normalizedTime % 1f;
        _hitbox.SetActive(t >= _hitStart && t <= _hitEnd);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_hitbox != null) _hitbox.SetActive(false);
        if (_attacker != null) _attacker.OnAttackStateEnd();
    }
}