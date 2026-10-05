using UnityEngine;

/// <summary>
/// 青蛙敌人配置：跳跃节奏、追击范围、伤害开关
/// </summary>
[CreateAssetMenu(fileName = "FrogConfig", menuName = "Platformer/FrogConfig")]
public class FrogConfig : ScriptableObject
{
    [Header("节奏（秒）")]
    [Tooltip("两次跳跃之间的等待时间")]
    public float idleDuration = 1.5f;

    [Tooltip("起跳前的前摇，用于转向和加速")]
    public float windupDuration = 0.25f;

    [Tooltip("落地后的硬直时间")]
    public float landingDuration = 0.5f;

    [Header("追击")]
    [Tooltip("检测玩家的最大距离，超出则原地等待")]
    public float detectRange = 10f;

    [Tooltip("水平距离小于此值时不移动，直接垂直跳")]
    public float deadZone = 0.4f;

    [Tooltip("空中水平输入保持系数，0 = 垂直跳，1 = 全速水平移动")]
    [Range(0f, 1f)] public float airControl = 1f;

    [Header("伤害开关")]
    [Tooltip("上升阶段（Jump）是否造成伤害")]
    public bool damageOnRising = true;

    [Tooltip("下落阶段（Fall）是否造成伤害")]
    public bool damageOnFalling = true;
}