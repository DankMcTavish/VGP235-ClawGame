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
        // Use the widely-available API to find managers in the scene
        difficultyManager = FindObjectOfType<DifficultyManager>();
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
        }

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

    // Map the difficulty to the time limits requested:
    // Easy -> 120s, Medium -> 100s, Hard -> 60s
    void ApplyTimeLimitFromDifficulty()
    {
        if (difficultyManager == null)
        {
            timeLimit = 120f; // default easy if no manager
            return;
        }

        // currentDifficulty is an enum (DifficultyLevel). Call ToString() directly.
        string diff = difficultyManager.currentDifficulty.ToString();
        diff = diff.ToLowerInvariant();

        if (diff.Contains("easy"))
        {
            timeLimit = 120f;
        }
        else if (diff.Contains("medium") || diff.Contains("normal"))
        {
            timeLimit = 100f;
        }
        else if (diff.Contains("hard"))
        {
            timeLimit = 60f;
        }
        else
        {
            // fallback default
            timeLimit = 120f;
        }
    }

    // Helper to get a sensible level number for the UI if DifficultyManager doesn't provide one.
    int GetCurrentLevel()
    {
        // If DifficultyManager exposes a level or numeric difficulty, you can map that here.
        return 1;
    }
}
