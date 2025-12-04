using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInputController : MonoBehaviour
{

    [Header("Settings")]
    public bool useKeyboardInput = true;   // Toggle for testing in editor
    public bool useTouchInput = false;     // Toggle for mobile build

    public Vector2 MovementInput { get; private set; }
    public bool DropInput { get; private set; }

    void Update()
    {
        MovementInput = Vector2.zero;
        DropInput = false;

        if (useKeyboardInput)
        {
            HandleKeyboardInput();
        }

        if (useTouchInput)
        {
            HandleTouchInput();
        }
    }

    private void HandleKeyboardInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        MovementInput = new Vector2(horizontal, vertical);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DropInput = true;
        }
    }

    private void HandleTouchInput()
    {
        // Basic touch implementation - can be expanded later
        // For now, we'll just use simple on-screen button logic if we had UI buttons
        // Or we could use virtual joystick logic here
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
