using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int score = 0;
    public float timeLimit = 120.0f; // default, will be overridden by difficulty

    private float remainingTime;
    private bool isGameOver = false;

    public UIController uiController;
    private DifficultyManager difficultyManager;

    void Start()
    {
        // 1. UI Controller is local to the scene, so FindObjectOfType is still appropriate/needed if not assigned in Inspector
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
        }

        // 2. Global Managers accessed via Singleton
        // No need to cache them strictly, can access directly, but for clarity:
        // difficultyManager = DifficultyManager.Instance; // if you want to keep reference


        // Ensure game starts in a paused/menu state if needed, or just wait for StartGame call
        Time.timeScale = 0f; // Pause game initially for Main Menu
    }

    void Update()
    {
        if (!isGameOver && Time.timeScale > 0)
        {
            // countdown timer behavior
            remainingTime -= Time.deltaTime;
            if (uiController != null)
            {
                uiController.UpdateTimeText(remainingTime);
            }

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                GameOver();
            }
        }
    }

    public void StartGame()
    {
        score = 0;
        isGameOver = false;

        // Set the time limit based on difficulty each time a game starts
        ApplyTimeLimitFromDifficulty();

        remainingTime = timeLimit;

        // ensure UI shows the starting time immediately
        if (uiController != null)
        {
            uiController.UpdateScoreText(score);
            uiController.UpdateTimeText(remainingTime);
            uiController.UpdateLevelText(GetCurrentLevel());
        }

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
        if (uiController != null)
        {
            uiController.UpdateScoreText(score);
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (DifficultyManager.Instance != null && score >= DifficultyManager.Instance.prizeToWin)
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

    // Map the difficulty to the time limits requested:
    // This logic could be moved to DifficultyManager entirely, but if GameManager needs to pull it:
    void ApplyTimeLimitFromDifficulty()
    {
        if (DifficultyManager.Instance == null)
        {
            timeLimit = 120f; // default easy if no manager
            return;
        }

        // Pull the time limit that DifficultyManager calculated
        timeLimit = DifficultyManager.Instance.timeLimit;
    }

    // Helper to get a sensible level number for the UI if DifficultyManager doesn't provide one.
    int GetCurrentLevel()
    {
        // If DifficultyManager exposes a level or numeric difficulty, you can map that here.
        return 1;
    }
}
