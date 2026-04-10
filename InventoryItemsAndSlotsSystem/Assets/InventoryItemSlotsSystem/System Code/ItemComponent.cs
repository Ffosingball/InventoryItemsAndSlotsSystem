using UnityEngine;

public class ItemComponent : MonoBehaviour
{
    [SerializeField] private int maxNumberOfBlocksInAStack;
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private Sprite picture;

    [Header("Change only if 5 tier rareness is enabled")]
    [SerializeField] private Rareness rareness = Rareness.None;

    [Header("Change only if items weight is enabled")]
    [SerializeField] private float itemWeight;

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
        if(this.inventoryConfiguration==null)
        {        
            this.inventoryConfiguration = inventoryConfiguration;
            if(inventoryConfiguration.useFiveTierRareness)
                rareness = Rareness.Common;
        }
    }



    private void Start()
    {
        if(itemName==null)
            itemName = System.Guid.NewGuid().ToString();

        if(inventoryConfiguration!=null)
        {
            if(inventoryConfiguration.useFiveTierRareness)
                rareness = Rareness.Common;
        }
    }
}
