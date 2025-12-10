using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using GameInput;


public class GameInputController : MonoBehaviour
{

    [Header("Settings")]
    public bool useKeyboardInput = true;   // Toggle for testing in editor
    public bool useTouchInput = false;     // Toggle for mobile build

    public Vector2 MovementInput { get; private set; }
    public bool DropInput { get; private set; }

    private InputActionReference InputActionReference;
    private InputActionReference DropInputActionReference;

    void Update()
    {
        MovementInput = Vector2.zero;
        DropInput = false;

        if (useKeyboardInput)
        {
            HandleKeyboardInput(InputActionReference);
            HandleKeyboardInput(DropInputActionReference);
        }

        if (useTouchInput)
        {
            HandleTouchInput();
        }
    }


    private void OnEnable()
    {
        InputActionReference.Enable();
        DropInputActionReference.Enable();
    }

    private void OnDisable()
    {
        InputActionReference.Disable();
        DropInputActionReference.Disable();
    }

    private void HandleKeyboardInput(InputAction inputAction)
    {
        InputActionReference inputActionReference = inputAction;
        float horizontal = inputActionReference.ReadValue<Vector2>().x;
        float vertical = inputActionReference.ReadValue<Vector2>().y;
        MovementInput = new Vector2(horizontal, vertical);

        if (InputActionReference.GetPressed(KeyCode.Space))
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
