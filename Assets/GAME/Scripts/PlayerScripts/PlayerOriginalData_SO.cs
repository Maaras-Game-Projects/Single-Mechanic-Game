
using UnityEngine;

namespace EternalKeep
{
    [CreateAssetMenu(fileName = "PlayerOGData", menuName = "ScriptableObjects/PlayerOriginalData")]
    public class PlayerOriginalData_SO : ScriptableObject
    {
        public float moveSpeed = 4f;
        public float horizontalJumpForce;
        public float fallControlStrength;
        //public float maxFallDistanceCheckValue;
        public float jumpHeight;
        public float gravityIntensity;
        public float maxhealth;
        public int maxHealthPotionCount;
        public bool canActivateFallDamage;
        public float baseAttackDamage = 30f;
        public float maxStamina;
        public float staminaRechargeSpeed;

        public float GetVerticalJumpForce()
        {
            return Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
        }
        
    }


}
