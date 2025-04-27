using System;
using UnityEngine;
using Mirror;

[Serializable]
public class SlotsData
{
    public Vector2Int coordinates;
    public int itemID = -1;
    public int amount = 0;
    public bool isEmpty = true;
}

// 🔹 SyncList to sync the slots with Mirror
public class SyncListSlotsData : SyncList<SlotsData> { }