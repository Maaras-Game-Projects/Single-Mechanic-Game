using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] float fadeTime = 1f;
    //[SerializeField] GameObject interactPromptGameObject;
    //[SerializeField] GameObject textPromptGameObject;
    [SerializeField] bool isFadeInAnimPlaying = false;
    [SerializeField] bool isFadeOutAnimPlaying = false;
    [SerializeField] bool canInteract = false;

    [SerializeField] Interactions currentInteraction;

    [SerializeField]Image interactPromptImage;
    [SerializeField]TMP_Text interactionPrompText;

    [SerializeField] Image showTextPromptImage;
    [SerializeField]TMP_Text showPrompText;

   

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

    public void SetCurrentInteraction(Interactions interaction)
    {
        currentInteraction = interaction;
    }

    public void ActivateInteraction()
    {
        if(!canInteract) return;
        canInteract = false;
        if(currentInteraction == Interactions.ShowTextPrompt)
        {
            ShowTextPrompt();
        }
    }

    private void ShowTextPrompt()
    {
        if(interactPromptImage.gameObject.activeSelf)
        {
            if(isFadeOutAnimPlaying) return;
            StartCoroutine(FadeOutImage(fadeTime,interactPromptImage,interactionPrompText,() =>
            {
                
                showTextPromptImage.gameObject.SetActive(true);
                StartCoroutine(FadeInImage(fadeTime,showTextPromptImage,showPrompText));
            }));
        }
        else
        {
            showTextPromptImage.gameObject.SetActive(true);
            StartCoroutine(FadeInImage(fadeTime,showTextPromptImage,showPrompText));
        }
    }

    public void HideTextPrompt()
    {
        if(isFadeOutAnimPlaying) return;
        StartCoroutine(FadeOutImage(fadeTime,showTextPromptImage,showPrompText));
    }

    public void EnableInteractPrompt()
    {
        interactPromptImage.gameObject.SetActive(true);
        if(isFadeInAnimPlaying) return;
        StartCoroutine(FadeInImage(fadeTime,interactPromptImage,interactionPrompText));
    }

    public void DisableInteractPrompt()
    {
        if(!interactPromptImage.gameObject.activeSelf) return;
        if(isFadeOutAnimPlaying) return;
        StartCoroutine(FadeOutImage(fadeTime,interactPromptImage,interactionPrompText));
    }

    IEnumerator FadeOutImage(float waitTime,Image image, TMP_Text text, Action onFadeOutComplete = null)
    {

        isFadeOutAnimPlaying = true;
        float elapsedTime = 0f;
        Color startImageColor = image.color;
        Color startTextColor = text.color;
        Color endImageColor = new Color(image.color.r,image.color.g,image.color.b,0f);
        Color endTextColor = new Color(text.color.r,text.color.g,text.color.b,0f);
        
        while(elapsedTime < waitTime)
        {   
            float t = elapsedTime/waitTime;
            image.color = Color.Lerp(startImageColor, endImageColor,t);

            if(text.text != "")
            {
                text.color = Color.Lerp(startTextColor, endTextColor,t);

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

    IEnumerator FadeInImage(float waitTime,Image image,TMP_Text text)
    {
        isFadeInAnimPlaying = true;
        float elapsedTime = 0f;
        Color startImageColor = image.color;
        Color startTextColor = text.color;
        Color endImageColor = new Color(image.color.r,image.color.g,image.color.b,1f);
        Color endTextColor = new Color(text.color.r,text.color.g,text.color.b,1f);
        
        while(elapsedTime < waitTime)
        {   
            float t = elapsedTime/waitTime;
            image.color = Color.Lerp(startImageColor, endImageColor,t);

            if(text.text != "")
            {
                text.color = Color.Lerp(startTextColor, endTextColor,t);

            }

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        image.color = endImageColor;
        text.color = endTextColor;
        isFadeInAnimPlaying = false;


    }
}

public enum Interactions
{
    ShowTextPrompt, InteractDoor, ItemPickUp, EnterFogWall,
}
