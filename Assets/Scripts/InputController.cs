using UnityEngine;

public class InputController : MonoBehaviour
{
    [Header("References")]
    public ClawMovementController movementController;
    public ClawGripController gripController;

    [Header("Settings")]
    public bool useKeyboardInput = true;   // Toggle for testing in editor
    public bool useTouchInput = false;     // Toggle for mobile build

    void Update()
    {
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
        // Horizontal movement
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movementController.MoveHorizontal(-1f);
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movementController.MoveHorizontal(1f);
        }
        else
        {
            movementController.MoveHorizontal(0f); // stop
        }

        // Drop claw
        if (Input.GetKeyDown(KeyCode.Space))
        {
            movementController.DropClaw();
        }

        // Grip open/close
        if (Input.GetKeyDown(KeyCode.G))
        {
            gripController.ToggleGrip();
        }
    }

    private void HandleTouchInput()
    {
        // Example: simple left/right screen tap for movement
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                if (touch.position.x < Screen.width / 2)
                {
                    movementController.MoveHorizontal(-1f);
                }
                else
                {
                    movementController.MoveHorizontal(1f);
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                movementController.MoveHorizontal(0f);
            }
        }

        // You can add a UI button for DropClaw and Grip
        // and wire them directly to movementController.DropClaw()
        // and gripController.ToggleGrip() in the Inspector.
    }

}
