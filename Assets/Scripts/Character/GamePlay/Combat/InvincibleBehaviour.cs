using UnityEngine;

/// <summary>
/// 受击状态的 StateMachineBehaviour。进入时设置无敌，退出时清除。
/// </summary>
public class InvincibleBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 无敌时间由 CharacterHealth.TakeDamage 内部处理，这里只做状态标记
        // 如果需要额外的行为（比如闪白、忽略输入），在这里加
    }
}