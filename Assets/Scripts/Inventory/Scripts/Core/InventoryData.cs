using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryData{
    public int LinkedItemID = -1;
    public string containerName;
    public int containerIndex = -1; // Index of the container in the inventory
    public Vector2Int size;
    
    public SlotsData[] slotsData;
}