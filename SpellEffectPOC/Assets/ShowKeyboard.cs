using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

public class ShowKeyboard : MonoBehaviour
{
    private TMP_InputField inputField;
    private string lastKnownText = "";
    
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            inputField.onSelect.AddListener(x => OpenKeyboard());
            Debug.Log("ShowKeyboard: Successfully attached to input field");
        }
        else
        {
            Debug.LogError("ShowKeyboard: No TMP_InputField found on this GameObject!");
        }
    }

    void OnDisable()
    {
        // Hide keyboard when this GameObject (PlayerNamePanel) is disabled
        CloseKeyboard();
    }

    public void OpenKeyboard()
    {
        Debug.Log("ShowKeyboard: OpenKeyboard called");
        
        if (NonNativeKeyboard.Instance == null)
        {
            Debug.LogError("ShowKeyboard: NonNativeKeyboard.Instance is null! Make sure the keyboard prefab is in the scene and active.");
            return;
        }

        if (inputField == null)
        {
            Debug.LogError("ShowKeyboard: inputField is null!");
            return;
        }

        // Subscribe to keyboard events to sync text back to our input field
        NonNativeKeyboard.Instance.OnTextSubmitted += OnKeyboardTextSubmitted;
        NonNativeKeyboard.Instance.OnTextUpdated += OnKeyboardTextUpdated;
        
        // Subscribe to close event to preserve text when user closes keyboard
        NonNativeKeyboard.Instance.OnClosed += OnKeyboardClosed;

        // Set the keyboard's input field to reference our input field
        NonNativeKeyboard.Instance.InputField = inputField;
        
        // Store initial text
        lastKnownText = inputField.text;
        
        // Present the keyboard with current text
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
        
        // Fix positioning for VR - position keyboard in front of user at optimal distance
        PositionKeyboardForVR();
        
        Debug.Log($"ShowKeyboard: Keyboard presented with text: '{inputField.text}'");
    }

    private void PositionKeyboardForVR()
    {
        if (NonNativeKeyboard.Instance == null || Camera.main == null) return;

        // Position keyboard in front of user at a comfortable distance
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraUp = Camera.main.transform.up;
        Vector3 cameraRight = Camera.main.transform.right;

        // Position keyboard much lower and further to avoid blocking ANY UI elements
        Vector3 keyboardPosition = cameraPosition + 
                                  cameraForward * 1.2f +      // Further away
                                  cameraUp * -0.2f +          // Much lower so all buttons are visible
                                  cameraRight * 0f;        // More to the left to avoid blocking buttons

        // Position the keyboard
        NonNativeKeyboard.Instance.transform.position = keyboardPosition;

        // Make keyboard face the user but tilted slightly up for better viewing
        Vector3 lookDirection = (cameraPosition - keyboardPosition).normalized;
        Quaternion baseRotation = Quaternion.LookRotation(-lookDirection);
        // Tilt keyboard up by 15 degrees for better ergonomics
        NonNativeKeyboard.Instance.transform.rotation = baseRotation * Quaternion.Euler(15f, 0, 0);

        // Ensure keyboard is on top of other UI elements but not blocking input field
        Canvas keyboardCanvas = NonNativeKeyboard.Instance.GetComponent<Canvas>();
        if (keyboardCanvas != null)
        {
            keyboardCanvas.sortingOrder = 999; // High but not blocking input field
        }

        Debug.Log($"Positioned keyboard at: {keyboardPosition} (below input field)");
    }

    public void CloseKeyboard()
    {
        if (NonNativeKeyboard.Instance != null && NonNativeKeyboard.Instance.gameObject.activeInHierarchy)
        {
            // Preserve current text before closing using our stored text
            if (inputField != null)
            {
                string currentText = !string.IsNullOrEmpty(lastKnownText) ? lastKnownText : 
                                   (NonNativeKeyboard.Instance?.InputField?.text ?? "");
                inputField.text = currentText;
                Debug.Log($"Closing keyboard and preserving text (from lastKnownText): '{currentText}'");
            }
            
            // Close the keyboard
            NonNativeKeyboard.Instance.Close();
            
            // Unsubscribe from events
            UnsubscribeFromKeyboardEvents();
        }
    }

    private void OnKeyboardTextUpdated(string newText)
    {
        // Update our input field in real-time as user types
        if (inputField != null)
        {
            inputField.text = newText;
        }
        
        // Store the current text for preservation
        lastKnownText = newText;
    }

    private void OnKeyboardTextSubmitted(object sender, EventArgs e)
    {
        Debug.Log("ShowKeyboard: Text submitted from keyboard");
        
        // Use the last known text to avoid any clearing issues
        if (inputField != null)
        {
            // Use our stored text instead of keyboard's text (in case it got cleared)
            string finalText = !string.IsNullOrEmpty(lastKnownText) ? lastKnownText : 
                              (NonNativeKeyboard.Instance?.InputField?.text ?? "");
            inputField.text = finalText;
            Debug.Log($"Final text saved (from lastKnownText): '{finalText}'");
        }
        
        // Unsubscribe from events when done
        UnsubscribeFromKeyboardEvents();
    }

    private void OnKeyboardClosed(object sender, EventArgs e)
    {
        Debug.Log("ShowKeyboard: Keyboard closed by user");
        
        // Preserve the text when keyboard is closed using our stored text
        if (inputField != null)
        {
            // Use our stored text instead of keyboard's text (in case it got cleared)
            string currentText = !string.IsNullOrEmpty(lastKnownText) ? lastKnownText : 
                               (NonNativeKeyboard.Instance?.InputField?.text ?? "");
            inputField.text = currentText;
            Debug.Log($"Preserved text on close (from lastKnownText): '{currentText}'");
        }
        
        // Unsubscribe from events when keyboard is closed
        UnsubscribeFromKeyboardEvents();
    }

    private void UnsubscribeFromKeyboardEvents()
    {
        if (NonNativeKeyboard.Instance != null)
        {
            NonNativeKeyboard.Instance.OnTextSubmitted -= OnKeyboardTextSubmitted;
            NonNativeKeyboard.Instance.OnTextUpdated -= OnKeyboardTextUpdated;
            NonNativeKeyboard.Instance.OnClosed -= OnKeyboardClosed;
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        UnsubscribeFromKeyboardEvents();
    }
}
