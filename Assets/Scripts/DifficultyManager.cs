using UnityEngine;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Expert
}


public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }
    [Header("Difficulty Settings")]
    public int prizeToWin;
    public float clawSpeedMultiplier;
    public float timeLimit; // Store timeLimit here instead of pushing to GameManager
    public float timeLimitMultiplier;



    public DifficultyLevel currentDifficulty;

    public void SetDifficulty(DifficultyLevel level)
    {
        currentDifficulty = level;
        switch (level)
        {
            case DifficultyLevel.Easy:
                prizeToWin = 3;
                timeLimit = 200f;
                break;
            case DifficultyLevel.Medium:
                prizeToWin = 5;
                timeLimit = 120f;
                break;
            case DifficultyLevel.Hard:
                prizeToWin = 7;
                timeLimit = 80f;
                break;
            case DifficultyLevel.Expert:
                prizeToWin = 10;
                timeLimit = 60f;
                break;
        }
    }

    public void NextDifficulty()
    {
        int currentLevelIndex = (int)currentDifficulty;
        int nextLevelIndex = currentLevelIndex + 1;
        
        if (nextLevelIndex > (int)DifficultyLevel.Expert)
        {
            nextLevelIndex = (int)DifficultyLevel.Expert; // Cap at Expert or loop? Plan said "increase", let's cap.
        }

        SetDifficulty((DifficultyLevel)nextLevelIndex);
    }
}
