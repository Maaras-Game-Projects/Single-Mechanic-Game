using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    public class DealDamage_Enemy : MonoBehaviour
    {
        [SerializeField] float baseDamageVal = 50f;
        [SerializeField] NPC_Root nPC_Root;

        CapsuleCollider capsuleCollider;

        Vector3 capsulePoint0;
        Vector3 capsulePoint1;

        HashSet<Collider> damagedPlayerColliders = new HashSet<Collider>();

        float capsuleRadius_ScaleCorrected;
        float capsuleHeight_ScaleCorrected;

        void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            SetCapsuleRadiusAndHeightWithScaleCorrection(); // To convert Rad and height to world Space

        }

        private void SetCapsuleRadiusAndHeightWithScaleCorrection()
        {
            if (IsUniformScale(transform)) //if scale is uniform on xyz ,Multiply  any one component of scale with Rad,height to convert to world space
            {
                capsuleRadius_ScaleCorrected = capsuleCollider.radius * transform.lossyScale.x;
                capsuleHeight_ScaleCorrected = capsuleCollider.height * transform.lossyScale.x;
            }
            else
            {
                float heightScaleCorrectionFactor = GetHeightScaleCorrectionFactor();

                float radiusScaleCorrectedFactor = GetRadiusScaleCorrectionFactor();


                capsuleRadius_ScaleCorrected = capsuleCollider.radius * radiusScaleCorrectedFactor;
                capsuleHeight_ScaleCorrected = capsuleCollider.height * heightScaleCorrectionFactor;
            }
        }

        private float GetHeightScaleCorrectionFactor()
        {

            switch (capsuleCollider.direction)
            {
                case 0: return transform.lossyScale.x;    // X axis
                case 1: return transform.lossyScale.y;       // Y axis
                case 2: return transform.lossyScale.z;  // Z axis
            }

            return 1f;
        }

        private float GetRadiusScaleCorrectionFactor()
        {

            switch (capsuleCollider.direction)
            {
                case 0: return Mathf.Max(transform.lossyScale.y, transform.lossyScale.z);    // X axis
                case 1: return Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);       // Y axis
                case 2: return Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);  // Z axis
            }

            return 1f;
        }

        bool IsUniformScale(Transform transform, float tolerance = 0.0001f)
        {
            Vector3 scale = transform.lossyScale;
            return Mathf.Abs(scale.x - scale.y) < tolerance &&
                Mathf.Abs(scale.y - scale.z) < tolerance;
        }

        void Update()
        {
            DetectHit();
        }

        public void DetectHit()
        {
            if (nPC_Root == null) return;
            if (!nPC_Root.canDetectHit) return;

            Debug.Log($"<color=red>Hit Player with damage: {nPC_Root.currentDamageToDeal}</color>");
            CalculateCurrentCapsulePoints();



            Collider[] hits = Physics.OverlapCapsule(capsulePoint0, capsulePoint1, capsuleRadius_ScaleCorrected, nPC_Root.playerLayerMask);

            if (hits.Length > 0)
            {
                foreach (Collider hitCollider in hits)
                {
                    if (damagedPlayerColliders.Contains(hitCollider)) continue;

                    PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();

                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(nPC_Root.currentDamageToDeal, nPC_Root.parryable, nPC_Root);


                        damagedPlayerColliders.Add(hitCollider);

                    }


                }

            }



        }

        // Called From CanDetection EndEvent in npcRoot
        public void EndDetection()
        {
            if (damagedPlayerColliders.Count == 0) return;
            damagedPlayerColliders.Clear();
        }

        private void CalculateCurrentCapsulePoints()
        {
            Vector3 capsuleDirection = transform.up;
            switch (capsuleCollider.direction)
            {
                case 0: capsuleDirection = transform.right; break;   // X axis
                case 1: capsuleDirection = transform.up; break;      // Y axis
                case 2: capsuleDirection = transform.forward; break; // Z axis
            }

            Vector3 capsuleCenter_WorldSpace = transform.TransformPoint(capsuleCollider.center);

            float capsuleHeight_Half = capsuleHeight_ScaleCorrected * 0.5f;

            float capsuleOffsetFromCenterToHemiSphereCenter = capsuleHeight_Half - capsuleRadius_ScaleCorrected;

            capsulePoint0 = capsuleCenter_WorldSpace - capsuleDirection * capsuleOffsetFromCenterToHemiSphereCenter;
            capsulePoint1 = capsuleCenter_WorldSpace + capsuleDirection * capsuleOffsetFromCenterToHemiSphereCenter;
        }

        // private void OnTriggerEnter(Collider other)
        // {

        //     if(nPC_Root == null) return;
        //     if (!nPC_Root.canDetectHit) return;

        //     if (other == null)
        //     {
        //         return;
        //     }

        //     PlayerHealth playerHealth = other.GetComponent<Collider>().GetComponent<PlayerHealth>();

        //     if (playerHealth == null) return;


        //     playerHealth.TakeDamage(nPC_Root.currentDamageToDeal,nPC_Root.parryable,nPC_Root);
        // }

        void OnDrawGizmos()
        {
            if (!nPC_Root.canDetectHit) return;
            VisualiseDetectionCapsule();
        }

        private void VisualiseDetectionCapsule()
        {

            Vector3 capsuleStart = capsulePoint0;
            Vector3 capsuleEnd = capsulePoint1;

            Gizmos.color = Color.red;

            DrawCapsule(capsuleStart, capsuleEnd, capsuleRadius_ScaleCorrected);
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
    }

}


