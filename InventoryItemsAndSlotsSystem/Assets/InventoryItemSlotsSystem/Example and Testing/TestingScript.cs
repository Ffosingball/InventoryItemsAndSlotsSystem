using UnityEngine;
using TMPro;

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

    private InventoryComponent selectedInventory;
    private ItemComponent selectedItem;
    private int amount=0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedInventory = inventory1;
        selectedItem = apple;
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
}
