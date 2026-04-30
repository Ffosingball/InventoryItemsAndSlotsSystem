using UnityEngine;


//This component stores information about items
public class ItemComponent : MonoBehaviour
{
    //Stack cap
    [SerializeField] private int maxNumberOfBlocksInAStack;
    [Tooltip("Each distinct item should have unique name")]
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private Sprite picture;

    [Header("Change only if 5 tier rareness is enabled")]
    [SerializeField] private Rareness rareness = Rareness.None;

    [Header("Change only if items weight is enabled")]
    [Tooltip("Cannot be less than 0!")]
    [SerializeField] private float itemWeight;

    [Header("Change only if items stack limit is enabled")]
    [Tooltip("Cannot be less then 0!")]
    [SerializeField] private int itemStackLimit = 3;

    private InventoryConfiguration inventoryConfiguration;

    //Getters and setters
    public int getMaxNumberOfBlocksInAStack()
    {
        return maxNumberOfBlocksInAStack;
    }

    public void setMaxNumberOfBlocksInAStack(int value)
    {
        maxNumberOfBlocksInAStack = value;
    }

    public float getItemWeight()
    {
        return itemWeight;
    }

    public void setItemWeight(float value)
    {
        itemWeight = value;
    }

    public Rareness getRareness()
    {
        return rareness;
    }

    public void setRareness(Rareness rareness)
    {
        this.rareness = rareness;
    }

    public string getItemName()
    {
        return itemName;
    }

    public void setItemName(string itemName)
    {
        this.itemName = itemName;
    }

    public ItemType getItemType()
    {
        return itemType;
    }

    public void setItemType(ItemType itemType)
    {
        this.itemType = itemType;
    }

    public Sprite getPicture()
    {
        return picture;
    }

    public void setPicture(Sprite picture)
    {
        this.picture = picture;
    }

    public void setInventoryConfiguration(InventoryConfiguration inventoryConfiguration)
    {
        this.inventoryConfiguration = inventoryConfiguration;
        if(this.inventoryConfiguration!=null)
        {        
            if(inventoryConfiguration.useFiveTierRareness && rareness==Rareness.None)
                rareness = Rareness.Common;
            else if(!inventoryConfiguration.useFiveTierRareness && rareness!=Rareness.None)
                rareness = Rareness.None;
        }
    }

    public void setItemStackLimit(int value)
    {
        itemStackLimit = value;
    }

    public int getItemStackLimit()
    {
        return itemStackLimit;
    }



    private void Start()
    {
        //Initialize item
        if(itemName==null)
            itemName = System.Guid.NewGuid().ToString();

        if(inventoryConfiguration!=null)
        {
            if(inventoryConfiguration.useFiveTierRareness)
                rareness = Rareness.Common;
        }
    }
}
