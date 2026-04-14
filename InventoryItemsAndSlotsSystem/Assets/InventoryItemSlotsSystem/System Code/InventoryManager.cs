using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Microsoft.Unity.VisualStudio.Editor;


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

    private ItemStack selectedItemStack;
    private ItemStack pickedByMouse;
    private InventoryComponent inventoryOverWhichMouseIs;
    private List<InventoryViewComponent> inventoryViewsCurrentlyOpened;
    private InventoryComponent itemPickedAt;


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
    }



    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if(inventoryViewsCurrentlyOpened.Count>1)
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = mousePos;

            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                //Debug.Log("Hit UI: " + result.gameObject.name);
                if(result.gameObject.TryGetComponent<InventoryViewComponent>(out InventoryViewComponent inventoryView))
                {
                    inventoryOverWhichMouseIs = inventoryView.getInventoryComponent();
                    break;
                }
            }
        }
        else if(inventoryViewsCurrentlyOpened.Count==1)
            inventoryOverWhichMouseIs = inventoryViewsCurrentlyOpened[0].getInventoryComponent();
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

        return slotPosition;
    }



    public void LeftMouseClick()
    {
        Vector2Int slotPosition = GetSlotPosition();
        if(slotPosition.x==-1)
            return;

        GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(slotPosition);

        if(pickedByMouse==null)
            selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
        else
        {
            NewItemPlacementResult placementResult = inventoryOverWhichMouseIs.PlaceItemStackToInventory(pickedByMouse, slotPosition);
            if(placementResult.stackReplaced!=null)
            {
                pickedByMouse = placementResult.stackReplaced;
                itemPickedAt = inventoryOverWhichMouseIs;
                mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
            }
            else
            {
                pickedByMouse = null;
                itemPickedAt = null;
                mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
            }
        }
    }


    public void LeftMouseHold()
    {
        Vector2Int slotPosition = GetSlotPosition();
        if(slotPosition.x==-1)
            return;

        GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(slotPosition);

        if(pickedByMouse==null)
        {
            pickedByMouse = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
            inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
            mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
            selectedItemStack = null;
        }
    }


    public void RightMouseClick()
    {
        Vector2Int slotPosition = GetSlotPosition();
        if(slotPosition.x==-1)
            return;

        GetViewOfTheInventory(inventoryOverWhichMouseIs).SelectSlot(slotPosition);

        if(pickedByMouse==null)
        {
            selectedItemStack = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
            ItemStack itemStackToDivide = inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition);
            inventoryOverWhichMouseIs.RemoveItemStackByPosition(slotPosition);
            mouseItemIcon.GetComponent<SpriteRenderer>().sprite = pickedByMouse.getItem().getPicture();
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
            }
        }
        else
        {
            if(inventoryOverWhichMouseIs.GetItemStackByPosition(slotPosition)==null)
            {
                if(selectedItemStack.getNumOfItems()>1)
                {
                    ItemStack newStack = new ItemStack(pickedByMouse.getItem(),inventoryConfiguration,-1);
                    newStack.IncreaseAmountBy(1);
                    pickedByMouse.DecreaseAmountBy(1);
                    inventoryOverWhichMouseIs.PlaceItemStackToInventory(newStack, slotPosition);
                }
                else
                {
                    inventoryOverWhichMouseIs.PlaceItemStackToInventory(pickedByMouse, slotPosition);
                    pickedByMouse = null;
                    mouseItemIcon.GetComponent<SpriteRenderer>().sprite = null;
                    itemPickedAt = null;
                }
            }
        }
    }



    public void OpenInventory(InventoryComponent inventoryComponent)
    {
        
    }



    public void CloseInventory(InventoryComponent inventoryComponent)
    {
        
    }
}