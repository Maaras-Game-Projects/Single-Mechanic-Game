using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerAnimationManager playerAnimationManager;

    public void StartToAttack()
    {
        
        playerAnimationManager.PlayAnyInteractiveAnimation("Sword_Attack_1", false, true);
    }
}
