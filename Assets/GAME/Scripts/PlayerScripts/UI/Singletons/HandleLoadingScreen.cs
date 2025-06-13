using System.Collections;
using UnityEngine.UI;
using UnityEngine;

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

    [SerializeField] bool isfadingIN = false;
    [SerializeField] bool isfadingOUT = false;

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
    public void FadeInLoadingScreen()
    {
        if (!isfadingIN)
        {
            loadingScreenCanvasGroup.gameObject.SetActive(true); // Ensure the loading screen is active before fading in
            loadIconStaticImage.gameObject.SetActive(true); // Show the static loading icon
            loadIconDynamicImage.gameObject.SetActive(true); // Show the dynamic loading icon
            StartCoroutine(FadeInCoroutine());
        }
    }

    public void FadeOutLoadingScreen()
    {
        if (!isfadingIN)
        {
            StartCoroutine(FadeOutCoroutine());
        }
    }

    public

    IEnumerator FadeInCoroutine()
    {
        elapsedFadeTime = 0f;
        isfadingIN = true;
        while (elapsedFadeTime < fadeDuration)
        {

            elapsedFadeTime += Time.deltaTime;
            loadingScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedFadeTime / fadeDuration);
            yield return null;
        }

        loadingScreenCanvasGroup.alpha = 1f; // Ensure it's fully visible at the end
        isfadingIN = false;
    }

    IEnumerator FadeOutCoroutine()
    {
        elapsedFadeTime = 0f;
        isfadingIN = true;
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
        isfadingIN = false;
    }
}
