using UnityEngine;


namespace EternalKeep
{
    public class HandlePlayerCheatMode : MonoBehaviour
    {
        [SerializeField] bool isGodModeActive = false;
        [SerializeField] bool canEnableGodMode = true;

        [SerializeField] float maxHealth = 2000f;
        [SerializeField] int healthPotionCount = 10;

        [SerializeField] bool enableFallDamage = false;
        //float maxFallDistanceCheckValue = 500f; //max value to check void fall dmg

        [SerializeField] float maxStamina = 500f;
        [SerializeField] float staminaRechargeSpeed = 100f;
        [SerializeField] float baseAttackPower = 80;

        [SerializeField] float jumpForce = 25f;
        [SerializeField] float horizontaljumpForce = 12f;
        [SerializeField] float fallControlStrength = 10f;
        [SerializeField] float moveSpeed = 20f;

        [SerializeField] SwordDamage playerSwordDamage;
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] StaminaSystem_Player playerstaminaSystem;
        [SerializeField] PlayerLocomotion playerLocomotion;

        [SerializeField] PlayerOriginalData_SO playerOriginalData;


        public void ToggleGodMode()
        {
            if (!canEnableGodMode) return;

            if (isGodModeActive)
            {
                DeactivateGodMode();
            }
            else
            {
                ActivateGodMode();
            }
        }

        public void ActivateGodMode()
        {
            if (!canEnableGodMode) return;

            playerHealth.SetPlayerMaxHealth(maxHealth);
            playerHealth.SetPlayerCurrentHealth(maxHealth);
            playerHealth.SetMaxHealthPotionCount(healthPotionCount);
            playerHealth.SetCurrentHealthPotionCount(healthPotionCount);
            playerHealth.ToggleFallDamage(false);
            //playerLocomotion.SetMaxFallDistanceCheckValue(maxFallDistanceCheckValue);

            playerSwordDamage.SetBaseDamage(baseAttackPower);

            playerLocomotion.SetHorizontalJumpForce(horizontaljumpForce);
            playerLocomotion.SetDefaultHorizontalJumpForce(horizontaljumpForce);
            playerLocomotion.SetVerticalJumpForce(jumpForce);
            playerLocomotion.SetFallControlStrength(fallControlStrength);
            playerLocomotion.SetMoveSpeed(moveSpeed);
            playerLocomotion.SetDefaultMoveSpeed(moveSpeed);

            playerstaminaSystem.SetPlayerMaxStamina(maxStamina);
            playerstaminaSystem.SetPlayerCurrentStamina(maxStamina);
            playerstaminaSystem.SetPlayerStaminaRechargeSpeed(staminaRechargeSpeed);

            playerHealth.UpdatePlayerHealthPotionUI();

            isGodModeActive = true;

        }

        public void DeactivateGodMode()
        {
            playerHealth.SetPlayerMaxHealth(playerOriginalData.maxhealth);
            playerHealth.SetPlayerCurrentHealth(playerOriginalData.maxhealth);
            playerHealth.SetMaxHealthPotionCount(playerOriginalData.maxHealthPotionCount);
            playerHealth.SetCurrentHealthPotionCount(playerOriginalData.maxHealthPotionCount);
            playerHealth.ToggleFallDamage(playerOriginalData.canActivateFallDamage);
            //playerLocomotion.SetMaxFallDistanceCheckValue(playerOriginalData.maxFallDistanceCheckValue);

            playerSwordDamage.SetBaseDamage(playerOriginalData.baseAttackDamage);

            playerLocomotion.SetHorizontalJumpForce(playerOriginalData.horizontalJumpForce);
            playerLocomotion.SetDefaultHorizontalJumpForce(playerOriginalData.horizontalJumpForce);
            playerLocomotion.SetVerticalJumpForce(playerOriginalData.GetVerticalJumpForce());
            playerLocomotion.SetFallControlStrength(playerOriginalData.fallControlStrength);
            playerLocomotion.SetMoveSpeed(playerOriginalData.moveSpeed);
            playerLocomotion.SetDefaultMoveSpeed(playerOriginalData.moveSpeed);

            playerstaminaSystem.SetPlayerMaxStamina(playerOriginalData.maxStamina);
            playerstaminaSystem.SetPlayerCurrentStamina(playerOriginalData.maxStamina);
            playerstaminaSystem.SetPlayerStaminaRechargeSpeed(playerOriginalData.staminaRechargeSpeed);

            playerHealth.UpdatePlayerHealthPotionUI();

            isGodModeActive = false;
        }

        #region SAVE/LOAD

        public void SavePlayerGodModeData(ref PlayerGodModeData playerGodModeData)
        {
            playerGodModeData.isgodModeUsable = canEnableGodMode;
            playerGodModeData.isGodModeActive = isGodModeActive;

        }

        public void LoadPlayerGodModeData(PlayerGodModeData playerGodModeData)
        {
            canEnableGodMode = playerGodModeData.isgodModeUsable;
            isGodModeActive = playerGodModeData.isGodModeActive;

            if (canEnableGodMode && isGodModeActive)
            {
                ActivateGodMode();
            }
            else
            {
                DeactivateGodMode();
            }
        }

        public void ResetPlayerGodModeDataSaves(ref PlayerGodModeData playerGodModeData)
        {
            // must add logic (if gamecomplete set it to true or false)
            playerGodModeData.isgodModeUsable = true;

            playerGodModeData.isGodModeActive = false;
        }

        #endregion


    }
    
    [System.Serializable]
    public struct PlayerGodModeData
    {
        public bool isgodModeUsable;
        public bool isGodModeActive;

    }


}
