using Mirror;
using UnityEngine;

// This class is used to store references to local player components and manage their state
// you can create yours or use this one but in order to work, the inventory manager, viewer and the database reader must be part of a singleton pattern

public class LocalReferences : NetworkBehaviour
{
    public static LocalReferences instance { get; private set; }

    public InventoryManager inventoryManager;
    public InventoryViewer inventoryViewer;

    public DatabaseReader databaseReader;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        if(!isOwned) this.enabled = false; // Désactive le script si ce n'est pas le joueur local
        inventoryManager = GetComponent<InventoryManager>();
        inventoryViewer = GetComponent<InventoryViewer>();
        databaseReader = DatabaseReader.instance; // Assurez-vous que DatabaseReader est un singleton ou ajustez cette ligne en conséquence

        if (inventoryManager == null || inventoryViewer == null || databaseReader == null)
        {
            Debug.LogError("One or more components are missing in LocalReferences.");
        }
    }
}
