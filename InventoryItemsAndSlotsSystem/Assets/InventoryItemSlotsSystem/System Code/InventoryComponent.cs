using UnityEngine;
using System.Collections.Generic;


//For the position in the inventory count from (0,0)
public class InventoryComponent : MonoBehaviour
{
    [SerializeField] private bool isInventoryInfinite = false;
    [SerializeField] private int inventoryWidth=10, newInventoryWidth=10;
    [SerializeField] private int inventoryHeight=5, newInventoryHeight=10;
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
        newInventoryHeight = value;
    }

    public int getInventoryHeight()
    {
        return inventoryHeight;
    }

    public void setInventoryWidth(int value)
    {
        newInventoryWidth = value;
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
        itemsPositions = new ItemStack[inventoryWidth*inventoryHeight];
    }


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
            if(itemsPositions[arrPosition].getItem()==itemStack.getItem())
            {
                int overflow = itemStack.IncreaseAmountBy(itemsPositions[arrPosition].getNumOfItems());
                if(overflow>0)
                {
                    itemsPositions[arrPosition].DecreaseAmountBy(itemsPositions[arrPosition].getNumOfItems()-overflow);
                    placementResult.stackReplaced = itemsPositions[arrPosition];
                }
            }
            else
                placementResult.stackReplaced = itemsPositions[arrPosition];

            itemsInTheInventory[itemsPositions[arrPosition].getItem()].Remove(itemsPositions[arrPosition]);
            if(itemsInTheInventory[itemsPositions[arrPosition].getItem()].Count==0)
                itemsInTheInventory.Remove(itemsPositions[arrPosition].getItem());
        }

        if(!itemsInTheInventory.ContainsKey(itemStack.getItem()))
            itemsInTheInventory.Add(itemStack.getItem(), new List<ItemStack>());

        itemStack.getCellsOccupied()[0]=arrPosition;

        itemsInTheInventory[itemStack.getItem()].Add(itemStack);
        itemsPositions[arrPosition] = itemStack;

        return placementResult;
    }


    //Call it when new item is added to the inventory not by the player
    public int AddItemsToInventory(ItemComponent item, int amount)
    {
        if(itemsInTheInventory.ContainsKey(item))
        {
            foreach(ItemStack itemStack in itemsInTheInventory[item])
            {
                if(!itemStack.getIsFull())
                {
                    amount = itemStack.IncreaseAmountBy(amount);
                    if(amount<=0)
                        return 0;
                }
            }
        }
        else
            itemsInTheInventory.Add(item, new List<ItemStack>());

        for(int i=0; i<itemsPositions.Length; i++)
        {
            if(itemsPositions[i]==null)
            {
                ItemStack newItemStack = new ItemStack(item, inventoryConfiguration, i);
                amount = newItemStack.IncreaseAmountBy(amount);

                itemsPositions[i] = newItemStack;
                itemsInTheInventory[item].Add(newItemStack);

                if(amount<=0)
                    return 0;
            }
        }

        return amount;
    }


    public bool RemoveItemsFromInventory(ItemComponent item, int amount)
    {
        if(amount>GetTotalAmountOfThisItem(item))
            return false;

        while(true)
        {
            amount = itemsInTheInventory[item][0].DecreaseAmountBy(amount);

            if(amount>0)
            {
                itemsPositions[itemsInTheInventory[item][0].getCellsOccupied()[0]]=null;
                itemsInTheInventory[item].RemoveAt(0);
            }
            else
                return true;
        }
    }


    public int GetTotalAmountOfThisItem(ItemComponent item)
    {
        int totalAmount=0;
        foreach(ItemStack itemStack in itemsInTheInventory[item])
        {
            totalAmount+=itemStack.getNumOfItems();
        }

        return totalAmount;
    }


    public bool ContainsItem(ItemComponent item)
    {
        return itemsInTheInventory.ContainsKey(item);
    }


    public ItemStack GetItemStackByPosition(Vector2Int position)
    {
        return itemsPositions[position.y*inventoryWidth+position.x];
    }


    public void RemoveItemStackByPosition(Vector2Int position)
    {
        ItemStack stackToRemove = itemsPositions[position.y*inventoryWidth+position.x];
        itemsPositions[position.y*inventoryWidth+position.x]=null;
        itemsInTheInventory[stackToRemove.getItem()].Remove(stackToRemove);

        if(itemsInTheInventory[stackToRemove.getItem()].Count==0)
            itemsInTheInventory.Remove(stackToRemove.getItem());
    }


    //Call it when changing position of the stack size in the same inventory
    public void ChangeItemStackPositionTo(ItemStack itemStack, Vector2Int position)
    {
        int arrPosition = position.y*inventoryWidth+position.x;
        if(itemsPositions[arrPosition]==null)
            itemsPositions[itemStack.getCellsOccupied()[0]]=null;
        else
        {
            itemsPositions[itemStack.getCellsOccupied()[0]] = itemsPositions[arrPosition];
            itemsPositions[itemStack.getCellsOccupied()[0]].getCellsOccupied()[0] = itemStack.getCellsOccupied()[0];
        }
        
        itemsPositions[arrPosition] = itemStack;
        itemStack.getCellsOccupied()[0]=arrPosition;
    }


    //Call when you changed inventory sizes and want to resize it
    //Returns the list of items which did not fit in the new inventory
    //(important if you decrease size of the inventory)
    public List<ItemStack> ResizeInventory()
    {
        inventoryHeight = newInventoryHeight;
        inventoryWidth = newInventoryWidth;

        ItemStack[] resizedItemsPositions = itemsPositions = new ItemStack[inventoryWidth*inventoryHeight];

        if(itemsPositions.Length<resizedItemsPositions.Length)
        {
            for(int i=0; i<itemsPositions.Length; i++)
            {
                resizedItemsPositions[i] = itemsPositions[i];
            }

            itemsPositions = resizedItemsPositions;
            return null;
        }
        else
        {
            int positionsSkipped=0;
            for(int i=0; i<resizedItemsPositions.Length; i++)
            {
                if(itemsPositions[i]!=null)
                    resizedItemsPositions[i] = itemsPositions[i+positionsSkipped];
                else
                    positionsSkipped++;

                if(positionsSkipped+i>=resizedItemsPositions.Length)
                {
                    itemsPositions = resizedItemsPositions;
                    return null;
                }
            }

            List<ItemStack> itemStacksRemoved = new List<ItemStack>();
            for(int i=resizedItemsPositions.Length+positionsSkipped; i<itemsPositions.Length; i++)
            {
                if(itemsPositions[i]!=null)
                    itemStacksRemoved.Add(itemsPositions[i]);
            }

            itemsPositions = resizedItemsPositions;
            return itemStacksRemoved;
        }
    }
}
