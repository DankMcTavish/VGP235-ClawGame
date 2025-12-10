using UnityEngine;
using System.Collections;

public class ClawMovementController : MonoBehaviour
{
    [Header("Claw Movement Settings")]
    public float moveSpeed = 5.0f;
    public float dropSpeed = 3.0f;
    public float liftSpeed = 2.0f;

    [Header("Input")]
    public GameInputController inputController;
    public ClawGripController gripController;

    [Header("Movement Boundaries")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;

    [Header("Claw Components")]
    public Transform clawGrabPoint;

    [Header("Drop Settings")]
    public float maxDropDistance = 6f; // Maximum cable length
    public float minHeight = 0.5f; // floor height check
    public float grabDelay = 0.5f;

    [Header("Grab Settings")]
    [Range(0f, 1f)] public float gripStrengthFactor = 0.8f;
    public float grabRadius = 1f;
    public LayerMask grabbableLayer;

    private bool isDropping = false;
    private bool isReturning = false;
    private GameObject grabbedObject;

    // Physics / Swinging Implementation
    private GameObject gantryAnchor; // The invisible motorized part at the top
    private Rigidbody clawRb;
    private SpringJoint clawJoint;
    
    private float currentCableLength;
    private float minCableLength = 0.5f;
    private Vector3 initialGantryPos;

    void Start()
    {
        // 1. Setup Gantry (Anchor)
        // The Gantry is the "motor" that moves on the rails. It is invisible (or we assume this script was on the claw).
        gantryAnchor = new GameObject("ClawGantry_Anchor");
        gantryAnchor.transform.position = transform.position;
        initialGantryPos = transform.position;
        
        Rigidbody gantryRb = gantryAnchor.AddComponent<Rigidbody>();
        gantryRb.isKinematic = true; // Moved by code, not forces

        // 2. Setup Claw Body (This Object)
        clawRb = GetComponent<Rigidbody>();
        if (clawRb == null) clawRb = gameObject.AddComponent<Rigidbody>();
        
        clawRb.mass = 5f; // Give it weight
        clawRb.drag = 0.5f; // Air resistance dampens swing
        clawRb.angularDrag = 1.0f;
        clawRb.useGravity = true;
        clawRb.isKinematic = false;

        // 3. Connect Claw to Gantry with a SpringJoint (simulating a cable)
        clawJoint = gameObject.AddComponent<SpringJoint>();
        clawJoint.connectedBody = gantryRb;
        clawJoint.autoConfigureConnectedAnchor = false;
        clawJoint.anchor = Vector3.zero; // Attach to top of claw
        clawJoint.connectedAnchor = Vector3.zero; // Attach to gantry point
        
        // Configure for "Rope-like" behavior using Spring
        // High spring force keeps it attached, maxDistance changes length
        clawJoint.spring = 1000f; 
        clawJoint.damper = 10f;
        
        minCableLength = 0.1f;
        currentCableLength = minCableLength;
        clawJoint.maxDistance = currentCableLength;
        clawJoint.minDistance = 0f;
    }

    void Update()
    {
        // Constantly update cable length limit
        if (clawJoint != null)
        {
            clawJoint.maxDistance = currentCableLength;
        }

        // Logic
        if (!isDropping && !isReturning)
        {
            HandleGantryMovement();

            if (inputController != null && inputController.DropInput)
            {
                StartCoroutine(DropSequence());
            }
        }
    }

    void HandleGantryMovement()
    {
        if (gantryAnchor == null) return;

        // Horizontal movement (X-axis)
        float horizontal = inputController.MovementInput.x;
        float vertical = inputController.MovementInput.y;

        if (horizontal == 0 && vertical == 0) return;

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        Vector3 newPos = gantryAnchor.transform.position + movement;

        // Clamp position within boundaries
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        
        // Apply Move
        gantryAnchor.transform.position = newPos;
        
        // The claw (this.gameObject) will naturally swing due to the SpringJoint!
    }

    public IEnumerator DropSequence()
    {
        isDropping = true;

        if (gripController != null) gripController.OpenGrip();

        // 1. Drop Down (Extend Cable)
        while (currentCableLength < maxDropDistance)
        {
            currentCableLength += dropSpeed * Time.deltaTime;
            
            // Optional: Raycast check to stop if we hit something? 
            // Real claws usually just go to a set depth (limit switch) or slack detection.
            // keeping it simple: go to max depth.
            yield return null;
        }

        // 2. Wait at bottom
        yield return new WaitForSeconds(grabDelay);

        // 3. Grab
        GrabNearbyObject();
        if (gripController != null) gripController.GripObject();
        
        yield return new WaitForSeconds(0.5f); // Wait for close

        // 4. Lift Up (Retract Cable)
        while (currentCableLength > minCableLength)
        {
            currentCableLength -= liftSpeed * Time.deltaTime;
            yield return null;
        }
        currentCableLength = minCableLength;

        // 5. Slippage Check
        if (grabbedObject != null)
        {
            if (Random.value > gripStrengthFactor)
            {
                Debug.Log("Object Slipped!");
                ReleaseAndDropObject();
            }
        }

        // 6. Return Home (if we have a prize)
        if (grabbedObject != null)
        {
            isReturning = true;
            isDropping = false;

            // Move Gantry back to start
            while (Vector3.Distance(gantryAnchor.transform.position, initialGantryPos) > 0.1f)
            {
                gantryAnchor.transform.position = Vector3.MoveTowards(
                    gantryAnchor.transform.position,
                    initialGantryPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            // Drop Prize
            if (gripController != null) gripController.ReleaseObject();
            ReleaseAndDropObject();
            
            yield return new WaitForSeconds(1.0f); // Cooldown
            isReturning = false;
        }
        else
        {
            isDropping = false;
        }
    }

    void GrabNearbyObject()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            clawGrabPoint != null ? clawGrabPoint.position : transform.position,
            grabRadius,
            grabbableLayer
        );

        if (hitColliders.Length > 0)
        {
            GameObject closest = hitColliders[0].gameObject; // Simplify to first hit
            
            // Attach
            grabbedObject = closest;
            
            // For physics claw, we joints! But parenting is more stable for "Grip".
            // Since Claw is now a Rigidbody, if we parent the prize to it, the prize moves with it.
            // Logic:
            Rigidbody prizeRb = grabbedObject.GetComponent<Rigidbody>();
            if (prizeRb != null)
            {
                prizeRb.isKinematic = true; // Disable physics on prize so it doesn't fight the claw
            }
            
            grabbedObject.transform.SetParent(clawGrabPoint != null ? clawGrabPoint : transform);
            grabbedObject.transform.localPosition = Vector3.zero; // Snap to grip point? Or keep offset?
            // keeping offset is safer usually, but let's snap slightly or leave it. current logic: SetParent.
        }
    }

    void ReleaseAndDropObject()
    {
        if (grabbedObject == null) return;

        grabbedObject.transform.SetParent(null);

        Rigidbody prizeRb = grabbedObject.GetComponent<Rigidbody>();
        if (prizeRb != null)
        {
            prizeRb.isKinematic = false;
            prizeRb.useGravity = true;
            // Add a tiny push?
        }

        grabbedObject = null;
    }

    void OnDrawGizmosSelected()
    {
        if (clawGrabPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(clawGrabPoint.position, grabRadius);
        }
        
        // Draw cable limits if playing
        if (Application.isPlaying && gantryAnchor != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(gantryAnchor.transform.position, transform.position);
        }
    }
}