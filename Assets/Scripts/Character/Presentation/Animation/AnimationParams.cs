using UnityEngine;

/// <summary>
/// 角色 Animator 参数协议。所有参数名与 Animator Controller 中必须一致。
/// 代码只写这些参数，状态切换交给 Animator 状态树。
/// </summary>
public static class AnimationParams
{
    // 事实参数
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int IsDead = Animator.StringToHash("IsDead");

    // 事件参数
    public static readonly int Jump = Animator.StringToHash("Jump");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int Hurt = Animator.StringToHash("Hurt");
    public static readonly int Dash = Animator.StringToHash("Dash");
}