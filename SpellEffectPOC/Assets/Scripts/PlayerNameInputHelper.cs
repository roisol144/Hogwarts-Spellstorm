using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

/// <summary>
/// Helper script specifically for the Player Name input field
/// This can be called from your existing OnSelect event to ensure proper keyboard initialization
/// </summary>
public class PlayerNameInputHelper : MonoBehaviour
{
    [Header("Input Field")]
    [SerializeField] private TMP_InputField playerNameInputField;
    
    [Header("Keyboard")]
    [SerializeField] private NonNativeKeyboard keyboard;
    
    [Header("Text Sync")]
    [SerializeField] private KeyboardTextSync textSync;

    private void Start()
    {
        // Auto-find components if not assigned
        if (playerNameInputField == null)
            playerNameInputField = GetComponent<TMP_InputField>();
            
        if (keyboard == null)
            keyboard = FindObjectOfType<NonNativeKeyboard>();
            
        if (textSync == null)
            textSync = FindObjectOfType<KeyboardTextSync>();
    }

    /// <summary>
    /// Call this method from your OnSelect event in addition to NonNativeKeyboard.PresentKeyboard
    /// This ensures the keyboard starts with the current input field text
    /// </summary>
    public void OnPlayerNameFieldSelected()
    {
        if (keyboard != null && playerNameInputField != null)
        {
            // Set the keyboard's input field to match our input field
            string currentText = playerNameInputField.text;
            if (!string.IsNullOrEmpty(currentText))
            {
                keyboard.InputField.text = currentText;
            }
            
            Debug.Log($"Player name field selected. Current text: '{currentText}'");
        }

        // Make sure text sync is set up for this input field
        if (textSync != null)
        {
            textSync.SetTargetInputField(playerNameInputField);
        }
    }

    /// <summary>
    /// Alternative method that both presents the keyboard AND initializes the text
    /// You could use this instead of having two separate calls
    /// </summary>
    public void PresentKeyboardWithCurrentText()
    {
        if (keyboard != null && playerNameInputField != null)
        {
            string currentText = playerNameInputField.text;
            
            // Present keyboard with current text
            if (!string.IsNullOrEmpty(currentText))
            {
                keyboard.PresentKeyboard(currentText);
            }
            else
            {
                keyboard.PresentKeyboard();
            }
            
            Debug.Log($"Presented keyboard with text: '{currentText}'");
        }

        // Make sure text sync is set up
        if (textSync != null)
        {
            textSync.SetTargetInputField(playerNameInputField);
        }
    }
}
