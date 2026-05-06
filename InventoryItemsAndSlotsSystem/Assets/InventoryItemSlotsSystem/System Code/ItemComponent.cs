using System.Linq;
using UnityEngine;


//This component stores information about items
public class ItemComponent : MonoBehaviour
{
    //Stack cap
    [SerializeField] private int maxNumberOfBlocksInAStack;
    [Tooltip("Each distinct item should have unique name")]
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private Sprite picture;
    [SerializeField] private Sprite anySizeInventorypicture;

    [Header("Change only if 5 tier rareness is enabled")]
    [SerializeField] private Rareness rareness = Rareness.None;

    [Header("Change only if items weight is enabled")]
    [Tooltip("Cannot be less than 0!")]
    [SerializeField] private float itemWeight;

    [Header("Change only if items stack limit is enabled")]
    [Tooltip("Cannot be less then 0!")]
    [SerializeField] private int itemStackLimit = 3;

    [Header("Change only if inventory type is not sigle celled")]
    [Tooltip("Cannot be less then 1!")]
    [SerializeField] private Vector2Int itemSize = new Vector2Int(1,1);

    [Header("Change only if inventory type is any size type")]
    //Used only in the inspector
    //String should be like this: for example item size is 3x3
    //111
    //101
    //111
    //Item with the hole in the middle, no spaces, no extra letters, \n for the next row
    [Multiline][SerializeField] string cellsOccupiedString;
    
    private bool[] itemCellsOccupied;

    //Getters and setters
    public int getMaxNumberOfBlocksInAStack()
    {
        return maxNumberOfBlocksInAStack;
    }

    public void setMaxNumberOfBlocksInAStack(int value)
    {
        maxNumberOfBlocksInAStack = value;
    }

    public float getItemWeight()
    {
        return itemWeight;
    }

    public void setItemWeight(float value)
    {
        itemWeight = value;
    }

    public Rareness getRareness()
    {
        return rareness;
    }

    public void setRareness(Rareness rareness)
    {
        this.rareness = rareness;
    }

    public string getItemName()
    {
        return itemName;
    }

    public void setItemName(string itemName)
    {
        this.itemName = itemName;
    }

    public ItemType getItemType()
    {
        return itemType;
    }

    public void setItemType(ItemType itemType)
    {
        this.itemType = itemType;
    }

    public Sprite getPicture()
    {
        return picture;
    }

    public void setPicture(Sprite picture)
    {
        this.picture = picture;
    }

    public Sprite getAnySizeInventoryPicture()
    {
        return anySizeInventorypicture;
    }

    public void setAnySizeInventoryPicture(Sprite picture)
    {
       anySizeInventorypicture = picture;
    }

    public Vector2Int getItemSize()
    {
        return itemSize;
    }

    public void setItemSize(Vector2Int value)
    {
        itemSize = value;
    }

    public void setItemStackLimit(int value)
    {
        itemStackLimit = value;
    }

    public int getItemStackLimit()
    {
        return itemStackLimit;
    }

    //It will succed only if provided size of the array is equal to the number of cells in the item
    //it can be set only once! If you want to change it you have to create new item
    public void setCellsOccupiedArray(bool[] value)
    {
        if(value.Length==itemSize.x*itemSize.y && itemCellsOccupied==null)
        {
            itemCellsOccupied = value;
        }
    }

    public bool[] getCellsOccupiedArray()
    {
        return itemCellsOccupied;
    }



    private void Start()
    {
        //Initialize item
        if(itemName==null)
            itemName = System.Guid.NewGuid().ToString();

        if(itemCellsOccupied==null && cellsOccupiedString!=null)
        {
            if(cellsOccupiedString.Length>=itemSize.x*itemSize.y)
            {
                itemCellsOccupied = new bool[itemSize.x*itemSize.y];
                int arrX = 0;
                int arrY = 0;
                bool done = false;
                for(int i=0; i<cellsOccupiedString.Length; i++)
                {
                    switch(cellsOccupiedString[i])
                    {
                        case '1':
                        {
                            if(arrX<itemSize.x)
                                itemCellsOccupied[arrX+arrY*itemSize.x] = true;
                            
                            arrX++;
                            break;
                        }
                        case '0':
                        {
                            if(arrX<itemSize.x)
                                itemCellsOccupied[arrX+arrY*itemSize.x] = false;
                            
                            arrX++;
                            break;
                        }
                        case '\n':
                        {
                            arrY++;
                            arrX=0;
                            if(arrY>=itemSize.y)
                                done=true;
                            break;
                        }
                        default:
                        {
                            done=true;
                            itemCellsOccupied = null;
                            break;
                        }
                    }

                    if(arrX+arrY*itemSize.x>=itemSize.x*itemSize.y)
                        done=true;

                    if(done)
                        break;
                }
            }
        }

        if(itemCellsOccupied==null)
        {
            itemCellsOccupied = new bool[itemSize.x*itemSize.y];
            for(int i=0; i<itemSize.x*itemSize.y; i++)
            {
                itemCellsOccupied[i] = true;
            }
        }
    }
}
