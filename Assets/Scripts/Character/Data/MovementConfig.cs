using UnityEngine;

/// <summary>
/// 移动配置：水平速度、加速度
/// </summary>
[CreateAssetMenu(fileName = "MovementConfig", menuName = "Platformer/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("移动")]
    public float maxRunSpeed = 10f;
    public float runAcceleration = 60f;
}