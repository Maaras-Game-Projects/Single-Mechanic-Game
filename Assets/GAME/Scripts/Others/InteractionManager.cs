using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EternalKeep
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] float fadeTime = 1f;
        //[SerializeField] GameObject interactPromptGameObject;
        //[SerializeField] GameObject textPromptGameObject;
        [SerializeField] bool isFadeInAnimPlaying = false;
        [SerializeField] bool isFadeOutAnimPlaying = false;
        [SerializeField] bool canInteract = false;

        [SerializeField] Interactions currentInteraction;

        [SerializeField] AnimationClip fogwallEnterAnimationClip;
        [SerializeField] Transform currentTeleportPoint;
        [SerializeField] UnityEvent currentTeleportBeginEvent;
        [SerializeField] Transform playerTransform;
        [SerializeField] PlayerLocomotion playerLocomotion;
        [SerializeField] PlayerManager playerManager;
        [SerializeField] ResetGameManager resetGameManager;

        [SerializeField] Image interactPromptImage;
        [SerializeField] TMP_Text interactionPrompText;

        [SerializeField] Image showTextPromptImage;
        [SerializeField] TMP_Text showPrompText;

        [Space]
        [Header("Item Pick Up Variables")]
        [Space]
        [SerializeField] ItemPickUp currentItemPickUp;
        [SerializeField] bool isCurrentItemPickedUp = false;
        public bool IsCurrentItemPickedUp => isCurrentItemPickedUp;
        [SerializeField] HandleItemPromptUI itemPromptUIHandler;

        public HandleItemPromptUI GetItemPromptUIHandler => itemPromptUIHandler;

        [SerializeField] HandleNoteUI noteUIHandler;

        public HandleNoteUI GetnoteUIHandler => noteUIHandler;

        [SerializeField] public UnityEvent onItemPickUp;

        [Space]
        [Header("Door Interaction Variables")]
        [Space]
        [SerializeField] private bool canOpenFromThisSide_Status;
        [SerializeField] private InteractableDoor currentDoor;

        [SerializeField] string doorLockedPrompt = "Door is Locked, Need key to Open";
        [SerializeField] string doorDirectionLockPrompt = "Cannot open door from this side";
        [SerializeField] string doorKeyUsedPrompt = "Key Used";

        [SerializeField] PlayerAnimationManager playerAnimationManager;

        

        public void SetInteractionPromptText(string promptText)
        {
            interactionPrompText.text = "E: " + promptText;
        }

        public void SetShowPromptText(string promptText)
        {
            showPrompText.text = promptText;
        }

        public void SetCanInteract(bool value)
        {
            canInteract = value;
        }

        public void SetTeleportPoint(Transform teleportPoint)
        {
            currentTeleportPoint = teleportPoint;
        }
        public void SetCurrentTeleportBeginEvent(UnityEvent teleportEvent)
        {
            currentTeleportBeginEvent = teleportEvent;
        }

        public void SetCurrentItemPickUp(ItemPickUp itemPickUp)
        {
            currentItemPickUp = itemPickUp;

            itemPromptUIHandler.SetItemPickUpUI(itemPickUp);
        }

        public int GetCurrentItemPickUpID()
        {
            if (currentItemPickUp == null) return -1;

            return currentItemPickUp.GetID;
        }

        public void SetDoorInteractionParams(InteractableDoor door, bool canOpenFromThisSide)
        {
            canOpenFromThisSide_Status = canOpenFromThisSide;
            currentDoor = door;
        }

        public void SetCurrentInteraction(Interactions interaction)
        {
            currentInteraction = interaction;
        }

        public void ActivateInteraction()
        {
            if (!canInteract) return;
            canInteract = false;
            if (currentInteraction == Interactions.ShowTextPrompt)
            {
                ShowTextPrompt();
            }
            else if (currentInteraction == Interactions.EnterFogWall)
            {
                StartCoroutine(FadeOutImage(fadeTime, interactPromptImage, interactionPrompText));
                EnterFogwall();
            }
            else if (currentInteraction == Interactions.Rest)
            {
                StartCoroutine(FadeOutImage(fadeTime, interactPromptImage, interactionPrompText));

                //Reset game state after rest anim complete
                resetGameManager.ResetGameOnResting();

            }
            else if (currentInteraction == Interactions.InteractDoor)
            {
                HandleDoorInteraction();
            }
            else if (currentInteraction == Interactions.ItemPickUp)
            {
                if (noteUIHandler.IsNoteUIShowing)
                {
                    noteUIHandler.FadeOutNoteUI(fadeTime);
                    canInteract = false;
                    return;
                }

                if (!isCurrentItemPickedUp)
                {
                    ShowItemPrompt();
                    canInteract = true;
                    //onItemPickUp?.Invoke();
                    currentItemPickUp.PickUpItem();

                }
                else
                {
                    IUsableItem usableItem = currentItemPickUp.GetComponent<IUsableItem>();

                    if (usableItem != null)
                    {
                        if (currentItemPickUp.isNoteItemPickup)
                        {
                            //Debug.Log("Brush");
                            usableItem.UseItem();
                            itemPromptUIHandler.FadeOutItemPromptUI(fadeTime);
                            noteUIHandler.FadeInNoteUI(fadeTime);
                            isCurrentItemPickedUp = false;
                            canInteract = true;

                            return;
                        }
                        else
                        {
                            //Debug.Log("Bruh");
                            usableItem.UseItem();
                        }

                    }

                    isCurrentItemPickedUp = false;
                    canInteract = false;
                    itemPromptUIHandler.FadeOutItemPromptUI(fadeTime);
                }
            }
            
        }

        private void HandleDoorInteraction()
        {
            if (currentDoor.IsDoorOpened) return;

            if (currentDoor.IsDoorLocked)
            {
                SetShowPromptText(doorLockedPrompt);
                ShowTextPrompt();
            }
            else if (!canOpenFromThisSide_Status)
            {
                SetShowPromptText(doorDirectionLockPrompt);
                ShowTextPrompt();
            }
            else
            {
                currentDoor.OpenDoor();
            }
        }

        private void ShowItemPrompt()
        {
            if (interactPromptImage.gameObject.activeSelf)
            {
                if (isFadeOutAnimPlaying) return;
                StartCoroutine(FadeOutImage(fadeTime, interactPromptImage, interactionPrompText, () =>
                {

                    itemPromptUIHandler.FadeInItemPromptUI(fadeTime);
                    isCurrentItemPickedUp = true;
                }));
            }
            else
            {
                itemPromptUIHandler.FadeInItemPromptUI(fadeTime);
                isCurrentItemPickedUp = true;
            }
        }

        private void ShowTextPrompt()
        {
            if (interactPromptImage.gameObject.activeSelf)
            {
                if (isFadeOutAnimPlaying) return;
                StartCoroutine(FadeOutImage(fadeTime, interactPromptImage, interactionPrompText, () =>
                {

                    showTextPromptImage.gameObject.SetActive(true);
                    StartCoroutine(FadeInImage(fadeTime, showTextPromptImage, showPrompText));
                }));
            }
            else
            {
                showTextPromptImage.gameObject.SetActive(true);
                StartCoroutine(FadeInImage(fadeTime, showTextPromptImage, showPrompText));
            }
        }

        public void HideTextPrompt()
        {
            if (isFadeOutAnimPlaying) return;
            StartCoroutine(FadeOutImage(fadeTime, showTextPromptImage, showPrompText));
        }

        public void EnableInteractPrompt()
        {

            interactPromptImage.gameObject.SetActive(true);
            if (isFadeInAnimPlaying) return;
            StartCoroutine(FadeInImage(fadeTime, interactPromptImage, interactionPrompText));
        }

        public void DisableInteractPrompt()
        {
            if (!interactPromptImage.gameObject.activeSelf) return;
            if (isFadeOutAnimPlaying) return;
            StartCoroutine(FadeOutImage(fadeTime, interactPromptImage, interactionPrompText));
        }

        IEnumerator FadeOutImage(float waitTime, Image image, TMP_Text text, Action onFadeOutComplete = null)
        {

            isFadeOutAnimPlaying = true;
            float elapsedTime = 0f;
            Color startImageColor = image.color;
            Color startTextColor = text.color;
            Color endImageColor = new Color(image.color.r, image.color.g, image.color.b, 0f);
            Color endTextColor = new Color(text.color.r, text.color.g, text.color.b, 0f);

            while (elapsedTime < waitTime)
            {
                float t = elapsedTime / waitTime;
                image.color = Color.Lerp(startImageColor, endImageColor, t);

                if (text.text != "")
                {
                    text.color = Color.Lerp(startTextColor, endTextColor, t);

                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            image.color = endImageColor;
            text.color = endTextColor;
            image.gameObject.SetActive(false);
            isFadeOutAnimPlaying = false;

            onFadeOutComplete?.Invoke();

        }

        IEnumerator FadeInImage(float waitTime, Image image, TMP_Text text)
        {
            isFadeInAnimPlaying = true;
            float elapsedTime = 0f;
            Color startImageColor = image.color;
            Color startTextColor = text.color;
            Color endImageColor = new Color(image.color.r, image.color.g, image.color.b, 1f);
            Color endTextColor = new Color(text.color.r, text.color.g, text.color.b, 1f);

            while (elapsedTime < waitTime)
            {
                float t = elapsedTime / waitTime;
                image.color = Color.Lerp(startImageColor, endImageColor, t);

                if (text.text != "")
                {
                    text.color = Color.Lerp(startTextColor, endTextColor, t);

                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            image.color = endImageColor;
            text.color = endTextColor;
            isFadeInAnimPlaying = false;


        }

        public void EnterFogwall()
        {
            playerAnimationManager.PlayAnyInteractiveAnimation(fogwallEnterAnimationClip.name, true, true);
            currentTeleportBeginEvent?.Invoke();
            StartCoroutine(TeleportOnAnimationEnd(fogwallEnterAnimationClip.length + 0.1f));


        }

        IEnumerator TeleportOnAnimationEnd(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            yield return null;

            playerLocomotion.playerRigidBody.position = currentTeleportPoint.position;
            playerLocomotion.mainCamera.transform.position = currentTeleportPoint.position + Vector3.forward * -.025f;

            Debug.Log("Teleporting to: " + playerTransform.position);

        }
    }

    public enum Interactions
    {
        ShowTextPrompt, InteractDoor, ItemPickUp, EnterFogWall, Rest
    }

}




