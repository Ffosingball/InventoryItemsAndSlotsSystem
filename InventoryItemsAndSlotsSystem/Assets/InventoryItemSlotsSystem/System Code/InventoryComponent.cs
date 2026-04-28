using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;


//For the position in the inventory count from (0,0)
public class InventoryComponent : MonoBehaviour
{
    public event Action OnResizing;

    [SerializeField] private bool isInventoryInfinite = false;
    [SerializeField] private int inventoryWidth=10, inventoryHeight=10;
    [Header("How many cells of the inventory is visible in a single view, if height is more than visible height then you will need to scroll an inventory")]
    [SerializeField] private string inventoryName = null;
    [SerializeField] private InventoryConfiguration inventoryConfiguration;

    private Dictionary<(string, Rareness), List<ItemStack>> itemsInTheInventory;
    private List<ItemStack> itemsPositions, sortedPositions;
    private int newInventoryWidth=-1, newInventoryHeight=-1, initialInventoryHeight;
    private int totalItems=0;
    private bool showItemsSorted=false;
    private Func<List<ItemStack>, List<ItemStack>> sortFunction;
    private Action<ItemStack, List<ItemStack>> addToSortedFunction;
    //private Action<ItemStack, List<ItemStack>> removeFromSortedFunction;
    private string searchTarget = "";


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

    public List<ItemStack> getItemsInTheInventory()
    {
        return itemsPositions;
    }

    public List<ItemStack> getSortedListOfItems()
    {
        return sortedPositions;
    }

    //public int getTotalItems()
    //{
    //    return totalItems;
    //}



    private void Start()
    {
        if(inventoryName==null)
            inventoryName = System.Guid.NewGuid().ToString();

        itemsInTheInventory = new Dictionary<(string, Rareness),List<ItemStack>>();
        
        itemsPositions = new List<ItemStack>();
        for(int i=0; i<inventoryWidth*inventoryHeight; i++)
        {
            itemsPositions.Add(null);
        }

        initialInventoryHeight = inventoryHeight;
    }


    //use this to move itemStack from one inventory to other!
    public NewItemPlacementResult PlaceItemStackToInventory(ItemStack itemStack, Vector2Int position)
    {
        if(inventoryConfiguration.arbitraryStackPlacement)
        {
            int arrPosition = position.y*inventoryWidth+position.x;
            NewItemPlacementResult placementResult = new NewItemPlacementResult();

            if(itemsPositions.Count<=arrPosition || arrPosition<0)
            {
                placementResult.invalidPosition=true;
                return placementResult;
            }

            if(showItemsSorted)
            {
                bool foundEmptySpace = false;
                for(int i=0; i<itemsPositions.Count; i++)
                {
                    if(itemsPositions[i]==null)
                    {
                        foundEmptySpace = true;
                        position.x = i%inventoryWidth;
                        position.y = i/inventoryWidth;
                        arrPosition = position.y*inventoryWidth+position.x;
                        break;
                    }
                }

                if(!foundEmptySpace)
                {
                    //placementResult.stackReplaced = sortedPositions[arrPosition];
                    position.x = sortedPositions[arrPosition].getCellsOccupied()[0]%inventoryWidth;
                    position.y = sortedPositions[arrPosition].getCellsOccupied()[0]/inventoryWidth;
                    arrPosition = position.y*inventoryWidth+position.x;
                    //sortedPositions.Remove(placementResult.stackReplaced);
                    //sortedPositions.Add(null);

                    //Debug.Log("After removal: "+sortedPositions.Count);
                }
            }


            if(itemsPositions[arrPosition]!=null)
            {
                totalItems--;
                if(itemsPositions[arrPosition].getItem()==itemStack.getItem())
                {
                    int overflow = itemStack.IncreaseAmountBy(itemsPositions[arrPosition].getNumOfItems());
                    if(overflow>0)
                    {
                        itemsPositions[arrPosition].DecreaseAmountBy(itemsPositions[arrPosition].getNumOfItems()-overflow);
                        placementResult.stackReplaced = itemsPositions[arrPosition];
                        if(showItemsSorted)
                        {
                            sortedPositions.Remove(placementResult.stackReplaced);
                            sortedPositions.Add(null);
                            //Debug.Log("After removal: "+sortedPositions.Count);
                        }
                    }
                }
                else
                {
                    if(!inventoryConfiguration.itemStacksLimit)
                    {
                        placementResult.stackReplaced = itemsPositions[arrPosition];
                        if(showItemsSorted)
                        {
                            sortedPositions.Remove(placementResult.stackReplaced);
                            sortedPositions.Add(null);
                            //Debug.Log("After removal: "+sortedPositions.Count);
                        }
                    }
                    else
                    {
                        if(itemStack.getItem().getItemStackLimit()>GetNumberOfStacksOfThisItem(itemStack.getItem()))
                        {
                            placementResult.stackReplaced = itemsPositions[arrPosition];
                            if(showItemsSorted)
                            {
                                sortedPositions.Remove(placementResult.stackReplaced);
                                sortedPositions.Add(null);
                                //Debug.Log("After removal: "+sortedPositions.Count);
                            }
                        }
                    }
                }

                ItemComponent existingItem = itemsPositions[arrPosition].getItem();
                itemsInTheInventory[(existingItem.getItemName(),existingItem.getRareness())].Remove(itemsPositions[arrPosition]);
                if(itemsInTheInventory[(existingItem.getItemName(),existingItem.getRareness())].Count==0)
                    itemsInTheInventory.Remove((existingItem.getItemName(),existingItem.getRareness()));
            }


            if(!inventoryConfiguration.itemStacksLimit || itemStack.getItem().getItemStackLimit()>GetNumberOfStacksOfThisItem(itemStack.getItem()))
            {
                ItemComponent newItem = itemStack.getItem();
                if(!itemsInTheInventory.ContainsKey((newItem.getItemName(),newItem.getRareness())))
                    itemsInTheInventory.Add((newItem.getItemName(),newItem.getRareness()), new List<ItemStack>());

                itemStack.getCellsOccupied()[0]=arrPosition;
                if(showItemsSorted)
                    addToSortedFunction(itemStack, sortedPositions);

                itemsInTheInventory[(newItem.getItemName(),newItem.getRareness())].Add(itemStack);
                itemsPositions[arrPosition] = itemStack;
                totalItems++;
                //Debug.Log(inventoryName+") "+totalItems);

                if(isInventoryInfinite && (inventoryHeight*inventoryWidth)-totalItems<=1)
                {
                    //Debug.Log("Start Growing!");
                    //itemsPositions.Add(null);

                    for(int i=0; i<inventoryWidth; i++)
                    {
                        itemsPositions.Add(null);
                        if(showItemsSorted)
                            sortedPositions.Add(null);
                    }
                    //Debug.Log("After adding more slots: "+sortedPositions.Count);
                    inventoryHeight++;
                    //Debug.Log(itemsPositions.Count);
                }

                return placementResult;
            }
            else
            {
                placementResult.stackCapReached=true;
                return placementResult;
            }
        }

        return null;
    }


    //Call it when new item is added to the inventory not by the player
    public int AddItemsToInventory(ItemComponent item, int amount)
    {
        if(amount<1)
            return -1;

        if(itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness())))
        {
            //Debug.Log("Item already in the inventory!");
            foreach(ItemStack itemStack in itemsInTheInventory[(item.getItemName(),item.getRareness())])
            {
                if(!itemStack.getIsFull())
                {
                    amount = itemStack.IncreaseAmountBy(amount);
                    
                    if(showItemsSorted)
                    {
                        sortedPositions.Remove(itemStack);
                        sortedPositions.Add(null);
                        addToSortedFunction(itemStack, sortedPositions);
                    }

                    if(amount<=0)
                        return 0;
                }
            }
        }
        else
            itemsInTheInventory.Add((item.getItemName(),item.getRareness()), new List<ItemStack>());

        for(int i=0; i<itemsPositions.Count; i++)
        {
            if(itemsPositions[i]==null)
            {
                //Debug.Log("Found empty cell!");
                if(!inventoryConfiguration.itemStacksLimit || item.getItemStackLimit()>GetNumberOfStacksOfThisItem(item))
                {
                    //Debug.Log("Stack limit not reached!");
                    ItemStack newItemStack = new ItemStack(item, inventoryConfiguration, i);
                    amount = newItemStack.IncreaseAmountBy(amount);
                    totalItems++;

                    if(showItemsSorted)
                        addToSortedFunction(newItemStack, sortedPositions);

                    itemsPositions[i] = newItemStack;
                    itemsInTheInventory[(item.getItemName(),item.getRareness())].Add(newItemStack);

                    if(amount<=0)
                    {
                        //Debug.Log(inventoryName+") "+totalItems);
                        return 0;
                    }
                }
                else
                {
                    //Debug.Log(inventoryName+") "+totalItems);
                    return amount;
                }
            }
        }

        if(isInventoryInfinite)
        {
            int numOfStacksWillAppear = amount/item.getMaxNumberOfBlocksInAStack();
            if(numOfStacksWillAppear%item.getMaxNumberOfBlocksInAStack()!=0)
                numOfStacksWillAppear++;

            int heightToAdd = numOfStacksWillAppear/inventoryWidth;
            if(numOfStacksWillAppear%inventoryWidth!=0)
                heightToAdd++;

            for(int i=0; i<heightToAdd*inventoryWidth; i++)
            {
                itemsPositions.Add(null);
                if(showItemsSorted)
                    sortedPositions.Add(null);
            }
            /*while(itemsPositions.Count%inventoryWidth!=0)
            {
                //Debug.Log(itemsPositions.Count);
                itemsPositions.Add(null);
            }*/
            inventoryHeight+=heightToAdd;

            for(int i=(inventoryHeight-heightToAdd)*inventoryWidth; i<itemsPositions.Count; i++)
            {
                if(itemsPositions[i]==null)
                {
                    if(!inventoryConfiguration.itemStacksLimit || item.getItemStackLimit()>GetNumberOfStacksOfThisItem(item))
                    {
                        ItemStack newItemStack = new ItemStack(item, inventoryConfiguration, i);
                        amount = newItemStack.IncreaseAmountBy(amount);
                        totalItems++;

                        if(showItemsSorted)
                            addToSortedFunction(newItemStack, sortedPositions);

                        itemsPositions[i] = newItemStack;
                        itemsInTheInventory[(item.getItemName(),item.getRareness())].Add(newItemStack);

                        if(amount<=0)
                        {
                            //Debug.Log(itemsPositions.Count);
                            return 0;
                        }
                    }
                }
            }

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

        //Debug.Log(inventoryName+") "+totalItems);
        return amount;
    }


    
    private void ShrinkInfiniteInventory()
    {
        int j = 0;
        //Debug.Log("Length: "+itemsPositions.Count);
        for(int i=itemsPositions.Count-1; i>0; i--)
        {
            if(itemsPositions[i]!=null)
            {
                while(itemsPositions[j]!=null)
                {
                    j++;
                    //Debug.Log("i: "+i+"; j: "+j);

                    if(j>=i)
                        break;
                }

                if(j>=i)
                    break;

                itemsPositions[j] = itemsPositions[i];
                itemsPositions[j].getCellsOccupied().RemoveAt(0);
                itemsPositions[j].getCellsOccupied().Add(j);
            }

            itemsPositions.RemoveAt(i);
            if(showItemsSorted)
                sortedPositions.RemoveAt(sortedPositions.Count-1);
        }

        j++;

        if(j>initialInventoryHeight*inventoryWidth)
        {
            //Debug.Log("J: "+j);
            inventoryHeight=j/inventoryWidth;
            if(j%inventoryWidth!=0)
                inventoryHeight++;

            while(itemsPositions.Count%inventoryWidth!=0)
            {
                itemsPositions.Add(null);
                if(showItemsSorted)
                    sortedPositions.Add(null);
            }
        }
        else
        {
            inventoryHeight=initialInventoryHeight;
            for(int i=j; i<inventoryHeight*inventoryWidth; i++)
            {
                itemsPositions.Add(null);
                if(showItemsSorted)
                    sortedPositions.Add(null);
            }
            /*while(itemsPositions.Count%inventoryWidth!=0)
            {
                //Debug.Log(itemsPositions.Count);
                itemsPositions.Add(null);
            }*/
        }

        //Debug.Log(itemsPositions.Count);
    }



    public bool RemoveItemsFromInventory(ItemComponent item, int amount)
    {
        //COMPLETE HERE!
        if(amount>GetTotalAmountOfThisItem(item) || amount<=0)
            return false;

        if(itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness())))
        {
            while(true)
            {
                amount = itemsInTheInventory[(item.getItemName(),item.getRareness())][0].DecreaseAmountBy(amount);

                if(amount>0)
                {
                    if(showItemsSorted)
                    {
                        sortedPositions.Remove(itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]]);
                        sortedPositions.Add(null);
                    }

                    itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]]=null;
                    itemsInTheInventory[(item.getItemName(),item.getRareness())].RemoveAt(0);
                    totalItems--;
                }
                else if(amount==0)
                {
                    if(itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getNumOfItems()==0)
                    {
                        if(showItemsSorted)
                        {
                            sortedPositions.Remove(itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]]);
                            sortedPositions.Add(null);
                        }

                        itemsPositions[itemsInTheInventory[(item.getItemName(),item.getRareness())][0].getCellsOccupied()[0]]=null;
                        itemsInTheInventory[(item.getItemName(),item.getRareness())].RemoveAt(0);
                        totalItems--;
                    }

                    break;
                }
                else
                    break;
            }
        }

        if(inventoryHeight>initialInventoryHeight)
        {
            ShrinkInfiniteInventory();
        }

        //Debug.Log(inventoryName+") "+totalItems);
        return true;
    }


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


    public bool ContainsItem(ItemComponent item)
    {
        return itemsInTheInventory.ContainsKey((item.getItemName(),item.getRareness()));
    }


    public ItemStack GetItemStackByPosition(Vector2Int position)
    {
        if(position.x<0 || position.y<0 || position.x>=inventoryWidth || position.y>=inventoryHeight)
            return null;

        //Debug.Log("Get pos: "+(position.y*inventoryWidth+position.x)+"; h: "+inventoryHeight+"; itPos.Count: "+itemsPositions.Count);
        if(showItemsSorted)
            return sortedPositions[position.y*inventoryWidth+position.x];
        else
            return itemsPositions[position.y*inventoryWidth+position.x];
    }


    public void RemoveItemStackByPosition(Vector2Int position)
    {
        if(inventoryConfiguration.arbitraryStackPlacement)
        {
            ItemStack stackToRemove;         
            if(showItemsSorted)
            {
                stackToRemove = sortedPositions[position.y*inventoryWidth+position.x];
                    
                if(stackToRemove!=null)
                {
                    sortedPositions.Remove(stackToRemove);
                    sortedPositions.Add(null);
                }
            }
            else
                stackToRemove = itemsPositions[position.y*inventoryWidth+position.x];
                
            if(stackToRemove!=null)
            {
                itemsPositions[stackToRemove.getCellsOccupied()[0]]=null;
                itemsInTheInventory[(stackToRemove.getItem().getItemName(),stackToRemove.getItem().getRareness())].Remove(stackToRemove);
                totalItems--;

                if(itemsInTheInventory[(stackToRemove.getItem().getItemName(),stackToRemove.getItem().getRareness())].Count==0)
                    itemsInTheInventory.Remove((stackToRemove.getItem().getItemName(),stackToRemove.getItem().getRareness()));
            }
        }

        //Debug.Log(inventoryName+") "+totalItems);

        if(isInventoryInfinite && (inventoryHeight*inventoryWidth)-totalItems>=inventoryWidth+1 && inventoryHeight>initialInventoryHeight)
        {
            ShrinkInfiniteInventory();
        }
    }


    //Call when you changed inventory sizes and want to resize it
    //Returns the list of items which did not fit in the new inventory
    //(important if you decrease size of the inventory)
    public List<ItemStack> ResizeInventory()
    {
        if(!isInventoryInfinite)
        {
            inventoryHeight = newInventoryHeight<0 ? inventoryHeight : newInventoryHeight;
            initialInventoryHeight = inventoryHeight;
            inventoryWidth = newInventoryWidth<0 ? inventoryWidth : newInventoryWidth;

            List<ItemStack> resizedItemsPositions = new List<ItemStack>();
            for(int i=0; i<inventoryWidth*inventoryHeight; i++)
            {
                resizedItemsPositions.Add(null);
            }

            if(itemsPositions.Count<resizedItemsPositions.Count)
            {
                for(int i=0; i<itemsPositions.Count; i++)
                {
                    resizedItemsPositions[i] = itemsPositions[i];
                }

                itemsPositions = resizedItemsPositions;

                if(showItemsSorted)
                    sortedPositions = sortFunction(itemsPositions);
                OnResizing?.Invoke();
                return null;
            }
            else
            {
                int positionsSkipped=0;
                for(int i=0; i<resizedItemsPositions.Count; i++)
                {
                    if(itemsPositions[i]!=null)
                        resizedItemsPositions[i] = itemsPositions[i+positionsSkipped];
                    else
                        positionsSkipped++;

                    if(positionsSkipped+i>=resizedItemsPositions.Count)
                    {
                        itemsPositions = resizedItemsPositions;
                        
                        if(showItemsSorted)
                            sortedPositions = sortFunction(itemsPositions);
                        OnResizing?.Invoke();
                        return null;
                    }
                }

                List<ItemStack> itemStacksRemoved = new List<ItemStack>();
                for(int i=resizedItemsPositions.Count+positionsSkipped; i<itemsPositions.Count; i++)
                {
                    if(itemsPositions[i]!=null)
                        itemStacksRemoved.Add(itemsPositions[i]);
                }

                itemsPositions = resizedItemsPositions;

                if(showItemsSorted)
                    sortedPositions = sortFunction(itemsPositions);
                OnResizing?.Invoke();
                return itemStacksRemoved;
            }
        }

        return null;
    }



    public void ShowInventorySorted(Func<List<ItemStack>, List<ItemStack>> sortFunction, Action<ItemStack, List<ItemStack>> addToSortedListFunction)
    {
        this.sortFunction = sortFunction;
        addToSortedFunction = addToSortedListFunction;
        //removeFromSortedFunction = removeFromSortedListFunction;

        sortedPositions = this.sortFunction(itemsPositions);
        showItemsSorted = true;

        //Debug.Log("sorted: "+sortedPositions.Count+"; unsorted: "+itemsPositions.Count);
    }



    public void CancelShowSortedInventory()
    {
        showItemsSorted = false;
    }



    private List<ItemStack> SearchRedirect(List<ItemStack> list)
    {
        return InventorySortingFunctions.LinearSearchByName(list, searchTarget);
    }


    private void AddSearchRedirect(ItemStack stack, List<ItemStack> list)
    {
        InventorySortingFunctions.addItemToSearchResultByName(stack, list, searchTarget);
    }


    public void SearchByNamePart(string namePart)
    {
        searchTarget = namePart;
        ShowInventorySorted(SearchRedirect, AddSearchRedirect);
    }
}
