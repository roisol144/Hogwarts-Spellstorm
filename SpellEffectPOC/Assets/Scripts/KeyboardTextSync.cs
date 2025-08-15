using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

/// <summary>
/// Simple script to sync text between MRTK NonNativeKeyboard and your TMP_InputField
/// Since you already have the keyboard showing, this just handles the text synchronization
/// </summary>
public class KeyboardTextSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField targetInputField;
    [SerializeField] private NonNativeKeyboard keyboard;

    private bool isSubscribed = false;

    private void Start()
    {
        SetupTextSync();
    }

    private void SetupTextSync()
    {
        // Find keyboard if not assigned
        if (keyboard == null)
        {
            keyboard = FindObjectOfType<NonNativeKeyboard>();
            if (keyboard == null)
            {
                Debug.LogError("NonNativeKeyboard not found in scene!");
                return;
            }
        }

        // Subscribe to keyboard events
        SubscribeToKeyboardEvents();
    }

    private void SubscribeToKeyboardEvents()
    {
        if (keyboard != null && !isSubscribed)
        {
            keyboard.OnTextSubmitted += OnKeyboardTextSubmitted;
            keyboard.OnTextUpdated += OnKeyboardTextUpdated;
            isSubscribed = true;
            Debug.Log("Subscribed to keyboard events for text sync");
        }
    }

    private void UnsubscribeFromKeyboardEvents()
    {
        if (keyboard != null && isSubscribed)
        {
            keyboard.OnTextSubmitted -= OnKeyboardTextSubmitted;
            keyboard.OnTextUpdated -= OnKeyboardTextUpdated;
            isSubscribed = false;
        }
    }

    private void OnKeyboardTextUpdated(string newText)
    {
        // Update your input field in real-time as user types
        if (targetInputField != null)
        {
            targetInputField.text = newText;
        }
    }

    private void OnKeyboardTextSubmitted(object sender, EventArgs e)
    {
        // When user presses Enter, get the final text
        if (keyboard != null && targetInputField != null)
        {
            string finalText = keyboard.InputField.text;
            targetInputField.text = finalText;
            Debug.Log($"Player name entered: {finalText}");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromKeyboardEvents();
    }

    // Public method to manually set the input field
    public void SetTargetInputField(TMP_InputField inputField)
    {
        targetInputField = inputField;
    }

    // Public method to initialize the keyboard with current input field text
    public void InitializeKeyboardWithCurrentText()
    {
        if (keyboard != null && targetInputField != null)
        {
            keyboard.InputField.text = targetInputField.text;
        }
    }
}
