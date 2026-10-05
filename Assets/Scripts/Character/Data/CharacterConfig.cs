using UnityEngine;

/// <summary>
/// 角色总配置。聚合移动、跳跃、攻击、健康等子配置。
/// 新增模块只需在这里加一行引用，不改 Character 的结构。
/// </summary>
[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Platformer/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    [SerializeField] private MovementConfig _movement;
    [SerializeField] private JumpConfig _jump;
    [SerializeField] private AttackConfig _attack;
    [SerializeField] private HealthConfig _health;
    [SerializeField] private DashConfig _dash;

    public MovementConfig Movement => _movement;
    public JumpConfig Jump => _jump;
    public AttackConfig Attack => _attack;
    public HealthConfig Health => _health;
    public DashConfig Dash => _dash;
}