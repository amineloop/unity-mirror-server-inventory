using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Container {
    public string containerType;
    public Vector2Int size;
    public string containerName;
    public int containerIndex = -1; // Index du conteneur dans l'inventaire
    public ContainerGrid containerGrid;
}

public class InventoryViewer : NetworkBehaviour {
    private List<Container> containerTypes = new List<Container>();
    public GameObject inventoryUI;
    public GameObject containerPrefab;
    public KeyCode toggleKey = KeyCode.I;

    [Header("Selection")]
    private ContainerSlot selectedSlot;
    public Transform selectedSlotGhost;
    public Vector2Int selectedSlotCoordinates;
    public int selectedContainerIndex = -1;
    private bool isDragging = false;

    void Update() {
        if (isLocalPlayer) {
            LocalPlayerActions();
        }
    }

    /// <summary>
    /// Handles local player actions such as toggling the inventory and dragging items.
    /// This method is called every frame to check for player input.
    /// </summary>
    void LocalPlayerActions() {
        if (Input.GetKeyDown(toggleKey)) {
            ToggleInventory();
        }

        if (isDragging && selectedSlotGhost != null) {
            selectedSlotGhost.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Toggles the visibility of the inventory UI.
    /// This method is called when the player presses the toggle key.
    /// It shows or hides the inventory UI and all container grids.
    /// </summary>
    void ToggleInventory() {
        bool state = !inventoryUI.activeSelf;
        inventoryUI.SetActive(state);

        foreach (var container in containerTypes) {
            container.containerGrid.gameObject.SetActive(state);
        }
    }

    /// <summary>
    /// Adds a new container to the inventory UI.
    /// This method is called by the server to create a new container for the player.
    /// The container is instantiated from a prefab and added to the inventory UI.
    /// </summary>
    /// <param name="_size"></param>
    /// <param name="_containerType"></param>
    /// <param name="_containerName"></param>
    /// <param name="_containerIndex"></param>
    public void AddNewContainer(Vector2Int _size, string _containerType, string _containerName, int _containerIndex) {
        GameObject newContainer = Instantiate(containerPrefab, inventoryUI.transform);
        ContainerGrid containerGrid = newContainer.GetComponent<ContainerGrid>();
        containerGrid.containerType = _containerType;
        containerGrid.containerIndex = _containerIndex;
        containerGrid.SetSize(_size.x, _size.y);

        containerTypes.Add(new Container {
            containerType = _containerType,
            size = _size,
            containerName = _containerName,
            containerIndex = _containerIndex,
            containerGrid = containerGrid
        });
    }

    /// <summary>
    /// Removes a container from the inventory UI.
    /// This method is called by the server to remove a container from the player's inventory.
    /// The container is destroyed and removed from the list of containers.
    /// </summary>
    /// <param name="containerType"></param>
    public void RemoveContainer(string containerType) {
        Container containerToRemove = containerTypes.Find(c => c.containerType == containerType);
        if (containerToRemove != null) {
            containerTypes.Remove(containerToRemove);
            Destroy(containerToRemove.containerGrid.gameObject);
        }
    }

    /// <summary>
    /// Updates the visual representation of a slot in a container.
    /// This method is called by the server to update the slot's item ID, amount, and empty state.
    /// The slot is identified by its coordinates and the container index.
    /// The slot's visual representation is updated accordingly.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="coordinates"></param>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    /// <param name="isEmpty"></param>
    public void UpdateSlot(int containerIndex, Vector2Int coordinates, int itemID, int amount, bool isEmpty) {
        if (containerIndex < 0 || containerIndex >= containerTypes.Count) return;

        ContainerGrid containerGrid = containerTypes[containerIndex].containerGrid;
        ContainerSlot slot = containerGrid.GetSlotAt(coordinates);
        if (slot != null) {
            slot.UpdateSlotVisual(itemID, amount, isEmpty);
        }
    }

    /// <summary>
    /// Selects a slot in the inventory UI for dragging or dropping items.
    /// This method is called when the player clicks on a slot in the inventory.
    /// If the slot is empty, it starts a drag operation. If the slot is already selected, it attempts to drop the item.
    /// The slot is identified by its coordinates and the container index.
    /// The selected slot is highlighted with a ghost image.
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="containerIndex"></param>
    public void SelectSlot(Vector2Int coordinates, int containerIndex) {
        if (containerIndex < 0 || containerIndex >= containerTypes.Count) return;

        ContainerGrid containerGrid = containerTypes[containerIndex].containerGrid;
        ContainerSlot clickedSlot = containerGrid.GetSlotAt(coordinates);

        if (clickedSlot == null || selectedSlot == null && clickedSlot.IsEmpty) {
            Debug.LogWarning("Tried to select invalid slot.");
            return;
        }

        if (!isDragging) {
            // Start drag
            selectedSlot = clickedSlot;
            selectedContainerIndex = containerIndex;
            selectedSlotCoordinates = coordinates;

            Image ghostImage = selectedSlotGhost.GetComponent<Image>();
            ghostImage.sprite = clickedSlot.slotImage.sprite;
            selectedSlotGhost.gameObject.SetActive(true);
            isDragging = true;
            selectedSlot.SelectSlot(true);
        } else {
            // Drop attempt
            if (clickedSlot == selectedSlot) {
                // Same slot or cancel
                selectedSlotGhost.gameObject.SetActive(false);
                selectedSlot.SelectSlot(false);
                isDragging = false;
                selectedSlot = null;
                return;
            }

            LocalReferences.instance.inventoryManager.AskMoveItem(
                selectedContainerIndex,
                selectedSlotCoordinates,
                containerIndex,
                coordinates,
                selectedSlot.Amount
            );

            selectedSlotGhost.gameObject.SetActive(false);
            selectedSlot.SelectSlot(false);
            isDragging = false;
            selectedSlot = null;
        }
    }

    /// <summary>
    /// Gets the slot at the specified coordinates in the specified container grid.
    /// </summary>
    /// <remarks>
    /// This method is used to get the slot at the specified coordinates in the specified container grid.
    /// </remarks>
    /// </summary>
    /// <param name="containerGrid"></param>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    private ContainerSlot GetSlot(ContainerGrid containerGrid, Vector2Int coordinates) {
        ContainerSlot slot = containerGrid.GetSlotAt(coordinates);
        if (slot == null) {
            Debug.LogError("Slot not found at coordinates: " + coordinates);
        }
        return slot;
    }
}