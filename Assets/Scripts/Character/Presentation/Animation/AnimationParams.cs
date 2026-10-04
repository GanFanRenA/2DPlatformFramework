using UnityEngine;

/// <summary>
/// 角色 Animator 参数协议。所有参数名与 Animator Controller 中必须一致。
/// 代码只写这些参数，状态切换交给 Animator 状态树。
/// </summary>
public static class AnimationParams
{
    // ---- 事实参数：每帧写入 ----
    /// <summary>归一化水平速度，0 ~ 1</summary>
    public static readonly int Speed = Animator.StringToHash("Speed");
    /// <summary>原始垂直速度，正值上升，负值下落</summary>
    public static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
    /// <summary>是否着地</summary>
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

    // ---- 事件参数：发生时写入 ----
    /// <summary>跳跃触发</summary>
    public static readonly int Jump = Animator.StringToHash("Jump");

    // 后续步骤会加：
    // Attack, Hurt, Dead, Dash
}