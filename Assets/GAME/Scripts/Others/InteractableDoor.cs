
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace EternalKeep
{
    public class InteractableDoor : MonoBehaviour
    {
        [SerializeField] Transform doorMain;
        [SerializeField] Transform doorLeft;
        [SerializeField] Transform doorRight;

        [SerializeField] bool isDoubleDoor = false;
        [SerializeField] bool isDoorOpened = false;
        public bool IsDoorOpened => isDoorOpened;
        [SerializeField] bool isDoorLocked = false;
        public bool IsDoorLocked => isDoorLocked;

        [SerializeField] float openAngle = 135f;
        [SerializeField] float openDuration = 1f;
        [SerializeField] private Quaternion closeRotation_Main;

        [SerializeField] Interactable interactTrigger_1;
        [SerializeField] Interactable interactTrigger_2;

        bool isDoorMoving = false;

        Coroutine doorMoveCoroutine;

        [SerializeField] UnityEvent OnDoorOpened;

        public void OpenDoor(Action onDoorOpen = null)
        {
            if (isDoubleDoor)
            {
                OpenDoorDouble(onDoorOpen);
            }
            else
            {
                OpenDoorSingle(onDoorOpen);
            }
        }

        private void OpenDoorSingle(Action onDoorOpen = null)
        {
            if (isDoorMoving) return;

            if (doorMoveCoroutine != null)
                StopCoroutine(doorMoveCoroutine);

            doorMoveCoroutine = StartCoroutine(OpenDoorSingleCoroutine(onDoorOpen));

        }

        IEnumerator OpenDoorSingleCoroutine(Action onDoorOpen = null)
        {
            isDoorMoving = true;
            float elapsedTime = 0f;
            Quaternion startRotation = doorMain.rotation;
            Quaternion targetRotation = doorMain.rotation * Quaternion.Euler(0, openAngle, 0);
            while (elapsedTime <= openDuration)
            {
                elapsedTime += Time.deltaTime / openDuration;
                doorMain.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime);
                yield return null;
            }
            doorMain.rotation = targetRotation;
            isDoorMoving = false;
            isDoorOpened = true;
            DisableAllDoorTriggers();
            onDoorOpen?.Invoke();
        }

        private void DisableAllDoorTriggers()
        {
            interactTrigger_1.gameObject.SetActive(false);
            interactTrigger_2.gameObject.SetActive(false);
        }

        private void OpenDoorDouble(Action onDoorOpen = null)
        {
            if (isDoorMoving) return;

            if (doorMoveCoroutine != null)
                StopCoroutine(doorMoveCoroutine);

            doorMoveCoroutine = StartCoroutine(OpenDoorDoubleCoroutine(onDoorOpen));

        }

        IEnumerator OpenDoorDoubleCoroutine(Action onDoorOpen = null)
        {
            isDoorMoving = true;
            float elapsedTime = 0f;
            Quaternion startRotation_Left = doorLeft.rotation;
            Quaternion targetRotation_Left = doorLeft.rotation * Quaternion.Euler(0, openAngle, 0);
            Quaternion startRotation_Right = doorRight.rotation;
            Quaternion targetRotation_Right = doorRight.rotation * Quaternion.Euler(0, -openAngle, 0);
            while (elapsedTime <= openDuration)
            {
                elapsedTime += Time.deltaTime / openDuration;
                doorLeft.rotation = Quaternion.Lerp(startRotation_Left, targetRotation_Left, elapsedTime);
                doorRight.rotation = Quaternion.Lerp(startRotation_Right, targetRotation_Right, elapsedTime);
                yield return null;
            }
            doorLeft.rotation = targetRotation_Left;
            doorRight.rotation = targetRotation_Right;
            isDoorMoving = false;
            isDoorOpened = true;
            DisableAllDoorTriggers();
            onDoorOpen?.Invoke();
            
        }
    }

}

