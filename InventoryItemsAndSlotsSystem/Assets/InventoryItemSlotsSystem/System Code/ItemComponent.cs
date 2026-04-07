using UnityEngine;

public class ItemComponent : MonoBehaviour
{
    [SerializeField] private int maxNumberOfBlocksInAStack;
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private Sprite picture;

    [Header("Change only if 5 tier rareness is enabled")]
    [SerializeField] private Rareness rareness;

    [Header("Change only if items weight is enabled")]
    [SerializeField] private float itemWeight;

    //Getters and setters
    public int getMaxNumberOfBlocksInAStack()
    {
        return maxNumberOfBlocksInAStack;
    }

    public float getItemWeight()
    {
        return itemWeight;
    }
}
