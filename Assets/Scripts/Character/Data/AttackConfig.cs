using UnityEngine;

/// <summary>
/// 攻击配置：攻击力、连击窗口、命中判定参数
/// </summary>
[CreateAssetMenu(fileName = "AttackConfig", menuName = "Platformer/AttackConfig")]
public class AttackConfig : ScriptableObject
{
    [Header("伤害")]
    public int attackPower = 1;

    [Header("命中检测")]
    public Vector2 hitboxOffset = new Vector2(0.8f, 0f);
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public LayerMask targetLayer;

    [Header("连击")]
    public float comboWindow = 0.6f;
}