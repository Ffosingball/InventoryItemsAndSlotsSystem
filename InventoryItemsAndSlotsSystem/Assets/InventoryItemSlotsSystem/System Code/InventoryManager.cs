using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;


//There should be just one inventory manager on the scene!
//Also maybe make it so, when none of the inventories is opened than 
//turn it off so it do not process all the time whether mouse is over 
//the inventory or not
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject mouseItemIcon;
    //I assumed that mouse is sprite gameObject which 
    [SerializeField] private Vector2 mouseIconOffset;
    //If in 2d this will be usefull. If 0 it will not change z value
    [SerializeField] private float zValueForMouseIcon=0f;
    [SerializeField] private InventoryConfiguration inventoryConfiguration;
    [SerializeField] private GameObject inventoryView;
    [SerializeField] private GameObject inventorySlot;
    [SerializeField] private GameObject uiCanvas;

    private ItemStack selectedItemStack;
    private ItemStack pickedByMouse;
    private InventoryComponent inventoryOverWhichMouseIs;
    private List<InventoryViewComponent> inventoryViewsCurrentlyOpened;
    private InventoryComponent itemPickedAt;
    //private InventoryViewComponent previouslySelectedView;


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
        inventoryViewsCurrentlyOpened = new List<InventoryViewComponent>();
        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = "";
    }



    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if(inventoryViewsCurrentlyOpened.Count>0)
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = mousePos;

            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            if(inventoryOverWhichMouseIs!=null)
            {
                GetViewOfTheInventory(inventoryOverWhichMouseIs).DeselectSlot();
            }

            inventoryOverWhichMouseIs=null;
            InventorySlotComponent inventorySlot=null;
            foreach (RaycastResult result in results)
            {
                if(inventorySlot==null)
                    inventorySlot = result.gameObject.GetComponent<InventorySlotComponent>();

                if(result.gameObject.TryGetComponent<InventoryViewComponent>(out InventoryViewComponent inventoryView))
                {
                    inventoryOverWhichMouseIs = inventoryView.getInventoryComponent();
                    //break;
                }
            }

            if(inventorySlot!=null && inventoryOverWhichMouseIs!=null)
            {
                selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(inventorySlot.getPosition());
                GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(inventorySlot.getPosition());
            }
        }
        //else if(inventoryViewsCurrentlyOpened.Count==1)
        //    inventoryOverWhichMouseIs = inventoryViewsCurrentlyOpened[0].getInventoryComponent();
        else
            inventoryOverWhichMouseIs = null;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        worldPos.x+=mouseIconOffset.x;
        worldPos.y+=mouseIconOffset.y;
        if(zValueForMouseIcon!=0f)
            worldPos.z = zValueForMouseIcon;

        mouseItemIcon.transform.position = worldPos;
    }



    private InventoryViewComponent GetViewOfTheInventory(InventoryComponent inventoryComponent)
    {
        foreach(InventoryViewComponent inventoryViewComponent in inventoryViewsCurrentlyOpened)
        {
            if(inventoryViewComponent.getInventoryComponent()==inventoryComponent)
                return inventoryViewComponent;
        }

        return null;
    }



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
            //Debug.Log("Hit UI: " + result.gameObject.name);
            if(result.gameObject.TryGetComponent<InventorySlotComponent>(out InventorySlotComponent inventorySlot))
            {
                slotPosition = inventorySlot.getPosition();
                break;
            }
        }

        //Debug.Log("Slot selected: "+slotPosition.x+"; "+slotPosition.y);
        return slotPosition;
    }



    public void LeftMouseClick()
    {
        if(inventoryOverWhichMouseIs!=null)
        {
            if(inventoryConfiguration.arbitraryStackPlacement)
            {
                Vector2Int slotPosition = GetSlotPosition();
                if(slotPosition.x==-1)
                    return;

                //GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(slotPosition);

                if(pickedByMouse==null)
                {
                    selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
                    pickedByMouse = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);

                    if(pickedByMouse!=null)
                    {
                        //Debug.Log("Not called?");
                        inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                        selectedItemStack = null;
                    }
                }
                else
                {
                    NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(pickedByMouse, slotPosition);
                    if(placementResult.stackCapReached)
                    {
                        Debug.Log("Stack cap reached! Cannot insert more items of this type!");
                    }
                    else if(placementResult.stackReplaced!=null)
                    {
                        pickedByMouse = placementResult.stackReplaced;
                        itemPickedAt = inventoryOverWhichMouseIs;
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                    }
                    else
                    {
                        pickedByMouse = null;
                        itemPickedAt = null;
                        mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
                        mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = "";
                    }
                }

                GetViewOfTheInventory(inventoryOverWhichMouseIs).UpdateView();
            }
        }
    }


    public void RightMouseClick()
    {
         if(inventoryOverWhichMouseIs!=null)
        {
            if(inventoryConfiguration.arbitraryStackPlacement)
            {
                Vector2Int slotPosition = GetSlotPosition();
                if(slotPosition.x==-1)
                    return;

                //GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(slotPosition);

                if(pickedByMouse==null)
                {
                    selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
                    if(selectedItemStack==null)
                        return;
                    
                    ItemStack itemStackToDivide = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
                    //inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
                    itemPickedAt = inventoryOverWhichMouseIs;

                    if(selectedItemStack.getNumOfItems()>1)
                    {
                        pickedByMouse = new ItemStack(selectedItemStack.getItem(),inventoryConfiguration,-1);
                        pickedByMouse.IncreaseAmountBy(selectedItemStack.getNumOfItems()/2);
                        selectedItemStack.DecreaseAmountBy(pickedByMouse.getNumOfItems());
                    }
                    else
                    {
                        pickedByMouse = selectedItemStack;
                        selectedItemStack = null;
                        inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
                    }

                    mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                    mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                }
                else
                {
                    if(inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition)==null)
                    {
                        if(pickedByMouse.getNumOfItems()>1)
                        {
                            ItemStack newStack = new ItemStack(pickedByMouse.getItem(),inventoryConfiguration,-1);
                            newStack.IncreaseAmountBy(1);
                            
                            NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(newStack, slotPosition);
                            if(placementResult.stackCapReached)
                                Debug.Log("Stack cap reached! Cannot insert more items of this type!");
                            else
                                pickedByMouse.DecreaseAmountBy(1);

                            mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
                            mouseItemIcon.transform.Find("ItemText").GetComponent<TMP_Text>().text = pickedByMouse.getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : pickedByMouse.getNumOfItems().ToString();
                        }
                        else
                        {
                            NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(pickedByMouse, slotPosition);
                            if(placementResult.stackCapReached)
                                Debug.Log("Stack cap reached! Cannot insert more items of this type!");
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

                GetViewOfTheInventory(inventoryOverWhichMouseIs).UpdateView();
            }
        }
    }



    // Size and position should be from 0 to 1 
    public void OpenInventory(InventoryComponent inventoryComponent, Vector2 size, Vector2 position)
    {
        if(GetViewOfTheInventory(inventoryComponent)==null)
        {
            GameObject newView = Instantiate(inventoryView);
            newView.transform.SetParent(uiCanvas.transform);
            newView.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
            InventoryViewComponent newInventoryView = newView.GetComponent<InventoryViewComponent>();
            
            newInventoryView.setInventoryComponent(inventoryComponent);
            newInventoryView.setPosition(position);
            newInventoryView.setViewHeight(size.y);
            newInventoryView.setViewWidth(size.x);
            newInventoryView.setInventorySlot(inventorySlot);
            
            newInventoryView.CreateView();
            newInventoryView.UpdateView();
            inventoryViewsCurrentlyOpened.Add(newInventoryView);
        }
    }



    public void CloseInventory(InventoryComponent inventoryComponent)
    {
        InventoryViewComponent viewToDelete = GetViewOfTheInventory(inventoryComponent);

        if(viewToDelete!=null)
        {
            inventoryViewsCurrentlyOpened.Remove(viewToDelete);

            if(pickedByMouse!=null)
            {
                if(itemPickedAt==inventoryComponent)
                {
                    int result = inventoryComponent.AddItemsToInventory(pickedByMouse.getItem(),pickedByMouse.getNumOfItems());

                    if(result>0)
                    {
                        Debug.Log("DROP "+pickedByMouse.getItem().getItemName());
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



    public void UpdateInventoryView(InventoryComponent inventoryComponent)
    {
        InventoryViewComponent invView = GetViewOfTheInventory(inventoryComponent);
        if(invView!=null)
            invView.UpdateView();
    }
}