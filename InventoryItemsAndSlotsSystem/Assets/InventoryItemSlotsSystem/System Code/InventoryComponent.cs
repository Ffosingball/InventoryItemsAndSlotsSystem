using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    [SerializeField] private bool isInventoryInfinite = false;
    [SerializeField] private int inventoryWidth=10;
    [SerializeField] private int inventoryHeight=5;
    [Header("How many cells of the inventory is visible in a single view, if height is more than visible height then you will need to scroll an inventory")]
    [SerializeField] private int visibleHeight=5;
    [SerializeField] private string inventoryName;
}
