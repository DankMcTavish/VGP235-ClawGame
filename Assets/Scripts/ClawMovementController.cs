using UnityEngine;
using System.Collections;

public class ClawMovementController : MonoBehaviour
{
    [Header("Claw Components")]
    public Rigidbody clawBody; // ASSIGN THE SPHERE HERE
    public Transform clawGrabPoint; // Assign the grab point (child of sphere)
    public LineRenderer cableRenderer; // Assign the Line (cylinder) or a LineRenderer component

    [Header("Movement Settings")]
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

    [Header("Drop Settings")]
    public float maxDropDistance = 6f; 
    public float minHeight = 0.5f; 
    public float grabDelay = 0.5f;

    [Header("Grab Settings")]
    [Range(0f, 1f)] public float gripStrengthFactor = 0.8f;
    public float grabRadius = 1f;
    public LayerMask grabbableLayer;

    private bool isDropping = false;
    private bool isReturning = false;
    private GameObject grabbedObject;

    // Physics
    private ConfigurableJoint clawJoint;
    private float currentCableLength;
    private float minCableLength = 0.1f;
    private Vector3 initialGantryPos;

    void Start()
    {
        Debug.Log("Initializing Gantry-Based Claw Controller...");

        if (clawBody == null)
        {
            Debug.LogError("Claw Body is not assigned! Please assign the Sphere object to 'Claw Body'.");
            return;
        }

        // 1. Setup Gantry (This Object)
        initialGantryPos = transform.position;
        Rigidbody gantryRb = GetComponent<Rigidbody>();
        if (gantryRb == null)
        {
            gantryRb = gameObject.AddComponent<Rigidbody>();
            gantryRb.isKinematic = true; // Gantry is kinematic (moved by script)
        }

        // 2. Setup Claw Body (The Sphere)
        // CRITICAL FIX: Detach from Gantry so physics don't fight hierarchy
        clawBody.transform.SetParent(null);

        clawBody.mass = 5f;
        clawBody.linearDamping = 0.5f;
        clawBody.angularDamping = 1.0f;
        clawBody.useGravity = true;
        clawBody.isKinematic = false;

        // 3. Connect Claw to Gantry
        // We add the joint to the CLAW BODY and connect it to US (Gantry)
        clawJoint = clawBody.gameObject.AddComponent<ConfigurableJoint>();
        clawJoint.connectedBody = gantryRb;
        clawJoint.autoConfigureConnectedAnchor = false;

        // Anchor on Claw (Center)
        clawJoint.anchor = Vector3.zero;
        
        // Anchor on Gantry (Relative position of claw at start)
        // This ensures the rope starts exactly vertical or wherever the claw is placed
        // Note: We calculate this BEFORE detaching or use the initial relative position if we saved it, 
        // but since we just detached, the world positions haven't changed yet.
        clawJoint.connectedAnchor = transform.InverseTransformPoint(clawBody.position);

        // Setup Limits
        currentCableLength = 0.1f; // Start tight
        
        clawJoint.xMotion = ConfigurableJointMotion.Limited;
        clawJoint.yMotion = ConfigurableJointMotion.Limited;
        clawJoint.zMotion = ConfigurableJointMotion.Limited;

        clawJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        clawJoint.projectionDistance = 0.1f;

        UpdateCableLimit();

        // 4. Setup LineRenderer
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
        if (clawBody == null) return;

        UpdateVisuals();
        UpdateCableLimit();

        if (!isDropping && !isReturning)
        {
            HandleMovement();

            if (inputController != null && inputController.DropInput)
            {
                StartCoroutine(DropSequence());
            }
        }
    }

    void UpdateVisuals()
    {
        if (cableRenderer != null && clawJoint != null)
        {
            // Start point: Anchor on Gantry (World Space)
            Vector3 startPos = transform.TransformPoint(clawJoint.connectedAnchor);
            // End point: Claw Body Center
            Vector3 endPos = clawBody.position;

            cableRenderer.SetPosition(0, startPos);
            cableRenderer.SetPosition(1, endPos);
        }
    }

    void UpdateCableLimit()
    {
        if (clawJoint != null)
        {
            SoftJointLimit limit = new SoftJointLimit();
            limit.limit = currentCableLength;
            limit.bounciness = 0f;
            limit.contactDistance = 0.1f;
            clawJoint.linearLimit = limit;
        }
    }

    void HandleMovement()
    {
        // Move THIS object (The Gantry)
        float horizontal = inputController.MovementInput.x;
        float vertical = inputController.MovementInput.y;

        if (horizontal == 0 && vertical == 0) return;

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + movement;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);

        // Use MovePosition for better physics interaction since we are a Rigidbody now
        Rigidbody gantryRb = GetComponent<Rigidbody>();
        if (gantryRb != null)
        {
            gantryRb.MovePosition(newPos);
        }
        else
        {
            transform.position = newPos;
        }
    }

    public IEnumerator DropSequence()
    {
        isDropping = true;

        if (gripController != null) gripController.OpenGrip();

        // 1. Drop
        while (currentCableLength < maxDropDistance)
        {
            currentCableLength += dropSpeed * Time.deltaTime;
            yield return null;
        }

        // 2. Wait
        yield return new WaitForSeconds(grabDelay);

        // 3. Grab
        GrabNearbyObject();
        if (gripController != null) gripController.GripObject();
        
        yield return new WaitForSeconds(0.5f);

        // 4. Lift
        while (currentCableLength > minCableLength)
        {
            currentCableLength -= liftSpeed * Time.deltaTime;
            yield return null;
        }
        currentCableLength = minCableLength;

        // 5. Slip Check
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

            // Move Gantry back to start
            while (Vector3.Distance(transform.position, initialGantryPos) > 0.1f)
            {
                // Use MovePosition here too if possible, but MoveTowards is okay for return
                Vector3 targetPos = Vector3.MoveTowards(
                    transform.position,
                    initialGantryPos,
                    moveSpeed * Time.deltaTime
                );
                
                Rigidbody gantryRb = GetComponent<Rigidbody>();
                if (gantryRb != null) gantryRb.MovePosition(targetPos);
                else transform.position = targetPos;

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
            clawGrabPoint != null ? clawGrabPoint.position : clawBody.position,
            grabRadius,
            grabbableLayer
        );

        if (hitColliders.Length > 0)
        {
            GameObject closest = hitColliders[0].gameObject;

            // Safety: Don't grab self or gantry
            if (closest == clawBody.gameObject || closest == gameObject) return;
            
            grabbedObject = closest;
            
            Rigidbody prizeRb = grabbedObject.GetComponent<Rigidbody>();
            if (prizeRb != null)
            {
                prizeRb.isKinematic = true; 
            }
            
            // Parent to Grab Point if available, otherwise Body
            Transform parent = clawGrabPoint != null ? clawGrabPoint : clawBody.transform;
            grabbedObject.transform.SetParent(parent);
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
        else if (clawBody != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(clawBody.position, grabRadius);
        }
    }
}