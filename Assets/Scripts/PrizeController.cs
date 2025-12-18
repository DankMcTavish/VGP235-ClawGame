using UnityEngine;


public class PrizeController : MonoBehaviour
{
    public int scoreValue = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Dynamic Sizing based on Score Value
        // Higher score = smaller size. 
        // Logic: Inverse proportional or simple Lerp.
        // Let's assume score ranges roughly 10 (easy) to 100 (hard).

        float scaleFactor = 1.0f;
        if (scoreValue <= 10) scaleFactor = 1.2f; // Large for low value
        else if (scoreValue <= 20) scaleFactor = 1.0f; // Normal
        else if (scoreValue <= 50) scaleFactor = 0.8f; // Smaller
        else scaleFactor = 0.6f; // Tiny for high value

        transform.localScale = transform.localScale * scaleFactor;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.isKinematic = false;
        
        // Ensure continuous collision detection to prevent falling through floor
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        // Physics handles movement now
    }
}
