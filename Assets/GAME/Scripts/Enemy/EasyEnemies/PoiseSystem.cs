using System.Collections;
using UnityEngine;

namespace EternalKeep
{
    public class PoiseSystem : MonoBehaviour
    {
        [SerializeField] float maxPoiseAmount = 1000f;
        [SerializeField] float currentPoiseAmount;

        [Range(0f, 1f)]
        [SerializeField] float poiseRechargeRate = .15f;
        public float CurrentPoise => currentPoiseAmount;
        private bool isDepleting = false;
        private Coroutine depleteCoroutine;

        void Awake()
        {
            currentPoiseAmount = maxPoiseAmount;
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.O))
                DepletePoise(25);

            if (isDepleting) return;
            RechargePoise();

        }



        public void DepletePoise(float depleteAmount)
        {
            isDepleting = true;
            currentPoiseAmount -= depleteAmount;

            if (depleteCoroutine != null)
            {
                StopCoroutine(depleteCoroutine);
            }
            depleteCoroutine = StartCoroutine(EndDepletingDelayed(.15f));

        }

        IEnumerator EndDepletingDelayed(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            isDepleting = false;
        }

        public void ResetPoise()
        {
            if (currentPoiseAmount <= 0)
            {
                currentPoiseAmount = maxPoiseAmount;
            }
        }

        private void RechargePoise()
        {

            if (currentPoiseAmount >= maxPoiseAmount) return;


            currentPoiseAmount += poiseRechargeRate * maxPoiseAmount * Time.deltaTime;

            if (currentPoiseAmount >= maxPoiseAmount)
            {
                currentPoiseAmount = maxPoiseAmount;
            }

        }
    }

}


