using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class InventorySortingFunctions
{
    public static List<ItemStack> sortInventoryByRarenessDESCAfterByAmountASC(List<ItemStack> unsortedList)
    {
        //Debug.Log("DESC");
        List<ItemStack> sortedList = sortInventoryByRarenessASCAfterByAmountDESC(unsortedList);

        int endPosition = -1;
        for(int i=0; i<sortedList.Count; i++)
        {
            //Debug.Log("i: "+i);
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

            //Debug.Log("Swap: "+i+" - "+(endPosition-i));
            ItemStack itemStack = sortedList[i];
            sortedList[i] = sortedList[endPosition-i];
            sortedList[endPosition-i] = itemStack;
        }

        return sortedList;
    }



    public static List<ItemStack> sortInventoryByRarenessASCAfterByAmountDESC(List<ItemStack> unsortedList)
    {
        //Debug.Log("ASC");
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

        /*Debug.Log("Start: "+start+"; end: "+end);
        foreach(ItemStack stack in sortedList)
        {
            Debug.Log("Num: "+stack.getNumOfItems());
        }*/

        return sortedList;
    }


    public static void addItemToSortedListByRarenessDESC(ItemStack stackToInsert, List<ItemStack> sortedList)
    {
        for(int i=0; i<sortedList.Count; i++)
        {
            if(sortedList[i]==null)
            {
                sortedList[i]=stackToInsert;
                //Debug.Log("After insert: "+sortedList.Count);
                break;
            }
            if((sortedList[i].getItem().getRareness()==stackToInsert.getItem().getRareness() && sortedList[i].getNumOfItems()<stackToInsert.getNumOfItems()) || sortedList[i].getItem().getRareness()<stackToInsert.getItem().getRareness())
            {
                sortedList.Insert(i, stackToInsert);
                sortedList.RemoveAt(sortedList.Count-1);
                //Debug.Log("After insert: "+sortedList.Count);
                break;
            }
        }
    }




    public static void addItemToSortedListByRarenessASC(ItemStack stackToInsert, List<ItemStack> sortedList)
    {
        for(int i=0; i<sortedList.Count; i++)
        {
            if(sortedList[i]==null)
            {
                sortedList[i]=stackToInsert;
                //Debug.Log("After insert: "+sortedList.Count);
                break;
            }

            if((sortedList[i].getItem().getRareness()==stackToInsert.getItem().getRareness() && sortedList[i].getNumOfItems()>stackToInsert.getNumOfItems()) || sortedList[i].getItem().getRareness()>stackToInsert.getItem().getRareness())
            {
                sortedList.Insert(i, stackToInsert);
                sortedList.RemoveAt(sortedList.Count-1);
                //Debug.Log("After insert: "+sortedList.Count);
                break;
            }
        }
    }


    //Search function
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



    public static void addItemToSearchResultByName(ItemStack stackToInsert, List<ItemStack> searchList, string targetPart)
    {
        if(stackToInsert.getItem().getItemName().Contains(targetPart, StringComparison.OrdinalIgnoreCase))
        {
            for(int i=0; i<searchList.Count; i++)
            {
                if(searchList[i]==null)
                {
                    searchList[i]=stackToInsert;
                    //Debug.Log("After insert: "+sortedList.Count);
                    break;
                }
            }
        }
    }
}