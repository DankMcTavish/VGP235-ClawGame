using UnityEngine;
using System.Collections;

public class ClawMovementController : MonoBehaviour
{
    [Header("Claw Movement Settings")]
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 100.0f;
    public float dropSpeed = 3.0f;
    public float liftSpeed = 2.0f;

    [Header("Input")]
    public GameInputController inputController;

    [Header("Input Settings")]
    public ClawGripController gripController;

    [Header("Movement Boundaries")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;

    [Header("Claw Components")]
    public GameObject clawBase;
    public GameObject leftClawArm;
    public GameObject rightClawArm;
    public GameObject centerClawArm;
    public Transform clawGrabPoint;

    [Header("Drop Settings")]
    public float dropHeight = 5f;
    public float minHeight = 0.5f;
    public float grabDelay = 0.5f;

    [Header("Grab Settings")]
    public float grabRadius = 1f;
    public LayerMask grabbableLayer;

    private bool isDropping = false;
    private bool isReturning = false;
    private Vector3 startPosition;
    private GameObject grabbedObject;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Only allow movement when not dropping or returning
        if (!isDropping && !isReturning)
        {
            HandleMovement();

            // Drop claw when space is pressed
            if (inputController != null && inputController.DropInput)
            {
                StartCoroutine(DropSequence());
            }
        }
    }

    void HandleMovement()
    {
        // Horizontal movement (X-axis)
        float horizontal = inputController.MovementInput.x;
        MoveHorizontal(horizontal);

        // Forward/backward movement (Z-axis)
        float vertical = inputController.MovementInput.y;
        MoveVertical(vertical);
    }

    public void MoveHorizontal(float direction)
    {
        if (direction == 0) return;

        Vector3 movement = Vector3.right * direction * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        // Clamp position within boundaries
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        transform.position = newPos;
    }

    public void MoveVertical(float direction)
    {
        if (direction == 0) return;

        Vector3 movement = Vector3.forward * direction * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        // Clamp position within boundaries
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        transform.position = newPos;
    }

    public IEnumerator DropSequence()
    {
        isDropping = true;
        Vector3 currentPos = transform.position;
        Vector3 dropTarget = new Vector3(currentPos.x, minHeight, currentPos.z);

        gripController.GripObject();

        // Drop down
        while (Vector3.Distance(transform.position, dropTarget) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                dropTarget,
                dropSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Wait a moment at the bottom
        yield return new WaitForSeconds(grabDelay);

        // Try to grab something
        GrabNearbyObject();

        // Return to start
        isDropping = false;
        isReturning = true;

        while (Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                liftSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = startPosition;

        // Release object at collection point
        if (grabbedObject != null)
        {
            gripController.ReleaseObject(); // Release object from claw
            ReleaseObject(); // Release object from claw
        }

        isReturning = false;
    }

    void GrabNearbyObject()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            clawGrabPoint.position,
            grabRadius,
            grabbableLayer
        );

        if (hitColliders.Length > 0)
        {
            // Grab the closest object
            GameObject closest = null;
            float closestDist = float.MaxValue;

            foreach (Collider col in hitColliders)
            {
                float dist = Vector3.Distance(clawGrabPoint.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = col.gameObject;
                }
            }

            if (closest != null)
            {
                grabbedObject = closest;

                // Disable physics while grabbed
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                // Parent to claw
                grabbedObject.transform.SetParent(clawGrabPoint);
            }
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject == null) return;

        // Unparent
        grabbedObject.transform.SetParent(null);

        // Re-enable physics
        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Optional: Move to collection bin or destroy
        // Destroy(grabbedObject, 1f);

        grabbedObject = null;
    }

    // Visual debug for grab radius
    void OnDrawGizmosSelected()
    {
        if (clawGrabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(clawGrabPoint.position, grabRadius);
        }
    }
}