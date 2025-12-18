using UnityEngine;


public class PrizeController : MonoBehaviour
{
    public int scoreValue = 10;
    public string prizeID = "prize_default"; // Identifier for Remote Config

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Update score from ConfigManager if available
        if (ConfigManager.Instance != null)
        {
            // If prizeID is set, try to get a specific value, otherwise fallback to existing or default
            int configScore = ConfigManager.Instance.GetInt(prizeID, scoreValue);
            if (configScore > 0)
            {
                scoreValue = configScore;
                Debug.Log($"Prize {prizeID} score updated to {scoreValue}");
            }
        }

        // Dynamic Sizing based on Score Value
        // Higher score = smaller size. 
        // Logic: Inverse proportional or simple Lerp.
        // Let's assume score ranges roughly 10 (easy) to 100 (hard).

        float scaleFactor = 1.0f;
        if (scoreValue <= 10) scaleFactor = 1.2f; // Large for low value
        else if (scoreValue <= 20) scaleFactor = 1.0f; // Normal
        else if (scoreValue <= 50) scaleFactor = 0.8f; // Smaller
        else scaleFactor = 0.6f; // Tiny for high value

        // Apply scale only once to avoid compounding if Start runs multiple times (though unlikely here)
        // Better: Set absolute localScale based on prefab default or reset it first if needed.
        // For now, assuming prefab scale implies a base of 1.
        transform.localScale = Vector3.one * scaleFactor; 

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
