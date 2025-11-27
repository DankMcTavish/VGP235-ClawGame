using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int score = 0;
    public float timeLimit = 120.0f;


    private float currentTime;
    private float startTime;
    private bool isGameOver = false;



    void Start()
    {
        Debug.Log("Game Started! Time Limit: " + timeLimit + " seconds");
    }
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score Updated: " + score);
    }
    private void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + score);
        // Additional game over logic here
    }
}
