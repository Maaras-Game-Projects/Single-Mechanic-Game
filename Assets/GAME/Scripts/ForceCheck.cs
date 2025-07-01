using UnityEngine;

namespace EternalKeep
{
    public class ForceCheck : MonoBehaviour
    {
        [SerializeField] float forceVal = 5f;
        Rigidbody rb;
        [SerializeField] private Vector3 targetPosition_OnStairs;
        [SerializeField] private float groundRaycastOffset;
        [SerializeField] private float maxGroundCheckDistance;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float hoverOffset = 0f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            /*if(Input.GetKeyDown(KeyCode.X))
            {
                AddForceUp();
            }*/
            HandleStairsManeuver();
        }

        private void HandleStairsManeuver()
        {
            RaycastHit hit;
            Vector3 raycastOrigin = transform.position;
            raycastOrigin.y = raycastOrigin.y + groundRaycastOffset;

            targetPosition_OnStairs = transform.position;


            if (Physics.SphereCast(raycastOrigin, 0.2f, -Vector3.up, out hit, maxGroundCheckDistance, groundLayer))
            {

                Vector3 rayHitPoint = hit.point;
                targetPosition_OnStairs.y = rayHitPoint.y + hoverOffset;
                //y_targetPosition_OnStairs = rayHitPoint.y;
                //Debug.Log("Ground hit: " + hit.collider.name);



            }
            // float smoothSpeed = 10f; // you can tweak this
            // Vector3 currentPosition = rigidBody.position;
            // Vector3 smoothed = new Vector3(
            //     currentPosition.x,
            //     Mathf.Lerp(currentPosition.y, targetPosition_OnStairs.y, Time.fixedDeltaTime * smoothSpeed),
            //     currentPosition.z
            // );

            // rigidBody.MovePosition(smoothed);

            // if (isInteracting)
            // {

            //     transform.position = Vector3.Lerp(transform.position, targetPosition_OnStairs, Time.deltaTime/0.1f);
            //     //transform.position = targetPosition_OnStairs;


            // }
            // else
            // {
            //     transform.position = targetPosition_OnStairs;
            // }

            transform.position = targetPosition_OnStairs;



        }

        private void AddForceUp()
        {
            //rb.AddForce(Vector3.up * forceVal,ForceMode.VelocityChange);
            //rb.linearVelocity = Vector3.up * forceVal;
            rb.AddExplosionForce(forceVal, transform.position, forceVal * 2);
        }
    }


}

