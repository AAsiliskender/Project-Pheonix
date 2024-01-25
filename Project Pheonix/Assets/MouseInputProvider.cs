using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class MouseInputProvider : MonoBehaviour
{
    [SerializeField]
    public UnityEvent onClicked;
    public UnityEvent onReleased;

    // Input (cursor select action and position)
    [SerializeField]
    private InputActionReference selectAction;
    [SerializeField]
    private InputActionReference pointerPosition;

    private Vector2 prevPointerInput;

    private EventManager eventManager;

    private void OnEnable()
    {
        eventManager = FindObjectOfType<EventManager>();
        if (eventManager != null)
        {
            eventManager.SetMouseInputProvider(this);
        }

        // Subscribe to the select action's started and canceled events
        selectAction.action.started += OnClickStarted;
        selectAction.action.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        // Unsubscribe from the select action's started and canceled events
        selectAction.action.started -= OnClickStarted;
        selectAction.action.canceled -= OnClickCanceled;
    }

    private void Update()
    {
        // Check if the mouse position has changed
        Vector2 currentPointerInput = GetCursorInput(pointerPosition);
        if (currentPointerInput != prevPointerInput)
        {
            // Handle cursor position change here if needed
            prevPointerInput = currentPointerInput;
        }
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Mouse Clicked");
        // Invoke the onClicked UnityEvent when the mouse click is started
        onClicked.Invoke();
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Mouse Click Released");
        // Invoke the onReleased UnityEvent when the mouse click is released
        onReleased.Invoke();
    }

    // Get cursor position in screen coordinates
    private Vector2 GetCursorInput(InputActionReference _pointerPosition)
    {
        return _pointerPosition.action.ReadValue<Vector2>();
    }
}
