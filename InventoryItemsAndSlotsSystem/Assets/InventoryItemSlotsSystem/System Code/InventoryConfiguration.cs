using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfiguration", menuName = "Scriptable Objects/InventoryConfiguration")]
public class InventoryConfiguration : ScriptableObject
{
    public bool useFiveTierRareness=false;
    [Tooltip("If -1, then unlimited amount of stacks")]
    //How many stacks of the same item can have in the inventory
    public int itemStacksLimit = -1;
    public InventoryType inventoryType = InventoryType.SingleCelled;
    public bool arbitraryStackPlacement = true;
    public bool itemsHasAWeight = false;
}
