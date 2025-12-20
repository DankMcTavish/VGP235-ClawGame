using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }
    [Header("Audio Settings")]
    public float masterVolume = 1.0f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 0.8f;

    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip victoryMusic;
    public AudioClip gameOverMusic;
    public AudioClip menuMusic;
    public AudioClip pauseMusic;

    [Header("Claw Sounds")]
    public GameObject clawObject;
    public AudioClip clawMovement;
    public AudioClip clawExtend;
    public AudioClip clawRetract;
    public AudioClip prizeGrabbed;


    [Header("UI Sounds")]
    public AudioClip buttonClick;

    [Header("Prize Sounds")]
    public AudioClip prizeFall; // squishy sound or whistle when it falls
    public AudioClip prizeClaimed; // sound when prize is successfully claimed


}
