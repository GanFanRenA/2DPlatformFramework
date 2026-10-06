using UnityEngine;

[CreateAssetMenu(fileName = "FrogConfig", menuName = "Platformer/FrogConfig")]
public class FrogConfig : ScriptableObject
{
    [Header("节奏（秒）")]
    [Tooltip("两次跳跃之间的等待时间")]
    public float idleDuration = 1.5f;

    [Tooltip("落地后的硬直时间")]
    public float landingDuration = 0.5f;

    [Header("追击")]
    [Tooltip("检测玩家的最大距离，超出则原地等待")]
    public float detectRange = 10f;

    [Tooltip("水平距离小于此值时不移动，直接垂直跳")]
    public float deadZone = 0.4f;

    [Header("跳跃")]
    [Tooltip("起跳瞬间的水平速度。0 = 垂直跳，越大跳得越远")]
    public float horizontalJumpSpeed = 6f;

    [Tooltip("下降阶段重力倍率。1 = 与上升相同，1.5~2.5 = 下落更快，更利落")]
    [Range(1f, 3f)] public float fallMultiplier = 1.8f;

    [Tooltip("上升阶段重力倍率。0.7~0.9 = 上升更飘，滞空感更强")]
    [Range(0.5f, 1f)] public float riseMultiplier = 0.85f;

    [Tooltip("空中水平速度每秒衰减量。0 = 不衰减，越大越快停下来")]
    [Range(0f, 10f)] public float airDrag = 2f;

    [Tooltip("顶点水平速度倍率。到抛物线顶点时水平速度乘以这个值")]
    [Range(0.5f, 1f)] public float apexSpeedMultiplier = 0.85f;

    [Header("伤害开关")]
    [Tooltip("上升阶段（Jump）是否造成伤害")]
    public bool damageOnRising = true;

    [Tooltip("下落阶段（Fall）是否造成伤害")]
    public bool damageOnFalling = true;
}