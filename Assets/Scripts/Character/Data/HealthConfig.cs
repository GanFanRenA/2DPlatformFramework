using UnityEngine;

/// <summary>
/// 生命值配置：最大生命值、受伤无敌时间、击退参数
/// </summary>
[CreateAssetMenu(fileName = "HealthConfig", menuName = "Platformer/HealthConfig")]
public class HealthConfig : ScriptableObject
{
    [Header("生命值")]
    public int maxHealth = 5;

    [Header("无敌")]
    public float invincibleDuration = 0.8f;

    [Header("击退")]
    public Vector2 knockbackForce = new Vector2(5f, 8f);
}