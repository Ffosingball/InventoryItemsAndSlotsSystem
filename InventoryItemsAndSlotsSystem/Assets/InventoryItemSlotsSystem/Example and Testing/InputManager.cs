using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private float holdTime = 0.5f;

    private float counter = 0;
    private bool leftMousePressed = false;


    private void Update()
    {
        counter+=Time.deltaTime;

        if(counter>holdTime && leftMousePressed)
        {
            inventoryManager.LeftMouseHold();
            leftMousePressed = false;
        }
    }


    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            counter = 0;
            leftMousePressed = true;
        }
        else if (context.canceled)
        {
            if(counter<holdTime && leftMousePressed)
            {
                inventoryManager.LeftMouseClick();
                leftMousePressed = false;
            }
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
