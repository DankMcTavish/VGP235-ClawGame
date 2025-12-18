using UnityEngine;
using Firebase.Extensions;

public class AppStart : MonoBehaviour
{
    // Dependency check logic
    private Firebase.FirebaseApp app;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;

                // Initialize Remote Config
                InitializeRemoteConfig();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    void InitializeRemoteConfig()
    {
        System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero);
        fetchTask.ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (task.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (task.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }

            var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
            if (info.LastFetchStatus == Firebase.RemoteConfig.LastFetchStatus.Success)
            {
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(activateTask => {
                     Debug.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");
                     
                     // Notify ConfigManager if it exists
                     if (ConfigManager.Instance != null)
                     {
                         ConfigManager.Instance.UpdateConfigValues();
                     }
                });
            }
        });
    }
}
