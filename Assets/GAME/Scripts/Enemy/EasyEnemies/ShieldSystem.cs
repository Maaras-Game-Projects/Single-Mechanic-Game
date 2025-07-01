using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EternalKeep
{
    public class ShieldSystem : MonoBehaviour
    {
        [SerializeField] bool isShieldActive = false;
        [SerializeField] float maxShieldAmount = 1000f;
        [SerializeField] float currentshieldAmount;

        [Range(0f, 1f)]
        [SerializeField] float shieldRechargeRate = .15f;

        [SerializeField] List<Image> shieldIcons = new List<Image>();
        [SerializeField] int shieldCount = 0;
        [SerializeField] int activeShieldCount = 0;
        [SerializeField] private Camera mainCamera;

        public int ActiveShieldCount => activeShieldCount;

        void Awake()
        {
            shieldCount = shieldIcons.Count;
            activeShieldCount = shieldCount;
            currentshieldAmount = maxShieldAmount;
        }

        void Update()
        {
            if (!isShieldActive) return;
            if (shieldCount == 0) return;

            // if (Input.GetKeyDown(KeyCode.T))
            //     BreakSheild();

            RechargeAllShields();

            //RotateShieldIconsTowardsPlayer();
        }

        private void RotateShieldIconsTowardsPlayer()
        {
            if (shieldCount == 0) return;

            for (int i = 0; i < shieldCount; i++)
            {
                RotateShildIconTowardsPlayer(shieldIcons[i].transform);
            }
        }

        private void RotateShildIconTowardsPlayer(Transform iconTransform)
        {
            if (iconTransform.gameObject.activeSelf == false) return;

            Vector3 cameraDir = mainCamera.transform.position - iconTransform.position;

            Quaternion lookRotation = Quaternion.LookRotation(cameraDir);

            iconTransform.transform.rotation = lookRotation;
        }



        public void BreakSheild()
        {
            if (!isShieldActive) return;

            if (activeShieldCount == 0)
            {
                currentshieldAmount = 0;
                return;
            }
            else
            {
                shieldIcons[activeShieldCount - 1].gameObject.SetActive(false);
                activeShieldCount--;
                currentshieldAmount = 0f;
            }
        }

        public void BreakAllShields()
        {
            if (!isShieldActive) return;

            for (int i = 0; i < shieldCount; i++)
            {
                shieldIcons[i].gameObject.SetActive(false);
            }

            activeShieldCount = 0;
            currentshieldAmount = 0f;
        }

        private void RechargeAllShields()
        {
            if (activeShieldCount == shieldCount) return;

            if (currentshieldAmount > maxShieldAmount)
            {
                currentshieldAmount = 0;
            }

            currentshieldAmount += shieldRechargeRate * maxShieldAmount * Time.deltaTime;

            if (currentshieldAmount >= maxShieldAmount)
            {
                shieldIcons[activeShieldCount].gameObject.SetActive(true);
                activeShieldCount++;
            }

        }
    }

}

