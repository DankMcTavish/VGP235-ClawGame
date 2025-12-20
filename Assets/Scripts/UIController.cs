using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UIController : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject winningScreen;


    [Header("Buttons")]
    public Button startButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button retryButton;
    public Button continueButton;
    public Button mainMenuButton;
    public Button exitButton;

    [Header("Text")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;    // countdown timer display
    public TextMeshProUGUI levelText;

    [Header("Managers")]
    public GameManager gameManager;
    public DifficultyManager difficultyManager;

    [Header("Defaults")]
    public int defaultScore = 0;
    public int defaultLives = 3; // kept for backwards compatibility if needed
    public int defaultLevel = 1;

    // Current state (keeps UI in sync)
    private int currentScore;
    private int currentLives;
    private int currentLevel;

    private bool isPaused = false;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        // DifficultyManager is global now
        if (difficultyManager == null) difficultyManager = DifficultyManager.Instance;

        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (retryButton != null) retryButton.onClick.AddListener(RetryGame);
        if (continueButton != null) continueButton.onClick.AddListener(ContinueToNextLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        // Initialize current state from defaults
        currentScore = defaultScore;
        currentLives = defaultLives;
        currentLevel = defaultLevel;

        // Update UI with default values
        UpdateScoreText(currentScore);
        UpdateTimeText(0f); // show 00:00 until game starts
        UpdateLevelText(currentLevel);

        ShowMainMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else if (!mainMenu.activeSelf && !gameOverMenu.activeSelf && !winningScreen.activeSelf)
            {
                PauseGame();
            }
        }
    }

    public void UpdateScoreText(int score)
    {
        currentScore = score;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    // Update the timer display. Accepts remaining seconds (float) and formats mm:ss
    public void UpdateTimeText(float remainingSeconds)
    {
        if (timeText == null) return;

        if (remainingSeconds <= 0f)
        {
            timeText.text = "Time: 00:00";
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    public void UpdateLevelText(int level)
    {
        currentLevel = level;
        if (levelText != null)
        {
            levelText.text = "Level: " + level;
        }
    }

    private void HideAllMenus()
    {
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        winningScreen.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllMenus();
        mainMenu.SetActive(true);
        Time.timeScale = 0f; // Ensure game is paused in main menu
    }

    public void StartGame()
    {
        // Transition from Current Scene (Main Menu) to Gameplay (Index 2)
        // We assume this script is in the Main Menu scene.
        AppLoader.Instance.TransitionToScene(2, gameObject.scene.buildIndex);
    }

    public void ToggleDifficulty()
    {
        if (difficultyManager != null)
        {
            difficultyManager.NextDifficulty();
            Debug.Log("Difficulty set to: " + difficultyManager.currentDifficulty);
            // Ideally update UI text here to show current difficulty
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        HideAllMenus();
        pauseMenu.SetActive(true);
        gameManager.PauseGame();
    }

    public void ResumeGame()
    {
        isPaused = false;
        HideAllMenus();
        Time.timeScale = 1f;
        gameManager.ResumeGame();
    }

    public void ShowGameOverMenu()
    {
        HideAllMenus();
        gameOverMenu.SetActive(true);
    }

    public void RetryGame()
    {
        HideAllMenus();
        // Reset state to defaults when retrying
        currentScore = defaultScore;
        currentLives = defaultLives;
        currentLevel = defaultLevel;
        UpdateScoreText(currentScore);
        UpdateLevelText(currentLevel);

        gameManager.RestartGame();
    }

    public void ShowWinningScreen()
    {
        HideAllMenus();
        winningScreen.SetActive(true);
    }

    public void ContinueToNextLevel()
    {
        if (difficultyManager != null)
        {
            difficultyManager.NextDifficulty();
        }
        winningScreen.SetActive(false);

        // advance level counter
        currentLevel++;
        UpdateLevelText(currentLevel);

        gameManager.RestartGame();
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }
}
