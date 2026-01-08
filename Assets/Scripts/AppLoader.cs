using System.Collections;
using System.Collections;
using UnityEngine;



public class AppLoader : AsyncLoader
{
    // The only singleton you should have.
    public static AppLoader Instance { get { return _instance; } }
    private static AppLoader _instance;

    public static Transform SystemsParent { get { return _systemsParent; } }
    private static Transform _systemsParent;

    [Header("Core Systems - Prefabs")]
    [SerializeField] private GameObject _firebaseManagerPrefab;
    [SerializeField] private GameObject _configManagerPrefab;
    [SerializeField] private GameObject _difficultyManagerPrefab;
    [SerializeField] private GameObject _audioManagerPrefab;

    protected override void Awake()
    {
        Debug.Log($"{nameof(AppLoader)} Starting");

        if (_instance != null && _instance != this)
        {
            Debug.Log("A duplicate instance of the GameLoader was found, and will be ignored. Only one instance is permitted");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        GameObject systemsGO = new GameObject("[Systems]");
        _systemsParent = systemsGO.transform;
        DontDestroyOnLoad(systemsGO);

        Enqueue(InitializeCoreSystems(), 1);
        CallOnComplete(OnComplete);
    }

    private IEnumerator InitializeCoreSystems()
    {
        Debug.Log("Loading Core Systems");

        if (_firebaseManagerPrefab != null)
        {
            GameObject go = Instantiate(_firebaseManagerPrefab, _systemsParent);
            go.name = "FirebaseManager";
            var manager = go.GetComponent<FirebaseManager>();
            
            manager.Initialize(); 
            manager.SetupRemoteConfig();
            var fetchTask = manager.FetchDataAsync();
            yield return new WaitUntil(() => fetchTask.IsCompleted);

            Debug.Log("<color=lime>Firebase Manager Loaded</color>");
        }

        if (_configManagerPrefab != null)
        {
            GameObject go = Instantiate(_configManagerPrefab, _systemsParent);
            go.name = "ConfigManager";
            var manager = go.GetComponent<ConfigManager>();
            manager.UpdateConfigValues();
            Debug.Log("<color=lime>Config Manager Loaded</color>");
        }

        if (_difficultyManagerPrefab != null)
        {
            GameObject go = Instantiate(_difficultyManagerPrefab, _systemsParent);
            go.name = "DifficultyManager";
            Debug.Log("<color=lime>Difficulty Manager Loaded</color>");
        }

        if (_audioManagerPrefab != null)
        {
            Object.Instantiate(_audioManagerPrefab, _systemsParent).name = "AudioManager";
            Debug.Log("<color=lime>Audio Manager Loaded</color>");
        }

        GameObject sceneControllerGO = new GameObject("SceneController");
        sceneControllerGO.transform.SetParent(_systemsParent);
        sceneControllerGO.AddComponent<SceneController>();
        Debug.Log("<color=lime>Scene Controller Loaded</color>");

        yield return null;
    }

    private void OnComplete()
    {
        Debug.Log("GameLoader Completed");
    }
}