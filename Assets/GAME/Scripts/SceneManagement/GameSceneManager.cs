using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalKeep
{
    public class GameSceneManager : MonoBehaviour
    {
        public static GameSceneManager Instance { get; private set; }
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
        [SerializeField] float minimumSceneLoadTime = 2f; // Minimum time to wait before loading the scene
        [SerializeField] string scene1Name = "Scene1"; // Name of the first scene to load


        public void OnPlayTestButtonClicked()
        {
            // Load the test scene (replace "TestScene" with the actual name of your test scene)
            //SceneManager.LoadScene(1);
            //LoadScene
        }

        public void OnNewGameButtonClicked()
        {
            SaveSystem.DeleteSaveFile();
            LoadScene(scene1Name);
        }

        public void OnQuitGameButtonClicked()
        {
            // Quit the game (this will only work in a built application, not in the editor)
            Application.Quit();

            // If you want to stop playing in the editor, uncomment the line below
            // UnityEditor.EditorApplication.isPlaying = false;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        IEnumerator LoadSceneCoroutine(string scene)
        {
            HandleLoadingScreen.Instance.FadeInLoadingScreen();

            yield return new WaitForSeconds(HandleLoadingScreen.Instance.FadeDuration); //delay to show loading screen before starting the scene load

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene);
            asyncOperation.allowSceneActivation = false;

            float elapsedTime = 0f;
            while (asyncOperation.progress < 0.9f || elapsedTime < minimumSceneLoadTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null; // Wait for the next frame
            }

            asyncOperation.allowSceneActivation = true;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            HandleLoadingScreen.Instance.FadeOutLoadingScreen();

            yield return new WaitForSeconds(HandleLoadingScreen.Instance.FadeDuration);
        }
    }


}

