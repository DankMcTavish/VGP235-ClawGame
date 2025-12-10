using UnityEngine;
using TMPro;
using UnityEngine.UI;


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
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI levelText;

    [Header("Managers")]
    public GameManager gameManager;
    public DifficultyManager difficultyManager;

    private bool isPaused = false;

    void Start()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (difficultyManager == null) difficultyManager = FindFirstObjectByType<DifficultyManager>();

        startButton.onClick.AddListener(StartGame);
        pauseButton.onClick.AddListener(PauseGame);
        resumeButton.onClick.AddListener(ResumeGame);
        retryButton.onClick.AddListener(RetryGame);
        continueButton.onClick.AddListener(ContinueToNextLevel);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        exitButton.onClick.AddListener(ExitGame);
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

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        winningScreen.SetActive(false);
        Time.timeScale = 0f; // Ensure game is paused in main menu
    }

    public void StartGame()
    {
        mainMenu.SetActive(false);
        gameManager.StartGame();
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
        pauseMenu.SetActive(true);
        gameManager.PauseGame();
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        gameManager.ResumeGame();
    }

    public void ShowGameOverMenu()
    {
        gameOverMenu.SetActive(true);
    }

    public void RetryGame()
    {
        gameOverMenu.SetActive(false);
        winningScreen.SetActive(false);
        gameManager.RestartGame();
    }

    public void ShowWinningScreen()
    {
        winningScreen.SetActive(true);
    }

    public void ContinueToNextLevel()
    {
        if (difficultyManager != null)
        {
            difficultyManager.NextDifficulty();
        }
        winningScreen.SetActive(false);
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
