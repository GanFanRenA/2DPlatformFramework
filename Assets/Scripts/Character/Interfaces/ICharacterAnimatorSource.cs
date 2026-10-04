/// <summary>
/// 角色动画数据源，提供角色的朝向、速度、着地状态
/// </summary>
public interface ICharacterAnimatorSource
{
    float Facing { get; }
    bool IsGrounded { get; }
    float HorizontalSpeed { get; }
    float VerticalSpeed { get; }
    bool IsMoving { get; }
}