using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


//This component has to be put at the scrollView ui gameObject!!
public class InventoryViewComponent : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventoryComponent;
    //[SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private float viewWidth = 0.3f; 
    [SerializeField] private float viewHeight = 0.3f;
    //Percentage of the pixel size of the inventorySlot
    [SerializeField] private float marginBetweenSlots = 0.2f;
    //Percentage of the pixel size of the inventoryView
    [SerializeField] private float outerMargin = 0.1f;
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
        inventoryComponent = value;
    }

    public float getViewWidth()
    {
        return viewWidth;
    }

    public void setViewWidth(float value)
    {
        viewWidth = value;
    }

    public float getViewHeight()
    {
        return viewHeight;
    }

    public void setViewHeight(float value)
    {
        viewHeight = value;
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
        rectTransform.sizeDelta = new Vector2(viewWidth*Screen.width, viewHeight*Screen.height);

        float slotSize = (rectTransform.sizeDelta.x*(1-2*outerMargin))/(inventoryComponent.getInventoryWidth()+((inventoryComponent.getInventoryWidth()-1)*marginBetweenSlots));

        RectTransform contentTransform = content.GetComponent<RectTransform>();
        Vector2 contentSize = contentTransform.sizeDelta;
        contentSize.y = slotSize*(inventoryComponent.getInventoryHeight()+marginBetweenSlots*(inventoryComponent.getInventoryHeight()-1))+outerMargin*2*rectTransform.sizeDelta.x;
        //Debug.Log("Content y: "+contentSize.y);
        contentTransform.sizeDelta = contentSize;

        Vector2 currentPosition = new Vector2((outerMargin*rectTransform.sizeDelta.x+slotSize/2)-(rectTransform.sizeDelta.x/2),(contentTransform.sizeDelta.y/2)-(outerMargin*rectTransform.sizeDelta.x+slotSize/2));

        for (int i=0; i<inventoryComponent.getInventoryHeight(); i++)
        {
            for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
            {
                CreateInventorySlotAt(j, i, currentPosition, slotSize);
                currentPosition.x += slotSize + marginBetweenSlots*slotSize;
            }

            currentPosition.x -= (slotSize + marginBetweenSlots*slotSize)*inventoryComponent.getInventoryWidth();
            currentPosition.y -= slotSize + marginBetweenSlots*slotSize;
        }

        viewCreated = true;
        lastInventoryHeight = inventoryComponent.getInventoryHeight();
    }



    public void DeleteView()
    {
        foreach (Transform child in content.transform)
        {
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



    public bool SelectSlot(Vector2Int position)
    {
        if(viewCreated)
        {
            if(selectedSlotPosition.x!=-1)
                DeselectSlot(selectedSlotPosition);

            GameObject selecetdSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString()).gameObject;

            Image slotBackground = selecetdSlot.GetComponent<Image>();
            Color color = slotBackground.color;
            color.r+=0.3f;
            color.g+=0.3f;
            color.b+=0.3f;
            slotBackground.color = color;

            selectedSlotPosition = position;

            return true;
        }

        return false;
    }



    public bool DeselectSlot(Vector2Int position)
    {
        if(viewCreated)
        {
            if(content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString())!=null)
            {
                GameObject selecetdSlot = content.transform.Find((position.x+position.y*inventoryComponent.getInventoryWidth()).ToString()).gameObject;

                Image slotBackground = selecetdSlot.GetComponent<Image>();
                Color color = slotBackground.color;
                color.r-=0.3f;
                color.g-=0.3f;
                color.b-=0.3f;
                slotBackground.color = color;

                selectedSlotPosition = new Vector2Int(-1,-1);

                return true;
            }
        }

        return false;
    }



    public bool UpdateView()
    {
        //Debug.Log("Should update it!");
        if(viewCreated)
        {
            Debug.Log("Inv. height: "+inventoryComponent.getInventoryHeight());
            if(inventoryComponent.getInventoryHeight()!=lastInventoryHeight)
            {
                float slotSize = (rectTransform.sizeDelta.x*(1-2*outerMargin))/(inventoryComponent.getInventoryWidth()+((inventoryComponent.getInventoryWidth()-1)*marginBetweenSlots));

                RectTransform contentTransform = content.GetComponent<RectTransform>();
                Vector2 contentSize = contentTransform.sizeDelta;
                contentSize.y = slotSize*(inventoryComponent.getInventoryHeight()+marginBetweenSlots*(inventoryComponent.getInventoryHeight()-1))+outerMargin*2*rectTransform.sizeDelta.x;
                //Debug.Log("Content y: "+contentSize.y);
                contentTransform.sizeDelta = contentSize;

                Vector2 currentPosition = new Vector2((outerMargin*rectTransform.sizeDelta.x+slotSize/2)-(rectTransform.sizeDelta.x/2),(contentTransform.sizeDelta.y/2)-(outerMargin*rectTransform.sizeDelta.x+slotSize/2));

                if(inventoryComponent.getInventoryHeight()>lastInventoryHeight)
                {
                    for (int i=0; i<lastInventoryHeight; i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).GetComponent<RectTransform>().anchoredPosition = currentPosition;
                            currentPosition.x += slotSize + marginBetweenSlots*slotSize;
                        }

                        currentPosition.x -= (slotSize + marginBetweenSlots*slotSize)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + marginBetweenSlots*slotSize;
                    }

                    for (int i=lastInventoryHeight; i<inventoryComponent.getInventoryHeight(); i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            CreateInventorySlotAt(j, i, currentPosition, slotSize);
                            currentPosition.x += slotSize + marginBetweenSlots*slotSize;
                        }

                        currentPosition.x -= (slotSize + marginBetweenSlots*slotSize)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + marginBetweenSlots*slotSize;
                    }
                }
                else
                {
                    for (int i=0; i<inventoryComponent.getInventoryHeight(); i++)
                    {
                        for (int j=0; j<inventoryComponent.getInventoryWidth(); j++)
                        {
                            content.transform.Find((j+i*inventoryComponent.getInventoryWidth()).ToString()).GetComponent<RectTransform>().anchoredPosition = currentPosition;
                            currentPosition.x += slotSize + marginBetweenSlots*slotSize;
                        }

                        currentPosition.x -= (slotSize + marginBetweenSlots*slotSize)*inventoryComponent.getInventoryWidth();
                        currentPosition.y -= slotSize + marginBetweenSlots*slotSize;
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
            Debug.Log("Length: "+itemStacks.Count);

            for(int i=0; i<itemStacks.Count; i++)
            {
                //Debug.Log("i: "+i);
                Image slotImage = content.transform.Find(i.ToString()).Find("ItemImage").gameObject.GetComponent<Image>();
                TMP_Text slotText = content.transform.Find(i.ToString()).Find("ItemText").gameObject.GetComponent<TMP_Text>();

                if(itemStacks[i]!=null)
                {
                    slotImage.sprite = itemStacks[i].getItem().getPicture();
                    slotText.text = itemStacks[i].getNumOfItems().ToString();
                }
                else
                {
                    slotImage.sprite = nothing;
                    slotText.text = "";
                }
            }

            return true;
        }

        return false;
    }
}