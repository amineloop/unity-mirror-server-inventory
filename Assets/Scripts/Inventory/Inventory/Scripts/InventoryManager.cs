using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class InventoryManager : NetworkBehaviour {
    [Header("Inventory Settings")]
    public InventoryViewer inventoryViewer;

    [Header("Inventory Sync")]
    public float inventorySyncInterval = 300f;
    private float lastSyncTime;

    [Header("Inventory Data")]
    public SyncList<InventoryData> inventoryData = new SyncList<InventoryData>();

    void Awake() {
        inventoryViewer = GetComponent<InventoryViewer>();
    }

    void Start() {
        if (isOwned) {
            CmdRequestAddContainer(netIdentity, new Vector2Int(4, 2), "Pockets", -1);
        }
    }

    void Update() {
        if (!isOwned) return;

        if (isServer && Time.time - lastSyncTime > inventorySyncInterval) {
            SyncAllContainers(netIdentity);
            lastSyncTime = Time.time;
        }
        // DEBUG, DELETE LATER
        if (Input.GetKeyDown(KeyCode.C)) {
            CmdRequestAddContainer(netIdentity, new Vector2Int(4, 3), "Test", 1);
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            CmdRequestRemoveContainer("Test");
        }
        // DEBUG, DELETE LATER
    }

    #region Container Management
    /// <summary>
    /// Request the server to add a new container to the player's inventory.
    /// This method is called when the player presses the 'C' key.
    /// </summary>
    /// <param name="targetPlayer"></param>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="linkedItemID"></param>
    [Command]
    public void CmdRequestAddContainer(NetworkIdentity targetPlayer, Vector2Int size, string type, int linkedItemID) {
        if (size.x <= 0 || size.y <= 0) return;
        AddNewContainer(targetPlayer, size, type, linkedItemID);
    }

    [Server]
    public void AddNewContainer(NetworkIdentity targetPlayer, Vector2Int size, string type, int linkedItemID = -1) {
        InventoryData container = new InventoryData {
            containerType = type,
            containerName = "Default",
            size = size,
            LinkedItemID = linkedItemID,
            containerIndex = inventoryData.Count,
            slotsData = new SlotsData[size.x * size.y]
        };

        for (int i = 0; i < container.slotsData.Length; i++) {
            container.slotsData[i] = new SlotsData {
                coordinates = new Vector2Int(i % size.x, i / size.x),
                isEmpty = true
            };
        }

        inventoryData.Add(container);
        TargetAddContainer(targetPlayer.connectionToClient, container);
    }

    [TargetRpc]
    void TargetAddContainer(NetworkConnection target, InventoryData container) {
        inventoryViewer.AddNewContainer(container.size, container.containerType, container.containerName, container.containerIndex);
    }

    [Command]
    public void CmdRequestRemoveContainer(string containerType) {
        RemoveContainer(containerType);
    }

    [Server]
    void RemoveContainer(string containerType) {
        var container = inventoryData.Find(c => c.containerType == containerType);
        if (container != null) {
            inventoryData.Remove(container);
            RpcRemoveContainer(containerType);
        }
    }

    [ClientRpc]
    void RpcRemoveContainer(string containerType) {
        inventoryViewer.RemoveContainer(containerType);
    }

    #endregion

    #region Item Handling

    /// <summary>
    /// Request the server to add an item to a specific container.
    /// /// This method is called when the player wants to add an item to their inventory.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    [Server]
    public void RequestAddItem(int containerIndex, int itemID, int amount) {
        if (!ValidateContainerIndex(containerIndex)) return;

        InventoryData container = inventoryData[containerIndex];
        ItemSO itemSO = DatabaseReader.instance.GetItem(itemID);
        if(itemSO == null) return;

        if (!ThereIsSpaceInContainer(containerIndex, itemID, amount)) {
            containerIndex = FindContainerWithSpace(itemID, amount);
            if (containerIndex == -1) return;
        }

        if (itemSO.stackable) {
            foreach (var slot in container.slotsData) {
                if (!slot.isEmpty && slot.itemID == itemID && slot.amount < itemSO.maxStackSize) {
                    int spaceLeft = itemSO.maxStackSize - slot.amount;
                    int added = Mathf.Min(spaceLeft, amount);
                    slot.amount += added;
                    amount -= added;

                    RpcUpdateSlot(containerIndex, slot.coordinates, itemID, slot.amount, false);
                    if (amount <= 0) return;
                }
            }
        }

        foreach (var slot in container.slotsData) {
            if (slot.isEmpty) {
                slot.itemID = itemID;
                slot.amount = Mathf.Min(amount, itemSO.maxStackSize);
                slot.isEmpty = false;

                RpcUpdateSlot(containerIndex, slot.coordinates, itemID, slot.amount, false);
                return;
            }
        }
    }

    /// <summary>
    /// Request the server to add an item to a specific container.
    /// /// This method is called when the player wants to add an item to their inventory.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="coord"></param>
    /// <param name="amount"></param>
    [Server]
    public void DropItem(int containerIndex, Vector2Int coord, int amount) {
        if (!ValidateContainerIndex(containerIndex)) return;

        var container = inventoryData[containerIndex];
        int index = coord.x + coord.y * container.size.x;
        var slot = container.slotsData[index];

        if (slot.isEmpty || slot.amount < amount) return;

        slot.amount -= amount;
        if (slot.amount <= 0) {
            slot.itemID = -1;
            slot.isEmpty = true;
        }

        RpcUpdateSlot(containerIndex, coord, slot.itemID, slot.amount, slot.isEmpty);
    }

    /// <summary>
    /// Request the server to drop an item from a specific container.
    /// /// This method is called when the player wants to drop an item from their inventory.
    /// This method is called by the client when the player wants to drop an item.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="coord"></param>
    /// <param name="amount"></param>
    [Command]
    public void CmdRequestDropItem(int containerIndex, Vector2Int coord, int amount) {
        DropItem(containerIndex, coord, amount);
    }

    /// <summary>
    /// Request the server to drop an item from a specific container.
    /// This method is called when the player wants to drop an item from their inventory.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="coord"></param>
    /// <param name="amount"></param>
    public void AskDropItem(int containerIndex, Vector2Int coord, int amount) {
        if (!isOwned) return;
        CmdRequestDropItem(containerIndex, coord, amount);
    }

    #endregion

    #region Sync Slot Updates
    /// <summary>
    /// Update the visual representation of a slot in a container.
    /// This method is called to update the visual representation of a slot in a container.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="coordinates"></param>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    /// <param name="isEmpty"></param>
    [ClientRpc]
    void RpcUpdateSlot(int containerIndex, Vector2Int coordinates, int itemID, int amount, bool isEmpty) {
        if (isServer) return;
        inventoryViewer.UpdateSlot(containerIndex, coordinates, itemID, amount, isEmpty);
    }

    /// <summary>
    /// Sync all containers to the client.
    /// /// This method is called to sync all containers to the client when the player connects.
    /// </summary>
    /// <param name="netId"></param>
    [Server]
    void SyncAllContainers(NetworkIdentity netId) {
        foreach (var container in inventoryData) {
            TargetAddContainer(netId.connectionToClient, container);
            foreach (var slot in container.slotsData) {
                RpcUpdateSlot(container.containerIndex, slot.coordinates, slot.itemID, slot.amount, slot.isEmpty);
            }
        }
    }

    #endregion

        #region MoveItem
    /// <summary>
    /// Ask the server to move an item from one container to another.
    /// This method is called by the client when the player wants to move an item.
    /// </summary>
    /// <param name="fromContainerIndex"></param>
    /// <param name="fromCoordinates"></param>
    /// <param name="toContainerIndex"></param>
    /// <param name="toCoordinates"></param>
    /// <param name="amount"></param>
    public void AskMoveItem(int fromContainerIndex, Vector2Int fromCoordinates, int toContainerIndex, Vector2Int toCoordinates, int amount)
    {
        if (!isOwned) return;
        CmdRequestMoveItem(fromContainerIndex, fromCoordinates, toContainerIndex, toCoordinates, amount);
    }

    /// <summary>
    /// Request the server to move an item from one container to another.
    /// This method is called by the client when the player wants to move an item.
    /// </summary>
    /// <param name="fromContainerIndex"></param>
    /// <param name="fromCoordinates"></param>
    /// <param name="toContainerIndex"></param>
    /// <param name="toCoordinates"></param>
    /// <param name="amount"></param>
    [Command]
    public void CmdRequestMoveItem(int fromContainerIndex, Vector2Int fromCoordinates, int toContainerIndex, Vector2Int toCoordinates, int amount)
    {
        if (!isServer) return;
        MoveItem(fromContainerIndex, fromCoordinates, toContainerIndex, toCoordinates, amount);
    }

    /// <summary>
    /// Move an item from one container to another.
    /// This method is called by the server when a player wants to move an item.
    /// </summary>
    /// <param name="fromContainerIndex"></param>
    /// <param name="fromCoordinates"></param>
    /// <param name="toContainerIndex"></param>
    /// <param name="toCoordinates"></param>
    /// <param name="amount"></param>
    [Server]
    public void MoveItem(int fromContainerIndex, Vector2Int fromCoordinates, int toContainerIndex, Vector2Int toCoordinates, int amount)
    {
        if (fromContainerIndex < 0 || fromContainerIndex >= inventoryData.Count || toContainerIndex < 0 || toContainerIndex >= inventoryData.Count) return;

        InventoryData fromContainer = inventoryData[fromContainerIndex];
        InventoryData toContainer = inventoryData[toContainerIndex];

        SlotsData fromSlot = fromContainer.slotsData[fromCoordinates.x + fromCoordinates.y * fromContainer.size.x];
        SlotsData toSlot = toContainer.slotsData[toCoordinates.x + toCoordinates.y * toContainer.size.x];

        if (fromSlot.isEmpty || fromSlot.amount < amount)
        {
            Debug.LogWarning($"SERVER: Cannot move item. Source slot is empty or amount is insufficient.");
            return;
        }

        ItemSO itemData = DatabaseReader.instance.GetItem(fromSlot.itemID);

        // 1️⃣ - Si destination vide -> déplacer l’item
        if (toSlot.isEmpty)
        {
            toSlot.itemID = fromSlot.itemID;
            toSlot.amount = amount;
            toSlot.isEmpty = false;
            fromSlot.amount -= amount;

            if (fromSlot.amount <= 0)
            {
                fromSlot.isEmpty = true;
                fromSlot.itemID = -1; // Reset du slot source
            }
        }
        // 2️⃣ - Si item stackable et même ID -> stacker
        else if (toSlot.itemID == fromSlot.itemID && itemData.stackable)
        {
            int stackSpace = itemData.maxStackSize - toSlot.amount;
            int amountToMove = Mathf.Min(stackSpace, amount);

            toSlot.amount += amountToMove;
            fromSlot.amount -= amountToMove;

            if (fromSlot.amount <= 0)
            {
                fromSlot.isEmpty = true;
                fromSlot.itemID = -1;
            }
        }
        else
        {
            Debug.LogWarning($"SERVER: Cannot move item {fromSlot.itemID}. Destination slot occupied by different item.");
            return;
        }

        // Mettre à jour les clients
        RpcUpdateSlot(fromContainerIndex, fromCoordinates, fromSlot.itemID, fromSlot.amount, fromSlot.isEmpty);
        RpcUpdateSlot(toContainerIndex, toCoordinates, toSlot.itemID, toSlot.amount, false);
    }
    #endregion

    #region Helpers

    /// <summary>
    /// Validates the index of a container.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    bool ValidateContainerIndex(int index) => index >= 0 && index < inventoryData.Count;

    /// <summary>
    /// Checks if there is space in a specific container for a given item and amount.
    /// This method is called to check if there is space in a container for a specific item.
    /// </summary>
    /// <param name="containerIndex"></param>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    bool ThereIsSpaceInContainer(int containerIndex, int itemID, int amount) {
        if (!ValidateContainerIndex(containerIndex)) return false;

        var container = inventoryData[containerIndex];
        ItemSO itemSO = DatabaseReader.instance.GetItem(itemID);

        if (itemSO.stackable) {
            foreach (var slot in container.slotsData) {
                if (!slot.isEmpty && slot.itemID == itemID && slot.amount < itemSO.maxStackSize) {
                    int spaceLeft = itemSO.maxStackSize - slot.amount;
                    amount -= Mathf.Min(spaceLeft, amount);
                    if (amount <= 0) return true;
                }
            }
        }

        foreach (var slot in container.slotsData)
            if (slot.isEmpty) return true;

        return false;
    }

    /// <summary>
    /// Finds a container with space for a given item and amount.
    /// This method is called to find a container with space for a specific item.
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    int FindContainerWithSpace(int itemID, int amount) {
        for (int i = 0; i < inventoryData.Count; i++)
            if (ThereIsSpaceInContainer(i, itemID, amount)) return i;

        return -1;
    }

    #endregion
}