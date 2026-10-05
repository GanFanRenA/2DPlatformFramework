using UnityEngine;

/// <summary>
/// 跳跃配置：跳跃力、冷却、预输入、土狼时间
/// </summary>
[CreateAssetMenu(fileName = "JumpConfig", menuName = "Platformer/JumpConfig")]
public class JumpConfig : ScriptableObject
{
    [Header("跳跃")]
    public float jumpForce = 14f;
    public float jumpCooldown = 0.2f;

    [Header("手感辅助")]
    public float jumpBufferTime = 0.1f;
    public float coyoteTime = 0.1f;
}