using UnityEngine;

namespace EternalKeep
{
    public class ChaseState : State
    {

        [SerializeField] public float chaseRadius = 2f;
        [SerializeField] public float chaseDetectionDistance = 2.5f;


        //[SerializeField] private float chaseSpeed = 1.5f;

        [SerializeField] IdleState idleState;
        [SerializeField] CombatAdvanced_State combatState_Advanced;


        //[SerializeField] State attackState;  


        public override void SetCurrentSubState()
        {
            //subStatemachine.currentState = patrolSTate;
        }

        public override void OnEnter()
        {
            npcRoot.isChasingTarget = true;
        }

        //Need to Refactor
        public override void TickLogic()
        {
            //npcRoot.statemachine.SwitchState(combatState_Advanced);

            if (npcRoot.isInteracting) return;

            //npcRoot.UpdateStrafeDirection();

            if (combatState_Advanced.EnteredCombat)
            {
                if (combatState_Advanced.CheckIfInCombatRange())
                {

                    combatState_Advanced.inCombatRadius = true; // debug var
                    if (npcRoot.isPlayerInLineOfSight())
                    {
                        if (!combatState_Advanced.chaseToAttackAtStart)
                        {
                            npcRoot.statemachine.SwitchState(combatState_Advanced);
                            return;
                        }
                        else if (combatState_Advanced.IsPlayerInCloseRange())
                        {
                            npcRoot.statemachine.SwitchState(combatState_Advanced);
                            return;
                        }

                    }


                }
            }
            else if (combatState_Advanced.CheckIfInCombatModified_Range())
            {

                combatState_Advanced.inCombatRadius = true; // debug var
                if (npcRoot.isPlayerInLineOfSight())
                {
                    if (!combatState_Advanced.chaseToAttackAtStart)
                    {
                        npcRoot.statemachine.SwitchState(combatState_Advanced);
                        return;
                    }
                    else if (combatState_Advanced.IsPlayerInCloseRange())
                    {
                        npcRoot.statemachine.SwitchState(combatState_Advanced);
                        return;
                    }

                }


            }


            combatState_Advanced.inCombatRadius = false; // debug var

            Vector3 startPoint = npcRoot.transform.position;
            Vector3 endPoint = startPoint + npcRoot.transform.forward * chaseDetectionDistance;
            if (npcRoot.IsPlayerInRange_Capsule(startPoint, endPoint, chaseRadius))
            {
                idleState.GoToLocomotionAnimation();

                if (npcRoot.isPlayerInLineOfSight())
                {
                    //npcRoot.TurnCharacter();
                    npcRoot.LookAtPlayer(npcRoot.lookRotationSpeed);
                }
                //npcRoot.LookAtPlayer();
                npcRoot.SetNavMeshAgentDestination(npcRoot.targetTransform.position);
                npcRoot.SetStrafeAnimatorValues_Run();
                npcRoot.UpdateMoveDirection();

            }
            else
            {
                npcRoot.statemachine.SwitchState(idleState);
            }
        }



        public override void OnExit()
        {
            //navMeshAgent.isStopped = true;
            npcRoot.SetNavMeshAgentVelocityToZero();
            // navMeshAgent.updateRotation = true;
            // navMeshAgent.updatePosition = true;

            npcRoot.isChasingTarget = false;


        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            //chase Detection 
            VisualiseDetectionCapsule(chaseDetectionDistance, chaseRadius, Color.magenta);
        }

        private void VisualiseDetectionCapsule(float maxDistance, float lockONDetectionRadius, Color color)

        {
            Vector3 capsuleStart = transform.position;
            Vector3 capsuleEnd = transform.position + transform.forward * maxDistance;

            Gizmos.color = color;

            DrawCapsule(capsuleStart, capsuleEnd, lockONDetectionRadius);
        }

        private void DrawCapsule(Vector3 start, Vector3 end, float radius)
        {
            Gizmos.DrawWireSphere(start, radius);
            Gizmos.DrawWireSphere(end, radius);
            Gizmos.DrawLine(start + Vector3.up * radius, end + Vector3.up * radius);
            Gizmos.DrawLine(start + Vector3.down * radius, end + Vector3.down * radius);
            Gizmos.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
            Gizmos.DrawLine(start + Vector3.left * radius, end + Vector3.left * radius);
        }


#endif

    }
    
}



