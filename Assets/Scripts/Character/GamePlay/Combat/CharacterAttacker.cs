using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 角色攻击组件，负责请求攻击、连击窗口，并通过事件通知动画层
/// </summary>
public class CharacterAttacker : MonoBehaviour
{
    public bool IsAttacking { get; private set; }
    public int ComboIndex { get; private set; }

    /// <summary>攻击请求事件，动画桥订阅后写 Attack Trigger</summary>
    public event System.Action AttackRequested;
    /// <summary>攻击状态真正开始</summary>
    public event System.Action AttackStarted;
    /// <summary>攻击状态结束</summary>
    public event System.Action AttackEnded;

    private AttackConfig _config;

    private float _comboWindowEnd;
    private bool _attackBuffered;
    private bool _ready;

    public AttackConfig Config => _config;

    public void Initialize(AttackConfig config)
    {
        _config = config;
        if (_config == null) { DebugOutputService.RunNullFatal(gameObject, nameof(_config)); enabled = false; return; }
        _ready = true;
    }

    /// <summary>由输入层调用，请求攻击</summary>
    public void RequestAttack()
    {
        if (!_ready) return;

        if (IsAttacking)
        {
            // 攻击中再次按下，缓存，等当前段结束自动接下一段
            if (Time.time < _comboWindowEnd)
                _attackBuffered = true;
            return;
        }

        StartAttack();
    }

    private void StartAttack()
    {
        Debug.Log("[Attacker] StartAttack");

        IsAttacking = true;
        ComboIndex = 0;
        _comboWindowEnd = Time.time + _config.comboWindow;

        AttackStarted?.Invoke();
        AttackRequested?.Invoke();   // 通知动画层写 Trigger
    }

    /// <summary>由 AttackHitboxBehaviour 在攻击状态结束时调用</summary>
    public void OnAttackStateEnd()
    {
        IsAttacking = false;

        if (_attackBuffered)
        {
            _attackBuffered = false;
            StartAttack();   // 连击下一段
        }
        else
        {
            ComboIndex = 0;
            AttackEnded?.Invoke();
        }
    }

    private void OnDisable()
    {
        IsAttacking = false;
        _attackBuffered = false;
        ComboIndex = 0;
        _ready = false;
    }
}