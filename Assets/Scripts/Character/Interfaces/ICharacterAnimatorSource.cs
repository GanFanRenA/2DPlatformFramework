/// <summary>
/// 该接口为角色动画数据源，提供角色的朝向、是否着地、移动状态及垂直速度等信息
/// </summary>
public interface ICharacterAnimatorSource
{
    float Facing { get; }
    bool IsGrounded { get; }
    bool IsMoving { get; }
    float VerticalSpeed { get; }
}