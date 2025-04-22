using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContainerSlot : MonoBehaviour
{
    [Header("Slot UI")]
    public TMP_Text coordinatesText;
    public Image slotImage;
    public TMP_Text amountText;
    public Sprite defaultSprite;
    public int selectedSlotOpacity = 130; // Opacity of the selected slot
    
    [Header("Slot Content")]
    private Vector2Int coordinates;
    [SerializeField]
    private int itemID = -1;
    private int amount = 0;
    public int ItemID => itemID; // Property to get the item ID
    public int Amount => amount; // Property to get the amount of items in the slot
    private bool isEmpty = true;
    public bool IsEmpty => isEmpty; // Property to check if the slot is empty
    public int linkedContainerIndex = -1; // Index of the container this slot belongs to

    private Button slotButton;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        slotButton = GetComponent<Button>();
        slotButton.onClick.AddListener(OnClick);
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Sets the coordinates of the slot.
    /// /// This method is called to initialize the slot's position in the inventory grid.
    /// The coordinates are represented as a Vector2Int, where x and y represent the position in the grid.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="containerIndex"></param>
    public void SetCoordinates(int x, int y, int containerIndex = -1)
    {
        linkedContainerIndex = containerIndex; // Set the linked container index
        coordinates.x = x;
        coordinates.y = y;
        coordinatesText.text = $"{x}, {y}";
    }

    /// <summary>
    /// Updates the visual representation of the slot based on the item ID, amount, and empty state.
    /// This method is called to refresh the slot's appearance when the item ID, amount, or empty state changes.
    /// </summary>
    /// <param name="newItemID"></param>
    /// <param name="newAmount"></param>
    /// <param name="newIsEmpty"></param>
    public void UpdateSlotVisual(int newItemID, int newAmount, bool newIsEmpty)
    {
        itemID = newItemID;
        amount = newAmount;
        isEmpty = newIsEmpty;

        if (isEmpty)
        {
            slotImage.enabled = false;
            slotImage.sprite = defaultSprite; // Remplace par l'icône par défaut
            amountText.text = "";
        }
        else
        {
            slotImage.enabled = true;
            slotImage.sprite = GetItemIcon(newItemID);
            amountText.text = newAmount.ToString();
        }
    }
    /// <summary>
    /// Handles the click event on the slot.
    /// This method is called when the player clicks on the slot in the inventory UI.
    /// </summary>
    public void OnClick()
    {
        Debug.Log($"Slot clicked: {coordinates.x}, {coordinates.y}, itemID: {itemID}, amount: {amount}");
        // LocalReferences.instance.inventoryManager.AskDropItem(linkedContainerIndex, coordinates, amount);
        LocalReferences.instance.inventoryViewer.SelectSlot(coordinates, linkedContainerIndex); // Call the SelectSlot method in InventoryViewer
    }

    private Sprite GetItemIcon(int itemID)
    {
        ItemSO itemSO = DatabaseReader.instance.GetItem(itemID);
        if (itemSO != null)
        {
            return itemSO.icon; // Remplace par l'icône de l'item
        }
        return defaultSprite; // Remplace par l'icône correspondante
    }

    /// <summary>
    /// Gets the coordinates of the slot.
    /// This method is called to retrieve the position of the slot in the inventory grid.
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetCoordinates()
    {
        return coordinates;
    }

    /// <summary>
    /// Updates the visual representation of the slot.
    /// This method is called to update the visual representation of the slot.
    /// </summary>
    /// <param name="newItemID"></param>
    /// <param name="newAmount"></param>
    /// <param name="newIsEmpty"></param>
    public void SetSlotState(int newItemID, int newAmount, bool newIsEmpty)
    {
        itemID = newItemID;
        amount = newAmount;
        isEmpty = newIsEmpty;
        UpdateSlotVisual(itemID, amount, isEmpty);
    }

    /// <summary>
    /// Selects or deselects the slot in the inventory UI.
    /// /// This method is called to highlight the selected slot with a ghost image.
    /// If the slot is selected, it sets the opacity to selectedSlotOpacity.
    /// </summary>
    /// <param name="isSelected"></param>
    public void SelectSlot(bool isSelected)
    {
        if (isSelected)
        {
            // Set all elements opacity to selectedSlotOpacity
            canvasGroup.alpha = selectedSlotOpacity / 255f; // Set the alpha value to selectedSlotOpacity
        }else{
            canvasGroup.alpha = 1f; // Reset the alpha value to 1 (fully opaque)
        }
    }
}