
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [SerializeField] InteractionManager interactionManager;

    [SerializeField] string interactPrompt = "Interact";

    [Space]
    [Header("Interaction Variables")]
    [Space]

    [SerializeField] Interactions interactionType;

    [SerializeField] string textForShowTextPrompt;

    [Space]
    public UnityEvent onZoneEnter;
    public UnityEvent onZoneExit;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            interactionManager.SetInteractionPromptText(interactPrompt);
            interactionManager.EnableInteractPrompt();

            interactionManager.SetCanInteract(true);

            if(interactionType == Interactions.ShowTextPrompt)
            {
                interactionManager.SetShowPromptText(textForShowTextPrompt);
            }

            interactionManager.SetCurrentInteraction(interactionType);

            onZoneEnter?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            interactionManager.DisableInteractPrompt();

            if(interactionType == Interactions.ShowTextPrompt)
            {
                interactionManager.HideTextPrompt();
            }

            interactionManager.SetCanInteract(false);

            onZoneExit?.Invoke();
        }
    }
}
