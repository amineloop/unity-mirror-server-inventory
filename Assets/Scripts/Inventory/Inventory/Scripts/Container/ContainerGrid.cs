using TMPro;
using UnityEngine;

public class ContainerGrid : MonoBehaviour
{
    public ContainerSlot slotPrefab;
    public RectTransform slotContainer;
    public RectTransform labelBackground; // Background for the label
    public TMP_Text labelText; // Label for the container
    private Vector2Int size;
    private ContainerSlot[] slots;
    public int containerIndex = -1; // Index of the container in the inventory
    public string containerType = "Pockets"; // Type of container
    
    public bool isDebug = false; // Set to true for debug mode

    private void Awake()
    {
        if (slotContainer == null)
        {
            Debug.LogError("Slot container RectTransform is not assigned.");
            return;
        }
        if(isDebug){
            SetSize(size.x, size.y); // Set coordinates based on the number of existing containers
            isDebug = false; // Reset debug mode after setting the item
        }
    }

    public void SetSize(int x, int y)
    {
        size.x = x;
        size.y = y;
        InitializeContainer(); // Initialize the container with the new size
        labelText.text = containerType; // Set the label text to the container type
    }
    

    private void InitializeContainer()
    {
        // this recTransfrom size is size based on the slot size + one time 25 (Label UI)
        GetComponent<RectTransform>().sizeDelta = new Vector2(size.x * 100, size.y * 100 + 25); // Adjust size based on the slot size
        slotContainer.sizeDelta = new Vector2(size.x * 100, size.y * 100); // Adjust size based on the slot size
        labelBackground.sizeDelta = new Vector2(size.x * 100, 25); // Adjust size based on the slot size
        size.x = size.x;
        size.y = size.y;
        slots = new ContainerSlot[size.x * size.y]; // Initialize the slots array based on the size

        for (int y = 0; y < size.y; ++y)
        {
            for (int x = 0; x < size.x; ++x)
            {
                ContainerSlot newSlot = Instantiate(slotPrefab, slotContainer.transform);
                newSlot.SetCoordinates(x, y);
                newSlot.linkedContainerIndex = containerIndex; // Set the linked container index
                slots[x + y * size.x] = newSlot; // Store the slot in the array
            }
        }
    }

    public void SetSlotState(Vector2Int coordinates, bool isEmpty)
    {
        int index = coordinates.x + coordinates.y * size.x; // Calculate the index based on coordinates
        if (index >= 0 && index < slots.Length)
        {
            slots[index].UpdateSlotVisual(-1, 0, isEmpty); // Update the slot visual based on the state
        }
        else
        {
            Debug.LogError("Invalid slot coordinates: " + coordinates);
        }
    }

    public ContainerSlot GetSlotAt(Vector2Int coordinates)
    {
        foreach (ContainerSlot slot in slots)
        {
            if (slot.GetCoordinates() == coordinates)
                return slot;
        }
        return null; // Return null if no matching slot is found
    }

}