using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameInputController : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference dropActionReference;

    [Header("Settings")]
    public bool useKeyboardInput = true;   // Toggle for testing in editor
    public bool useTouchInput = false;     // Toggle for mobile build

    public Vector2 MovementInput { get; private set; }
    public bool DropInput { get; private set; }

    private void OnEnable()
    {
        if (moveActionReference != null) moveActionReference.action.Enable();
        if (dropActionReference != null) dropActionReference.action.Enable();
    }

    private void OnDisable()
    {
        if (moveActionReference != null) moveActionReference.action.Disable();
        if (dropActionReference != null) dropActionReference.action.Disable();
    }

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
        if (moveActionReference != null)
        {
            MovementInput = moveActionReference.action.ReadValue<Vector2>();
        }

        if (dropActionReference != null && dropActionReference.action.WasPressedThisFrame())
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
