using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppStart : MonoBehaviour
{
    [SerializeField] private int _sceneIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (FindObjectOfType<AppLoader>() == null)
        {
            Debug.LogError("AppLoader not found in scene! AppStart requires AppLoader to function. Please ensure AppLoader is present in the Loading Scene.");
            return;
        }

        AppLoader.CallOnComplete(LoadMainMenu);
    }

    private void LoadMainMenu()
    {
        // Scene Index Check
        if (_sceneIndex < 0 || _sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"Invalid Scene Index {_sceneIndex}");
            return;
        }

        StartCoroutine(LoadInitialScene());
    }

    private IEnumerator LoadInitialScene()
    {
        Debug.Log("GameLoader Starting Scene Load");
        yield return SceneManager.LoadSceneAsync(_sceneIndex);
    }
}
