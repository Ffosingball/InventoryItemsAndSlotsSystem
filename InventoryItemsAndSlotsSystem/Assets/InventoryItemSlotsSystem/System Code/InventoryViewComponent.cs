using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;


//This component has to be put at the scrollView ui gameObject!!
public class InventoryViewComponent : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventoryComponent;
    [SerializeField] private InventoryConfiguration inventoryConfiguration;
    //[SerializeField] private InventoryManager inventoryManager;
    private float viewWidth = 0.3f; 
    private float viewHeight = 0.3f;
    [SerializeField] private int visibleSlotsInHeight = 5; 
    //Percentage of the pixel size of the screen
    [SerializeField] private float marginBetweenSlots = 0.01f;
    //Percentage of the pixel size of the screen
    [SerializeField] private float outerMargin = 0.08f;
    //Percentage of the pixel size of the scree
    public float slotSizePercentage = 0.04f;
    [SerializeField] private GameObject inventorySlot;   
    //In percentages, if less than 0 or more than 1 than it is outside of the window
    //(0.5, 0.5) is the center
    [SerializeField] private Vector2 position; 
    [SerializeField] private Sprite nothing; 


    private RectTransform rectTransform;
    private GameObject content;
    private bool viewCreated=false;
    private Vector2Int selectedSlotPosition;
    private int lastInventoryHeight;
    


    //Setters and getters
    public InventoryComponent getInventoryComponent()
    {
        return inventoryComponent;
    }

    public void setInventoryComponent(InventoryComponent value)
    {
        if(inventoryComponent!=null)
            inventoryComponent.OnResizing-=ResizeInventoryUI;
            
        inventoryComponent = value;
        inventoryComponent.OnResizing+=ResizeInventoryUI;
    }

    public void setInventoryConfiguration(InventoryConfiguration value)
    {
        inventoryConfiguration = value;
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

    public int getVisibleSlotsInHeight()
    {
        return visibleSlotsInHeight;
    }

    public void setVisibleSlotsInHeight(int value)
    {
        visibleSlotsInHeight = value;
    }

    public bool getIsViewCreated()
    {
        return viewCreated;
    }



    private void CreateInventorySlotAt(int x, int y, Vector2 currentPosition, float slotSize)
    {
        GameObject newSlot = Instantiate(inventorySlot);
        newSlot.transform.SetParent(content.transform);
        newSlot.name = (x+y*inventoryComponent.getInventoryWidth()).ToString();
                
        RectTransform slotTransform = newSlot.GetComponent<RectTransform>();
        slotTransform.localScale = new Vector3(1,1,1);
        slotTransform.anchoredPosition = currentPosition;
        //Debug.Log(newSlot.name+") "+currentPosition.x+"; "+currentPosition.y);
        slotTransform.sizeDelta = new Vector2(slotSize, slotSize);

        InventorySlotComponent invSlotCom = newSlot.GetComponent<InventorySlotComponent>();
        invSlotCom.setPosition(new Vector2Int(x,y));
    }



    public void CreateView()
    {
        rectTransform = GetComponent<RectTransform>();
        content = transform.Find("Viewport").transform.Find("Content").gameObject;
        selectedSlotPosition = new Vector2Int(-1,-1);

        rectTransform.anchoredPosition = new Vector2((position.x*Screen.width)-(Screen.width/2),(Screen.height/2)-(position.y*Screen.height));
        viewWidth = (outerMargin*2)+(slotSizePercentage*inventoryComponent.getInventoryWidth())+(marginBetweenSlots*(inventoryComponent.getInventoryWidth()-1));
        int slotsHeight = inventoryComponent.getInventoryHeight()>visibleSlotsInHeight ? visibleSlotsInHeight : inventoryComponent.getInventoryHeight();
        float windowCorrelation = (float)Screen.width/(float)Screen.height;
        viewHeight = ((outerMargin*2)+(slotSizePercentage*slotsHeight)+(marginBetweenSlots*(slotsHeight-1)))*windowCorrelation;
        rectTransform.sizeDelta = new Vector2(viewWidth*Screen.width, viewHeight*Screen.height);

        float slotSize = slotSizePercentage*Screen.width;

        RectTransform contentTransform = content.GetComponent<RectTransform>();
        Vector2 contentSize = contentTransform.sizeDelta;
        contentSize.y = slotSize*inventoryComponent.getInventoryHeight()+outerMargin*2*Screen.width+marginBetweenSlots*Screen.width*(inventoryComponent.getInventoryHeight()-1);
        //Debug.Log("Content y: "+contentSize.y);
        contentTransform.sizeDelta = contentSize;

        Vector2 currentPosition = new Vector2((outerMargin*Screen.width+slotSize/2)-(rectTransform.sizeDelta.x/2),(contentTransform.sizeDelta.y/2)-(outerMargin*Screen.width+slotSize/2));

        for (int i=0; i<inventoryComponent.getInventoryHeight(); i++)
        {
            for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
            {
                CreateInventorySlotAt(j, i, currentPosition, slotSize);
                currentPosition.x += slotSize + marginBetweenSlots*Screen.width;
            }

            currentPosition.x -= (slotSize + marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
            currentPosition.y -= slotSize + marginBetweenSlots*Screen.width;
        }

        viewCreated = true;
        lastInventoryHeight = inventoryComponent.getInventoryHeight();
    }



    private void ResizeInventoryUI()
    {
        DeleteView();
        CreateView();
        UpdateView();
    }


    private void OnDestruction()
    {
        inventoryComponent.OnResizing-=ResizeInventoryUI;
    }



    public void DeleteView()
    {
        foreach (Transform child in content.transform)
        {
            child.name = child.name+"deleted";
            Destroy(child.gameObject);
        }

        viewCreated = false;
    }



    public void HideView()
    {
        gameObject.SetActive(false);
    }



    public void ShowView()
    {
        gameObject.SetActive(true);
    }



    public void ChangeSlotBackgroundColor(Vector2Int position, Color backgroundColor)
    {
        GameObject foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString()).gameObject;

        if(foundSlot!=null)
        {
            Image slotBackground = foundSlot.GetComponent<Image>();
            Color color = slotBackground.color;
            color.r+=backgroundColor.r;
            color.g+=backgroundColor.g;
            color.b+=backgroundColor.b;
            slotBackground.color = color;
        }
    }



    public void SetSlotOutline(Vector2Int position, Color outlineColor, float thikness)
    {
        GameObject foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString()).gameObject;

        if(foundSlot!=null)
        {
            Outline slotOutline = foundSlot.GetComponent<Outline>();
            slotOutline.enabled = true;

            Color color = slotOutline.effectColor;
            color.r+=outlineColor.r;
            color.g+=outlineColor.g;
            color.b+=outlineColor.b;
            slotOutline.effectColor = color;

            slotOutline.effectDistance = new Vector2(thikness, -thikness);
        }
    }



    public void RemoveSlotOutline(Vector2Int position)
    {
        GameObject foundSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString()).gameObject;

        if(foundSlot!=null)
        {
            Outline slotOutline = foundSlot.GetComponent<Outline>();
            slotOutline.enabled = false;
        }
    }


    //If you want another type of selection you can change it here!
    //But after do not forget to change Deselect as well!
    public bool SelectSlot(Vector2Int position)
    {
        if(viewCreated)
        {
            if(selectedSlotPosition!=position && selectedSlotPosition.x!=-1)
                DeselectSlot();

            SetSlotOutline(position, inventoryConfiguration.selectionOutlineColor, inventoryConfiguration.selectionOutlineThikness);
            selectedSlotPosition = position;

            return true;
        }

        return false;
    }



    public bool DeselectSlot()
    {
        if(viewCreated && selectedSlotPosition.x!=-1)
        {
            if(content.transform.Find((selectedSlotPosition.x+selectedSlotPosition.y*inventoryComponent.getInventoryWidth()).ToString())!=null)
            {
                RemoveSlotOutline(selectedSlotPosition);
                selectedSlotPosition = new Vector2Int(-1,-1);

                return true;
            }
        }

        return false;
    }


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



    public bool UpdateView()
    {
        //Debug.Log("Should update it!");
        if(viewCreated)
        {
            //Debug.Log("Inv. height: "+inventoryComponent.getInventoryHeight());
            if(inventoryComponent.getInventoryHeight()!=lastInventoryHeight)
            {
                float slotSize = slotSizePercentage*Screen.width;

                RectTransform contentTransform = content.GetComponent<RectTransform>();
                Vector2 contentSize = contentTransform.sizeDelta;
                contentSize.y = slotSize*inventoryComponent.getInventoryHeight()+outerMargin*2*Screen.width+marginBetweenSlots*Screen.width*(inventoryComponent.getInventoryHeight()-1);
                //Debug.Log("Content y: "+contentSize.y);
                contentTransform.sizeDelta = contentSize;

                Vector2 currentPosition = new Vector2((outerMargin*Screen.width+slotSize/2)-(rectTransform.sizeDelta.x/2),(contentTransform.sizeDelta.y/2)-(outerMargin*Screen.width+slotSize/2));

                if(inventoryComponent.getInventoryHeight()>lastInventoryHeight)
                {
                    for (int i=0; i<lastInventoryHeight; i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).GetComponent<RectTransform>().anchoredPosition = currentPosition;
                            currentPosition.x += slotSize + marginBetweenSlots*Screen.width;
                        }

                        currentPosition.x -= (slotSize + marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + marginBetweenSlots*Screen.width;
                    }

                    for (int i=lastInventoryHeight; i<inventoryComponent.getInventoryHeight(); i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            CreateInventorySlotAt(j, i, currentPosition, slotSize);
                            currentPosition.x += slotSize + marginBetweenSlots*Screen.width;
                        }

                        currentPosition.x -= (slotSize + marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + marginBetweenSlots*Screen.width;
                    }
                }
                else
                {
                    for (int i=0; i<inventoryComponent.getInventoryHeight(); i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).GetComponent<RectTransform>().anchoredPosition = currentPosition;
                            currentPosition.x += slotSize + marginBetweenSlots*Screen.width;
                        }

                        currentPosition.x -= (slotSize + marginBetweenSlots*Screen.width)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + marginBetweenSlots*Screen.width;
                    }

                    for (int i=inventoryComponent.getInventoryHeight(); i<lastInventoryHeight; i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            Destroy(content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).gameObject);
                        }
                    }
                }

                viewCreated = true;
                lastInventoryHeight = inventoryComponent.getInventoryHeight();
            }

            //Debug.Log("Updating view!");
            List<ItemStack> itemStacks = inventoryComponent.getItemsInTheInventory();
            //Debug.Log("Length: "+itemStacks.Count);

            for(int i=0; i<itemStacks.Count; i++)
            {
                //Debug.Log("i: "+i);
                Image slotImage = content.transform.Find(i.ToString()).Find("ItemImage").gameObject.GetComponent<Image>();
                TMP_Text slotText = content.transform.Find(i.ToString()).Find("ItemText").gameObject.GetComponent<TMP_Text>();
                Image backgroundImage = content.transform.Find(i.ToString()).gameObject.GetComponent<Image>();

                if(itemStacks[i]!=null)
                {
                    slotImage.sprite = itemStacks[i].getItem().getPicture();
                    slotText.text = itemStacks[i].getItem().getMaxNumberOfBlocksInAStack()==1 ? "" : itemStacks[i].getNumOfItems().ToString();
                
                    if(inventoryConfiguration.useFiveTierRareness)
                        IndicateItemRareness(slotImage, backgroundImage, itemStacks[i].getItem().getRareness());
                }
                else
                {
                    slotImage.sprite = nothing;
                    slotText.text = "";

                    if(inventoryConfiguration.useFiveTierRareness)
                        IndicateItemRareness(slotImage, backgroundImage, Rareness.None);
                }
            }

            return true;
        }

        return false;
    }
}