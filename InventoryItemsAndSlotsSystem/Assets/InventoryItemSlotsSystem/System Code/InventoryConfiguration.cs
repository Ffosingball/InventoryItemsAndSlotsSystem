using UnityEngine;

[CreateAssetMenu(fileName = "InventoryConfiguration", menuName = "Scriptable Objects/InventoryConfiguration")]
public class InventoryConfiguration : ScriptableObject
{
    public bool useFiveTierRareness=false;
    [Tooltip("If -1, then unlimited amount of stacks")]
    //How many stacks of the same item can have in the inventory
    public bool itemStacksLimit = false;
    public InventoryType inventoryType = InventoryType.SingleCelled;
    public bool arbitraryStackPlacement = true;
    public bool itemsHasAWeight = false;
    public Color selectionOutlineColor = Color.white;
    public float selectionOutlineThikness = 4;
    public Color commonItemColor = new Color(0.5f, 0.5f, 0.5f);
    public Color uncommonItemColor = new Color(0.32f, 0.57f, 0.29f);
    public Color rareItemColor = new Color(0.21f, 0.43f, 0.8f);
    public Color epicItemColor = new Color(0.68f, 0.21f, 0.8f);
    public Color legendaryItemColor = new Color(0.86f, 0.61f, 0.17f);
    public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f);
}
