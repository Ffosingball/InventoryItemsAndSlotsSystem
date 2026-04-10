using System.Collections.Generic;
using UnityEngine;

public class ItemStack
{
    private int numOfItems=0;
    private float totalWeight=0f;
    private ItemComponent item;
    private List<int> cellsOccupied;
    private InventoryConfiguration inventoryConfiguration;
    private bool isFull=false;

    //Getters and setters
    public int getNumOfItems()
    {
        return numOfItems;
    }

    public float getTotalWeight()
    {
        return totalWeight;
    }

    public ItemComponent getItem()
    {
        return item;
    }

    public bool getIsFull()
    {
        return isFull;
    }

    public List<int> getCellsOccupied()
    {
        return cellsOccupied;
    }

    public void setCellsOccupied(List<int> cellsOccupied)
    {
        this.cellsOccupied = cellsOccupied;
    }


    //Contructor if all items are signle celled
    public ItemStack(ItemComponent item, InventoryConfiguration inventoryConfiguration, int cellOccupied)
    {
        this.item = item;
        this.inventoryConfiguration = inventoryConfiguration;
        cellsOccupied = new List<int>();
        cellsOccupied.Add(cellOccupied);
    }


    //Constructor if items can take more than one cell space
    public ItemStack(ItemComponent item, InventoryConfiguration inventoryConfiguration, List<int> cellsOccupied)
    {
        this.item = item;
        this.cellsOccupied = cellsOccupied;
        this.inventoryConfiguration = inventoryConfiguration;
    }


    //If returns -1 it means failed, 0 suceeded, more than 0 amount left to add into new stack
    public int IncreaseAmountBy(int amount)
    {
        if(amount<0)
            return -1;

        numOfItems+=amount;

        if(numOfItems>item.getMaxNumberOfBlocksInAStack())
        {
            int overflow = numOfItems - item.getMaxNumberOfBlocksInAStack();

            if(inventoryConfiguration.itemsHasAWeight)
                totalWeight+=(amount-overflow)*item.getItemWeight();

            numOfItems -= overflow;
            isFull=true;
            return overflow;
        }
        else
        {
            if(inventoryConfiguration.itemsHasAWeight)
                totalWeight+=amount*item.getItemWeight();

            return 0;
        }
    }


    //If returns -1 it means failed, 0 suceeded, more than 0 amount left to decrease in other stacks
    public int DecreaseAmountBy(int amount)
    {

        if(amount<0)
            return -1;

        numOfItems-=amount;

        if(numOfItems<0)
        {
            return -numOfItems;
        }
        else
        {
            if(inventoryConfiguration.itemsHasAWeight)
                totalWeight-=amount*item.getItemWeight();
            
            isFull=false;

            return 0;
        }
    }
}