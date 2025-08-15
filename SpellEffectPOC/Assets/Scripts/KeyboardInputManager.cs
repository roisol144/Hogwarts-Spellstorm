using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

/// <summary>
/// Manages the integration between MRTK NonNativeKeyboard and TMP_InputField
/// This script handles showing/hiding the keyboard and synchronizing text input
/// </summary>
public class KeyboardInputManager : MonoBehaviour
{
    [Header("Input Field Configuration")]
    [SerializeField] private TMP_InputField targetInputField;
    
    [Header("Keyboard Configuration")]
    [SerializeField] private GameObject keyboardGameObject;
    [SerializeField] private bool showKeyboardOnInputFieldSelect = true;
    [SerializeField] private bool repositionKeyboardForVR = true;
    [SerializeField] private Vector3 keyboardOffset = new Vector3(0, -0.5f, 0.8f);

    private NonNativeKeyboard keyboard;
    private bool isKeyboardSubscribed = false;

    private void Start()
    {
        InitializeKeyboard();
        SetupInputFieldInteraction();
    }

    private void InitializeKeyboard()
    {
        // Find the keyboard component
        if (keyboardGameObject != null)
        {
            keyboard = keyboardGameObject.GetComponent<NonNativeKeyboard>();
            if (keyboard == null)
            {
                Debug.LogError("NonNativeKeyboard component not found on keyboard GameObject!");
                return;
            }
        }
        else
        {
            // Try to find it in the scene
            keyboard = FindObjectOfType<NonNativeKeyboard>();
            if (keyboard == null)
            {
                Debug.LogError("NonNativeKeyboard not found in scene! Make sure the keyboard prefab is added to the scene.");
                return;
            }
            keyboardGameObject = keyboard.gameObject;
        }

        // Make sure keyboard starts hidden
        keyboardGameObject.SetActive(false);
    }

    private void SetupInputFieldInteraction()
    {
        if (targetInputField == null)
        {
            Debug.LogError("Target input field is not assigned!");
            return;
        }

        // Add event listeners for input field interaction
        targetInputField.onSelect.AddListener(OnInputFieldSelected);
        targetInputField.onDeselect.AddListener(OnInputFieldDeselected);
    }

    private void OnInputFieldSelected(string text)
    {
        if (showKeyboardOnInputFieldSelect && keyboard != null)
        {
            ShowKeyboard();
        }
    }

    private void OnInputFieldDeselected(string text)
    {
        // We'll let the user close the keyboard manually or via the keyboard's close button
        // Don't auto-hide on deselect as it can be annoying in VR
    }

    public void ShowKeyboard()
    {
        if (keyboard == null) return;

        // Subscribe to keyboard events if not already subscribed
        if (!isKeyboardSubscribed)
        {
            keyboard.OnTextSubmitted += OnKeyboardTextSubmitted;
            keyboard.OnTextUpdated += OnKeyboardTextUpdated;
            isKeyboardSubscribed = true;
        }

        // Position keyboard for VR if needed
        if (repositionKeyboardForVR)
        {
            PositionKeyboardForVR();
        }

        // Present the keyboard with current input field text
        string currentText = targetInputField != null ? targetInputField.text : "";
        keyboard.PresentKeyboard(currentText);

        Debug.Log("Keyboard presented for player name input");
    }

    public void HideKeyboard()
    {
        if (keyboard != null && keyboardGameObject.activeInHierarchy)
        {
            keyboard.Close();
        }
    }

    private void PositionKeyboardForVR()
    {
        if (targetInputField == null || keyboardGameObject == null) return;

        // Position keyboard relative to the input field
        Vector3 inputFieldPosition = targetInputField.transform.position;
        Vector3 keyboardPosition = inputFieldPosition + keyboardOffset;

        // If we have a camera, position relative to camera instead
        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            Vector3 cameraUp = Camera.main.transform.up;

            keyboardPosition = Camera.main.transform.position + 
                              cameraForward * keyboardOffset.z + 
                              cameraRight * keyboardOffset.x + 
                              cameraUp * keyboardOffset.y;
        }

        keyboardGameObject.transform.position = keyboardPosition;

        // Make keyboard look at camera
        if (Camera.main != null)
        {
            Vector3 lookDirection = (Camera.main.transform.position - keyboardGameObject.transform.position).normalized;
            keyboardGameObject.transform.rotation = Quaternion.LookRotation(-lookDirection);
        }
    }

    private void OnKeyboardTextUpdated(string newText)
    {
        // Update the input field in real-time as user types
        if (targetInputField != null)
        {
            targetInputField.text = newText;
        }
    }

    private void OnKeyboardTextSubmitted(object sender, EventArgs e)
    {
        // User pressed Enter on the keyboard
        if (keyboard != null && targetInputField != null)
        {
            // Get the final text from the keyboard
            string finalText = keyboard.InputField.text;
            targetInputField.text = finalText;

            Debug.Log($"Keyboard text submitted: {finalText}");
        }

        // Keyboard will close automatically after text submission
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (keyboard != null && isKeyboardSubscribed)
        {
            keyboard.OnTextSubmitted -= OnKeyboardTextSubmitted;
            keyboard.OnTextUpdated -= OnKeyboardTextUpdated;
        }

        if (targetInputField != null)
        {
            targetInputField.onSelect.RemoveListener(OnInputFieldSelected);
            targetInputField.onDeselect.RemoveListener(OnInputFieldDeselected);
        }
    }

    // Public methods that can be called from buttons or other scripts
    public void SetTargetInputField(TMP_InputField inputField)
    {
        if (targetInputField != null)
        {
            targetInputField.onSelect.RemoveListener(OnInputFieldSelected);
            targetInputField.onDeselect.RemoveListener(OnInputFieldDeselected);
        }

        targetInputField = inputField;
        SetupInputFieldInteraction();
    }

    public void ToggleKeyboard()
    {
        if (keyboardGameObject != null)
        {
            if (keyboardGameObject.activeInHierarchy)
            {
                HideKeyboard();
            }
            else
            {
                ShowKeyboard();
            }
        }
    }
}
