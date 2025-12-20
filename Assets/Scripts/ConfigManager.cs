using UnityEngine;
using System.Collections.Generic;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }

    private Dictionary<string, long> longValues = new Dictionary<string, long>();
    // You can add other types (bool, string, double) as needed.

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateConfigValues()
    {
        // Example: Fetching all keys is not directly exposed by simpler Firebase APIs easily 
        // without knowing keys, but here we assume the fetch happened in AppStart 
        // and we can query keys on demand or pre-cache commonly used ones if we knew them.
        // For now, we will rely on fetching directly from Firebase RemoteConfig singleton 
        // when requested, or caching them here if we want an abstraction layer.
        
        // This method can be used to signal that config has been fetched.
        Debug.Log("Config values updated from Remote Config.");
    }

    public long GetLong(string key, long defaultValue = 0)
    {
        // Wrapper around Firebase Remote Config to be safe
        try
        {
            if (Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Keys != null)
            {
                 var value = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                 if (value.Source != Firebase.RemoteConfig.ValueSource.StaticValue)
                 {
                     return value.LongValue;
                 }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error fetching remote config for {key}: {e.Message}");
        }
        return defaultValue;
    }

    // Helper for int since most game values are int
    public int GetInt(string key, int defaultValue = 0)
    {
        return (int)GetLong(key, defaultValue);
    }
}
