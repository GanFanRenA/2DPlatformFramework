/// <summary>
/// 该枚举为角色的动画状态，定义了角色可能的动作状态，所有角色公用
/// </summary>
public enum AnimState
{
    Idle = 0,
    Moving = 1,
    Jumping = 5,
    Falling = 6,
    Attacking = 10,
    Hurt = 15,
    Dead = 20,
}