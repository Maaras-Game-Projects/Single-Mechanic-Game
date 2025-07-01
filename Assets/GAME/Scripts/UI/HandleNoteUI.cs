using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

namespace EternalKeep
{
    public class HandleNoteUI : MonoBehaviour
    {

        [SerializeField] Image noteOverlayImage;
        [SerializeField] TMP_Text noteText;

        [SerializeField] Image noteInteractOverlayImage;
        [SerializeField] TMP_Text interactText;
        [SerializeField] private bool isFadeOutAnimPlaying;
        [SerializeField] private bool isFadeInAnimPlaying;
        [SerializeField] private bool isNoteUIShowing = false;

        public bool IsNoteUIShowing => isNoteUIShowing;

        public void SetNoteText(string text)
        {
            noteText.text = text;
        }



        public void FadeOutNoteUI(float waitTime, Action onFadeOutComplete = null)
        {
            if (isFadeOutAnimPlaying) return;
            StartCoroutine(FadeOutImage(waitTime, noteOverlayImage, noteInteractOverlayImage, noteText,
             interactText, () => isNoteUIShowing = false));
        }

        public void FadeInNoteUI(float waitTime, Action onFadeInComplete = null)
        {

            if (isFadeInAnimPlaying) return;
            Debug.Log("Im IN");
            noteOverlayImage.gameObject.SetActive(true);
            isNoteUIShowing = true;
            StartCoroutine(FadeInImage(waitTime, noteOverlayImage, noteInteractOverlayImage, noteText,
             interactText, onFadeInComplete));
        }



        IEnumerator FadeOutImage(float waitTime, Image overlayImage, Image interactOverlayImage, TMP_Text noteText,
        TMP_Text interactText, Action onFadeOutComplete = null)
        {

            isFadeOutAnimPlaying = true;
            float elapsedTime = 0f;
            Color startOverlayImageColor = overlayImage.color;
            Color startInteractOverlayImageColor = interactOverlayImage.color;

            Color startnoteTextColor = noteText.color;
            Color startInteractTextColor = interactText.color;

            Color endOverlayImageColor = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 0f);
            Color endInteractOverlayImageColor = new Color(interactOverlayImage.color.r, interactOverlayImage.color.g, interactOverlayImage.color.b, 0f);


            Color endNoteTextColor = new Color(noteText.color.r, noteText.color.g, noteText.color.b, 0f);
            Color endInteractTextColor = new Color(interactText.color.r, interactText.color.g, interactText.color.b, 0f);

            while (elapsedTime < waitTime)
            {
                float t = elapsedTime / waitTime;
                overlayImage.color = Color.Lerp(startOverlayImageColor, endOverlayImageColor, t);
                interactOverlayImage.color = Color.Lerp(startInteractOverlayImageColor, endInteractOverlayImageColor, t);

                if (noteText.text != "")
                {
                    noteText.color = Color.Lerp(startnoteTextColor, endNoteTextColor, t);

                }
                if (interactText.text != "")
                {
                    interactText.color = Color.Lerp(startInteractTextColor, endInteractTextColor, t);
                }


                elapsedTime += Time.deltaTime;

                yield return null;
            }

            overlayImage.color = endOverlayImageColor;
            interactOverlayImage.color = endInteractOverlayImageColor;

            noteText.color = endNoteTextColor;
            interactText.color = endInteractTextColor;

            overlayImage.gameObject.SetActive(false);
            isFadeOutAnimPlaying = false;

            onFadeOutComplete?.Invoke();

        }

        IEnumerator FadeInImage(float waitTime, Image overlayImage, Image interactOverlayImage, TMP_Text noteText,
        TMP_Text interactText, Action onFadeInComplete = null)
        {

            isFadeInAnimPlaying = true;
            float elapsedTime = 0f;
            Color startOverlayImageColor = overlayImage.color;
            Color startInteractOverlayImageColor = interactOverlayImage.color;

            Color startnoteTextColor = noteText.color;
            Color startInteractTextColor = interactText.color;

            Color endOverlayImageColor = new Color(overlayImage.color.r, overlayImage.color.g, overlayImage.color.b, 1f);
            Color endInteractOverlayImageColor = new Color(interactOverlayImage.color.r, interactOverlayImage.color.g, interactOverlayImage.color.b, 1f);


            Color endNoteTextColor = new Color(noteText.color.r, noteText.color.g, noteText.color.b, 1f);
            Color endInteractTextColor = new Color(interactText.color.r, interactText.color.g, interactText.color.b, 1f);

            while (elapsedTime < waitTime)
            {
                float t = elapsedTime / waitTime;
                overlayImage.color = Color.Lerp(startOverlayImageColor, endOverlayImageColor, t);
                interactOverlayImage.color = Color.Lerp(startInteractOverlayImageColor, endInteractOverlayImageColor, t);

                if (noteText.text != "")
                {
                    noteText.color = Color.Lerp(startnoteTextColor, endNoteTextColor, t);

                }
                if (interactText.text != "")
                {
                    interactText.color = Color.Lerp(startInteractTextColor, endInteractTextColor, t);
                }


                elapsedTime += Time.deltaTime;

                yield return null;
            }

            overlayImage.color = endOverlayImageColor;
            interactOverlayImage.color = endInteractOverlayImageColor;

            noteText.color = endNoteTextColor;
            interactText.color = endInteractTextColor;

            isFadeInAnimPlaying = false;

            onFadeInComplete?.Invoke();

        }
    }

}

