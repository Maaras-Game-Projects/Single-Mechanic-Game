
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] InteractionManager interactionManager;

    [SerializeField] string interactPrompt = "Interact";

    [Space]
    [Header("Interaction Variables")]
    [Space]

    [SerializeField] Interactions interactionType;

    [SerializeField] string textForShowTextPrompt;

    [SerializeField] Transform endTeleportPointForBossZone;
    [SerializeField] UnityEvent onTeleportBegin;

    [Space]
    [Header("Item Pickup Variables")]
    [Space]

    [SerializeField] ItemPickUp itemPickUp;

    [Space]
    public UnityEvent onZoneEnter;
    public UnityEvent onZoneExit;

    void OnEnable()
    {
        if(interactionType == Interactions.ItemPickUp)
            interactionManager.onItemPickUp.AddListener(DisableInteractlbeItemAfterPickup);
    }

    void OnDisable()
    {
        if(interactionType == Interactions.ItemPickUp)
            interactionManager.onItemPickUp.RemoveListener(DisableInteractlbeItemAfterPickup);
    }

    private void DisableInteractlbeItemAfterPickup()
    {
        if (interactionManager.GetCurrentItemPickUpID() != itemPickUp.GetID) return;

        DisableInteractable();
    }


    public void DisableInteractable()
    {
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (interactionManager.GetItemPromptUIHandler.IsItemPromptShowing) return;

            interactionManager.SetInteractionPromptText(interactPrompt);
            interactionManager.EnableInteractPrompt();

            interactionManager.SetCanInteract(true);

            if (interactionType == Interactions.ShowTextPrompt)
            {
                interactionManager.SetShowPromptText(textForShowTextPrompt);
            }
            else if (interactionType == Interactions.EnterFogWall)
            {
                interactionManager.SetTeleportPoint(endTeleportPointForBossZone);
                interactionManager.SetCurrentTeleportBeginEvent(onTeleportBegin);
            }
            else if (interactionType == Interactions.ItemPickUp)
            {
                interactionManager.SetCurrentItemPickUp(itemPickUp);
                
            }

            interactionManager.SetCurrentInteraction(interactionType);

            onZoneEnter?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (interactionManager.GetItemPromptUIHandler.IsItemPromptShowing) return;

            interactionManager.DisableInteractPrompt();

            if(interactionType == Interactions.ShowTextPrompt)
            {
                interactionManager.HideTextPrompt();
            }

            if (interactionType != Interactions.ItemPickUp)
            {

                interactionManager.SetCanInteract(false);
            }
            else if (!interactionManager.IsCurrentItemPickedUp)
            {
                interactionManager.SetCanInteract(false);
            }

           

            onZoneExit?.Invoke();
        }
    }
}
