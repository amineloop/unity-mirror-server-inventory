using UnityEngine;

public class ItemUI : MonoBehaviour
{
    private RectTransform rectTransform; // Reference to the RectTransform component
    public Vector2Int size; // Size of the item in grid slots
    public Vector2Int position; // Position of the item in the grid
    public int itemID = -1; // Unique identifier for the item
    private ItemSO item; // Reference to the ItemSO scriptable object

    public string containerType; // Type of the container this item belongs to
    private ContainerGrid grid; // Reference to the InventoryGrid component


    public void InitializeItem(Vector2Int _size, Vector2Int _position, int _itemID, string _containerType, ContainerGrid _grid){
        // set anchor and pivot to the top left corner
        item = DatabaseReader.instance.GetItem(_itemID); // Get the item from the database using the item ID
        size = _size; // Set the size of the item
        grid = _grid; // Set the grid reference
        rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
    }
}
