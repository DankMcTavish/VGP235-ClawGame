using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }
    [Header("Collected Items")]
    public GameObject[] collectedItems;

    [Header("Collection Settings")]
    public int maxCollectionSize = 10;



    void Start()
    {
        // Initialize collected items array
        collectedItems = new GameObject[0];

        if (collectedItems.Length == 0)
        {
            Debug.Log("No items collected yet.");
        }
        else
        {
            DisplayCollectibles(collectedItems);
        }
    }


    void DisplayCollectibles(GameObject[] collection)
    {
        foreach (GameObject item in collection)
        {
            Debug.Log("Collected Item: " + item.name);
        }
    }
    void AddCollectible(GameObject item)
    {
        int newSize = collectedItems.Length + 1;
        GameObject[] updatedItems = new GameObject[newSize];
        for (int i = 0; i < collectedItems.Length; i++)
        {
            updatedItems[i] = collectedItems[i];
        }
        updatedItems[newSize - 1] = item;
        collectedItems = updatedItems;

    }

    void RemoveCollectible(GameObject item)
    {
        // Example method to remove a collectible item
        int index = System.Array.IndexOf(collectedItems, item);
        if (index >= 0)
        {
            int newSize = collectedItems.Length - 1;
            GameObject[] updatedItems = new GameObject[newSize];
            for (int i = 0, j = 0; i < collectedItems.Length; i++)
            {
                if (i != index)
                {
                    updatedItems[j++] = collectedItems[i];
                }
            }
            collectedItems = updatedItems;
            Destroy(item);
        }
    }

    void ClearCollectibles()
    {
        foreach (GameObject item in collectedItems)
        {
            Destroy(item);
        }
        collectedItems = new GameObject[0];
    }
}
