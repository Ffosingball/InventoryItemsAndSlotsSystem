using System;
using System.Collections.Generic;
using UnityEngine;


//This is a place where you can place your custom sorting and/or search functions for the 
//inventoryComponent
public static class InventorySortingFunctions
{
    //This method just calls sortInventoryByRarenessASCAfterByAmountDESC method
    //and reverses its result
    public static List<ItemStack> sortInventoryByRarenessDESCAfterByAmountASC(List<ItemStack> unsortedList)
    {
        List<ItemStack> sortedList = sortInventoryByRarenessASCAfterByAmountDESC(unsortedList);

        int endPosition = -1;
        for(int i=0; i<sortedList.Count; i++)
        {
            if(sortedList[i]==null)
            {
                endPosition=i-1;
                break;
            }
        }

        if(endPosition==-1)
            endPosition = sortedList.Count-1;

        for(int i=0; i<sortedList.Count; i++)
        {
            if(i>=endPosition-i)
                break;

            ItemStack itemStack = sortedList[i];
            sortedList[i] = sortedList[endPosition-i];
            sortedList[endPosition-i] = itemStack;
        }

        return sortedList;
    }



    //This method sorts all items in the inventory by rareness ascending and in each
    //rareness category by amount of items descending
    public static List<ItemStack> sortInventoryByRarenessASCAfterByAmountDESC(List<ItemStack> unsortedList)
    {
        List<ItemStack> intermediateList = rarenessMergeSort(unsortedList, 0, unsortedList.Count-1);

        List<ItemStack> sortedList = new List<ItemStack>();
        int start = 0;
        bool areaSelected = false;
        for(int i=0; i<intermediateList.Count+1; i++)
        {
            if(i==intermediateList.Count)
                areaSelected = true;

            if(!areaSelected)
            {
                if(intermediateList[i]==null)
                    areaSelected = true;
            }

            if(!areaSelected)
            {
                if(intermediateList[start].getItem().getRareness()!=intermediateList[i].getItem().getRareness())
                    areaSelected = true;
            }

            if(areaSelected)
            {
                if(i-start>1)
                {
                    List<ItemStack> partList = itemAmountMergeSort(intermediateList, start, i-1);

                    foreach(ItemStack itemStack in partList)
                    {
                        sortedList.Add(itemStack);
                    }

                    start = i;
                }
                else if(i-start==1)
                    sortedList.Add(intermediateList[start]);
                else
                    Debug.Log("Unreachable message! Start: "+start+"; i: "+i);

                start = i;
            }
            else
            {
                if(intermediateList[i]==null)
                    break;
            }

            areaSelected = false;
        }

        while(sortedList.Count<unsortedList.Count)
        {
            sortedList.Add(null);
        }

        return sortedList;
    }



    //Recursive merge sort by rareness
    private static List<ItemStack> rarenessMergeSort(List<ItemStack> unsortedList, int start, int end)
    {
        List<ItemStack> sortedList = new List<ItemStack>();

        if(end-start==1)
        {
            if(unsortedList[start]==null && unsortedList[end]==null)
            {
                sortedList.Add(null);
                sortedList.Add(null);
            }
            else if(unsortedList[start]==null)
            {
                sortedList.Add(unsortedList[end]);
                sortedList.Add(null);
            }
            else if(unsortedList[end]==null)
            {
                sortedList.Add(unsortedList[start]);
                sortedList.Add(null);
            }
            else if(unsortedList[start].getItem().getRareness()<=unsortedList[end].getItem().getRareness())
            {
                sortedList.Add(unsortedList[start]);
                sortedList.Add(unsortedList[end]);
            }
            else
            {
                sortedList.Add(unsortedList[end]);
                sortedList.Add(unsortedList[start]);
            }
        }
        else if(end-start==0)
            sortedList.Add(unsortedList[start]);
        else if(end-start<0)
            return sortedList;
        else
        {
            List<ItemStack> leftPart = rarenessMergeSort(unsortedList, start, end-((end-start)/2));
            List<ItemStack> rightPart = rarenessMergeSort(unsortedList, end-((end-start)/2)+1, end);

            while(true)
            {
                if(leftPart.Count==0 && rightPart.Count==0)
                    break;
                else if(leftPart.Count==0)
                {
                    foreach(ItemStack itemStack in rightPart)
                    {
                        sortedList.Add(itemStack);
                    }
                    break;
                }
                else if(rightPart.Count==0)
                {
                    foreach(ItemStack itemStack in leftPart)
                    {
                        sortedList.Add(itemStack);
                    }
                    break;
                }
                else
                {
                    if(leftPart[0]==null && rightPart[0]==null)
                    {
                        foreach(ItemStack itemStack in rightPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        foreach(ItemStack itemStack in leftPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        break;
                    }
                    else if(leftPart[0]==null)
                    {
                        foreach(ItemStack itemStack in rightPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        foreach(ItemStack itemStack in leftPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        break;
                    }
                    else if(rightPart[0]==null)
                    {
                        foreach(ItemStack itemStack in leftPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        foreach(ItemStack itemStack in rightPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        break;
                    }
                    else if(leftPart[0].getItem().getRareness()<=rightPart[0].getItem().getRareness())
                    {
                        sortedList.Add(leftPart[0]);
                        leftPart.RemoveAt(0);
                    }
                    else
                    {
                        sortedList.Add(rightPart[0]);
                        rightPart.RemoveAt(0);
                    }
                }
            }
        }

        return sortedList;
    }



    //Recursive merge sort by amount
    private static List<ItemStack> itemAmountMergeSort(List<ItemStack> unsortedList, int start, int end)
    {
        List<ItemStack> sortedList = new List<ItemStack>();

        if(end-start==1)
        {
            if(unsortedList[start]==null && unsortedList[end]==null)
            {
                sortedList.Add(null);
                sortedList.Add(null);
            }
            else if(unsortedList[start]==null)
            {
                sortedList.Add(unsortedList[end]);
                sortedList.Add(null);
            }
            else if(unsortedList[end]==null)
            {
                sortedList.Add(unsortedList[start]);
                sortedList.Add(null);
            }
            else if(unsortedList[start].getNumOfItems()<=unsortedList[end].getNumOfItems())
            {
                sortedList.Add(unsortedList[start]);
                sortedList.Add(unsortedList[end]);
            }
            else
            {
                sortedList.Add(unsortedList[end]);
                sortedList.Add(unsortedList[start]);
            }
        }
        else if(end-start==0)
            sortedList.Add(unsortedList[start]);
        else if(end-start<0)
            return sortedList;
        else
        {
            List<ItemStack> leftPart = itemAmountMergeSort(unsortedList, start, end-((end-start)/2));
            List<ItemStack> rightPart = itemAmountMergeSort(unsortedList, end-((end-start)/2)+1, end);

            while(true)
            {
                if(leftPart.Count==0 && rightPart.Count==0)
                    break;
                else if(leftPart.Count==0)
                {
                    foreach(ItemStack itemStack in rightPart)
                    {
                        sortedList.Add(itemStack);
                    }
                    break;
                }
                else if(rightPart.Count==0)
                {
                    foreach(ItemStack itemStack in leftPart)
                    {
                        sortedList.Add(itemStack);
                    }
                    break;
                }
                else
                {
                    if(leftPart[0]==null && rightPart[0]==null)
                    {
                        foreach(ItemStack itemStack in rightPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        foreach(ItemStack itemStack in leftPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        break;
                    }
                    else if(leftPart[0]==null)
                    {
                        foreach(ItemStack itemStack in rightPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        foreach(ItemStack itemStack in leftPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        break;
                    }
                    else if(rightPart[0]==null)
                    {
                        foreach(ItemStack itemStack in leftPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        foreach(ItemStack itemStack in rightPart)
                        {
                            sortedList.Add(itemStack);
                        }

                        break;
                    }
                    else if(leftPart[0].getNumOfItems()<=rightPart[0].getNumOfItems())
                    {
                        sortedList.Add(leftPart[0]);
                        leftPart.RemoveAt(0);
                    }
                    else
                    {
                        sortedList.Add(rightPart[0]);
                        rightPart.RemoveAt(0);
                    }
                }
            }
        }

        return sortedList;
    }



    //This method adds item to already sorted list
    public static void addItemToSortedListByRarenessDESC(ItemStack stackToInsert, List<ItemStack> sortedList)
    {
        for(int i=0; i<sortedList.Count; i++)
        {
            if(sortedList[i]==null)
            {
                sortedList[i]=stackToInsert;
                break;
            }
            if((sortedList[i].getItem().getRareness()==stackToInsert.getItem().getRareness() && sortedList[i].getNumOfItems()<stackToInsert.getNumOfItems()) || sortedList[i].getItem().getRareness()<stackToInsert.getItem().getRareness())
            {
                sortedList.Insert(i, stackToInsert);
                sortedList.RemoveAt(sortedList.Count-1);
                break;
            }
        }
    }



    //This method adds item to already sorted list
    public static void addItemToSortedListByRarenessASC(ItemStack stackToInsert, List<ItemStack> sortedList)
    {
        for(int i=0; i<sortedList.Count; i++)
        {
            if(sortedList[i]==null)
            {
                sortedList[i]=stackToInsert;
                break;
            }

            if((sortedList[i].getItem().getRareness()==stackToInsert.getItem().getRareness() && sortedList[i].getNumOfItems()>stackToInsert.getNumOfItems()) || sortedList[i].getItem().getRareness()>stackToInsert.getItem().getRareness())
            {
                sortedList.Insert(i, stackToInsert);
                sortedList.RemoveAt(sortedList.Count-1);
                break;
            }
        }
    }


    //Search method by substring in the item name
    public static List<ItemStack> LinearSearchByName(List<ItemStack> list, string targetPart)
    {
        List<ItemStack> searchResultList = new List<ItemStack>();

        foreach(ItemStack stack in list)
        {
            if(stack!=null)
            {
                if(stack.getItem().getItemName().Contains(targetPart, StringComparison.OrdinalIgnoreCase))
                {
                    searchResultList.Add(stack);
                }
            }
        }

        while(searchResultList.Count<list.Count)
        {
            searchResultList.Add(null);
        }

        return searchResultList;
    }



    //This method adds item to the search result list
    public static void addItemToSearchResultByName(ItemStack stackToInsert, List<ItemStack> searchList, string targetPart)
    {
        if(stackToInsert.getItem().getItemName().Contains(targetPart, StringComparison.OrdinalIgnoreCase))
        {
            for(int i=0; i<searchList.Count; i++)
            {
                if(searchList[i]==null)
                {
                    searchList[i]=stackToInsert;
                    break;
                }
            }
        }
    }
}