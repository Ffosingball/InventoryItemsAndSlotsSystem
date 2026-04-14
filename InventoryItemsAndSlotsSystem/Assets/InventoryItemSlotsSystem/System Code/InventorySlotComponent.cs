using UnityEngine;


//This component just exists to tell that gameobject is inventorySlot!
public class InventorySlotComponent : MonoBehaviour
{
    private Vector2Int position;

    public Vector2Int getPosition()
    {
        return position;
    }

    public void setPosition(Vector2Int pos)
    {
        position=pos;
    }
}