using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace EternalKeep
{
    public class HandleLoadingScreen : MonoBehaviour
    {
        public static HandleLoadingScreen Instance { get; private set; }
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
        [SerializeField] CanvasGroup loadingScreenCanvasGroup;
        [SerializeField] float fadeDuration = 1f;

        public float FadeDuration => fadeDuration; // Expose fade duration for other scripts if needed

        [SerializeField] bool isfading = false;


        [SerializeField] Image loadIconStaticImage; // Reference to the static loading icon image
        [SerializeField] Image loadIconDynamicImage; // Reference to the dynamic loading icon image

        float elapsedFadeTime = 0f;

        void Update()
        {
            // if (Input.GetKeyDown(KeyCode.L)) // For testing purposes, press 'L' to fade in
            // {
            //     FadeInLoadingScreen();
            // }

            // if (Input.GetKeyDown(KeyCode.O)) // For testing purposes, press 'O' to fade out
            // {
            //     FadeOutLoadingScreen();
            // }
        }
        public void FadeInLoadingScreen(float delay = 0f)
        {
            if (!isfading)
            {
                loadingScreenCanvasGroup.gameObject.SetActive(true); // Ensure the loading screen is active before fading in
                loadIconStaticImage.gameObject.SetActive(true); // Show the static loading icon
                loadIconDynamicImage.gameObject.SetActive(true); // Show the dynamic loading icon
                StartCoroutine(FadeInCoroutine(delay));
            }
        }

        public void FadeOutLoadingScreen(float delay = 0f)
        {
            if (!isfading)
            {
                StartCoroutine(FadeOutCoroutine(delay));
            }
        }


        IEnumerator FadeInCoroutine(float delay = 0f)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay); // Wait for the specified delay before starting the fade-in
            }

            elapsedFadeTime = 0f;
            isfading = true;
            loadingScreenCanvasGroup.alpha = 0f; // Ensure it starts fully transparent
            loadingScreenCanvasGroup.gameObject.SetActive(true); // Ensure the loading screen is active
            {
                elapsedFadeTime = 0f;
                isfading = true;
                while (elapsedFadeTime < fadeDuration)
                {

                    elapsedFadeTime += Time.deltaTime;
                    loadingScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedFadeTime / fadeDuration);
                    yield return null;
                }

                loadingScreenCanvasGroup.alpha = 1f; // Ensure it's fully visible at the end
                isfading = false;
            }
        }

        IEnumerator FadeOutCoroutine(float delay = 0f)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay); // Wait for the specified delay before starting the fade-in
            }
            elapsedFadeTime = 0f;
            isfading = true;
            while (elapsedFadeTime < fadeDuration)
            {

                elapsedFadeTime += Time.deltaTime;
                loadingScreenCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedFadeTime / fadeDuration);
                yield return null;
            }

            loadingScreenCanvasGroup.alpha = 0f; // Ensure it's fully hidden at the end
                                                 //loadingScreenCanvasGroup.gameObject.SetActive(false); 
            loadIconStaticImage.gameObject.SetActive(false); // hide the static loading icon
            loadIconDynamicImage.gameObject.SetActive(false); // hide the dynamic loading icon
            isfading = false;
        }
    }

}

