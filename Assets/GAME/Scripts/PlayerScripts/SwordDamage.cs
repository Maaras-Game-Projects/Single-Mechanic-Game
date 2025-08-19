using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    public class SwordDamage : MonoBehaviour
    {
        [SerializeField] float baseDamgeVal = 50f;
        [SerializeField] float criticalDamage = 50f;
        [SerializeField] float AttackPower = 50f;

        [SerializeField] float attackScaleFactor = 0.4f;
        [SerializeField] float criticalAttackScaleFactor = 1.5f;
        [SerializeField] float swordRotVal_X_AtDeath = 50f;
        [SerializeField] PlayerCombat playerCombat;
        [SerializeField] LayerMask enemyLayerMask;

        Collider swordCollider;
        [SerializeField] private bool dealtDamage = false;

        CapsuleCollider capsuleCollider;

        Vector3 capsulePoint0;
        Vector3 capsulePoint1;

        HashSet<Collider> damagedEnemyColliders = new HashSet<Collider>();

        float capsuleRadius;
        private void Start()
        {
            swordCollider = GetComponent<Collider>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            AttackPower = baseDamgeVal + Mathf.Round(baseDamgeVal * attackScaleFactor);
            criticalDamage = baseDamgeVal + Mathf.Round(baseDamgeVal * criticalAttackScaleFactor);

            capsuleRadius = capsuleCollider.radius;

        }


        void Update()
        {
            DetectHit();
        }

        public void SetBaseDamage(float baseDamageVal)
        {
            baseDamgeVal = baseDamageVal;
            AttackPower = baseDamgeVal + Mathf.Round(baseDamgeVal * attackScaleFactor);
            criticalDamage = baseDamgeVal + Mathf.Round(baseDamgeVal * criticalAttackScaleFactor);
        }

        /* private void OnCollisionEnter(Collision collision)
         {
             Debug.Log("col enter chek");
             if (!playerCombat.canDetectHit) return;

             if (collision == null)
             {
                 Debug.Log("Sword Not Collided");
                 return;
             }

             Debug.Log("Sword Collided");

             IDamagable damagable = collision.collider.GetComponent<IDamagable>();

             if (damagable == null) return;

             Debug.Log("Got IDAmagable");

             damagable.TakeDamage(baseDamgeVal);
         }*/

        public void SetSwordRotationValue(float x, float y, float z)
        {
            transform.localRotation = Quaternion.Euler(x, y, z);
        }

        public void SetSwordRotationValueAtPlayerDeath()
        {
            Vector3 currentRotation = transform.localEulerAngles;
            currentRotation.x = swordRotVal_X_AtDeath;
            transform.localRotation = Quaternion.Euler(currentRotation);
        }

        public void EnableDisableSwordCollider(bool value)
        {
            if (swordCollider == null) return;
            swordCollider.enabled = value;
        }

        public void DetectHit()
        {
            if (!playerCombat.canDetectHit) return;
            CalculateCurrentCapsulePoints();

            Collider[] hits = Physics.OverlapCapsule(capsulePoint0, capsulePoint1, capsuleRadius, enemyLayerMask);

            if (hits.Length > 0)
            {
                foreach (Collider hitCollider in hits)
                {
                    if (damagedEnemyColliders.Contains(hitCollider)) continue;

                    IDamagable damagable = hitCollider.GetComponent<IDamagable>();

                    if (damagable != null)
                    {
                        damagable.TakeDamage(AttackPower, criticalDamage);
                        damagedEnemyColliders.Add(hitCollider);
                    }


                }

            }



        }

        // Called From CanDetection EndEvent in PlayerCombat
        public void EndDetection()
        {
            if (damagedEnemyColliders.Count == 0) return;
            damagedEnemyColliders.Clear();
        }

        private void CalculateCurrentCapsulePoints()
        {
            Vector3 capsuleDirection = Vector3.up;
            switch (capsuleCollider.direction)
            {
                case 0: capsuleDirection = transform.right; break;   // X axis
                case 1: capsuleDirection = transform.up; break;      // Y axis
                case 2: capsuleDirection = transform.forward; break; // Z axis
            }

            Vector3 capsuleCenter_WorldSpace = transform.TransformPoint(capsuleCollider.center);

            float capsuleHeight_Half = capsuleCollider.height * 0.5f;

            float capsuleOffsetFromCenterToHemiSphereCenter = capsuleHeight_Half - capsuleRadius;

            capsulePoint0 = capsuleCenter_WorldSpace - capsuleDirection * capsuleOffsetFromCenterToHemiSphereCenter;
            capsulePoint1 = capsuleCenter_WorldSpace + capsuleDirection * capsuleOffsetFromCenterToHemiSphereCenter;
        }


        void OnDrawGizmos()
        {
            if (!playerCombat.canDetectHit) return;
            VisualiseDetectionCapsule();
        }

        private void VisualiseDetectionCapsule()
        {
            Vector3 capsuleStart = capsulePoint0;
            Vector3 capsuleEnd = capsulePoint1;

            Gizmos.color = Color.yellow;

            DrawCapsule(capsuleStart, capsuleEnd, capsuleRadius);
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

