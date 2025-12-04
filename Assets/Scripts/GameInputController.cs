using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInputController : MonoBehaviour
{

    [Header("Settings")]
    public bool useKeyboardInput = true;   // Toggle for testing in editor
    public bool useTouchInput = false;     // Toggle for mobile build

    void Update()
    {

    }


    private void Restart()
    {
        // Handle game restart logic here
        Debug.Log("Game Restarted");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }
}
