using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System.ComponentModel.Design;
using System;


//There should be just one inventory manager on the scene!
//The inventory manager manages interaction between inventories and player
public class InventoryManager : MonoBehaviour
{
    //It si called when item is droped from the mouse 
    public event Action<ItemStack> OnItemStackDrop;

    [SerializeField] private EventSystem eventSystem;
    //Mouse icon should be over all inventoryViews!
    [SerializeField] private GameObject mouseItemIcon;
    //Offset of the mouseIcon
    [SerializeField] private Vector2 mouseIconOffset;
    //If in 2d this will be usefull. If 0 it will not change z value
    [SerializeField] private float zValueForMouseIcon=0f;

    //Prefabs of inventoryView and inventorySlot which will be used to create UI of the inventory
    [SerializeField] private GameObject inventoryView;
    [SerializeField] private GameObject inventorySlot;
    //Canvas where inventory ui should be drawn
    [SerializeField] private GameObject uiCanvas;

    private ItemStack selectedItemStack;
    //ItemStack picked by mouse
    private ItemStack pickedByMouse;
    private InventoryComponent inventoryOverWhichMouseIs;
    //List of inventory view which are open now
    private List<InventoryViewComponent> inventoryViewsCurrentlyOpened;
    private InventoryComponent itemPickedAt;



    //Getters and setters
    public ItemStack getSelectedItemStack()
    {
        return selectedItemStack;
    }

    public InventoryComponent getSelectedInventory()
    {
        return inventoryOverWhichMouseIs;
    }



    private void Start()
    {
        //Initialize inventoryManager
        inventoryViewsCurrentlyOpened = new List<InventoryViewComponent>();
        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = "";
    }



    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        //Check if any inventory is open then proceed
        if(inventoryViewsCurrentlyOpened.Count>0)
        {
            //Get raycast results
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = mousePos;
            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            if(inventoryOverWhichMouseIs!=null)
            {
                GetViewOfTheInventory(inventoryOverWhichMouseIs).DeselectSlot();
            }

            //Check if mouse is over any inventory and any slot in the inventory
            inventoryOverWhichMouseIs=null;
            InventorySlotComponent inventorySlot=null;
            foreach (RaycastResult result in results)
            {
                if(inventorySlot==null)
                    inventorySlot = result.gameObject.GetComponent<InventorySlotComponent>();

                if(result.gameObject.TryGetComponent<InventoryViewComponent>(out InventoryViewComponent inventoryView))
                {
                    inventoryOverWhichMouseIs = inventoryView.getInventoryComponent();
                }
            }

            //If mouse is over some slot then select this slot
            if(inventorySlot!=null && inventoryOverWhichMouseIs!=null)
            {
                selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(inventorySlot.getPosition());
                GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(inventorySlot.getPosition());
            }
        }
        else
            inventoryOverWhichMouseIs = null;

        //Move mouse icon to the mouse
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.x+=mouseIconOffset.x;
        worldPos.y+=mouseIconOffset.y;
        if(zValueForMouseIcon!=0f)
            worldPos.z = zValueForMouseIcon;
        mouseItemIcon.transform.position = worldPos;
    }



    //This method return view of the provided inventoryComponent
    private InventoryViewComponent GetViewOfTheInventory(InventoryComponent inventoryComponent)
    {
        foreach(InventoryViewComponent inventoryViewComponent in inventoryViewsCurrentlyOpened)
        {
            if(inventoryViewComponent.getInventoryComponent()==inventoryComponent)
                return inventoryViewComponent;
        }

        return null;
    }



    //This method returns position of the slot over which mouse is
    private Vector2Int GetSlotPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = mousePos;

        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        Vector2Int slotPosition = new Vector2Int(-1,-1);
        foreach (RaycastResult result in results)
        {
            if(result.gameObject.TryGetComponent<InventorySlotComponent>(out InventorySlotComponent inventorySlot))
            {
                slotPosition = inventorySlot.getPosition();
                break;
            }
        }

        return slotPosition;
    }



    //This method behaves differently in different circumstances
    //If no item is picked by mouse and selectedSlot not empty then mouse will pick it up
    //If something already picked and selectedSlot is empty it will place picked item into that slot
    //If something is picked and slot is not emty then it will swap those two items
    public void LeftMouseClick()
    {
        //Check if mouse is over any inventory
        if(inventoryOverWhichMouseIs!=null)
        {
            //Check if moveing items in that inventory is allowed
            if(inventoryOverWhichMouseIs.getInventoryConfiguration().arbitraryStackPlacement)
            {
                //Check that mouse iv over any slot
                Vector2Int slotPosition = GetSlotPosition();
                if(slotPosition.x==-1)
                    return;

                //If no item is picked by mouse than pick from this slot
                if(pickedByMouse==null)
                {
                    selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
                    pickedByMouse = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);

                    //Check that slot was not empty, then remove item from that slot
                    if(pickedByMouse!=null)
                    {
                        inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                        selectedItemStack = null;
                        itemPickedAt = inventoryOverWhichMouseIs;
                    }
                }
                else
                {
                    //Otherwise try to place item into that slot
                    NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(pickedByMouse, slotPosition);
                    if(placementResult.stackCapReached)
                    {//Failed to insert
                        Debug.Log("Stack cap reached! Cannot insert more items of this type!");
                    }
                    else if(placementResult.weightLimitReached)
                    {//Failed to insert
                        Debug.Log("Weight limit reached! Cannot insert more items!");
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                    }
                    else if(placementResult.stackReplaced!=null)
                    {//Items were swapped
                        pickedByMouse = placementResult.stackReplaced;
                        itemPickedAt = inventoryOverWhichMouseIs;
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                    }
                    else
                    {//Best case, slot was empty
                        pickedByMouse = null;
                        itemPickedAt = null;
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = "";
                    }
                }

                //Update view of this inventory
                GetViewOfTheInventory(inventoryOverWhichMouseIs).UpdateView();
            }
        }
    }



    //This method behaves differently in different circumstances
    //If no item is picked by mouse and selectedSlot not empty then mouse will pick half of the 
    //items from that slot. If something already picked and selectedSlot is empty it will place 
    //just one item from the picked itemStack. If something is picked and slot is not emty then 
    //it will do nothing
    public void RightMouseClick()
    {
        //Check that mouse is over some inventory
         if(inventoryOverWhichMouseIs!=null)
        {
            //Check that moving items is allowed in this inventory
            if(inventoryOverWhichMouseIs.getInventoryConfiguration().arbitraryStackPlacement)
            {
                //check that mouse is over any slot
                Vector2Int slotPosition = GetSlotPosition();
                if(slotPosition.x==-1)
                    return;

                //Check if any item is picked
                if(pickedByMouse==null)
                {
                    //If slot is empty then do nothing
                    selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
                    if(selectedItemStack==null)
                        return;
                    
                    //If not then if slot has more than one of this items than divide it by 2
                    //Half stays in that slot, other half will be picked by mouse
                    itemPickedAt = inventoryOverWhichMouseIs;
                    if(selectedItemStack.getNumOfItems()>1)
                    {
                        pickedByMouse = new ItemStack(selectedItemStack.getItem(),inventoryOverWhichMouseIs.getInventoryConfiguration(),-1);
                        pickedByMouse.IncreaseAmountBy(selectedItemStack.getNumOfItems()/2);

                        selectedItemStack.DecreaseAmountBy(pickedByMouse.getNumOfItems());
                        inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
                        inventoryOverWhichMouseIs.PlaceItemStackToInventory(selectedItemStack, slotPosition);
                    }
                    else
                    {
                        //Otherwise pick item from the slot
                        pickedByMouse = selectedItemStack;
                        inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
                    }

                    //Update mouse icon
                    mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                    mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                }
                else
                {
                    //If item is picked and slot over mouse is empty then
                    if(inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition)==null)
                    {
                        //If picked item has more than 1, then remove one from the picked item
                        //and insert into empty slot copy of this item with amount 1
                        if(pickedByMouse.getNumOfItems()>1)
                        {
                            ItemStack newStack = new ItemStack(pickedByMouse.getItem(),inventoryOverWhichMouseIs.getInventoryConfiguration(),-1);
                            newStack.IncreaseAmountBy(1);
                            
                            NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(newStack, slotPosition);
                            if(placementResult.stackCapReached)
                                Debug.Log("Stack cap reached! Cannot insert more items of this type!");
                            else if(placementResult.weightLimitReached)
                                Debug.Log("Weight limit reached! Cannot insert more items!");
                            else
                                pickedByMouse.DecreaseAmountBy(1);

                            mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                            mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                        }
                        else
                        {
                            //Otherwise put the picked item in the slot
                            NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(pickedByMouse, slotPosition);
                            if(placementResult.stackCapReached)
                                Debug.Log("Stack cap reached! Cannot insert more items of this type!");
                            else if(placementResult.weightLimitReached)
                                Debug.Log("Weight limit reached! Cannot insert more items!");
                            else
                            {
                                pickedByMouse = null;
                                mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
                                mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = "";
                                itemPickedAt = null;
                            }
                        }
                    }
                }

                //Update view
                GetViewOfTheInventory(inventoryOverWhichMouseIs).UpdateView();
            }
        }
    }



    //This method creates an inventoryView for the provided inventoryComponent
    //at the provided position. Position should be from 0 to 1 (where 0 is left or top 
    //and 1 is right or bottom)
    public void OpenInventory(InventoryComponent inventoryComponent, Vector2 position)
    {
        //Check if view for this inventory already opened
        if(GetViewOfTheInventory(inventoryComponent)==null)
        {
            GameObject newView = Instantiate(inventoryView);
            newView.transform.SetParent(uiCanvas.transform);
            newView.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
            InventoryViewComponent newInventoryView = newView.GetComponent<InventoryViewComponent>();
            
            newInventoryView.setInventoryComponent(inventoryComponent);
            newInventoryView.setPosition(position);
            newInventoryView.setInventorySlot(inventorySlot);
            
            newInventoryView.CreateView();
            newInventoryView.UpdateView();
            inventoryViewsCurrentlyOpened.Add(newInventoryView);
        }
    }



    //This method closes inventoryView for provided inventoryComponent
    //and deals with picked item
    public void CloseInventory(InventoryComponent inventoryComponent)
    {
        //If view for this inventory is opened then close it
        InventoryViewComponent viewToDelete = GetViewOfTheInventory(inventoryComponent);
        if(viewToDelete!=null)
        {
            inventoryViewsCurrentlyOpened.Remove(viewToDelete);

            //Check if some itemStack is picked by mouse
            if(pickedByMouse!=null)
            {
                //And it picked from this inventory
                if(itemPickedAt==inventoryComponent)
                {
                    //Than try to insert it back
                    int result = inventoryComponent.AddItemsToInventory(pickedByMouse.getItem(),pickedByMouse.getNumOfItems());

                    if(result>0)
                    {
                        //If failed than do something
                        Debug.Log("DROP "+pickedByMouse.getItem().getItemName());
                        OnItemStackDrop?.Invoke(pickedByMouse);
                    }

                    pickedByMouse = null;
                    itemPickedAt = null;
                    mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
                    mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = "";
                }
            }

            Destroy(viewToDelete.gameObject);
        }
    }



    //This method just updates inventory view of the provided inventoryComponent
    public void UpdateInventoryView(InventoryComponent inventoryComponent)
    {
        InventoryViewComponent invView = GetViewOfTheInventory(inventoryComponent);
        if(invView!=null)
            invView.UpdateView();
    }
}