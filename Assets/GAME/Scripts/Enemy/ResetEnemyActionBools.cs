using UnityEngine;

namespace EternalKeep
{
    public class ResetEnemyActionBools : StateMachineBehaviour
    {

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            BaseEnemy baseEnemy = animator.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.isAttacking = false;
            }
        }


    }
}


