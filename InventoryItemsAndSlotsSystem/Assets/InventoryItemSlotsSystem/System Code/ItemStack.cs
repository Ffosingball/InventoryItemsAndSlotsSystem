using System.Collections.Generic;


//This class simulates a single stack of item in the inventoryComponent
public class ItemStack
{
    //How many items in the stack
    private int numOfItems=0;
    //Total weight of the stack
    private float totalWeight=0f;
    //The item of the stack
    private ItemComponent item;
    //Stores place where the itemStack is placed in the list,
    //or top left corner position if inventory typy is not single celled
    private int cellOccupied;
    private InventoryConfiguration inventoryConfiguration;
    //Is stack full or not
    private bool isFull=false;
    private bool onlyOneItemPerStack = false;

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

    public int getCellOccupied()
    {
        return cellOccupied;
    }

    public void setCellOccupied(int cellOccupied)
    {
        this.cellOccupied = cellOccupied;
    }

    public bool getOnlyOneItemPerStack()
    {
        return onlyOneItemPerStack;
    }

    public void setOnlyOneItemPerStack(bool vlaue)
    {
        onlyOneItemPerStack = vlaue;
    }


    //Contructor if all items are signle celled
    public ItemStack(ItemComponent item, InventoryConfiguration inventoryConfiguration, int cellOccupied, bool onlyOneItemPerStack=false)
    {
        this.item = item;
        this.inventoryConfiguration = inventoryConfiguration;
        this.cellOccupied=cellOccupied;
        this.onlyOneItemPerStack = onlyOneItemPerStack;
    }


    //This method increases stack amount
    //If returns -1 it means failed, 0 suceeded, more than 0 amount left to add into new stack
    public int IncreaseAmountBy(int amount)
    {
        if(amount<0)
            return -1;
        else if(amount==0)
            return 0;

        if(onlyOneItemPerStack)
        {
            if(numOfItems==0)
            {
                numOfItems++;
                amount--;
            }

            return amount;
        }

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


    //This method decreases stack amount
    //If returns -1 it means failed, 0 suceeded, more than 0 amount left to decrease in other stacks
    public int DecreaseAmountBy(int amount)
    {

        if(amount<0)
            return -1;
        else if(amount==0)
            return 0;

        if(onlyOneItemPerStack)
        {
            if(numOfItems>0)
            {
                numOfItems--;
                amount--;
            }

            return amount;
        }

        numOfItems-=amount;

        if(numOfItems<0)
        {
            if(inventoryConfiguration.itemsHasAWeight)
                totalWeight = 0;
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