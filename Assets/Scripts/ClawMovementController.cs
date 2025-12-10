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
    public Transform customGantry; // User assigned gantry
    public LineRenderer cableRenderer; // Visual for the rope

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
    private GameObject gantryAnchor; // The actual anchor we use
    private Rigidbody clawRb;
    private ConfigurableJoint clawJoint;
    
    // Physics State
    private float currentCableLength;
    private float minCableLength = 0.5f;
    private Vector3 initialGantryPos;

    void Start()
    {
        // 1. Setup Gantry (Anchor)
        if (customGantry != null)
        {
            // Use user provided Gantry
            gantryAnchor = customGantry.gameObject;
            initialGantryPos = gantryAnchor.transform.position;
            
            // Ensure it has RB
            Rigidbody gantryRb = gantryAnchor.GetComponent<Rigidbody>();
            if (gantryRb == null) 
            {
                gantryRb = gantryAnchor.AddComponent<Rigidbody>();
                gantryRb.isKinematic = true;
            }
        }
        else
        {
            // Auto-create fallback
            string gantryName = "ClawGantry_Anchor_Auto";
            GameObject existingGantry = GameObject.Find(gantryName);
            if (existingGantry != null) Destroy(existingGantry);

            gantryAnchor = new GameObject(gantryName);
            gantryAnchor.transform.position = transform.position; 
            initialGantryPos = transform.position;
            
            Rigidbody gantryRb = gantryAnchor.AddComponent<Rigidbody>();
            gantryRb.isKinematic = true;
        }

        // 2. Setup Claw Body (This Object)
        clawRb = GetComponent<Rigidbody>();
        if (clawRb == null) clawRb = gameObject.AddComponent<Rigidbody>();
        
        clawRb.mass = 5f; 
        clawRb.linearDamping = 0.5f; 
        clawRb.angularDamping = 1.0f;
        clawRb.useGravity = true;
        clawRb.isKinematic = false;
        
        // Zero out any velocity
        clawRb.linearVelocity = Vector3.zero;
        clawRb.angularVelocity = Vector3.zero;

        // 3. Connect Claw to Gantry with ConfigurableJoint (Rope)
        clawJoint = gameObject.AddComponent<ConfigurableJoint>();
        clawJoint.connectedBody = gantryRb;
        clawJoint.autoConfigureConnectedAnchor = false;
        
        // Anchor points
        clawJoint.anchor = Vector3.zero; // Top of claw
        clawJoint.connectedAnchor = Vector3.zero; // Bottom of gantry
        
        // Lock rotation? No, allow swing.
        // Lock Motion?
        // We want "Connected" behavior, so X/Y/Z are locked relative to anchor... 
        // BUT we want swinging.
        // Swinging means we are constrained by a RADIUS (LinearLimit).
        clawJoint.xMotion = ConfigurableJointMotion.Limited;
        clawJoint.yMotion = ConfigurableJointMotion.Limited;
        clawJoint.zMotion = ConfigurableJointMotion.Limited;
        
        // Set the Limit
        minCableLength = 0.1f;
        currentCableLength = minCableLength;
        
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = currentCableLength;
        limit.bounciness = 0f; // No bounce
        limit.contactDistance = 0.1f;
        clawJoint.linearLimit = limit;
        
        // 4. Setup LineRenderer if missing
        if (cableRenderer == null)
        {
            cableRenderer = gameObject.AddComponent<LineRenderer>();
            cableRenderer.startWidth = 0.05f;
            cableRenderer.endWidth = 0.05f;
            cableRenderer.material = new Material(Shader.Find("Sprites/Default"));
            cableRenderer.startColor = Color.black;
            cableRenderer.endColor = Color.black;
        }
    }

    void Update()
    {
        // Update Rope Visual
        if (cableRenderer != null && gantryAnchor != null)
        {
            cableRenderer.SetPosition(0, gantryAnchor.transform.position);
            cableRenderer.SetPosition(1, transform.position);
        }

        // Update Cable Length Limit
        if (clawJoint != null)
        {
            SoftJointLimit limit = clawJoint.linearLimit;
            limit.limit = currentCableLength;
            clawJoint.linearLimit = limit;
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
    }

    public IEnumerator DropSequence()
    {
        isDropping = true;

        if (gripController != null) gripController.OpenGrip();

        // 1. Drop Down (Extend Cable)
        while (currentCableLength < maxDropDistance)
        {
            currentCableLength += dropSpeed * Time.deltaTime;
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

        // 6. Return Home
        if (grabbedObject != null)
        {
            isReturning = true;
            isDropping = false;

            while (Vector3.Distance(gantryAnchor.transform.position, initialGantryPos) > 0.1f)
            {
                gantryAnchor.transform.position = Vector3.MoveTowards(
                    gantryAnchor.transform.position,
                    initialGantryPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            if (gripController != null) gripController.ReleaseObject();
            ReleaseAndDropObject();
            
            yield return new WaitForSeconds(1.0f); 
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
            GameObject closest = hitColliders[0].gameObject;
            
            grabbedObject = closest;
            
            Rigidbody prizeRb = grabbedObject.GetComponent<Rigidbody>();
            if (prizeRb != null)
            {
                prizeRb.isKinematic = true; 
            }
            
            grabbedObject.transform.SetParent(clawGrabPoint != null ? clawGrabPoint : transform);
            grabbedObject.transform.localPosition = Vector3.zero; 
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