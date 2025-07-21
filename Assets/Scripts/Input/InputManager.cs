using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public static InputSystem_Actions InputActions { get; private set; }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            InputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
        
        InputActions.Enable();
    }

    private void OnDisable()
    {
        InputActions.Player.Disable();
        InputActions.UI.Disable();
        InputActions.Disable();
    }

    public void EnablePlayerInputs()
    {
        InputActions.UI.Disable();
        InputActions.Player.Enable();
    }
    
    public void EnableUIInputs()
    {
        InputActions.Player.Disable();
        InputActions.UI.Enable();
    }
}
