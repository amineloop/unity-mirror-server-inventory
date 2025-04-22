using UnityEngine;

public static class ItemIDManager
{
    private const string ID_KEY = "LastItemID";

    public static int GetNextID()
    {
        int lastID = PlayerPrefs.GetInt(ID_KEY, 0); // Get the last ID from PlayerPrefs, default to 0 if not set
        int newID = lastID + 1; // Automatically increment the ID
        PlayerPrefs.SetInt(ID_KEY, newID); // Save the new ID to PlayerPrefs
        PlayerPrefs.Save(); // Apply the changes to PlayerPrefs
        return newID;
    }
}
