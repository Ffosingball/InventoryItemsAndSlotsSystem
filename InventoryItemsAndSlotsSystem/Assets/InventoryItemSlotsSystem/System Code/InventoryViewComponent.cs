using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;


//This component has to be placed at the scrollView ui gameObject!!
//This component shows UI represantation of the inventoryComponent
//by the rules described in the inventoryComponent's inventoryConfiguration
public class InventoryViewComponent : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventoryComponent;
    [SerializeField] private InventoryConfiguration inventoryConfiguration;
    [SerializeField] private InventoryManager inventoryManager;
    //Prefab of the slot which this view will use
    [SerializeField] private GameObject inventorySlot;   
    //In percentages, 0 is left and top, 1 is bottom and right
    [SerializeField] private Vector2 position; 
    //If slot is empty then this sprite is used instead of the itemPicture
    [SerializeField] private Sprite nothing; 

    //Sizes of the view in percentages towards window size
    private float viewWidth = 0.3f; 
    private float viewHeight = 0.3f;
    //Reference to view rectTransform
    private RectTransform rectTransform;
    //Reference to the gameObject where all slots will be placed
    private GameObject content;
    //Boolean flag whether view is created or not
    private bool viewCreated=false;
    private Vector2Int selectedSlotPosition;
    //This is used to resize view if inventoryComponent is infinite and it resized
    private int lastInventoryHeight;
    //This list is used when inventory is of type anyCell and rectangular
    //it stores all slots which has been selected
    private List<string> slotsSelected;
    


    //Setters and getters
    public InventoryComponent getInventoryComponent()
    {
        return inventoryComponent;
    }

    public void setInventoryComponent(InventoryComponent value)
    {
        if(inventoryComponent!=null)
            inventoryComponent.OnChangeUpdateView-=ResizeInventoryUI;
            
        inventoryComponent = value;
        inventoryComponent.OnChangeUpdateView+=ResizeInventoryUI;
    }

    public void setInventoryConfiguration(InventoryConfiguration value)
    {
        inventoryConfiguration = value;
    }

    public InventoryConfiguration getInventoryConfiguration()
    {
        return inventoryConfiguration;
    }

    public void setInventoryManager(InventoryManager value)
    {
        inventoryManager = value;
    }

    public InventoryManager getInventoryManager()
    {
        return inventoryManager;
    }

    public float getViewWidth()
    {
        return viewWidth;
    }

    public float getViewHeight()
    {
        return viewHeight;
    }

    public void setInventorySlot(GameObject value)
    {
        inventorySlot = value;
    }

    public Vector2 getPosition()
    {
        return position;
    }

    public void setPosition(Vector2 value)
    {
        position = value;
    }

    public bool getIsViewCreated()
    {
        return viewCreated;
    }



    //This method creates slot at the specified place with specified name and size
    private void CreateInventorySlotAt(int x, int y, Vector2 currentPosition, float slotSize)
    {
        GameObject newSlot = Instantiate(inventorySlot);
        newSlot.transform.SetParent(content.transform);
        newSlot.name = (x+y*inventoryComponent.getInventoryWidth()).ToString();
                
        RectTransform slotTransform = newSlot.GetComponent<RectTransform>();
        slotTransform.localScale = new Vector3(1,1,1);
        slotTransform.anchoredPosition = currentPosition;
        slotTransform.sizeDelta = new Vector2(slotSize, slotSize);

        InventorySlotComponent invSlotCom = newSlot.GetComponent<InventorySlotComponent>();
        invSlotCom.setPosition(new Vector2Int(x,y));
    }



    //This method initializes inventoryView and creates a UI view for the inventoryComponent
    //Before calling this method, a view should be placed in the correct place, 
    //you should provide inventoryComponent, slot prefab and view position!
    public void CreateView()
    {
        //initialize view
        rectTransform = GetComponent<RectTransform>();
        content = transform.Find("Viewport").transform.Find("Content").gameObject;
        selectedSlotPosition = new Vector2Int(-1,-1);
        inventoryConfiguration = inventoryComponent.getInventoryConfiguration();
        slotsSelected = new List<string>();

        //Set size, calculate and set size
        rectTransform.anchoredPosition = new Vector2((position.x*Screen.width)-(Screen.width/2),(Screen.height/2)-(position.y*Screen.height));
        viewWidth = (inventoryConfiguration.outerMargin*2)+(inventoryConfiguration.slotSizePercentage*inventoryComponent.getInventoryWidth())+(inventoryConfiguration.marginBetweenSlots*(inventoryComponent.getInventoryWidth()-1));
        int slotsHeight = inventoryComponent.getInventoryHeight()>inventoryConfiguration.visibleSlotsInHeight ? inventoryConfiguration.visibleSlotsInHeight : inventoryComponent.getInventoryHeight();
        float windowCorrelation = (float)Screen.width/(float)Screen.height;
        viewHeight = ((inventoryConfiguration.outerMargin*2)+(inventoryConfiguration.slotSizePercentage*slotsHeight)+(inventoryConfiguration.marginBetweenSlots*(slotsHeight-1)))*windowCorrelation;
        rectTransform.sizeDelta = new Vector2(viewWidth*Screen.width, viewHeight*Screen.height);

        //Calculate slot size
        float slotSize = inventoryConfiguration.slotSizePercentage*Screen.width;

        //set size of the content gameObject where all slots will be placed
        RectTransform contentTransform = content.GetComponent<RectTransform>();
        Vector2 contentSize = contentTransform.sizeDelta;
        contentSize.y = slotSize*inventoryComponent.getInventoryHeight()+inventoryConfiguration.outerMargin*2*Screen.width+inventoryConfiguration.marginBetweenSlots*Screen.width*(inventoryComponent.getInventoryHeight()-1);
        contentTransform.sizeDelta = contentSize;

        //Calculate position of the first slot
        Vector2 currentPosition = new Vector2((inventoryConfiguration.outerMargin*Screen.width+slotSize/2)-(rectTransform.sizeDelta.x/2),(contentTransform.sizeDelta.y/2)-(inventoryConfiguration.outerMargin*Screen.width+slotSize/2));

        //Create slots
        for (int i=0; i<inventoryComponent.getInventoryHeight(); i++)
        {
            for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
            {
                CreateInventorySlotAt(j, i, currentPosition, slotSize);
                currentPosition.x += slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
            }

            currentPosition.x -= (slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
            currentPosition.y -= slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
        }

        viewCreated = true;
        lastInventoryHeight = inventoryComponent.getInventoryHeight();
    }



    //This method deletes all slots and creates new one
    private void ResizeInventoryUI()
    {
        DeleteView();
        CreateView();
        UpdateView();
    }



    //When gameobject is destroyed is should remove its method from the event of the
    //inventoryComponent
    private void OnDestroy()
    {
        inventoryComponent.OnChangeUpdateView-=ResizeInventoryUI;
    }



    //This method just deletes all slots in this view
    public void DeleteView()
    {
        foreach (Transform child in content.transform)
        {
            child.name = child.name+"deleted";
            Destroy(child.gameObject);
        }

        viewCreated = false;
    }



    //This method hides view
    public void HideView()
    {
        gameObject.SetActive(false);
    }



    //This method shows view
    public void ShowView()
    {
        gameObject.SetActive(true);
    }



    //This method changes slot background to provided color at the provided position
    public void ChangeSlotBackgroundColor(Vector2Int position, Color backgroundColor)
    {
        Transform foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString());

        if(foundSlot!=null)
        {
            Image slotBackground = foundSlot.gameObject.GetComponent<Image>();
            Color color = slotBackground.color;
            color.r+=backgroundColor.r;
            color.g+=backgroundColor.g;
            color.b+=backgroundColor.b;
            slotBackground.color = color;
        }
    }



    //This method changes slot background to provided sprite at the provided position
    public void ChangeSlotBackgroundImage(Vector2Int position, Sprite backgroundSprite)
    {
        Transform foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString());

        if(foundSlot!=null)
        {
            Image slotBackground = foundSlot.gameObject.GetComponent<Image>();
            slotBackground.sprite = backgroundSprite;
        }
    }



    //This method turns on outline with provided color and thikness at the provided position
    public void SetSlotOutline(Vector2Int position, Color outlineColor, float thikness)
    {
        Transform foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString());

        if(foundSlot!=null)
        {
            Outline slotOutline = foundSlot.gameObject.GetComponent<Outline>();
            slotOutline.enabled = true;

            Color color = slotOutline.effectColor;
            color.r+=outlineColor.r;
            color.g+=outlineColor.g;
            color.b+=outlineColor.b;
            slotOutline.effectColor = color;

            slotOutline.effectDistance = new Vector2(thikness, -thikness);
        }
    }

    //This method turns on outline with provided color and thikness for the provided slot
    public void SetSlotOutline(GameObject slot, Color outlineColor, float thikness)
    {
        Outline slotOutline = slot.GetComponent<Outline>();
        slotOutline.enabled = true;

        Color color = slotOutline.effectColor;
        color.r+=outlineColor.r;
        color.g+=outlineColor.g;
        color.b+=outlineColor.b;
        slotOutline.effectColor = color;

        slotOutline.effectDistance = new Vector2(thikness, -thikness);
    }



    //This method turns off outline in the slot at the provided position
    public void RemoveSlotOutline(Vector2Int position)
    {
        Transform foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString());

        if(foundSlot!=null)
        {
            Outline slotOutline = foundSlot.gameObject.GetComponent<Outline>();
            slotOutline.enabled = false;
        }
    }

    public void RemoveSlotOutline(GameObject slot)
    {
        Outline slotOutline = slot.GetComponent<Outline>();
        slotOutline.enabled = false;
    }



    private void SelectAnySizeItemOutline(int firstPosition, ItemComponent itemComponent, Color selectColor)
    {
        for(int x=0; x<itemComponent.getItemSize().x; x++)
        {
            for(int y=0; y<itemComponent.getItemSize().y; y++)
            {
                if(itemComponent.getCellsOccupiedArray()[x+(y*itemComponent.getItemSize().x)])
                {
                    SetSlotOutline(new Vector2Int(firstPosition%inventoryComponent.getInventoryWidth(), firstPosition/inventoryComponent.getInventoryWidth()), selectColor, inventoryConfiguration.selectionOutlineThikness);
                    slotsSelected.Add(firstPosition.ToString());
                }
                firstPosition++;
            }

            firstPosition-=itemComponent.getItemSize().x;
            firstPosition+=inventoryComponent.getInventoryWidth();
        }
    }



    //This method makes slot at the provided position selected!
    //If you want another type of selection you can change it here!
    //But after do not forget to change Deselect as well!
    public bool SelectSlot(Vector2Int position)
    {
        if(viewCreated)
        {
            if(selectedSlotPosition!=position && selectedSlotPosition.x!=-1)
                DeselectSlot();

            if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
            {
                SetSlotOutline(position, inventoryConfiguration.selectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
                selectedSlotPosition = position;
            }
            else
            {
                if(inventoryManager.getPickedByMouseItemStack()==null)
                {
                    if(inventoryComponent.GetItemStackByPosition(position)==null)
                    {
                        SetSlotOutline(position, inventoryConfiguration.selectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
                        selectedSlotPosition = position;
                    }
                    else
                    {
                        if(inventoryConfiguration.inventoryType==InventoryType.Rectangular)
                        {
                            int firstPosition = inventoryComponent.GetItemStackByPosition(position).getCellOccupied();
                            SetSlotOutline(content.transform.Find(firstPosition+"icon").gameObject, inventoryConfiguration.selectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
                            selectedSlotPosition = new Vector2Int(firstPosition%inventoryComponent.getInventoryWidth(), firstPosition/inventoryComponent.getInventoryWidth());
                        }
                        else
                        {
                            SelectAnySizeItemOutline(inventoryComponent.GetItemStackByPosition(position).getCellOccupied(), inventoryComponent.GetItemStackByPosition(position).getItem(), inventoryConfiguration.selectionOutlineColor);
                        }
                    }
                }
                else
                {
                    selectedSlotPosition = position;
                    ItemComponent item = inventoryManager.getPickedByMouseItemStack().getItem();
                    
                    Action<Vector2Int, int, int> selectFunction;
                    if(inventoryConfiguration.inventoryType==InventoryType.Rectangular)
                    {
                        selectFunction = (pos, x, y) =>
                        {
                            if(inventoryComponent.GetItemStackByPosition(pos)==null)
                            {
                                SetSlotOutline(pos, inventoryConfiguration.selectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
                                slotsSelected.Add((pos.x+pos.y*inventoryComponent.getInventoryWidth()).ToString());
                            }
                            else
                            {
                                SelectAnySizeItemOutline(inventoryComponent.GetItemStackByPosition(pos).getCellOccupied(), inventoryComponent.GetItemStackByPosition(pos).getItem(), inventoryConfiguration.selectionOutlineColor);
                            }
                        };
                    }
                    else
                    {
                        selectFunction = (pos, x, y) =>
                        {
                            if(inventoryComponent.GetItemStackByPosition(pos)==null && inventoryManager.getPickedByMouseItemStack().getItem().getCellsOccupiedArray()[x+(y*inventoryManager.getPickedByMouseItemStack().getItem().getItemSize().x)])
                            {
                                SetSlotOutline(pos, inventoryConfiguration.selectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
                                slotsSelected.Add((pos.x+pos.y*inventoryComponent.getInventoryWidth()).ToString());
                            }
                            else if(inventoryManager.getPickedByMouseItemStack().getItem().getCellsOccupiedArray()[x+(y*inventoryManager.getPickedByMouseItemStack().getItem().getItemSize().x)])
                            {
                                int firstPosition = inventoryComponent.GetItemStackByPosition(pos).getCellOccupied();
                                SetSlotOutline(content.transform.Find(firstPosition+"icon").gameObject, inventoryConfiguration.wrongSelectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
                                slotsSelected.Add(firstPosition+"icon");
                            }
                        };
                    }
                    
                    for(int y=0; y<item.getItemSize().y; y++)
                    {
                        for(int x=0; x<item.getItemSize().x; x++)
                        {
                            if(position.x>=inventoryComponent.getInventoryWidth() || position.y>=inventoryComponent.getInventoryHeight())
                            {
                                position.x++;
                                continue;
                            }
                            else
                            {
                                selectFunction(position, x, y);
                            }

                            position.x++;
                        }

                        position.x-=item.getItemSize().x;
                        position.y++;
                    }
                }
            }

            return true;
        }

        return false;
    }



    //This method cancels all selection of the selected slot
    public bool DeselectSlot()
    {
        if(viewCreated && selectedSlotPosition.x!=-1)
        {
            if(content.transform.Find((selectedSlotPosition.x+selectedSlotPosition.y*inventoryComponent.getInventoryWidth()).ToString())!=null)
            {
                if(slotsSelected.Count>0)
                {
                    foreach(string slotName in slotsSelected)
                    {
                        RemoveSlotOutline(content.transform.Find(slotName).gameObject);
                    }
                    slotsSelected.Clear();
                }

                RemoveSlotOutline(selectedSlotPosition);
                if(inventoryConfiguration.inventoryType!=InventoryType.SingleCelled)
                {
                    if(content.transform.Find((selectedSlotPosition.x+selectedSlotPosition.y*inventoryComponent.getInventoryWidth())+"icon")!=null)
                    {
                        //Debug.Log((selectedSlotPosition.x+selectedSlotPosition.y*inventoryComponent.getInventoryWidth())+"icon"+" exist!");
                        RemoveSlotOutline(content.transform.Find((selectedSlotPosition.x+selectedSlotPosition.y*inventoryComponent.getInventoryWidth())+"icon").gameObject);
                    }
                }
                selectedSlotPosition = new Vector2Int(-1,-1);

                return true;
            }
        }

        return false;
    }



    //This method changes background of the slot according to the item rareness
    //If you want to indicate rareness of the item in other way you can change it here
    private void IndicateItemRareness(Image slotImage, Image backgroundImage, Rareness rareness)
    {
        if(rareness!=Rareness.None)
        {
            switch(rareness)
            {
                case Rareness.Common:
                    backgroundImage.color = inventoryConfiguration.commonItemColor;
                    break;
                case Rareness.Uncommon:
                    backgroundImage.color = inventoryConfiguration.uncommonItemColor;
                    break;
                case Rareness.Rare:
                    backgroundImage.color = inventoryConfiguration.rareItemColor;
                    break;
                case Rareness.Epic:
                    backgroundImage.color = inventoryConfiguration.epicItemColor;
                    break;
                case Rareness.Legendary:
                    backgroundImage.color = inventoryConfiguration.legendaryItemColor;
                    break;
            }
        }
        else
            backgroundImage.color = inventoryConfiguration.emptySlotColor;
    }



    //This method updates all slots content
    public bool UpdateView()
    {
        //check if view created
        if(viewCreated)
        {
            //Check if inventoryComponent is infiniteInventory and it changed its amount of rows
            if(inventoryComponent.getInventoryHeight()!=lastInventoryHeight)
            {
                //calculate slot size
                float slotSize = inventoryConfiguration.slotSizePercentage*Screen.width;

                //Calculate and set new size for the content gameObject
                RectTransform contentTransform = content.GetComponent<RectTransform>();
                Vector2 contentSize = contentTransform.sizeDelta;
                contentSize.y = slotSize*inventoryComponent.getInventoryHeight()+inventoryConfiguration.outerMargin*2*Screen.width+inventoryConfiguration.marginBetweenSlots*Screen.width*(inventoryComponent.getInventoryHeight()-1);
                contentTransform.sizeDelta = contentSize;

                //Calculate new position for the first slot
                Vector2 currentPosition = new Vector2((inventoryConfiguration.outerMargin*Screen.width+slotSize/2)-(rectTransform.sizeDelta.x/2),(contentTransform.sizeDelta.y/2)-(inventoryConfiguration.outerMargin*Screen.width+slotSize/2));

                //Check if inventory height increased
                if(inventoryComponent.getInventoryHeight()>lastInventoryHeight)
                {
                    //Then set new position for all existing slots
                    for (int i=0; i<lastInventoryHeight; i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).GetComponent<RectTransform>().anchoredPosition = currentPosition;
                            currentPosition.x += slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
                        }

                        currentPosition.x -= (slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
                    }

                    //and add new slots at the bottom
                    for (int i=lastInventoryHeight; i<inventoryComponent.getInventoryHeight(); i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            CreateInventorySlotAt(j, i, currentPosition, slotSize);
                            currentPosition.x += slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
                        }

                        currentPosition.x -= (slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
                    }
                }
                else
                {
                    //else if inventory decreased then set new position for some slots
                    for (int i=0; i<inventoryComponent.getInventoryHeight(); i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).GetComponent<RectTransform>().anchoredPosition = currentPosition;
                            currentPosition.x += slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
                        }

                        currentPosition.x -= (slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + inventoryConfiguration.marginBetweenSlots*Screen.width;
                    }

                    //after delete all other extra slots
                    for (int i=inventoryComponent.getInventoryHeight(); i<lastInventoryHeight; i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            Destroy(content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).gameObject);
                            GameObject iconSlot = content.transform.Find((j+i*inventoryComponent.getInventoryWidth())+"icon").gameObject;
                            if(iconSlot!=null)
                                Destroy(iconSlot);
                        }
                    }
                }

                viewCreated = true;
                lastInventoryHeight = inventoryComponent.getInventoryHeight();
            }

            //Get list of items positions
            List<ItemStack> itemStacks;
            if(inventoryComponent.getShowItemsSorted())
                itemStacks = inventoryComponent.getSortedListOfItems();
            else
                itemStacks = inventoryComponent.getItemsInTheInventory();

            List<ItemStack> itemBigPictureUpdated = new List<ItemStack>();
            //Update every slot
            for(int i=0; i<itemStacks.Count; i++)
            {
                Image slotImage = content.transform.Find(i.ToString()).Find("ItemImage").gameObject.GetComponent<Image>();
                TMP_Text slotText = content.transform.Find(i.ToString()).Find("ItemText").gameObject.GetComponent<TMP_Text>();
                Image backgroundImage = content.transform.Find(i.ToString()).gameObject.GetComponent<Image>();
                RectTransform slotTransform = content.transform.Find(i.ToString()).gameObject.GetComponent<RectTransform>();

                //Check if item at this position is not null
                if(itemStacks[i]!=null)
                {
                    //then set slot accordingly
                    if(inventoryConfiguration.inventoryType==InventoryType.SingleCelled)
                    {
                        slotImage.sprite = itemStacks[i].getItem().getPicture();
                        slotText.text = itemStacks[i].getItem().getMaxNumberOfBlocksInAStack()==1 || inventoryComponent.getOnlyOneItemPerStack() ? "" : itemStacks[i].getNumOfItems().ToString();
                    }
                    else
                    {
                        if(itemStacks[i].getCellOccupied()==i  || (inventoryConfiguration.inventoryType==InventoryType.AnySize && itemBigPictureUpdated.Contains(itemStacks[i])))
                        {
                            itemBigPictureUpdated.Add(itemStacks[i]);
                            GameObject iconSlot;
                            if(content.transform.Find(i+"icon")==null)
                            {
                                iconSlot = Instantiate(inventorySlot);
                                iconSlot.transform.SetParent(content.transform);
                                iconSlot.name = i+"icon";
                            }
                            else
                                iconSlot = content.transform.Find(i+"icon").gameObject;

                            iconSlot.GetComponent<InventorySlotComponent>().setPosition(new Vector2Int(i%inventoryComponent.getInventoryWidth(),i/inventoryComponent.getInventoryWidth()));
                            RectTransform iconSlotTransform = iconSlot.GetComponent<RectTransform>();
                            iconSlotTransform.localScale = new Vector3(1,1,1);

                            Vector2 position = new Vector2();
                            position.x = slotTransform.anchoredPosition.x + (((itemStacks[i].getItem().getItemSize().x*inventoryConfiguration.slotSizePercentage*Screen.width)+((itemStacks[i].getItem().getItemSize().x-1)*inventoryConfiguration.marginBetweenSlots*Screen.width))/2) - (inventoryConfiguration.slotSizePercentage*Screen.width/2);
                            position.y = slotTransform.anchoredPosition.y - (((itemStacks[i].getItem().getItemSize().y*inventoryConfiguration.slotSizePercentage*Screen.width)+((itemStacks[i].getItem().getItemSize().y-1)*inventoryConfiguration.marginBetweenSlots*Screen.width))/2) + (inventoryConfiguration.slotSizePercentage*Screen.width/2);
                            iconSlotTransform.anchoredPosition = position;

                            Vector2 size = new Vector2();
                            size.x = (itemStacks[i].getItem().getItemSize().x*inventoryConfiguration.slotSizePercentage*Screen.width)+((itemStacks[i].getItem().getItemSize().x-1)*inventoryConfiguration.marginBetweenSlots*Screen.width);
                            size.y = (itemStacks[i].getItem().getItemSize().y*inventoryConfiguration.slotSizePercentage*Screen.width)+((itemStacks[i].getItem().getItemSize().y-1)*inventoryConfiguration.marginBetweenSlots*Screen.width);
                            iconSlotTransform.sizeDelta = size;

                            if(inventoryConfiguration.inventoryType==InventoryType.Rectangular)
                            {
                                IndicateItemRareness(null, iconSlot.GetComponent<Image>(), itemStacks[i].getItem().getRareness());
                                Color color = iconSlot.GetComponent<Image>().color;
                                color.a = inventoryConfiguration.alphaValueForMultiCelledSlots;
                                iconSlot.GetComponent<Image>().color = color;

                                iconSlot.transform.Find("ItemImage").gameObject.GetComponent<Image>().sprite = itemStacks[i].getItem().getPicture();
                                iconSlot.transform.Find("ItemText").gameObject.GetComponent<TMP_Text>().text = itemStacks[i].getItem().getMaxNumberOfBlocksInAStack()==1 || inventoryComponent.getOnlyOneItemPerStack() ? "" : itemStacks[i].getNumOfItems().ToString();
                            }
                            else
                            {
                                Color color = iconSlot.GetComponent<Image>().color;
                                color.a = 0;
                                iconSlot.GetComponent<Image>().color = color;

                                iconSlot.transform.Find("ItemImage").gameObject.GetComponent<Image>().sprite = itemStacks[i].getItem().getAnySizeInventoryPicture();
                                iconSlot.transform.Find("ItemText").gameObject.GetComponent<TMP_Text>().text = "";
                            }
                        }

                        slotImage.sprite = nothing;
                        slotText.text = "";
                    }

                    if(inventoryConfiguration.useFiveTierRareness)
                        IndicateItemRareness(slotImage, backgroundImage, itemStacks[i].getItem().getRareness());
                }
                else
                {
                    //otherwise make slot empty
                    slotImage.sprite = nothing;
                    slotText.text = "";

                    if(inventoryConfiguration.inventoryType!=InventoryType.SingleCelled)
                    {
                        Transform iconSlot = content.transform.Find(i+"icon");
                        if(iconSlot!=null)
                            Destroy(iconSlot.gameObject);
                    }

                    if(inventoryConfiguration.useFiveTierRareness)
                        IndicateItemRareness(slotImage, backgroundImage, Rareness.None);
                }
            }

            return true;
        }

        return false;
    }
}