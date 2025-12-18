using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using UnityEngine;

public class PrizeController : MonoBehaviour
{
    public int scoreValue = 10;
    public string prizeID = "prize_default";
    public string prizeName = "Default Prize";

    // Static cache for remote config values
    private static Dictionary<string, int> _cachedScoreValues = new Dictionary<string, int>();
    private static bool _isConfigFetched = false;
    private static bool _isFetching = false;
    private static List<PrizeController> _waitingPrizes = new List<PrizeController>();

    void Start()
    {
        _ = InitializePrizeAsync();
    }

    private async Task InitializePrizeAsync()
    {
        string keyToRequest = DetermineKey(prizeID, gameObject.name);

        // If config already fetched, use cached value immediately
        if (_isConfigFetched)
        {
            ApplyCachedScore(keyToRequest);
            ApplyScaleAndPhysics();
            return;
        }

        // If another prize is already fetching, wait for it
        if (_isFetching)
        {
            _waitingPrizes.Add(this);
            await WaitForConfigFetch();
            ApplyCachedScore(keyToRequest);
            ApplyScaleAndPhysics();
            return;
        }

        // This prize will fetch for everyone
        _isFetching = true;
        _waitingPrizes.Add(this);

        await FetchAllPrizeScores();

        _isConfigFetched = true;
        _isFetching = false;

        // Apply scores to all waiting prizes
        foreach (var prize in _waitingPrizes)
        {
            if (prize != null)
            {
                string key = DetermineKey(prize.prizeID, prize.gameObject.name);
                prize.ApplyCachedScore(key);
                prize.ApplyScaleAndPhysics();
            }
        }
        _waitingPrizes.Clear();
    }

    private async Task FetchAllPrizeScores()
    {
        var firebaseManager = FirebaseManager.Instance;

        if (firebaseManager != null)
        {
            // Fetch all prize scores at once
            string[] prizeKeys = { "Prize01", "Prize02", "Prize03", "Prize04" };

            foreach (string key in prizeKeys)
            {
                try
                {
                    ConfigValue remoteVal = await firebaseManager.GetRemoteConfigValue(key);
                    int remoteScore = ParseConfigValue(remoteVal, 0);
                    if (remoteScore > 0)
                    {
                        _cachedScoreValues[key] = remoteScore;
                        Debug.Log($"Cached remote score for '{key}': {remoteScore}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to get remote config for '{key}': {ex.Message}");
                }
            }
        }
        else if (ConfigManager.Instance != null)
        {
            // Fallback to ConfigManager
            string[] prizeKeys = { "Prize01", "Prize02", "Prize03", "Prize04" };

            foreach (string key in prizeKeys)
            {
                int configScore = ConfigManager.Instance.GetInt(key, 0);
                if (configScore > 0)
                {
                    _cachedScoreValues[key] = configScore;
                    Debug.Log($"Cached config score for '{key}': {configScore}");
                }
            }
        }
    }

    private async Task WaitForConfigFetch()
    {
        // Wait until config is fetched
        while (_isFetching && !_isConfigFetched)
        {
            await Task.Delay(50);
        }
    }

    private void ApplyCachedScore(string key)
    {
        if (_cachedScoreValues.TryGetValue(key, out int cachedScore))
        {
            scoreValue = cachedScore;
            Debug.Log($"Prize '{gameObject.name}' using cached score: {scoreValue}");
        }
        else
        {
            Debug.Log($"Prize '{gameObject.name}' using default score: {scoreValue}");
        }
    }

    private void ApplyScaleAndPhysics()
    {
        float scaleFactor = 1.0f;
        if (scoreValue <= 10) scaleFactor = 1.2f;
        else if (scoreValue <= 20) scaleFactor = 1.0f;
        else if (scoreValue <= 50) scaleFactor = 0.8f;
        else scaleFactor = 0.6f;

        transform.localScale = transform.localScale * scaleFactor;

        Rigidbody rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    static string DetermineKey(string prizeId, string objectName)
    {
        if (!string.IsNullOrEmpty(prizeId) && prizeId != "prize_default")
            return prizeId;

        for (int i = 1; i <= 4; i++)
        {
            string p1 = $"Prize0{i}";
            string p2 = $"Prize{i}";
            if (objectName.IndexOf(p1, StringComparison.OrdinalIgnoreCase) >= 0) return p1;
            if (objectName.IndexOf(p2, StringComparison.OrdinalIgnoreCase) >= 0) return p2;
        }

        return "Prize01";
    }

    static int ParseConfigValue(ConfigValue cfgVal, int fallback)
    {
        if (cfgVal.Equals(default(ConfigValue)))
            return fallback;

        var s = cfgVal.StringValue;
        if (!string.IsNullOrEmpty(s) && int.TryParse(s, out var parsed))
            return parsed;

        try
        {
            long l = cfgVal.LongValue;
            if (l > int.MaxValue) return int.MaxValue;
            if (l < int.MinValue) return int.MinValue;
            return (int)l;
        }
        catch
        {
            // ignore parsing errors
        }

        return fallback;
    }

    void Update()
    {
        // Physics handles movement now
    }

    // Optional: Clear cache when scene reloads
    void OnDestroy()
    {
        _waitingPrizes.Remove(this);
    }

    // Optional: Public method to manually clear cache if needed
    public static void ClearCache()
    {
        _cachedScoreValues.Clear();
        _isConfigFetched = false;
        _isFetching = false;
        _waitingPrizes.Clear();
        Debug.Log("Prize config cache cleared");
    }
}