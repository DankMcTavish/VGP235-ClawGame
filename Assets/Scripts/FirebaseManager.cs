using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    private FirebaseApp _app = null;
    private bool _isInitialized = false;
    private readonly Dictionary<string, object> _defaults = new();

    protected override void Awake()
    {
        base.Awake();
        // Custom Awake logic if any needed besides singleton setup
    }

    // ... rest of your existing code ...

public void Initialize()
    {
        Debug.Log($"{nameof(FirebaseManager)} Initializing...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                _app = FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                _isInitialized = true;
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    private void OnDestroy()
    {
        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener
          -= OnRemoteConfigUpdated;
    }

    public void SetupRemoteConfig()
    {
        // These are the values that are used if we haven't fetched data from the
        // server
        // yet, or if we ask for values that the server doesn't have:
        _defaults.Add("GameDifficulty", 5);

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(_defaults)
          .ContinueWithOnMainThread(task =>
          {
              Debug.Log("Remote Config Default Set");
          });

        FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += OnRemoteConfigUpdated;
    }

    private void OnRemoteConfigUpdated(object sender, ConfigUpdateEventArgs args)
    {
        Debug.Log("<color=yellow>Remote Config Updated!</color>");
        // Handle real-time Remote Config events.
        if (args.Error != RemoteConfigError.None)
        {
            Debug.Log(String.Format("Error occurred while listening: {0}", args.Error));
            return;
        }

        Debug.Log("Updated keys: " + string.Join(", ", args.UpdatedKeys));
        // Activate all fetched values and then display a welcome message.
        FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(
          task =>
          {
              Debug.Log("Remote Config Activated");
          });
    }

    public async Task<ConfigValue> GetRemoteConfigValue(string id)
    {
        Debug.Log($"Fetching Remote Config Value for: {id}");
        await FetchDataAsync();
        Debug.Log("Data fetched and Activated");
        ConfigValue val = FirebaseRemoteConfig.DefaultInstance.GetValue(id);
        return val;
    }

    // Start a fetch request.
    // FetchAsync only fetches new data if the current data is older than the provided
    // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
    // By default the timespan is 12 hours, and for production apps, this is a good
    // number. For this example though, it's set to a timespan of zero, so that
    // changes in the console will always show up immediately.
    public Task FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }


    private void FetchComplete(Task fetchTask)
    {
        if (!fetchTask.IsCompleted)
        {
            Debug.LogError("Retrieval hasn't finished.");
            return;
        }

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;
        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
            return;
        }

        // Fetch successful. Parameter values must be activated to use.
        remoteConfig.ActivateAsync()
          .ContinueWithOnMainThread(
            task =>
            {
                Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");
            });
    }
}
