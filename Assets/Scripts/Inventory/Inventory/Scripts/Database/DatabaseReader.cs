using UnityEngine;

public class DatabaseReader : MonoBehaviour
{
    public static DatabaseReader instance;

    public ItemDatabase itemDatabase;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Gets the item prefab by ID.
    /// If the item doesn't exist, it logs a warning and returns null.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameObject GetItemPrefab(int id)
    {
        ItemSO item = itemDatabase.GetItemByID(id);
        if (item == null) return null;
        return item.prefab;
    }

    /// <summary>
    /// Gets the item by ID.
    /// If the item doesn't exist, it logs a warning and returns null.
    /// This method is used to retrieve the item data from the database based on its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemSO GetItem(int id)
    {
        if(itemDatabase.ItemExists(id))
        {
            return itemDatabase.GetItemByID(id);
        }
        else
        {
            Debug.LogWarning($"Item with ID {id} does not exist in the database.");
            return null;
        }
    }

    /// <summary>
    /// Gets a random item from the database.
    /// This method is used to retrieve a random item from the database for testing or gameplay purposes.
    /// </summary>
    /// <returns></returns>
    public ItemSO GetRandomItem()
    {
        return itemDatabase.GetItemByID(Random.Range(0, itemDatabase.allItems.Count));
    }
}
