using System.Collections;
using System.Collections;
using UnityEngine;

// SOLID
// S - Separation of Concerns: A class should only be responsible for one thing
// O - Open / Closed principle: A class should be open to extension and closed to modification
// L - Liskov Substitution Principle : A base object should be able to be replaced by a derived
// I - Interface Segregation: Keep your interfaces small and to the point.
// D - Dependency Inversion: Low level systems shouldn't depend on higher level systems
//      - Achieved with our ServiceLocator


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
    [SerializeField] private GameObject _collectionManagerPrefab;

    protected override void Awake()
    {
        Debug.Log($"{nameof(AppLoader)} Starting");

        // Saftey check
        if (_instance != null && _instance != this)
        {
            Debug.Log("A duplicate instance of the GameLoader was found, and will be ignored. Only one instance is permitted");
            Destroy(gameObject);
            return;
        }

        // Set reference to this instance
        _instance = this;

        // Make persistent
        DontDestroyOnLoad(gameObject);

        // Setup System GameObject
        GameObject systemsGO = new GameObject("[Systems]");
        _systemsParent = systemsGO.transform;
        DontDestroyOnLoad(systemsGO);

        // Queue up loading routines
        Enqueue(InitializeCoreSystems(), 1);

        // Set completion callback
        CallOnComplete(OnComplete);
    }

    private IEnumerator InitializeCoreSystems()
    {
        // Setup Core Systems
        Debug.Log("Loading Core Systems");

        // 1. Firebase (Async)
        if (_firebaseManagerPrefab != null)
        {
            GameObject go = Instantiate(_firebaseManagerPrefab, _systemsParent);
            go.name = "FirebaseManager";
            var manager = go.GetComponent<FirebaseManager>();
            
            // Wait for initialization
            manager.Initialize(); 
            // Note: Initialize is async in its own way internally, but for now we just start it.
            // If we need to wait for it strictly, we might need a flag in FirebaseManager.
            // Assuming it's fast enough or allows non-blocking usage.
            
            manager.SetupRemoteConfig();

            // Fetch Data
            var fetchTask = manager.FetchDataAsync();
            yield return new WaitUntil(() => fetchTask.IsCompleted);

            Debug.Log("<color=lime>Firebase Manager Loaded</color>");
        }

        // 2. Config Manager
        if (_configManagerPrefab != null)
        {
            GameObject go = Instantiate(_configManagerPrefab, _systemsParent);
            go.name = "ConfigManager";
            var manager = go.GetComponent<ConfigManager>();
            manager.UpdateConfigValues(); // Read from Firebase if ready
            Debug.Log("<color=lime>Config Manager Loaded</color>");
        }

        // 3. Difficulty Manager
        if (_difficultyManagerPrefab != null)
        {
            GameObject go = Instantiate(_difficultyManagerPrefab, _systemsParent);
            go.name = "DifficultyManager";
            Debug.Log("<color=lime>Difficulty Manager Loaded</color>");
        }

    // 4. Audio Manager
        if (_audioManagerPrefab != null)
        {
            Object.Instantiate(_audioManagerPrefab, _systemsParent).name = "AudioManager";
            Debug.Log("<color=lime>Audio Manager Loaded</color>");
        }

        // 5. Collection Manager
        if (_collectionManagerPrefab != null)
        {
            Object.Instantiate(_collectionManagerPrefab, _systemsParent).name = "CollectionManager";
            Debug.Log("<color=lime>Collection Manager Loaded</color>");
        }

        // 6. Scene Controller (Generated dynamically)
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