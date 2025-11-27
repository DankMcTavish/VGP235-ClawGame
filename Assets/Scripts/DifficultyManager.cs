using UnityEngine;

enum DifficultyLevel
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



    private void Start()
    {
        SetDifficulty(DifficultyLevel.Medium);
    }


    void SetDifficulty(DifficultyLevel level)
    {
        switch (DifficultyLevel.Medium)
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
}
