using System.Collections;
using UnityEngine;

namespace EternalKeep
{
    public class HandleInGameMenu : MonoBehaviour
    {
        public static HandleInGameMenu Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [SerializeField] CanvasGroup inGameMenuCanvasGroup;
        [SerializeField] float fadeDuration = 1f;

        public float FadeDuration => fadeDuration; // Expose fade duration for other scripts if needed

        [SerializeField] bool isfading = false;
        [SerializeField] bool isMenuShowing = false;

        public bool IsMenuShowing => isMenuShowing; // Expose the menu state for other scripts if needed


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
                inGameMenuCanvasGroup.gameObject.SetActive(true); // Ensure the loading screen is active before fading in
                inGameMenuCanvasGroup.alpha = 0f; // Ensure it starts fully transparent

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
            inGameMenuCanvasGroup.alpha = 0f; // Ensure it starts fully transparent
            inGameMenuCanvasGroup.gameObject.SetActive(true); // Ensure the loading screen is active
            elapsedFadeTime = 0f;
            isfading = true;
            while (elapsedFadeTime < fadeDuration)
            {

                elapsedFadeTime += Time.deltaTime;
                inGameMenuCanvasGroup.alpha = Mathf.Lerp(0f, .75f, elapsedFadeTime / fadeDuration);
                yield return null;
            }

            inGameMenuCanvasGroup.alpha = .75f; // Ensure it's fully visible at the end
            inGameMenuCanvasGroup.blocksRaycasts = true;
            isfading = false;
            isMenuShowing = true; // Set the menu as showing
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
                inGameMenuCanvasGroup.alpha = Mathf.Lerp(.75f, 0f, elapsedFadeTime / fadeDuration);
                yield return null;
            }

            inGameMenuCanvasGroup.alpha = 0f; // Ensure it's fully hidden at the end
            inGameMenuCanvasGroup.gameObject.SetActive(false); // Deactivate the menu game object
            inGameMenuCanvasGroup.blocksRaycasts = false;
            isfading = false;
            isMenuShowing = false;
        }

        public void OnQuitGameButtonClicked()
        {
            SaveSystem.SaveGame();
            Application.Quit();
        }
    }


    
}

