using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple script to add to a button that will show/hide the keyboard
/// This provides a manual way for users to control the keyboard visibility
/// </summary>
public class KeyboardToggleButton : MonoBehaviour
{
    [Header("Keyboard Manager")]
    [SerializeField] private KeyboardInputManager keyboardManager;
    
    [Header("Button Configuration")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private string showKeyboardText = "Show Keyboard";
    [SerializeField] private string hideKeyboardText = "Hide Keyboard";

    private TMPro.TextMeshProUGUI buttonText;

    private void Start()
    {
        SetupButton();
    }

    private void SetupButton()
    {
        // Get button reference if not assigned
        if (toggleButton == null)
        {
            toggleButton = GetComponent<Button>();
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleKeyboard);
            buttonText = toggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = showKeyboardText;
            }
        }

        // Find keyboard manager if not assigned
        if (keyboardManager == null)
        {
            keyboardManager = FindObjectOfType<KeyboardInputManager>();
            if (keyboardManager == null)
            {
                Debug.LogWarning("KeyboardInputManager not found! Please assign it in the inspector.");
            }
        }
    }

    private void ToggleKeyboard()
    {
        if (keyboardManager != null)
        {
            keyboardManager.ToggleKeyboard();
            UpdateButtonText();
        }
    }

    private void UpdateButtonText()
    {
        if (buttonText == null) return;

        // Check if keyboard is currently visible
        // This is a simple check - you might want to add a proper IsVisible property to KeyboardInputManager
        bool keyboardVisible = keyboardManager != null && 
                              keyboardManager.GetComponent<KeyboardInputManager>() != null;
        
        buttonText.text = keyboardVisible ? hideKeyboardText : showKeyboardText;
    }

    public void ShowKeyboard()
    {
        if (keyboardManager != null)
        {
            keyboardManager.ShowKeyboard();
            UpdateButtonText();
        }
    }

    public void HideKeyboard()
    {
        if (keyboardManager != null)
        {
            keyboardManager.HideKeyboard();
            UpdateButtonText();
        }
    }
}
