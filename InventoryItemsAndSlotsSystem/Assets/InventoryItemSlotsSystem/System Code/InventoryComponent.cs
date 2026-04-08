using UnityEngine;
using System.Collections.Generic;


//For the position in the inventory count from (0,0)
public class InventoryComponent : MonoBehaviour
{
    [SerializeField] private bool isInventoryInfinite = false;
    [SerializeField] private int inventoryWidth=10;
    [SerializeField] private int inventoryHeight=5;
    [Header("How many cells of the inventory is visible in a single view, if height is more than visible height then you will need to scroll an inventory")]
    [SerializeField] private string inventoryName;
    [SerializeField] private InventoryConfiguration inventoryConfiguration;

    private Dictionary<ItemComponent, List<ItemStack>> itemsInTheInventory;
    private ItemStack[] itemsPositions;


    //Getters and setters
    public bool IsInventoryInfinite()
    {
        return isInventoryInfinite;
    }

    public void setIsInventoryInfinite(bool value)
    {
        isInventoryInfinite = value;
    }

    public int getInventoryWidth()
    {
        return inventoryWidth;
    }

    public void setInventoryHeight(int value)
    {
        inventoryHeight = value;
    }

    public int getInventoryHeight()
    {
        return inventoryHeight;
    }

    public void setInventoryWidth(int value)
    {
        inventoryWidth = value;
    }

    public string getInventoryName()
    {
        return inventoryName;
    }

    public void setInventoryName(string value)
    {
        inventoryName = value;
    }

    public void setInventoryConfiguration(InventoryConfiguration configuration)
    {
        inventoryConfiguration = configuration;
    }

    public ItemStack[] getItemsInTheInventory()
    {
        return itemsPositions;
    }



    private void Start()
    {
        itemsInTheInventory = new Dictionary<ItemComponent,List<ItemStack>>();
        InitializeInventory();
    }


    //
    //If at that 
    //use this to move itemStack from one inventory to other!
    public NewItemPlacementResult PlaceItemStackToInventory(ItemStack itemStack, Vector2Int position)
    {
        int arrPosition = position.y*inventoryWidth+position.x;
        NewItemPlacementResult placementResult = new NewItemPlacementResult();

        if(itemsPositions.Length<=arrPosition || arrPosition<0)
        {
            placementResult.invalidPosition=true;
            return placementResult;
        }

        if(itemsPositions[arrPosition]!=null)
        {
            placementResult.stackReplaced = itemsPositions[arrPosition];
        }

        itemsPositions[arrPosition] = itemStack;

        return placementResult;
    }


    public void InitializeInventory()
    {
        itemsPositions = new ItemStack[inventoryWidth*inventoryHeight];
    }


    //
    public int AddItemsToInventory(ItemComponent item, int amount)
    {
        //Check if this item already has a stack in the inventory
        //if not create new one in the free place
        //Try to add all amount, if all amount cannot get into a single stack create
        //more stacks in a free places
        //If free places are run out then return overflow
    }


    public bool RemoveItemsFromInventory(ItemComponent item, int amount)
    {
        
    }


    public int GetTotalAmountOfThisItem(ItemComponent item)
    {
        
    }


    public bool ContainsItem(ItemComponent item)
    {
        
    }


    public ItemStack GetItemStackByPosition(Vector2Int position)
    {
        
    }


    public void RemoveItemStackByPosition(Vector2Int position)
    {
        
    }


    public ItemComponent ReplaceItemPositionTo(ItemComponent itemComponent, Vector2Int position)
    {
        
    }
}
