using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnPlayTestButtonClicked()
    {
        // Load the test scene (replace "TestScene" with the actual name of your test scene)
        SceneManager.LoadScene(1);
    }

    public void OnQuitGameButtonClicked()
    {
        // Quit the game (this will only work in a built application, not in the editor)
        Application.Quit();
        
        // If you want to stop playing in the editor, uncomment the line below
        // UnityEditor.EditorApplication.isPlaying = false;
    }
}
