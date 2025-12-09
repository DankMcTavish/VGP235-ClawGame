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
    [Header("Difficulty Settings")]
    public int prizeToWin;
    public float clawSpeedMultiplier;
    public float timeLimitMultiplier;



    public DifficultyLevel currentDifficulty;

    private void Start()
    {
        SetDifficulty(DifficultyLevel.Medium);
    }


    public void SetDifficulty(DifficultyLevel level)
    {
        currentDifficulty = level;
        switch (level)
        {
            case DifficultyLevel.Easy:
                prizeToWin = 3;
                FindFirstObjectByType<GameManager>().timeLimit = 200f;
                break;
            case DifficultyLevel.Medium:
                prizeToWin = 5;
                FindFirstObjectByType<GameManager>().timeLimit = 120f;
                break;
            case DifficultyLevel.Hard:
                prizeToWin = 7;
                FindFirstObjectByType<GameManager>().timeLimit = 80f;
                break;
            case DifficultyLevel.Expert:
                prizeToWin = 10;
                FindFirstObjectByType<GameManager>().timeLimit = 60f;
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
