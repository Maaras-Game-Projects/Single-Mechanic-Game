using System.Collections;
using UnityEngine;

namespace EternalKeep
{
    public class PatrolState : State
    {
        [SerializeField] IdleState idleState;
        [SerializeField] ChaseState chaseState;
        [SerializeField] Transform homePosition;
        [SerializeField] Transform[] patrolPoints;

        [SerializeField] float stallRange_Minimum = 1f;
        [SerializeField] float stallRange_Maximum = 3f;

        [SerializeField] int currentPatrolIndex = 0;

        [SerializeField] bool canGoHomePoint = false;
        [SerializeField] bool canGoIdle = false;
        [SerializeField] bool canSetDestination = true;

        bool isIdling = false;
        Coroutine idleCoroutine;




        public override void OnEnter()
        {
            npcRoot.isPatrolling = true;
        }

        public override void OnExit()
        {
            npcRoot.isPatrolling = false;
        }


        public override void TickLogic()
        {

            if (npcRoot.isInteracting) return;

            if (!canGoIdle)
            {
                idleState.GoToLocomotionAnimation();

                Vector3 targetPoint;

                if (canGoHomePoint)
                {
                    targetPoint = homePosition.position;
                }
                else
                {
                    targetPoint = patrolPoints[currentPatrolIndex].position;
                }

                npcRoot.LookAtTargetPoint(npcRoot.lookRotationSpeed, targetPoint);

                if (canSetDestination)
                {
                    canSetDestination = false;
                    npcRoot.SetNavMeshAgentDestination(targetPoint);
                }

                npcRoot.SetStrafeAnimatorValues(direction.front);
                npcRoot.UpdateMoveDirection();

                if (hasReachedPatrolPoint(targetPoint, false))
                {
                    if (canGoHomePoint)
                        canGoHomePoint = false;

                    currentPatrolIndex++;
                    canSetDestination = true;
                    canGoIdle = true;
                    if (currentPatrolIndex >= patrolPoints.Length)
                    {
                        currentPatrolIndex = -1;
                        canGoHomePoint = true;

                    }
                   // Debug.Log("<color=green>Reached Patrol Point, moving to next point</color>");

                }
                
            }


            if (canGoIdle)
            {
                if (stallRange_Minimum != 0 && stallRange_Maximum != 0)
                {
                    if (isIdling == false)
                    {
                        float stallTime = Random.Range(stallRange_Minimum, stallRange_Maximum);
                        //Debug.Log($"<color=yellow>Idling for {stallTime} seconds</color>");
                        if (idleCoroutine != null)
                            StopCoroutine(idleCoroutine);
                        idleCoroutine = StartCoroutine(PerformIdling(stallTime));
                    }

                }
                else
                {
                    canGoIdle = false;
                }
              
            }

            Vector3 startPoint = npcRoot.transform.position;
            Vector3 endPoint = startPoint + npcRoot.transform.forward * chaseState.chaseDetectionDistance;
            if (npcRoot.IsPlayerInRange_Capsule(startPoint, endPoint, chaseState.chaseRadius))
            {
                npcRoot.statemachine.SwitchState(chaseState);
            }
        }

        private bool hasReachedPatrolPoint(Vector3 targetPoint, bool useNavMeshMethod)
        {
            if (useNavMeshMethod)
            {
                if (npcRoot.navMeshAgent.pathPending)
                {
                    return false;
                }
                return npcRoot.navMeshAgent.remainingDistance <= npcRoot.navMeshAgent.stoppingDistance;
            }
            else
            {
                float distanceToPoint = Vector3.Distance(npcRoot.transform.position, targetPoint);
                return distanceToPoint <= npcRoot.navMeshAgent.stoppingDistance + 0.1f;
            }

        }

        IEnumerator PerformIdling(float duration)
        {
            isIdling = true;
            idleState.GoToIdleAnimation();
            yield return new WaitForSeconds(duration);
            canGoIdle = false;
            isIdling = false;
        }
    }
}


 