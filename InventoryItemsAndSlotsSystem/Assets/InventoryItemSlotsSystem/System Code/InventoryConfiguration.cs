using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfiguration", menuName = "Scriptable Objects/InventoryConfiguration")]
public class InventoryConfiguration : ScriptableObject
{
    public bool useFiveTierRareness=false;
    [Tooltip("If -1, then unlimited amount of stacks")]
    public int itemStacksLimit = -1;
    public bool onlySingleCelledItems=true;
    public bool allLargerItemsAreQuadrilateral = true;
    public bool itemsHasAWeight = false;
}
