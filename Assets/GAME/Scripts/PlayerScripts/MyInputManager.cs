using System;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalKeep
{
    public class MyInputManager : MonoBehaviour
    {
        MyInputActions myInputActions;

        [SerializeField] Queue<BufferedInput> bufferedInputs = new Queue<BufferedInput>();
        [SerializeField] float defaultInputBufferDuration = 0.25f;

        [SerializeField] PlayerAnimationManager playerAnimationManager;
        [SerializeField] PlayerLocomotion playerLocomotion;
        [SerializeField] PlayerCombat playerCombat;
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] InteractionManager interactionManager;
        [SerializeField] public float moveAmount;
        public Vector2 movementInput;

        public float verticalMovementInput;
        public float horizontalMovementInput;

        public bool walkInput = false;
        public bool jumpInput = false;
        public bool rollInput = false;
        public bool interactInput = false;

        public bool attackInput = false;
        public bool blockInput = false;
        public bool parryInput = false;
        public bool healInput = false;
        public bool lockOnInput = false;
        public float switchTargetDelta_Left;
        public float switchTargetDelta_Right;
        public float switchTargetDeltaThreshold = 50f;

        public bool hasSwipedLeft = false;
        public bool hasSwipedRight = false;
        public bool lockOnleftSwitchInput = false;
        public bool lockOnRightSwitchInput = false;

        public bool escapeMenuInGameInput = false;

        private void OnEnable()
        {
            if (myInputActions == null)
            {
                myInputActions = new MyInputActions();
            }

            myInputActions.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            // myInputActions.PlayerMovement.Walk.performed += i => walkInput = true;
            // myInputActions.PlayerMovement.Walk.canceled += i => walkInput = false;

            myInputActions.PlayerMovement.Roll.performed += i => rollInput = true;
            myInputActions.PlayerMovement.Jump.performed += i => jumpInput = true;

            myInputActions.PlayerActions.Interact.performed += i => interactInput = true;
            myInputActions.PlayerActions.Heal.performed += i => healInput = true;

            myInputActions.PlayerCombat.Parry.performed += i => parryInput = true;
            myInputActions.PlayerCombat.Attack.performed += i => attackInput = true;

            myInputActions.PlayerCombat.Block.performed += i => blockInput = true;
            myInputActions.PlayerCombat.Block.canceled += i =>
            {
                blockInput = false;
                //playerAnimationManager.playerAnimator.SetBool("inBlocking", false);
                playerAnimationManager.playerAnimator.SetBool("Block_test", false);
                walkInput = false;
                playerCombat.isBlocking = false;
                playerCombat.EnableCounter();
                playerCombat.AllowParry();
                playerCombat.DisableParry();
            };


            myInputActions.PlayerCombat.TargetLockOn.performed += i => lockOnInput = true;

            // gamepad input not working for target switching need to seperate actions
            myInputActions.PlayerCombat.SwitchLeftTarget.performed += i => switchTargetDelta_Left = i.ReadValue<float>();
            myInputActions.PlayerCombat.SwitchRightTarget.performed += i => switchTargetDelta_Right = i.ReadValue<float>();

            myInputActions.UI.GoToInGameMenu.performed += i => escapeMenuInGameInput = true;

            myInputActions.Enable();
        }


        private void OnDisable()
        {
            myInputActions.Disable();
        }

        public void HandleAllInput()
        {
            HandleLockONInput();
            HandleSwitchLockONInput_Left();
            HandleSwitchLockONInput_Right();
            HandleMovementInput();
            HandleJumpInput();
            HandleRollInput();
            HandleAttackInput();
            HandleBlockInput();
            HandleParryInput();
            HandleHealInput();
            HandleInteractInput();
            HandleInGameMenuUIInput();

            HandleInputBuffer();
        }

        public void TryOrBufferInput(Func<bool> canBufferNow, Action action, BufferInputType bufferInputType)
        {
            if (!canBufferNow())
            {
                action();
            }
            else
            {
                bufferedInputs.Enqueue(new BufferedInput(action, Time.time + defaultInputBufferDuration, bufferInputType));
            }
        }

        private bool CanAcceptInput()
        {
            return !playerAnimationManager.rootMotionUseStatus;
        }

        private bool CanAcceptDodgeBuffer()
        {
            return playerLocomotion.CanBufferDodge();
        }

        private bool CanAcceptAttackBuffer()
        {
            return playerCombat.canBufferAttack();
        }
        
         private bool CanAcceptParryBuffer()
        {
            return playerCombat.CanBufferParry();
        }

        private void HandleInputBuffer()
        {
            //if (!CanAcceptInput()) return;

            int count = bufferedInputs.Count;

            if (count == 0) return;
            Debug.Log("Queue Count = " + count);

            for (int i = 0; i < count; i++)
            {
                BufferedInput bufferedInput = bufferedInputs.Peek();

                if (Time.time >= bufferedInput.expireTime)
                {
                    Debug.Log("Expired");
                    bufferedInputs.Dequeue();
                    continue;
                }
                else
                    Debug.Log("Not Expired");

                // if (CanAcceptInput())
                // {
                //     bufferedInput.action();
                // }
                if (bufferedInput.bufferInputType == BufferInputType.DodgeRoll ||
                    bufferedInput.bufferInputType == BufferInputType.Parry)
                {
                    if (playerAnimationManager.CanOverrideAnimation)
                    {
                        bufferedInput.action();
                        bufferedInputs.Dequeue();
                        Debug.Log("Buffer Action" + bufferedInput.action.ToString());
                        //return;
                    }
                }
                else if (bufferedInput.bufferInputType == BufferInputType.Attack)
                {
                    if (playerCombat.CanCombo)
                    {
                        bufferedInput.action();
                        bufferedInputs.Dequeue();
                        Debug.Log("Buffer Action Attack Combo" + bufferedInput.action.ToString());
                        //return;
                    }
                    else if (playerAnimationManager.CanOverrideAnimation && !playerCombat.CanCombo) // buffer only from other actions to attack, not from attack to attack
                    {
                        bufferedInput.action();
                        bufferedInputs.Dequeue();
                        Debug.Log("Buffer Action" + bufferedInput.action.ToString());
                        //return;
                    }

                }

                // else if (bufferedInput.bufferInputType == BufferInputType.Attack)
                // {
                //     if (playerCombat.CanCombo)
                //     {
                //         bufferedInput.action();
                //         bufferedInputs.Dequeue();
                //         Debug.Log("Buffer Action Attack Combo" + bufferedInput.action.ToString());
                //     }
                // }

                // bufferedInput.action();
                // Debug.Log("Buffer Action" + bufferedInput.action.ToString());


            }
        }



        private void HandleJumpInput()
        {
            if (jumpInput)
            {
                jumpInput = false;
                playerLocomotion.HandleJump();
            }
        }

        private void HandleRollInput()
        {
            if (rollInput)
            {
                rollInput = false;
                TryOrBufferInput(() => CanAcceptDodgeBuffer(), () => playerLocomotion.HandleRolling(),BufferInputType.DodgeRoll);
                //playerLocomotion.HandleRolling();
            }
        }

        private void HandleAttackInput()
        {
            if (attackInput)
            {

                attackInput = false;
                TryOrBufferInput(() => CanAcceptAttackBuffer(), () => playerCombat.StartToAttack(),BufferInputType.Attack);
                //playerCombat.StartToAttack();
            }
        }

        private void HandleParryInput()
        {
            if (parryInput)
            {

                parryInput = false;
                TryOrBufferInput(() => CanAcceptParryBuffer(), () => playerCombat.Parry(),BufferInputType.Parry);
                //playerCombat.Parry();
            }
        }

        private void HandleHealInput()
        {
            if (healInput)
            {

                healInput = false;
                playerHealth.PlayHealAnimation();
            }
        }

        private void HandleInteractInput()
        {
            if (interactInput)
            {

                interactInput = false;
                interactionManager.ActivateInteraction();
            }
        }

        private void HandleLockONInput()
        {
            if (lockOnInput)
            {

                lockOnInput = false;
                playerLocomotion.HandleTargetLockON();
            }
        }

        private void HandleSwitchLockONInput_Left()
        {
            if (!playerLocomotion.isLockedOnTarget) return;

            if (switchTargetDelta_Left > switchTargetDeltaThreshold)
            {
                if (!hasSwipedLeft)
                {
                    lockOnleftSwitchInput = true;
                    hasSwipedLeft = true;
                }

            }
            else
            {
                hasSwipedLeft = false;
            }

            if (lockOnleftSwitchInput)
            {
                lockOnleftSwitchInput = false;
                Debug.Log("Switching Left");
                playerLocomotion.HandleSwitchLeftTarget();
            }

        }

        private void HandleSwitchLockONInput_Right()
        {
            if (!playerLocomotion.isLockedOnTarget) return;

            if (switchTargetDelta_Right > switchTargetDeltaThreshold)
            {
                if (!hasSwipedRight)
                {
                    lockOnRightSwitchInput = true;
                    hasSwipedRight = true;
                }

            }
            else
            {
                hasSwipedRight = false;
            }

            if (lockOnRightSwitchInput)
            {
                lockOnRightSwitchInput = false;
                Debug.Log("Switching Right");
                playerLocomotion.HandleSwitchRightTarget();
            }

        }


        private void HandleBlockInput()
        {
            if (blockInput)
            {

                playerCombat.BlockAttack();
            }
        }


        private void HandleMovementInput()
        {
            //if (playerAnimationManager.inAnimActionStatus) return;
            verticalMovementInput = movementInput.y;
            horizontalMovementInput = movementInput.x;

            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMovementInput) + Mathf.Abs(verticalMovementInput));

            if (moveAmount < 0.01)
            {
                playerAnimationManager.playerAnimator.SetBool("isMoving", false);
            }
            else
            {
                playerAnimationManager.playerAnimator.SetBool("isMoving", true);
            }

            if (playerLocomotion.isLockedOnTarget)
            {
                playerAnimationManager.UpdateAnimatorValuesForMovement(horizontalMovementInput, verticalMovementInput, playerLocomotion.isWalking);
            }
            else
            {
                playerAnimationManager.UpdateAnimatorValuesForMovement(0, moveAmount, playerLocomotion.isWalking);
            }

        }

        private void HandleInGameMenuUIInput()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) return; // Skip if in the main menu scene

            if (escapeMenuInGameInput)
            {
                escapeMenuInGameInput = false;

                if (!HandleInGameMenu.Instance.IsMenuShowing)
                {
                    HandleInGameMenu.Instance.FadeInLoadingScreen();

                }
                else
                {
                    HandleInGameMenu.Instance.FadeOutLoadingScreen();
                }

            }
        }
    }

    public class BufferedInput
    {
        public Action action;
        public float expireTime;

        public BufferInputType bufferInputType;

        public BufferedInput(Action a, float time, BufferInputType type)
        {
            action = a;
            expireTime = time;
            bufferInputType = type;
        }
    }

    public enum BufferInputType
    {
        DodgeRoll, Attack, Parry
    }

}
