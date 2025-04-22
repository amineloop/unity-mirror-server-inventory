using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemSO> allItems = new List<ItemSO>();

    /// <summary>
    /// Gets a random item from the database.
    /// This method is used to retrieve a random item from the database for testing or gameplay purposes.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemSO GetItemByID(int id)
    {
        foreach (ItemSO item in allItems)
        {
            if (item.GetID() == id)
            {
                return item;
            }
        }

        int randomInBounds = Random.Range(0, allItems.Count);
        return allItems[randomInBounds];
    }

    /// <summary>
    /// Gets the item by ID.
    /// If the item doesn't exist, it logs a warning and returns null.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool ItemExists(int id)
    {
        foreach (ItemSO item in allItems)
        {
            if (item.GetID() == id)
            {
                return true;
            }
        }
        return false;
    }

#if UNITY_EDITOR
// 🔹 This method scans all ItemSO assets in the project and adds them to the allItems list.
    [ContextMenu("Scan All Items")]
    public void ScanAllItems()
    {
        allItems.Clear();
        Dictionary<int, ItemSO> idCheck = new Dictionary<int, ItemSO>();

        string[] guids = AssetDatabase.FindAssets("t:ItemSO");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemSO item = AssetDatabase.LoadAssetAtPath<ItemSO>(path);

            if (item != null)
            {
                allItems.Add(item);

                if (idCheck.ContainsKey(item.GetID()))
                {
                    Debug.LogError($"❌ Duplicate ID detected! ID: {item.GetID()} used by:\n" +
                                   $"- {idCheck[item.GetID()].name} ({AssetDatabase.GetAssetPath(idCheck[item.GetID()])})\n" +
                                   $"- {item.name} ({path})");
                }
                else
                {
                    idCheck[item.GetID()] = item;
                }
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"✅ Scanned {allItems.Count} items.");
    }
#endif
}