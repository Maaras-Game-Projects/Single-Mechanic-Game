using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace EternalKeep
{
    public class BackOffState : State, IEnemyStateReset
    {
        [SerializeField] CombatAdvanced_State combatAdvanced_State;
        [SerializeField] IdleState idleState;

        [SerializeField] float strafeBackDuration = 3f;
        [SerializeField] bool isBackingOff;

        [SerializeField] AnimationClip backstepAnimClip;
        [SerializeField] AnimationClip rollBackAnimClip;
        [SerializeField] BackOffWeights backOffWeights;

        public void ResetEnemyState()
        {
            isBackingOff = false;
        }
        [SerializeField] BackOffType currentBackOffType;

        private float elapsedTime;

        private Coroutine backstepCoroutine;
        private Coroutine rollBackCoroutine;

        public override void OnEnter()
        {
            idleState.GoToLocomotionAnimation();

            currentBackOffType = RollAndGetBackOffType();

            elapsedTime = 0f;

        }

        public override void OnExit()
        {
            isBackingOff = false;
        }

        public override void TickLogic()
        {

            npcRoot.LookAtPlayer(npcRoot.lookRotationSpeed);

            // Check if any obstacle is in the direction backwards
            Vector3 raycastDirection = npcRoot.transform.forward * -1f;

            RaycastHit hit;
            Ray ray = new Ray(npcRoot.transform.position + Vector3.up * 0.75f, raycastDirection);
            Debug.DrawRay(npcRoot.transform.position + Vector3.up * 0.75f, raycastDirection * 2f, Color.black, 1f);
            if (Physics.Raycast(ray, out hit, 1f, npcRoot.obstacleLayerMask))
            {
                //if obstacle is detected, switch to idle state
                Debug.Log("Obstacle detected while backing off, switching to combat state.");

                npcRoot.statemachine.SwitchState(combatAdvanced_State);
                return;
            }

            if (currentBackOffType == BackOffType.strafe_Back)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime < strafeBackDuration)
                {
                    isBackingOff = true;
                    npcRoot.SetStrafeAnimatorValues(direction.back);
                    npcRoot.UpdateMoveDirection();
                }
                else
                {
                    npcRoot.statemachine.SwitchState(combatAdvanced_State);
                }
            }
            else if (currentBackOffType == BackOffType.Backstep)
            {
                if (isBackingOff) return;
                isBackingOff = true;

                //play backstep animation and switch to combat state after anim end
                npcRoot.PlayAnyActionAnimation(backstepAnimClip.name);
                float waitTime = backstepAnimClip.length;
                backstepCoroutine = StartCoroutine(OnBackOffComplete(waitTime));
            }
            else if (currentBackOffType == BackOffType.rollBack)
            {
                if (isBackingOff) return;
                isBackingOff = true;

                //play rollback animation and switch to combat state after anim end
                npcRoot.PlayAnyActionAnimation(rollBackAnimClip.name);
                float waitTime = rollBackAnimClip.length;
                rollBackCoroutine = StartCoroutine(OnBackOffComplete(waitTime));
            }
        }

        private BackOffType RollAndGetBackOffType()
        {
            Dictionary<BackOffType, float> backOffWeightPair =
            new Dictionary<BackOffType, float>
            {
                {BackOffType.strafe_Back,backOffWeights.strafe_Back_weight},
                {BackOffType.Backstep,backOffWeights.backStep_weight},
                {BackOffType.rollBack,backOffWeights.rollBack_weight},

            };

            float totalChance = backOffWeights.strafe_Back_weight + backOffWeights.backStep_weight
                                    + backOffWeights.rollBack_weight;

            float randomValue = UnityEngine.Random.Range(0.1f, totalChance);

            foreach (var pair in backOffWeightPair)
            {
                BackOffType backOffType = pair.Key;
                float weight = pair.Value;
                if (randomValue <= weight)
                {
                    return backOffType;
                }

                randomValue -= weight;
            }

            return BackOffType.strafe_Back;
        }

        IEnumerator OnBackOffComplete(float waitTime)
        {

            yield return new WaitForSeconds(waitTime);

            npcRoot.statemachine.SwitchState(combatAdvanced_State);
        }


    }

    [Serializable]

    public class BackOffWeights
    {
        public float strafe_Back_weight = 10f;
        public float backStep_weight = 0;
        public float rollBack_weight = 0f;
    }

    public enum BackOffType
    {
        strafe_Back, Backstep, rollBack
    }


}




