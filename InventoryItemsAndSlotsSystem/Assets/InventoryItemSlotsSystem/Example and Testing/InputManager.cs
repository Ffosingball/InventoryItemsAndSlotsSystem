using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;


    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            inventoryManager.LeftMouseClick();
        }
    }


    public void OnRightClick(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            inventoryManager.RightMouseClick();
        }
    }
}
