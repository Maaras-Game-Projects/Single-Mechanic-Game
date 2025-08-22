
using System.Collections;
using UnityEngine;

namespace EternalKeep
{
    public class InteractableDoor : MonoBehaviour
    {
        [SerializeField] Transform doorMain;
        [SerializeField] Transform doorLeft;
        [SerializeField] Transform doorRight;

        [SerializeField] bool isDoubleDoor = false;
        [SerializeField] bool isDoorOpened = false;
        [SerializeField] bool isDoorLocked = false;

        [SerializeField] float openAngle = 135f;
        [SerializeField] float openDuration = 1f;
        [SerializeField] private Quaternion closeRotation_Main;

        bool isDoorMoving = false;

        Coroutine doorMoveCoroutine;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                OpenDoorSingle();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                OpenDoorDouble();
            }
        }

        public void OpenDoorSingle()
        {
            if (isDoorMoving) return;

            if (doorMoveCoroutine != null)
                StopCoroutine(doorMoveCoroutine);

            doorMoveCoroutine = StartCoroutine(OpenDoorSingleCoroutine());

        }

        IEnumerator OpenDoorSingleCoroutine()
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
        }
        

         public void OpenDoorDouble()
        {
            if (isDoorMoving) return;

            if (doorMoveCoroutine != null)
                StopCoroutine(doorMoveCoroutine);

            doorMoveCoroutine = StartCoroutine(OpenDoorDoubleCoroutine());

        }

        IEnumerator OpenDoorDoubleCoroutine()
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
        }
    }

}

