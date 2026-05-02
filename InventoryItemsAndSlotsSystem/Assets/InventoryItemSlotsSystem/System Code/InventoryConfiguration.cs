using UnityEngine;


//This is configuration of the inventory. You can create many different
//configurations in the same game to simulate different things: such as usual inventory
//items slot grid (like in Factorio), a list of badges or cards (like in Hogwarts legacy)
//etc.
[CreateAssetMenu(fileName = "InventoryConfiguration", menuName = "Scriptable Objects/InventoryConfiguration")]
public class InventoryConfiguration : ScriptableObject
{
    [Header("InventoryComponent configuration")]
    //If true than same item can have different rareness tiers, and it will have different
    //background in the inventory if needed
    public bool useFiveTierRareness=false;
    //Should the items in the inventory have stack limits or not
    public bool itemStacksLimit = false;
    //What the type of the inventory this is
    public InventoryType inventoryType = InventoryType.SingleCelled;
    //Should player be able move items in the inventory or not
    public bool arbitraryStackPlacement = true;
    //Should there be any weight limits in the inventory or not
    public bool itemsHasAWeight = false;
    
    [Header("InventoryView configuration")]
    //How many slots should be visible before scroling down
    public int visibleSlotsInHeight = 5; 
    //Percentage of the pixel size of the screen
    public float marginBetweenSlots = 0.005f;
    //Percentage of the pixel size of the screen
    public float outerMargin = 0.001f;
    //Percentage of the pixel size of the screen
    public float slotSizePercentage = 0.03f;
    public Color selectionOutlineColor = Color.white;
    public Color wrongSelectionOutlineColor = Color.red;
    public float selectionOutlineThikness = 4;

    //You may use sprites instead of colors for background.
    //You can change it here
    [Header("Change only if 5 tier rareness is enabled")]
    public Color commonItemColor = new Color(0.5f, 0.5f, 0.5f);
    public Color uncommonItemColor = new Color(0.32f, 0.57f, 0.29f);
    public Color rareItemColor = new Color(0.21f, 0.43f, 0.8f);
    public Color epicItemColor = new Color(0.68f, 0.21f, 0.8f);
    public Color legendaryItemColor = new Color(0.86f, 0.61f, 0.17f);
    public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f);
    public float alphaValueForMultiCelledSlots = 0.3f;
}
