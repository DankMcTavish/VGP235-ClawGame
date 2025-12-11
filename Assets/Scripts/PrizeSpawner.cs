using UnityEngine;
using System.Collections.Generic;

public class PrizeSpawner : MonoBehaviour
{
    [Header("Prize Settings")]
    public GameObject[] prizePrefabs;       // Different prize types
    public Transform[] spawnPoints;         // Where prizes can appear
    public int poolSizePerPrefab = 10;      // How many of each prize to preload
    public float spawnInterval = 5.0f;

    private float timer;
    private Dictionary<GameObject, Queue<GameObject>> prizePools;

    void Awake()
    {
        // Initialize pools
        prizePools = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (GameObject prefab in prizePrefabs)
        {
            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false); // Hide until needed
                pool.Enqueue(obj);
            }

            prizePools[prefab] = pool;
        }
    }

    [Header("Spawn Settings")]
    public int initialSpawnCount = 10;

    void Start()
    {
        timer = spawnInterval;
        
        // Spawn initial prizes
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnPrize();
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnPrize();
            timer = spawnInterval;
        }
    }

    void SpawnPrize()
    {
        if (prizePrefabs.Length == 0 || spawnPoints.Length == 0)
            return;

        int prizeIndex = Random.Range(0, prizePrefabs.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);

        GameObject prefab = prizePrefabs[prizeIndex];
        GameObject prize = GetPrizeFromPool(prefab);

        if (prize != null)
        {
            prize.transform.position = spawnPoints[spawnIndex].position;
            prize.transform.rotation = Quaternion.identity;
            prize.SetActive(true);
        }
    }

    GameObject GetPrizeFromPool(GameObject prefab)
    {
        if (!prizePools.ContainsKey(prefab)) return null;

        Queue<GameObject> pool = prizePools[prefab];

        // Reuse inactive object if available
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        // If all are active, optionally expand pool
        GameObject newObj = Instantiate(prefab);
        newObj.SetActive(false);
        pool.Enqueue(newObj);
        return newObj;
    }
}