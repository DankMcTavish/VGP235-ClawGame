using UnityEngine;

public class PrizeDropZone : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("PrizeDropZone: GameManager not found in scene!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PrizeController prize = other.GetComponent<PrizeController>();
        if (prize != null)
        {
            if (gameManager != null)
            {
                gameManager.AddScore(prize.scoreValue);
                Debug.Log($"Prize Scored! Value: {prize.scoreValue}");
                
                // Optional: Destroy the prize or move it to a "collected" area
                // For now, we'll just disable it to prevent double scoring if it bounces around
                // or you could add a flag to the prize to mark it as 'scored'
                prize.gameObject.SetActive(false); 
            }
        }
    }
}
