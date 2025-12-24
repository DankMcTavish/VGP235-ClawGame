using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneController : Singleton<SceneController>
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupEventSystems();
        CleanupAudioListeners();
    }

    private void CleanupEventSystems()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        if (systems.Length <= 1) return;

        Debug.Log($"[{nameof(SceneController)}] Found {systems.Length} EventSystems. Cleaning up duplicates...");

        EventSystem mainSystem = null;

        // Prefer one marked as persistent (DontDestroyOnLoad)
        foreach (var sys in systems)
        {
            if (sys.gameObject.scene.buildIndex == -1) // -1 is DontDestroyOnLoad scene
            {
                mainSystem = sys;
                break;
            }
        }

        // If none persistent, pick the first one and make it persistent
        if (mainSystem == null && systems.Length > 0)
        {
            mainSystem = systems[0];
            DontDestroyOnLoad(mainSystem.gameObject);
        }

        foreach (var sys in systems)
        {
            if (sys != mainSystem)
            {
                Debug.Log($"[{nameof(SceneController)}] Destroying duplicate EventSystem in scene: {sys.gameObject.scene.name}");
                Destroy(sys.gameObject);
            }
        }
    }

    private void CleanupAudioListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length <= 1) return;

        Debug.Log($"[{nameof(SceneController)}] Found {listeners.Length} AudioListeners. Cleaning up duplicates...");

        // Strategy: Keep the listener in the "active" or "newest" scene. 
        // Typically the gameplay camera in the new scene is what we want.
        // If we have a listener in DontDestroyOnLoad (e.g. LoadingScreen camera), we might actually want to DISABLE it
        // in favor of the gameplay one, or destroy it.

        AudioListener targetListener = null;

        // 1. Prefer listener in the Active Scene (the one we just loaded and set active)
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (var listener in listeners)
        {
            if (listener.gameObject.scene == activeScene)
            {
                targetListener = listener;
                break;
            }
        }

        // 2. If no listener in active scene, just pick the first one (arbitrary fallback)
        if (targetListener == null && listeners.Length > 0)
        {
            targetListener = listeners[0];
        }

        // Destroy/Disable others
        foreach (var listener in listeners)
        {
            if (listener != targetListener)
            {
                Debug.Log($"[{nameof(SceneController)}] Destroying extra AudioListener on {listener.gameObject.name} in scene {listener.gameObject.scene.name}");
                Destroy(listener); // Just destroy the component, not the GameObject (camera might be needed)
            }
        }
    }

    public void LoadSceneAdditive(int index)
    {
        StartCoroutine(LoadSceneAdditiveRoutine(index));
    }

    public void UnloadScene(int index)
    {
        StartCoroutine(UnloadSceneRoutine(index));
    }

    public void TransitionToScene(int newSceneIndex, int oldSceneIndex)
    {
        StartCoroutine(TransitionRoutine(newSceneIndex, oldSceneIndex));
    }

    private IEnumerator LoadSceneAdditiveRoutine(int index)
    {
        yield return SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
        Scene newScene = SceneManager.GetSceneByBuildIndex(index);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }
    }

    private IEnumerator UnloadSceneRoutine(int index)
    {
        yield return SceneManager.UnloadSceneAsync(index);
    }

    private IEnumerator TransitionRoutine(int newSceneIndex, int oldSceneIndex)
    {
        yield return UnloadSceneRoutine(oldSceneIndex);
        yield return LoadSceneAdditiveRoutine(newSceneIndex);
    }
}
