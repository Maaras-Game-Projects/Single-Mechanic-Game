using System.Collections;
using UnityEngine;

namespace EternalKeep
{
    public class HandleYouDiedCanvas : MonoBehaviour
    {
        public static HandleYouDiedCanvas Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep this object across scene loads
            }
            else
            {
                Destroy(gameObject); // Ensure only one instance exists
            }
        }

        [SerializeField] CanvasGroup youDiedCanvasGroup;
        [SerializeField] float fadeDuration = 1f;

        public float FadeDuration => fadeDuration; // Expose fade duration for other scripts if needed
        [SerializeField] bool isFading = false;

        public void FadeInYouDiedScreen()
        {
            if (!isFading)
            {
                youDiedCanvasGroup.gameObject.SetActive(true); // Ensure the you died screen is active before fading in
                StartCoroutine(FadeInCoroutine());
            }
        }

        public void FadeOutYouDiedScreen()
        {
            if (!isFading)
            {
                StartCoroutine(FadeOutCoroutine());
            }
        }

        public

        IEnumerator FadeInCoroutine()
        {
            float elapsedFadeTime = 0f;
            isFading = true;
            while (elapsedFadeTime < fadeDuration)
            {

                elapsedFadeTime += Time.deltaTime;
                youDiedCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedFadeTime / fadeDuration);
                yield return null;
            }

            youDiedCanvasGroup.alpha = 1f; // Ensure it's fully visible at the end
            isFading = false;
        }

        IEnumerator FadeOutCoroutine()
        {
            float elapsedFadeTime = 0f;
            isFading = true;
            while (elapsedFadeTime < fadeDuration)
            {

                elapsedFadeTime += Time.deltaTime;
                youDiedCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedFadeTime / fadeDuration);
                yield return null;
            }

            youDiedCanvasGroup.alpha = 0f; // Ensure it's fully hidden at the end
                                           //youDiedCanvasGroup.gameObject.SetActive(false);
            isFading = false;
        }

    }



}

