using UnityEngine;

[CreateAssetMenu(fileName = "ContainerGrid", menuName = "Inventory/ContainerGrid")]
public class ContainerSO : ScriptableObject
{
    public ItemSO linkedItem;
    public Vector2Int size = new Vector2Int(1, 1); // Size of the container in grid slots (width, height)
    public bool baseContainer = false;
}