using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
using UnityEditor.Experimental.GraphView;


//This component represent the inventory itself, it has a list of slots which
//may or may not contain items
public class InventoryComponent : MonoBehaviour
{
    //When anything is changed in the inventory it will change view as well
    public event Action OnChangeUpdateView;

    [Tooltip("If inventoryInfinite is true it will add new row of slots to the inventory when there no free slot is left")]
    [SerializeField] private bool isInventoryInfinite = false;
    //Inventory size
    [SerializeField] private int inventoryWidth=10, inventoryHeight=10;
    [SerializeField] private string inventoryName = null;
    [SerializeField] private InventoryConfiguration inventoryConfiguration;
    [Tooltip("If less than 0 then no weight limit!")]
    [SerializeField] private float inventoryWeightLimit = 1000f;

    //All three of this data structures store the same inventory items
    //but used in different situations
    private Dictionary<(string, Rareness), List<ItemStack>> itemsInTheInventory;
    private List<ItemStack> itemsPositions, sortedPositions;
    //NewWidth and newHeight are used when you set new width or height, after when you
    //call resizeInventory it will use this new sizes and resize current inventory
    private int newInventoryWidth=-1, newInventoryHeight=-1;
    //InitaialInventoryHeight is used for infiniteInventory resizing
    private int initialInventoryHeight;
    //Total amount of stacks stored in the inventory
    private int totalItems=0;
    private bool showItemsSorted=false;
    //Function which are used to sort and add to sorted list items
    private Func<List<ItemStack>, List<ItemStack>> sortFunction;
    private Action<ItemStack, List<ItemStack>> addToSortedFunction;
    private Action<ItemStack, List<ItemStack>> removeFromSortedFunction;
    private string searchTarget = "";
    private float totalItemsWeight=0f;


    //Getters and setters
    public bool IsInventoryInfinite()
    {
        return isInventoryInfinite;
    }

    public bool getShowItemsSorted()
    {
        return showItemsSorted;
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
        if(value>0)
            newInventoryHeight = value;
    }

    public int getInventoryHeight()
    {
        return inventoryHeight;
    }

    public void setInventoryWidth(int value)
    {
        if(value>0)
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

    public InventoryConfiguration getInventoryConfiguration()
    {
        return inventoryConfiguration;
    }

    public List<ItemStack> getItemsInTheInventory()
    {
        return itemsPositions;
    }

    public List<ItemStack> getSortedListOfItems()
    {
        return sortedPositions;
    }

    public float GetTotalItemsWeight()
    {
        return totalItemsWeight;
    }

    public int getInventorySize()
    {
        return inventoryHeight*inventoryWidth;
    }

    public float getInventoryWeightLimit()
    {
        return inventoryWeightLimit;
    }

    public void setInventoryWeightLimit(int value)
    {
        inventoryWeightLimit = value;
    }

    public int getNumOfItemsInTheInventory()
    {
        return totalItems;
    }



    private void Start()
    {
        //Set new random name if its name is null
        if(inventoryName==null)
            inventoryName = System.Guid.NewGuid().ToString();

        //Initialize data structures
        itemsInTheInventory = new Dictionary<(string, Rareness),List<ItemStack>>();
        itemsPositions = new List<ItemStack>();
        for(int i=0; i<inventoryWidth*inventoryHeight; i++)
        {
            itemsPositions.Add(null);
        }

        initialInventoryHeight = inventoryHeight;
    }



    //This method checks if there are enough space for the itemStack to be placed
    //It only used when Rectangular and AnySize inventory types selected
    public bool IsEnoughSpaceForItem(ItemStack itemStackToPlace, Vector2Int slotPosition)
    {
        return IsEnoughSpaceForItem(itemStackToPlace.getItem(), slotPosition);
    }



    //This method checks if there are enough space for the itemStack to be placed
    //It only used when Rectangular and AnySize inventory types selected
    public bool IsEnoughSpaceForItem(ItemComponent itemToPlace, Vector2Int slotPosition)
    {
        if(isInventoryInfinite && itemsPositions[slotPosition.x+(slotPosition.y*inventoryWidth)]==null)
        {
            return true;
        }
        else
        {
            for(int y=0; y<itemToPlace.getItemSize().y; y++)
            {
                for(int x=0; x<itemToPlace.getItemSize().x; x++)
                {
                    if(slotPosition.x+x>inventoryWidth || slotPosition.y+y>inventoryHeight)
                        return false;
                    else if(itemsPositions[slotPosition.x+x+((slotPosition.y+y)*inventoryWidth)]!=null)
                        return false;
                }
            }

            return true;
        }
    }



    //This method checks if there are enough space for the itemStack to be placed
    //in a provided list
    //It only used when Rectangular and AnySize inventory types selected
    private bool IsEnoughSpaceForItem(ItemStack itemStackToPlace, Vector2Int slotPosition, List<ItemStack> listOfStacks, Vector2Int listDimensions)
    {
        if(isInventoryInfinite && listOfStacks[slotPosition.x+(slotPosition.y*listDimensions.x)]==null)
        {
            return true;
        }
        else
        {
            for(int y=0; y<itemStackToPlace.getItem().getItemSize().y; y++)
            {
                for(int x=0; x<itemStackToPlace.getItem().getItemSize().x; x++)
                {
                    if(slotPosition.x+x>listDimensions.x || slotPosition.y+y>listDimensions.x)
                        return false;
                    else if(listOfStacks[slotPosition.x+x+((slotPosition.y+y)*listDimensions.x)]!=null)
                        return false;
                }
            }

            return true;
        }
    }



    //This method places multidimensional items in the inventory (1x2, 2x2 etc.)
    //It only used when Rectangular and AnySize inventory types selected
    //Slot position in the array
    //IMPORTANT! Whe I call this method I assume there are enough empty space for the itemsStack
    //to be placed, so before calling this call IsEnoughSpaceForItem()
    private void PlaceItemStackBlock(ItemStack itemStackToPlace, int slotPosition)
    {
        if(isInventoryInfinite)
        {
            if(!IsEnoughSpaceForItem(itemStackToPlace, new Vector2Int(slotPosition%inventoryWidth, slotPosition/inventoryHeight)))
            {
                bool foundPosition=false;
                for(int i=0; i<itemsPositions.Count; i++)
                {
                    if(IsEnoughSpaceForItem(itemStackToPlace, new Vector2Int(i%inventoryWidth, i/inventoryHeight)))
                    {
                        slotPosition=i;
                        foundPosition=true;
                        break;
                    }
                }

                if(!foundPosition)
                {
                    inventoryHeight+=itemStackToPlace.getItem().getItemSize().y;
                    slotPosition = itemsPositions.Count;

                    for(int i=0; i<itemStackToPlace.getItem().getItemSize().y*inventoryWidth; i++)
                    {
                        itemsPositions.Add(null);
                    }
                }
            }
        }
        
        List<int> allPositionsList = new List<int>();
        for(int y=0; y<itemStackToPlace.getItem().getItemSize().y; y++)
        {
            for(int x=0; x<itemStackToPlace.getItem().getItemSize().x; x++)
            {
                itemsPositions[slotPosition+x+(y*inventoryWidth)]=itemStackToPlace;
                allPositionsList.Add(slotPosition+x+(y*inventoryWidth));
            }
        }

        itemStackToPlace.setCellsOccupied(allPositionsList);
    }



    //This method removes multidimensional items from the inventory (1x2, 2x2 etc.)
    //It only used when Rectangular and AnySize inventory types selected
    //Slot position in the array
    private void RemoveItemsBlockByPosition(int slotPosition, Vector2Int inventoryDimensions)
    {
        List<int> positionsToClear = itemsPositions[slotPosition].getCellsOccupied();

        foreach(int position in positionsToClear)
        {
            itemsPositions[position] = null;
        }
    }

    private void RemoveItemsBlockByPosition(int slotPosition)
    {
        RemoveItemsBlockByPosition(slotPosition, new Vector2Int(inventoryWidth, inventoryHeight));
    }



    //This function takes itemStack and position where it should be placed
    //and tries to place it. After it will return the result of the itemStack placement
    public NewItemPlacementResult PlaceItemStackToInventory(ItemStack itemStack, Vector2Int position)
    {
        //Check if moving items in the inventory is allowed
        if(inventoryConfiguration.arbitraryStackPlacement)
        {
            int arrPosition = position.y*inventoryWidth+position.x;
            NewItemPlacementResult placementResult = new NewItemPlacementResult();

            //If position out of inventory range then return failure
            if(itemsPositions.Count<=arrPosition || arrPosition<0)
            {
                placementResult.invalidPosition=true;
                OnChangeUpdateView?.Invoke();
                return placementResult;
            }

            //Check if sorting inventory is on
            if(showItemsSorted)
            {
                //Try to find an empty slot in the inventory
                bool foundEmptySpace = false;
                for(int i=0; i<itemsPositions.Count; i++)
                {
                    if((itemsPositions[i]==null && inventoryConfiguration.inventoryType==InventoryType.SingleCelled) || (inventoryConfiguration.inventoryType!=InventoryType.SingleCelled && IsEnoughSpaceForItem(itemStack, new Vector2Int(i%inventoryWidth,i/inventoryWidth))))
                    {
                        //If found then set position to this one
                        foundEmptySpace = true;
                        position.x = i%inventoryWidth;
                        position.y = i/inventoryWidth;
                        arrPosition = position.y*inventoryWidth+position.x;
                        break;
                    }
                }

                //If empty slot not found, then replace item at which mouse is over
                if(!foundEmptySpace)
                {
                    position.x = sortedPositions[arrPosition].getCellsOccupied()[0]%inventoryWidth;
                    position.y = sortedPositions[arrPosition].getCellsOccupied()[0]/inventoryWidth;
                    arrPosition = position.y*inventoryWidth+position.x;
                }
            }

            //If inventoryType is not sigle celled and area for this item is not empty then
            //do not try to place item
            if(inventoryConfiguration.inventoryType!=InventoryType.SingleCelled && !IsEnoughSpaceForItem(itemStack, position))
            {
                placementResult.areaIsOccupied = true;
                return placementResult;
            }

            //Check if slot at the provided position has itemStack
            if(itemsPositions[arrPosition]!=null && inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
            {
                //In this case remove it from the slot
                //If provided itemStack contains the same item as in the slot
                //then try to merge them
                bool removeItem = false;
                if(itemsPositions[arrPosition].getItem()==itemStack.getItem())
                {
                    //Check if weight limit reached if applicable
                    if(inventoryConfiguration.itemsHasAWeight && inventoryWeightLimit>0 && totalItemsWeight+(itemStack.getNumOfItems()*itemStack.getItem().getItemWeight())>inventoryWeightLimit)
                    {
                        int addAmount = itemStack.getNumOfItems()-(int)((totalItemsWeight+(itemStack.getNumOfItems()*itemStack.getItem().getItemWeight())-inventoryWeightLimit)/itemStack.getItem().getItemWeight());
                        itemsPositions[arrPosition].IncreaseAmountBy(addAmount);
                        totalItemsWeight += addAmount*itemStack.getItem().getItemWeight();
                        itemStack.DecreaseAmountBy(addAmount);
                        placementResult.weightLimitReached = true;
                        OnChangeUpdateView?.Invoke();
                        return placementResult;
                    }

                    int overflow = itemStack.IncreaseAmountBy(itemsPositions[arrPosition].getNumOfItems());
                    
                    //If total amount of items merged is more than stack cap than set those overflow items
                    //as replaced stack
                    if(overflow>0)
                    {
                        removeItem = true;
                        itemsPositions[arrPosition].DecreaseAmountBy(itemsPositions[arrPosition].getNumOfItems()-overflow);
                        totalItemsWeight-=itemsPositions[arrPosition].getTotalWeight();
                        placementResult.stackReplaced = itemsPositions[arrPosition];
                    }
                    else
                    {
                        totalItems--;
                        totalItemsWeight-=itemsPositions[arrPosition].getTotalWeight();
                    }
                }
                else
                {
                    //Otherwise check if itemStackLimit is off then
                    if(!inventoryConfiguration.itemStacksLimit)
                    {
                        //Set that itemStack at the slot as replaced
                        removeItem = true;
                        placementResult.stackReplaced = itemsPositions[arrPosition];
                    }
                    else
                    {
                        //If stackLimit is on and there are less than the maximum 
                        //allowed amount of stacks then
                        if(itemStack.getItem().getItemStackLimit()>GetNumberOfStacksOfThisItem(itemStack.getItem()))
                        {
                            //Set that itemStack at the slot as replaced
                            removeItem = true;
                            placementResult.stackReplaced = itemsPositions[arrPosition];
                        }
                    }
                }

                if(removeItem)
                {
                    //Check if weight limit reached if applicable
                    if(inventoryConfiguration.itemsHasAWeight && inventoryWeightLimit>0 && totalItemsWeight-placementResult.stackReplaced.getTotalWeight()+itemStack.getTotalWeight()>inventoryWeightLimit)
                    {
                        placementResult.weightLimitReached = true;
                        OnChangeUpdateView?.Invoke();
                        return placementResult;
                    }

                    totalItems--;
                    totalItemsWeight-=placementResult.stackReplaced.getTotalWeight();

                    //if sorting inventory is on then remove replaced stack from the sorted list
                    if(showItemsSorted)
                    {
                        removeFromSortedFunction(placementResult.stackReplaced, sortedPositions);
                    }

                    //Remove replaced item from all other lists
                    ItemComponent existingItem = itemsPositions[arrPosition].getItem();
                    itemsInTheInventory[(existingItem.getItemName(),existingItem.getRareness())].Remove(itemsPositions[arrPosition]);
                    if(itemsInTheInventory[(existingItem.getItemName(),existingItem.getRareness())].Count==0)
                        itemsInTheInventory.Remove((existingItem.getItemName(),existingItem.getRareness()));
                }
            }
            else
            {
                //Check if weight limit reached if applicable
                if(inventoryConfiguration.itemsHasAWeight && inventoryWeightLimit>0 && totalItemsWeight+itemStack.getTotalWeight()>inventoryWeightLimit)
                {
                    int addAmount = itemStack.getNumOfItems()-(int)((totalItemsWeight+itemStack.getTotalWeight()-inventoryWeightLimit)/itemStack.getItem().getItemWeight());
                    if(addAmount>0)
                    {
                        ItemStack newItemStack = new ItemStack(itemStack.getItem(),inventoryConfiguration,-1);
                        newItemStack.IncreaseAmountBy(addAmount);
                        itemStack.DecreaseAmountBy(addAmount);

                        placementResult.stackReplaced = itemStack;
                        itemStack = newItemStack;
                    }
                    else
                    {
                        placementResult.weightLimitReached = true;
                        OnChangeUpdateView?.Invoke();
                        return placementResult;
                    }
                }
            }

            //Check if stackLimit is off or stack limit is not reached
            if(!inventoryConfiguration.itemStacksLimit || itemStack.getItem().getItemStackLimit()>GetNumberOfStacksOfThisItem(itemStack.getItem()))
            {   
                ItemComponent newItem = itemStack.getItem();
                //Check if dictionary has key of this type, if not then create it
                if(!itemsInTheInventory.ContainsKey((newItem.getItemName(),newItem.getRareness())))
                    itemsInTheInventory.Add((newItem.getItemName(),newItem.getRareness()), new List<ItemStack>());

                //Insert item stack in the provided position
                itemStack.getCellsOccupied()[0]=arrPosition;
                if(showItemsSorted)
                    addToSortedFunction(itemStack, sortedPositions);
                itemsInTheInventory[(newItem.getItemName(),newItem.getRareness())].Add(itemStack);
                if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                    itemsPositions[arrPosition] = itemStack;
                else
                    PlaceItemStackBlock(itemStack, arrPosition);
                totalItems++;
                totalItemsWeight+=itemStack.getTotalWeight();

                //Check if inventory is infinite and no free slots is left than add 
                //new row of empty slots
                if(isInventoryInfinite && (inventoryHeight*inventoryWidth)-totalItems<=1)
                {
                    for(int i=0; i<inventoryWidth; i++)
                    {
                        itemsPositions.Add(null);
                        if(showItemsSorted)
                            sortedPositions.Add(null);
                    }
                    inventoryHeight++;
                }

                OnChangeUpdateView?.Invoke();
                return placementResult;
            }
            else
            {
                //Otherwise return failure
                placementResult.stackCapReached=true;
                OnChangeUpdateView?.Invoke();
                return placementResult;
            }
        }

        return null;
    }


    //This method will take itemComponent and how many of this items should be placed
    //In the inventory. It will return 0 if it managed to insert full amount, -1
    //if failed, and more than 0 (amount of items it failed to insert because 
    //inventory became full
    public int AddItemsToInventory(ItemComponent item, int amount)
    {
        //Check that amount is valid
        if(amount<1)
            return -1;

        //Check if there are already some stacks of this exact item in the inventory
        if(itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness())))
        {
            //If so, then find those which are not empty and insert some amount
            //in that stacks
            foreach(ItemStack itemStack in itemsInTheInventory[(item.getItemName(),item.getRareness())])
            {
                if(!itemStack.getIsFull())
                {
                    totalItemsWeight-=itemStack.getTotalWeight();
                    amount = itemStack.IncreaseAmountBy(amount);
                    totalItemsWeight+=itemStack.getTotalWeight();
                    
                    //Change position of this stack in the sorted list, as its 
                    //amount of items has changed
                    if(showItemsSorted)
                    {
                        removeFromSortedFunction(itemStack, sortedPositions);
                        addToSortedFunction(itemStack, sortedPositions);
                    }

                    //Check if items weight is on and this inventory has weight limit
                    if(inventoryConfiguration.itemsHasAWeight && inventoryWeightLimit>0 && totalItemsWeight>inventoryWeightLimit)
                    {
                        int itemsToRemove = (int)((totalItemsWeight-inventoryWeightLimit)/item.getItemWeight());
                        itemStack.DecreaseAmountBy(itemsToRemove);
                        totalItemsWeight-=item.getItemWeight()*itemsToRemove;
                        OnChangeUpdateView?.Invoke();
                        return amount+itemsToRemove;
                    }

                    if(amount<=0)
                    {
                        OnChangeUpdateView?.Invoke();
                        return 0;
                    }
                }
            }
        }
        else
            itemsInTheInventory.Add((item.getItemName(),item.getRareness()), new List<ItemStack>());

        //After if amount is still more than 0, than try to find empty slots
        //and insert itemStacks there
        for(int i=0; i<itemsPositions.Count; i++)
        {
            if((itemsPositions[i]==null && inventoryConfiguration.inventoryType==InventoryType.SingleCelled) || (inventoryConfiguration.inventoryType!=InventoryType.SingleCelled && IsEnoughSpaceForItem(item, new Vector2Int(i%inventoryWidth,i/inventoryWidth))))
            {
                int result = InsertItemAt(i, item, amount);
                if(result<-1)
                {
                    amount-=itemsPositions[i].getNumOfItems();
                    OnChangeUpdateView?.Invoke();
                    return amount;
                }
                else if(result==-1)
                {
                    OnChangeUpdateView?.Invoke();
                    return amount;
                }
                else if(result==0)
                {
                    OnChangeUpdateView?.Invoke();
                    return 0;
                }
                else
                    amount = result;
            }
        }

        //After if amount is still more than 0 then if it is infinite inventory
        //add more rows of slots
        if(isInventoryInfinite)
        {
            //Calculate how many more stacks will appear
            int numOfStacksWillAppear = amount/item.getMaxNumberOfBlocksInAStack();
            if(numOfStacksWillAppear%item.getMaxNumberOfBlocksInAStack()!=0)
                numOfStacksWillAppear++;

            //Calculate how many more rows should be added
            int heightToAdd = numOfStacksWillAppear/inventoryWidth;
            if(numOfStacksWillAppear%inventoryWidth!=0)
                heightToAdd++;

            //Add more rows of slots
            for(int i=0; i<heightToAdd*inventoryWidth; i++)
            {
                itemsPositions.Add(null);
                if(showItemsSorted)
                    sortedPositions.Add(null);
            }
            inventoryHeight+=heightToAdd;

            //Go through every new added slot and insert there 
            for(int i=(inventoryHeight-heightToAdd)*inventoryWidth; i<itemsPositions.Count; i++)
            {
                if((itemsPositions[i]==null && inventoryConfiguration.inventoryType==InventoryType.SingleCelled) || (inventoryConfiguration.inventoryType!=InventoryType.SingleCelled && IsEnoughSpaceForItem(item, new Vector2Int(i%inventoryWidth,i/inventoryWidth))))
                {
                    int result = InsertItemAt(i, item, amount);
                    if(result<-1)
                    {
                        amount-=itemsPositions[i].getNumOfItems();
                        break;
                    }
                    else if(result==-1)
                        break;
                    else if(result==0)
                    {
                        amount = result;
                        break;
                    }
                    else
                        amount = result;
                }
            }

            //Check if there are no empty slots left then add new row of empty slots
            if((inventoryHeight*inventoryWidth)-totalItems<=1)
            {
                for(int i=0; i<inventoryWidth; i++)
                {
                    itemsPositions.Add(null);
                    if(showItemsSorted)
                        sortedPositions.Add(null);
                }
                inventoryHeight++;
            }
        }

        OnChangeUpdateView?.Invoke();
        return amount;
    }



    //This method inserts item at the given position without check whether there are
    //itemStack at that position or not
    private int InsertItemAt(int arrPosition, ItemComponent item, int amount)
    {
        //Check if stackLimit is off or the limit is not reached
        if(!inventoryConfiguration.itemStacksLimit || item.getItemStackLimit()>GetNumberOfStacksOfThisItem(item))
        {
            //Then create new stack
            ItemStack newItemStack = new ItemStack(item, inventoryConfiguration, arrPosition);
            amount = newItemStack.IncreaseAmountBy(amount);
            totalItems++;
            totalItemsWeight+=newItemStack.getTotalWeight();

            if(showItemsSorted)
                addToSortedFunction(newItemStack, sortedPositions);
            if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                itemsPositions[arrPosition] = newItemStack;
            else
                PlaceItemStackBlock(newItemStack, arrPosition);
            itemsInTheInventory[(item.getItemName(),item.getRareness())].Add(newItemStack);

            //Check if items weight is on and this inventory has weight limit
            if(inventoryConfiguration.itemsHasAWeight && inventoryWeightLimit>0 && totalItemsWeight>inventoryWeightLimit)
            {
                int itemsToRemove = (int)((totalItemsWeight-inventoryWeightLimit)/item.getItemWeight());
                totalItemsWeight-=itemsToRemove*item.getItemWeight();
                if(itemsToRemove==newItemStack.getNumOfItems())
                {
                    if(showItemsSorted)
                    {
                        removeFromSortedFunction(newItemStack, sortedPositions);
                    }
                    if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                        itemsPositions[arrPosition] = null;
                    else
                        RemoveItemsBlockByPosition(arrPosition);
                    itemsInTheInventory[(item.getItemName(),item.getRareness())].Remove(newItemStack);
                }
                else
                {
                    newItemStack.DecreaseAmountBy(itemsToRemove);
                }

                //Weight limit reached, so return -2
                return -2;
            }
            return amount;
        }
        else
        {
            //Stack limit reached, so return -1
            return -1;
        }
    }


    
    //This method reduces amount of rows with empty slots in the infiniteInventory
    private void ShrinkInfiniteInventory()
    {
        //Check if all items are signgle celled then we can move them 
        //otherwise do not move items and only remove empty rows
        if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
        {
            int clearedPosition = 0;
            //This loop starts from the end of the inventory and tries to find empty
            //slots at the begining and replace itemStacks from the end of the inventory
            //into the empty slots at the begining
            for(int i=itemsPositions.Count-1; i>0; i--)
            {
                if(itemsPositions[i]!=null)
                {
                    while(itemsPositions[clearedPosition]!=null)
                    {
                        clearedPosition++;
                        if(clearedPosition>=i)
                            break;
                    }

                    if(clearedPosition>=i)
                        break;

                    itemsPositions[clearedPosition] = itemsPositions[i];
                    itemsPositions[clearedPosition].getCellsOccupied().RemoveAt(0);
                    itemsPositions[clearedPosition].getCellsOccupied().Add(clearedPosition);
                }

                itemsPositions.RemoveAt(i);
                if(showItemsSorted)
                    sortedPositions.RemoveAt(sortedPositions.Count-1);
            }
            clearedPosition++;

            //Check if current size of the inventory is larger than its initial
            if(clearedPosition>initialInventoryHeight*inventoryWidth)
            {
                //Then decrease its height
                inventoryHeight=clearedPosition/inventoryWidth;
                if(clearedPosition%inventoryWidth!=0)
                    inventoryHeight++;

                //And add missing slots at the last row
                while(itemsPositions.Count%inventoryWidth!=0)
                {
                    itemsPositions.Add(null);
                    if(showItemsSorted)
                        sortedPositions.Add(null);
                }
            }
            else
            {
                //Otherwise just add missing slots
                inventoryHeight=initialInventoryHeight;
                for(int i=clearedPosition; i<inventoryHeight*inventoryWidth; i++)
                {
                    itemsPositions.Add(null);
                    if(showItemsSorted)
                        sortedPositions.Add(null);
                }
            }
        }
        else
        {
            //Remove empty rows in the inventory
            int emptyCellsInARow=0;
            for(int i=0; i<itemsPositions.Count; i++)
            {
                if(itemsPositions[i]==null)
                {
                    emptyCellsInARow++;
                }

                if(i%inventoryWidth==0)
                {
                    if(emptyCellsInARow==inventoryWidth && initialInventoryHeight<inventoryHeight)
                    {
                        i-=inventoryWidth;
                        for(int j=0; j<inventoryWidth; j++)
                        {
                            itemsPositions.RemoveAt(i);
                        }
                        inventoryHeight--;
                    }

                    emptyCellsInARow=0;
                }
            }

            //Remove empty rows in sorted inventory if applicable
            if(showItemsSorted)
            {
                emptyCellsInARow=0;
                int currentHeight = sortedPositions.Count/inventoryWidth;
                for(int i=0; i<sortedPositions.Count; i++)
                {
                    if(sortedPositions[i]==null)
                    {
                        emptyCellsInARow++;
                    }

                    if(i%inventoryWidth==0)
                    {
                        if(emptyCellsInARow==inventoryWidth && initialInventoryHeight<currentHeight)
                        {
                            i-=inventoryWidth;
                            for(int j=0; j<inventoryWidth; j++)
                            {
                                sortedPositions.RemoveAt(i);
                            }
                            currentHeight--;
                        }

                        emptyCellsInARow=0;
                    }
                }
            }
        }
    }



    //This method will try to remove provided amount of the provided item from
    //the inventory. If you will ask to remove more items that the inventory has
    //this method will return false and will not remove anything
    public bool RemoveItemsFromInventory(ItemComponent item, int amount)
    {
        //Check that amount is valid
        if(amount>GetTotalAmountOfThisItem(item) || amount<=0)
            return false;

        //Go through every stack until amount to remove becomes 0
        while(true)
        {
            amount = itemsInTheInventory[(item.getItemName(),item.getRareness())][0].DecreaseAmountBy(amount);

            if(amount>=0)
            {
                //If items left in stack is 0 then remove that stack
                if(itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getNumOfItems()==0)
                {
                    if(showItemsSorted)
                    {
                        removeFromSortedFunction(itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]], sortedPositions);
                    }

                    totalItemsWeight-=itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]].getTotalWeight();
                    if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                        itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]]=null;
                    else
                        RemoveItemsBlockByPosition(itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]);
                    itemsInTheInventory[(item.getItemName(),item.getRareness())].RemoveAt(0);
                    totalItems--;
                }

                if(amount==0)
                    break;
            }
            else
                break;
        }

        //If inventory is infinite and a whole row of slots is empty then shhrink inventory
        if(isInventoryInfinite && (inventoryHeight*inventoryWidth)-totalItems>=inventoryWidth+1 && inventoryHeight>initialInventoryHeight)
        {
            ShrinkInfiniteInventory();
        }

        OnChangeUpdateView?.Invoke();
        return true;
    }



    //This method returns total sum of the provided item in the inventory
    public int GetTotalAmountOfThisItem(ItemComponent item)
    {
        if(itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness())))
        {
            int totalAmount=0;
            foreach(ItemStack itemStack in itemsInTheInventory[(item.getItemName(),item.getRareness())])
            {
                totalAmount+=itemStack.getNumOfItems();
            }

            return totalAmount;
        }

        return 0;
    }



    //This method counts number of stacks present of provided item in the inventory
    public int GetNumberOfStacksOfThisItem(ItemComponent item)
    {
        int stacksAmount=0;
        if(itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness())))
        {
            foreach(ItemStack itemStack in itemsInTheInventory[(item.getItemName(),item.getRareness())])
            {
                stacksAmount++;
            }
        }

        return stacksAmount;
    }



    //This method returns wheather inventory has this item or not
    public bool ContainsItem(ItemComponent item)
    {
        return itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness()));
    }



    //This method returns itemStack from the given slot position
    //Returns copy of itemStack at this position
    public ItemStack GetItemStackByPosition(Vector2Int position)
    {
        if(position.x<0 || position.y<0 || position.x>=inventoryWidth || position.y>=inventoryHeight)
            return null;
        
        ItemStack originalStack = null;
        if(showItemsSorted)
        {
            originalStack = sortedPositions[position.y*inventoryWidth+position.x];
        }
        else
        {
            originalStack = itemsPositions[position.y*inventoryWidth+position.x];
        }

        if(originalStack!=null)
        {
            ItemStack copiedStack = new ItemStack(originalStack.getItem(), inventoryConfiguration, originalStack.getCellsOccupied());
            copiedStack.IncreaseAmountBy(originalStack.getNumOfItems());
            return copiedStack;
        }

        return null;
    }



    //This method removes ItemStack from the slot of the given position
    public void RemoveItemStackByPosition(Vector2Int position)
    {
        //if moving items in the inventory is allowed 
        if(inventoryConfiguration.arbitraryStackPlacement)
        {
            //Check for which list position should be applied
            ItemStack stackToRemove;         
            if(showItemsSorted)
            {
                stackToRemove = sortedPositions[position.y*inventoryWidth+position.x];
                    
                if(stackToRemove!=null)
                {
                    removeFromSortedFunction(stackToRemove, sortedPositions);
                }
            }
            else
                stackToRemove = itemsPositions[position.y*inventoryWidth+position.x];
                
            //If slot at the given position not null then delete itemStack at that slot
            if(stackToRemove!=null)
            {
                totalItemsWeight-=itemsPositions[stackToRemove.getCellsOccupied()[0]].getTotalWeight();
                if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                    itemsPositions[stackToRemove.getCellsOccupied()[0]]=null;
                else
                    RemoveItemsBlockByPosition(stackToRemove.getCellsOccupied()[0]);
                itemsInTheInventory[(stackToRemove.getItem().getItemName(),stackToRemove.getItem().getRareness())].Remove(stackToRemove);
                totalItems--;

                if(itemsInTheInventory[(stackToRemove.getItem().getItemName(),stackToRemove.getItem().getRareness())].Count==0)
                    itemsInTheInventory.Remove((stackToRemove.getItem().getItemName(),stackToRemove.getItem().getRareness()));
            }
        }

        //If inventory is infinite and a whole row of slots is empty then shhrink inventory
        if(isInventoryInfinite && (inventoryHeight*inventoryWidth)-totalItems>=inventoryWidth+1 && inventoryHeight>initialInventoryHeight)
        {
            ShrinkInfiniteInventory();
        }

        OnChangeUpdateView?.Invoke();
    }


    //Call when you changed inventory width and/or height and want to resize it
    //Returns the list of items which did not fit in the new inventory (if you 
    //decreased size of the inventory and not all itemStacks fit into new inventory)
    public List<ItemStack> ResizeInventory()
    {
        //Infinite inventories cannot be resized this way!
        if(!isInventoryInfinite)
        {
            //Set new sizes
            int previousHeight = inventoryHeight;
            int previousWidth = inventoryWidth;
            inventoryHeight = newInventoryHeight<0 ? inventoryHeight : newInventoryHeight;
            initialInventoryHeight = inventoryHeight;
            inventoryWidth = newInventoryWidth<0 ? inventoryWidth : newInventoryWidth;

            //Remove all stored slot positions, because new ones will be inserted
            foreach(KeyValuePair<(string, Rareness), List<ItemStack>> kvp in itemsInTheInventory)
            {
                foreach(ItemStack stack in kvp.Value)
                {
                    stack.getCellsOccupied().Clear();
                }
            }

            //Create resized list of slots
            List<ItemStack> resizedItemsPositions = new List<ItemStack>();
            for(int i=0; i<inventoryWidth*inventoryHeight; i++)
            {
                resizedItemsPositions.Add(null);
            }

            //If resized inventory is larger than the previous one then just
            //copy all itemStacks into resized list
            if(previousHeight<=inventoryHeight && previousWidth<=inventoryWidth)
            {
                if(previousWidth==inventoryWidth)
                {
                    for(int i=0; i<itemsPositions.Count; i++)
                    {
                        resizedItemsPositions[i] = itemsPositions[i];
                        resizedItemsPositions[i].getCellsOccupied().Add(i);
                    }
                }
                else
                {
                    int newI = 0;
                    for(int i=0; i<itemsPositions.Count; i++)
                    {
                        if(i%previousWidth==0)
                            newI+=inventoryWidth;
                        resizedItemsPositions[newI+(i/previousWidth)] = itemsPositions[i];
                        resizedItemsPositions[newI+(i/previousWidth)].getCellsOccupied().Add(newI+(i/previousWidth));
                    }
                }

                itemsPositions = resizedItemsPositions;

                if(showItemsSorted)
                    sortedPositions = sortFunction(itemsPositions);
                OnChangeUpdateView?.Invoke();
                return null;
            }
            else
            {
                //Otherwise copy as much itemStacks into resized list as possible
                List<ItemStack> itemStacksRemoved = null;

                //Check if inventory is sigle celled then just copy all stacks and return 
                //those which not fit 
                if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                {
                    int positionsSkipped=0;
                    bool copiedAllItems=false;
                    for(int i=0; i<resizedItemsPositions.Count; i++)
                    {
                        if(itemsPositions[i]!=null)
                        {
                            resizedItemsPositions[i] = itemsPositions[i+positionsSkipped];
                            resizedItemsPositions[i].getCellsOccupied().Add(i);
                        }
                        else
                            positionsSkipped++;

                        //If all items were copied then complete resizing
                        if(positionsSkipped+i>=resizedItemsPositions.Count)
                        {
                            copiedAllItems = true;
                            break;
                        }
                    }

                    //If there are still some items left then add them to list of
                    //removed itemStacks
                    if(!copiedAllItems)
                    {
                        itemStacksRemoved = new List<ItemStack>();
                        for(int i=resizedItemsPositions.Count+positionsSkipped; i<itemsPositions.Count; i++)
                        {
                            if(itemsPositions[i]!=null)
                            {
                                itemStacksRemoved.Add(itemsPositions[i]);
                                totalItemsWeight-=itemsPositions[itemsPositions[i].getCellsOccupied()[0]].getTotalWeight();
                                totalItems--;
                                itemsInTheInventory[(itemsPositions[i].getItem().getItemName(),itemsPositions[i].getItem().getRareness())].Remove(itemsPositions[i]);
                                if(itemsInTheInventory[(itemsPositions[i].getItem().getItemName(),itemsPositions[i].getItem().getRareness())].Count==0)
                                    itemsInTheInventory.Remove((itemsPositions[i].getItem().getItemName(),itemsPositions[i].getItem().getRareness()));
                            }
                        }
                    }

                    itemsPositions = resizedItemsPositions;
                    if(showItemsSorted)
                        sortedPositions = sortFunction(itemsPositions);
                    OnChangeUpdateView?.Invoke();
                    return itemStacksRemoved;
                }
                else
                {
                    //Others more rigours check on whether the item can be fit should be done
                    List<ItemStack> itemStacksWhichWillStay = new List<ItemStack>();
                    itemStacksRemoved = new List<ItemStack>();

                    int newListPosition = 0;
                    for(int j=0; j<itemsPositions.Count; j++)
                    {
                        if(j%previousWidth==0)
                            newListPosition+=inventoryWidth;

                        if(itemsPositions[j]!=null)
                        {
                            if(itemStacksWhichWillStay.Contains(itemsPositions[j]))
                            {
                                resizedItemsPositions[newListPosition+(j%previousWidth)] = itemsPositions[j];
                                resizedItemsPositions[newListPosition+(j%previousWidth)].getCellsOccupied().Add(newListPosition+(j%previousWidth));
                            }
                            else
                            {
                                if(IsEnoughSpaceForItem(itemsPositions[j], new Vector2Int(j%previousWidth, j/previousWidth), resizedItemsPositions, new Vector2Int(inventoryWidth, inventoryHeight)))
                                {
                                    itemStacksWhichWillStay.Add(itemsPositions[j]);
                                    resizedItemsPositions[newListPosition+(j%previousWidth)] = itemsPositions[j];
                                    resizedItemsPositions[newListPosition+(j%previousWidth)].getCellsOccupied().Add(newListPosition+(j%previousWidth));
                                }
                                else
                                {
                                    itemStacksRemoved.Add(itemsPositions[j]);
                                    totalItemsWeight-=itemsPositions[j].getTotalWeight();
                                    totalItems--;
                                    itemsInTheInventory[(itemsPositions[j].getItem().getItemName(),itemsPositions[j].getItem().getRareness())].Remove(itemsPositions[j]);
                                    if(itemsInTheInventory[(itemsPositions[j].getItem().getItemName(),itemsPositions[j].getItem().getRareness())].Count==0)
                                        itemsInTheInventory.Remove((itemsPositions[j].getItem().getItemName(),itemsPositions[j].getItem().getRareness()));
                                    RemoveItemsBlockByPosition(j, new Vector2Int(previousWidth, previousHeight));
                                }
                            }
                        }
                    }

                    itemsPositions = resizedItemsPositions;
                    if(showItemsSorted)
                        sortedPositions = sortFunction(itemsPositions);
                    OnChangeUpdateView?.Invoke();
                    return itemStacksRemoved;
                }
            }
        }

        return null;
    }



    //This method allows you to give your custom sorting functions, and inventory
    //will show this inventory sorted by the provided functions.
    //sortFunction sort the hwole unsorted list, addToSortedListFunction adds new
    //item to already sorted list (IMPORTANT! For your custom add function the
    //length of the sortedList should remain the same before and after this function!)
    public void ShowInventorySorted(Func<List<ItemStack>, List<ItemStack>> sortFunction, Action<ItemStack, List<ItemStack>> addToSortedListFunction, Action<ItemStack, List<ItemStack>> removeFromSortedListFunction)
    {
        this.sortFunction = sortFunction;
        addToSortedFunction = addToSortedListFunction;
        removeFromSortedFunction = removeFromSortedListFunction;

        sortedPositions = this.sortFunction(itemsPositions);
        showItemsSorted = true;

        OnChangeUpdateView?.Invoke();
    }



    //Cancel sorting list
    public void CancelShowSortedInventory()
    {
        showItemsSorted = false;
        OnChangeUpdateView?.Invoke();
    }



    //These two are just redirections to search functions
    private List<ItemStack> SearchRedirect(List<ItemStack> list)
    {
        return InventorySortingFunctions.LinearSearchByName(list, searchTarget);
    }

    private void AddSearchRedirect(ItemStack stack, List<ItemStack> list)
    {
        InventorySortingFunctions.addItemToSearchResultByName(stack, list, searchTarget);
    }



    //This method searches all items in the inventory by name and leaves only those
    //items whose names contains the provided substring
    public void SearchByNamePart(string namePart)
    {
        searchTarget = namePart;
        ShowInventorySorted(SearchRedirect, AddSearchRedirect, InventorySortingFunctions.simpleRemoveFuction);
    }
}
