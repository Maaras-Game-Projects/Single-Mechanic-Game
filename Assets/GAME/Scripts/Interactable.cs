
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [SerializeField] float fadeTime = 1f;
    [SerializeField] Image interactPromptImage;
    [SerializeField] bool isFadeInAnimPlaying = false;
    [SerializeField] bool isFadeOutAnimPlaying = false;

    public void EnableInteractPrompt()
    {
        interactPromptImage.gameObject.SetActive(true);
        if(isFadeInAnimPlaying) return;
        StartCoroutine(FadeInImage(fadeTime,interactPromptImage));
    }

    public void DisableInteractPrompt()
    {
        if(isFadeOutAnimPlaying) return;
        StartCoroutine(FadeOutImage(fadeTime,interactPromptImage));
    }

    IEnumerator FadeOutImage(float waitTime,Image image)
    {
        isFadeOutAnimPlaying = true;
        float elapsedTime = 0f;
        Color startColor = image.color;
        Color endColor = new Color(image.color.r,image.color.g,image.color.b,0f);
        while(elapsedTime < waitTime)
        {   
            float t = elapsedTime/waitTime;
            image.color = Color.Lerp(startColor, endColor,t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        image.color = endColor;
        image.gameObject.SetActive(false);
        isFadeOutAnimPlaying = false;

    }

    IEnumerator FadeInImage(float waitTime,Image image)
    {
        isFadeInAnimPlaying = true;
        float elapsedTime = 0f;
        Color startColor = image.color;
        Color endColor = new Color(image.color.r,image.color.g,image.color.b,1f);
        while(elapsedTime < waitTime)
        {   
            float t = elapsedTime/waitTime;
            image.color = Color.Lerp(startColor, endColor,t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        image.color = endColor;
        isFadeInAnimPlaying = false;

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            EnableInteractPrompt();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            DisableInteractPrompt();
        }
    }
}
