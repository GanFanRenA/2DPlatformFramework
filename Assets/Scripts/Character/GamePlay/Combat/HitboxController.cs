using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击判定盒控制器。由 StateMachineBehaviour 在攻击判定帧内启用/禁用。
/// </summary>
public class HitboxController : MonoBehaviour
{
    [SerializeField] private AttackConfig _config;

    private readonly Collider2D[] _results = new Collider2D[8];
    private readonly HashSet<CharacterHealth> _hitTargets = new HashSet<CharacterHealth>();
    private bool _active;

    public void Initialize(AttackConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// 由 AttackHitboxBehaviour 调用。打开时执行一次命中检测。
    /// </summary>
    public void SetActive(bool active)
    {

        if (_active == active) return;
        _active = active;

        if (_active)
        {
            _hitTargets.Clear();   // 每次打开清空命中记录
            CheckHit();
        }
    }

    private void CheckHit()
    {
        if (_config == null)
        {
            Debug.LogError($"[Hitbox] _config 为空，无法进行命中检测。请检查 Inspector 引用。", this);
            return;
        }

        // 拿到角色朝向（用于翻转攻击盒）
        var motor = GetComponentInParent<CharacterMotor2D>();
        float facing = motor != null ? motor.Facing : 1f;

        // 只翻转 X 分量，Y 保持不变
        Vector2 offset = new Vector2(_config.hitboxOffset.x * facing, _config.hitboxOffset.y);
        Vector2 center = (Vector2)transform.position + offset;

        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = _config.targetLayer,
        };

        int count = Physics2D.OverlapBox(center, _config.hitboxSize, 0f, filter, _results);

        if (count == 0) return;

        // 自己的 Health，用于跳过自己
        var selfHealth = GetComponentInParent<CharacterHealth>();

        for (int i = 0; i < count; i++)
        {
            var target = _results[i].GetComponentInParent<CharacterHealth>();
            if (target == null) continue;
            if (target == selfHealth) continue;

            if (_hitTargets.Contains(target)) continue;
            _hitTargets.Add(target);

            bool damaged = target.TakeDamage(_config.attackPower, transform.position);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_config == null) return;

        float facing = 1f;
        if (Application.isPlaying)
        {
            var motor = GetComponentInParent<CharacterMotor2D>();
            if (motor != null) facing = motor.Facing;
        }

        Vector2 offset = new Vector2(_config.hitboxOffset.x * facing, _config.hitboxOffset.y);
        Vector3 center = transform.position + (Vector3)offset;

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawCube(center, _config.hitboxSize);

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f);
        Gizmos.DrawWireCube(center, _config.hitboxSize);
    }
#endif
}