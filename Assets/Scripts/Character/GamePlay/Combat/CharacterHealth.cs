using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 角色生命值组件，负责受伤、无敌、死亡，并通过事件通知表现层
/// </summary>
public class CharacterHealth : MonoBehaviour
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth => _config != null ? _config.maxHealth : 0;
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvincible => Time.time < _invincibleUntil;

    public event System.Action<int> Damaged;       // 参数：伤害值
    public event System.Action Died;
    public event System.Action<int> HealthChanged; // 参数：当前血量

    private HealthConfig _config;

    private Rigidbody2D _rb;
    private float _invincibleUntil;
    private bool _ready;

    public void Initialize(HealthConfig config, Rigidbody2D rb)
    {
        _config = config;
        _rb = rb;

        if (_config == null)
        {
            DebugOutputService.RunNullFatal(gameObject, nameof(_config)); enabled = false; return;
        }

        CurrentHealth = _config.maxHealth;
        _invincibleUntil = 0f;
        _ready = true;
    }

    /// <summary>受到伤害。返回是否实际受伤</summary>
    public bool TakeDamage(int amount, Vector2 hitSource)
    {
        if (!_ready || IsDead || IsInvincible) return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        _invincibleUntil = Time.time + _config.invincibleDuration;

        ApplyKnockback(hitSource);

        Damaged?.Invoke(amount);
        HealthChanged?.Invoke(CurrentHealth);

        if (IsDead) Died?.Invoke();

        return true;
    }

    private void ApplyKnockback(Vector2 hitSource)
    {
        if (_rb == null) return;

        float dirX = _rb.position.x >= hitSource.x ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dirX * _config.knockbackForce.x, _config.knockbackForce.y);
    }

    private void OnDisable()
    {
        _ready = false;
    }
}