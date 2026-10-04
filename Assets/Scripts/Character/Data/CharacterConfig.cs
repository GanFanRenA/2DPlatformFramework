using UnityEngine;
/// <summary>
/// 该代码为角色的配置数据，包含移动、跳跃及手感辅助参数
/// </summary>
[CreateAssetMenu(fileName = "CharacterConfig", menuName = "Platformer/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    [Header("移动")]
    public float maxRunSpeed = 10f;
    public float runAcceleration = 60f;

    [Header("跳跃")]
    public float jumpForce = 14f;
    public float jumpCooldown = 0.2f;

    [Header("手感辅助")]
    public float jumpBufferTime = 0.1f;
    public float coyoteTime = 0.1f;
}