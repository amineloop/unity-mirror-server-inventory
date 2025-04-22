#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemDatabase database = (ItemDatabase)target;
        if (GUILayout.Button("Scan All Items"))
        {
            database.ScanAllItems();
            EditorUtility.SetDirty(database);
        }

        if (GUILayout.Button("Reset IDs"))
        {
            PlayerPrefs.SetInt("LastItemID", 0); // Save the last ID to PlayerPrefs
        }
    }
}
#endif