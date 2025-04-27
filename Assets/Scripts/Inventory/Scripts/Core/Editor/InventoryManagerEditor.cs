#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerEditor : Editor {

    static int itemID = 1;
    static int amount = 1;

    public override void OnInspectorGUI() {
        DrawDefaultInspector(); // Show default inspector

        InventoryManager inventoryManager = (InventoryManager)target;
        if (!Application.isPlaying) {
            EditorGUILayout.HelpBox("SyncList<InventoryData> only visible in PlayMode", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📦 Inventory Sync", EditorStyles.boldLabel);

        if (inventoryManager.inventoryData.Count == 0) {
            EditorGUILayout.HelpBox("No containers for this inventory.", MessageType.Warning);
        } else {
            foreach (var container in inventoryManager.inventoryData) {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField($"🗃️ {container.containerName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("LinkedItemID : ", container.LinkedItemID.ToString());
                EditorGUILayout.LabelField("Type : ", container.containerName);
                EditorGUILayout.LabelField("Size : ", $"{container.size.x} x {container.size.y}");
                EditorGUILayout.LabelField("Slots : ", container.slotsData.Length.ToString());
                EditorGUILayout.EndVertical();
                // list all slots with their coordinates, their itemID and amount
                foreach (var slot in container.slotsData) {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    EditorGUILayout.LabelField($"Slot {slot.coordinates.x}, {slot.coordinates.y} : ", slot.itemID.ToString());
                    EditorGUILayout.LabelField("Amount : ", slot.amount.ToString());
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("IsEmpty : ", slot.isEmpty ? "Empty" : "Occupied");
                    // Button to remove item from the slot
                    if (GUILayout.Button("Drop item")) {
                        inventoryManager.AskDropItem(container.containerIndex, slot.coordinates, slot.amount);
                    }
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📦 Give Item", EditorStyles.boldLabel);
        // Let user choose an itemID to add to the first container
        itemID = EditorGUILayout.IntField("Item ID to add", itemID);
        amount = EditorGUILayout.IntField("Amount to add", amount);
        // Button to add item to the first container
        if (GUILayout.Button("Give item")) {
            if (inventoryManager.inventoryData.Count > 0) {
                inventoryManager.RequestAddItem(0, itemID, amount); // Add item to the first container
            } else {
                Debug.LogWarning("No containers available to add items.");
            }
        }
        EditorGUILayout.LabelField("Context", inventoryManager.isServer ? "🟢 Server" : "🔵 Client");
        // add a button to repaint the editor gui
        if (GUILayout.Button("Refresh")) {
            Repaint();
        }
    }
}
#endif