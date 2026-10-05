using UnityEngine;

/// <summary>
/// 冲刺配置：速度、持续时间、冷却
/// </summary>
[CreateAssetMenu(fileName = "DashConfig", menuName = "Platformer/DashConfig")]
public class DashConfig : ScriptableObject
{
    [Header("冲刺")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.5f;
}