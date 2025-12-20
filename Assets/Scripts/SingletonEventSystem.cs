using UnityEngine;
using UnityEngine.EventSystems;

public class SingletonEventSystem : MonoBehaviour
{
    private static SingletonEventSystem _instance;
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // Another Global EventSystem already exists.
            // We (the duplicate from a new scene) should be destroyed.
            Destroy(gameObject);
            return;
        }

        // We are the one true EventSystem
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
