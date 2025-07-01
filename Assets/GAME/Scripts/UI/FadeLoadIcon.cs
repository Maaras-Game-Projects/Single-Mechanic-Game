using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EternalKeep
{
    public class FadeLoadIcon : MonoBehaviour
    {
        [SerializeField] Image loadIconImage; // Reference to the UI Image component that will be faded in and out
        [SerializeField] float fadeDuration = .5f; // Duration of the fade effect
        [SerializeField] bool isFading = false; // Flag to check if fading in
        private Coroutine fadeCoroutine;

        void OnEnable()
        {
            if (isFading) return;

            fadeCoroutine = StartCoroutine(FadeCoroutine(fadeDuration));

        }

        void OnDisable()
        {
            isFading = false;
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
        }


        private IEnumerator FadeCoroutine(float duration)
        {
            isFading = true;
            while (gameObject.activeInHierarchy)
            {

                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    loadIconImage.color = new Color(loadIconImage.color.r,
                    loadIconImage.color.g, loadIconImage.color.b,
                    Mathf.Lerp(0f, 1f, elapsedTime / duration));
                    yield return null;
                }
                loadIconImage.color = new Color(loadIconImage.color.r,
                loadIconImage.color.g, loadIconImage.color.b, 1f);

                elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    loadIconImage.color = new Color(loadIconImage.color.r,
                    loadIconImage.color.g, loadIconImage.color.b,
                    Mathf.Lerp(1f, 0f, elapsedTime / duration));
                    yield return null;
                }
                loadIconImage.color = new Color(loadIconImage.color.r,
                loadIconImage.color.g, loadIconImage.color.b, 0f);


            }

            isFading = false;

        }
    }


}

