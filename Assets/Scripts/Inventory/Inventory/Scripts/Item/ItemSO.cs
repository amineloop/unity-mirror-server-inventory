using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    [SerializeField] private int id;
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public bool equipable = false; // If the item can be equipped
    public bool stackable = false; // If the item can be stacked in the inventory
    public int maxStackSize = 1;
    
    /// <summary>
    /// This method is called when the scriptable object is enabled.
    /// It assigns a unique ID to the item if it doesn't already have one.
    /// </summary>
    private void OnEnable()
    {
        if (id == 0) // If the item ID is not set, assign a new one
        {
            id = ItemIDManager.GetNextID();
        }
    }

    /// <summary>
    /// Gets the ID of the item.
    /// This method is used to retrieve the unique identifier of the item.
    /// </summary>
    /// <returns></returns>
    public int GetID()
    {
        return id;
    }

    /// <summary>
    /// Gets the name of the item.
    /// This method is used to retrieve the name of the item.
    /// </summary>
    /// <returns></returns>
    public bool IsEquippable()
    {
        return equipable;
    }

    /// <summary>
    /// Gets the name of the item.
    /// This method is used to retrieve the name of the item.
    /// </summary>
    /// <returns></returns>
    public bool IsStackable()
    {
        return stackable;
    }
}