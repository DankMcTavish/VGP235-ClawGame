using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int score = 0;
    public float timeLimit = 120.0f;


    private float currentTime;
    private float startTime;
    private bool isGameOver = false;



    public UIController uiController;
    private DifficultyManager difficultyManager;

    void Start()
    {
        difficultyManager = FindFirstObjectByType<DifficultyManager>();
        if (uiController == null)
        {
            uiController = FindFirstObjectByType<UIController>();
        }
        
        // Ensure game starts in a paused/menu state if needed, or just wait for StartGame call
        Time.timeScale = 0f; // Pause game initially for Main Menu
    }

    void Update()
    {
        if (!isGameOver && Time.timeScale > 0)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= timeLimit)
            {
                GameOver();
            }
        }
    }

    public void StartGame()
    {
        score = 0;
        currentTime = 0;
        isGameOver = false;
        Time.timeScale = 1f;
        Debug.Log("Game Started! Time Limit: " + timeLimit + " seconds");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score Updated: " + score);
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (difficultyManager != null && score >= difficultyManager.prizeToWin)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("You Won!");
        if (uiController != null)
        {
            uiController.ShowWinningScreen();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("Game Over! Final Score: " + score);
        if (uiController != null)
        {
            uiController.ShowGameOverMenu();
        }
    }
}
