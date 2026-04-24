using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TestingScript : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventory1;
    [SerializeField] private InventoryComponent inventory2;
    [SerializeField] private InventoryComponent inventory3;
    [SerializeField] private ItemComponent apple;
    [SerializeField] private ItemComponent brick;
    [SerializeField] private ItemComponent rock;
    [SerializeField] private ItemComponent knife;
    [SerializeField] private ItemComponent wood;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text numInStackText, itemNameText, itemTypeText, rarenessText, itemWeightText, itemStackLimitText;
    [SerializeField] private TMP_Text changeSizeByText;

    private InventoryComponent selectedInventory;
    private ItemComponent selectedItem;
    private int amount=0;
    private int changeSize=0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedInventory = inventory1;
        selectedItem = apple;
    }


    void Update()
    {
        ItemStack selectedStack = inventoryManager.getSelectedItemStack();

        if(selectedStack!=null)
        {
            ItemComponent selectedItem = selectedStack.getItem();

            numInStackText.text = selectedItem.getMaxNumberOfBlocksInAStack().ToString();
            itemNameText.text = selectedItem.getItemName();

            switch(selectedItem.getItemType())
            {
                case ItemType.Weapon:
                    itemTypeText.text = "Weapon";
                    break;
                case ItemType.Armour:
                    itemTypeText.text = "Armour";
                    break;
                case ItemType.Artefact:
                    itemTypeText.text = "Artefact";
                    break;
                case ItemType.Ingredient:
                    itemTypeText.text = "Ingredient";
                    break;
                case ItemType.Junk:
                    itemTypeText.text = "Junk";
                    break;
            }

            switch(selectedItem.getRareness())
            {
                case Rareness.Common:
                    rarenessText.text = "Common";
                    break;
                case Rareness.Uncommon:
                    rarenessText.text = "Uncommon";
                    break;
                case Rareness.Rare:
                    rarenessText.text = "Rare";
                    break;
                case Rareness.Epic:
                    rarenessText.text = "Epic";
                    break;
                case Rareness.Legendary:
                    rarenessText.text = "Legendary";
                    break;
            }

            itemWeightText.text = (selectedItem.getItemWeight()*selectedStack.getNumOfItems()).ToString();
            itemStackLimitText.text = selectedItem.getItemStackLimit().ToString();
        }
        else
        {
            numInStackText.text = "";
            itemNameText.text = "";
            itemTypeText.text = "";
            rarenessText.text = "";
            itemWeightText.text = "";
            itemStackLimitText.text = "";
        }
    }


    public void SelectInventory(int selected)
    {
        switch(selected)
        {
            case 0:
                selectedInventory = inventory1;
                break;
            case 1:
                selectedInventory = inventory2;
                break;
            case 2:
                selectedInventory = inventory3;
                break;
        }
    }


    public void SelectItem(int selected)
    {
        switch(selected)
        {
            case 0:
                selectedItem = apple;
                break;
            case 1:
                selectedItem = brick;
                break;
            case 2:
                selectedItem = knife;
                break;
            case 3:
                selectedItem = rock;
                break;
            case 4:
                selectedItem = wood;
                break;
        }
    }


    public void OpenInventory()
    {
        if(selectedInventory==inventory1)
            inventoryManager.OpenInventory(selectedInventory,new Vector2(0.4f, 0.4f),new Vector2(0.25f, 0.25f));

        if(selectedInventory==inventory2)
            inventoryManager.OpenInventory(selectedInventory,new Vector2(0.4f, 0.4f),new Vector2(0.75f, 0.25f));

        if(selectedInventory==inventory3)
            inventoryManager.OpenInventory(selectedInventory,new Vector2(0.4f, 0.4f),new Vector2(0.75f, 0.75f));
    }


    public void CloseInventory()
    {
        inventoryManager.CloseInventory(selectedInventory);
    }


    public void ClearInventory()
    {
        selectedInventory.RemoveItemsFromInventory(apple,selectedInventory.GetTotalAmountOfThisItem(apple));
        selectedInventory.RemoveItemsFromInventory(brick,selectedInventory.GetTotalAmountOfThisItem(brick));
        selectedInventory.RemoveItemsFromInventory(knife,selectedInventory.GetTotalAmountOfThisItem(knife));
        selectedInventory.RemoveItemsFromInventory(rock,selectedInventory.GetTotalAmountOfThisItem(rock));
        selectedInventory.RemoveItemsFromInventory(wood,selectedInventory.GetTotalAmountOfThisItem(wood));
    
        inventoryManager.UpdateInventoryView(selectedInventory);
    }


    public void IncreaseAmountBy1()
    {
        amount++;
        amountText.text = amount.ToString();
    }


    public void IncreaseAmountBy10()
    {
        amount+=10;
        amountText.text = amount.ToString();
    }


    public void DecreaseAmountBy1()
    {
        amount--;
        if(amount<0)
            amount=0;

        amountText.text = amount.ToString();
    }


    public void DecreaseAmountBy10()
    {
        amount-=10;
        if(amount<0)
            amount=0;
            
        amountText.text = amount.ToString();
    }


    public void AddItem()
    {
        //Debug.Log(selectedItem.name);
        selectedInventory.AddItemsToInventory(selectedItem,amount);
        inventoryManager.UpdateInventoryView(selectedInventory);
    }


    public void RemoveItem()
    {
        if(!selectedInventory.RemoveItemsFromInventory(selectedItem,amount))
            selectedInventory.RemoveItemsFromInventory(selectedItem,selectedInventory.GetTotalAmountOfThisItem(selectedItem));
        
        inventoryManager.UpdateInventoryView(selectedInventory);
    }


    public void IncreaseChangeSize()
    {
        changeSize++;
        changeSizeByText.text = changeSize.ToString();
    }


    public void DecreaseChangeSize()
    {
        changeSize--;
        if(changeSize<0)
            changeSize=0;

        changeSizeByText.text = changeSize.ToString();
    }



    public void IncreaseInventoryWidth()
    {
        selectedInventory.setInventoryWidth(selectedInventory.getInventoryWidth()+changeSize);
        ResizeInventory();
    }


    public void DecreaseInventoryWidth()
    {
        selectedInventory.setInventoryWidth(selectedInventory.getInventoryWidth()-changeSize);
        ResizeInventory();
    }


    public void IncreaseInventoryHeight()
    {
        selectedInventory.setInventoryHeight(selectedInventory.getInventoryHeight()+changeSize);
        ResizeInventory();
    }


    public void DecreaseInventoryHeight()
    {
        selectedInventory.setInventoryHeight(selectedInventory.getInventoryHeight()-changeSize);
        ResizeInventory();
    }


    private void ResizeInventory()
    {
        List<ItemStack> extraStacks = selectedInventory.ResizeInventory();
        if(extraStacks!=null)
        {
            string message = "Extra items: \n";

            foreach(ItemStack extraStack in extraStacks)
            {
                message = message+extraStack.getItem().getItemName()+" x"+extraStack.getNumOfItems()+"; \n";
            }

            Debug.Log(message);
        }
    }
}
