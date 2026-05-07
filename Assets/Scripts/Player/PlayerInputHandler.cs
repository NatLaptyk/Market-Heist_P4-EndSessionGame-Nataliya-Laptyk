using UnityEngine;
using UnityEngine.InputSystem;

// Input binding and handling for player actions, using the new Input System:
// Callback-based (performed / canceled), no polling in Update.

public class PlayerInputHandler : MonoBehaviour
{
    PlayerInputActions m_inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public event System.Action OnInteractPressed;
    public event System.Action OnAttackPressed;
    public event System.Action OnJumpPressed;
    public event System.Action OnSavePressed;
    public event System.Action OnLoadPressed;

    private void Awake()
    {
        m_inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        m_inputActions.Player.Enable();

        m_inputActions.Player.Move.performed += OnMovePerformed;
        m_inputActions.Player.Move.canceled += OnMoveCanceled;

        m_inputActions.Player.Look.performed += OnLookPerformed;
        m_inputActions.Player.Look.canceled += OnLookCanceled;

        m_inputActions.Player.Interact.performed += OnInteractPerformed;
        m_inputActions.Player.Attack.performed += OnAttackPerformed;

        m_inputActions.Player.Jump.performed += OnJumpPerformed;
        m_inputActions.Player.Save.performed += OnSavePerformed;
        m_inputActions.Player.Load.performed += OnLoadPerformed;
    }

    private void OnDisable()
    {
        m_inputActions.Player.Move.performed -= OnMovePerformed;
        m_inputActions.Player.Move.canceled -= OnMoveCanceled;

        m_inputActions.Player.Look.performed -= OnLookPerformed;
        m_inputActions.Player.Look.canceled -= OnLookCanceled;

        m_inputActions.Player.Interact.performed -= OnInteractPerformed;
        m_inputActions.Player.Attack.performed -= OnAttackPerformed;

        m_inputActions.Player.Jump.performed -= OnJumpPerformed;
        m_inputActions.Player.Save.performed -= OnSavePerformed;
        m_inputActions.Player.Load.performed -= OnLoadPerformed;

        m_inputActions.Player.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        LookInput = Vector2.zero;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        OnAttackPressed?.Invoke();
    } 
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
    }
    private void OnSavePerformed(InputAction.CallbackContext context)
    {
        OnSavePressed?.Invoke();
    }

    private void OnLoadPerformed(InputAction.CallbackContext context)
    {
        OnLoadPressed?.Invoke();
    }
    
}