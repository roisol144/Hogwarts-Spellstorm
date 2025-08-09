using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class VirtualKeyboard : MonoBehaviour
{
    [Header("Input Field")]
    [SerializeField] private TMP_InputField targetInputField;
    
    [Header("Keyboard Layout")]
    [SerializeField] private Transform keyboardContainer;
    [SerializeField] private GameObject keyButtonPrefab;
    
    [Header("Special Buttons")]
    [SerializeField] private Button spaceButton;
    [SerializeField] private Button backspaceButton;
    [SerializeField] private Button capsLockButton;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip keyPressSound;
    
    // Keyboard layout
    private string[][] keyboardRows = new string[][]
    {
        new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
        new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
        new string[] { "Z", "X", "C", "V", "B", "N", "M" }
    };
    
    private bool isCapsLockOn = false;
    private List<Button> letterButtons = new List<Button>();
    
    void Start()
    {
        CreateKeyboard();
        SetupSpecialButtons();
        
        // Setup audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }
    
    void CreateKeyboard()
    {
        if (keyboardContainer == null || keyButtonPrefab == null) return;
        
        // Clear existing buttons
        foreach (Transform child in keyboardContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
        }
        letterButtons.Clear();
        
        // Create keyboard rows
        for (int rowIndex = 0; rowIndex < keyboardRows.Length; rowIndex++)
        {
            GameObject rowObject = new GameObject($"Row{rowIndex + 1}");
            rowObject.transform.SetParent(keyboardContainer);
            
            // Add horizontal layout group
            HorizontalLayoutGroup layoutGroup = rowObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childControlWidth = true;
            
            // Create buttons for this row
            string[] row = keyboardRows[rowIndex];
            for (int keyIndex = 0; keyIndex < row.Length; keyIndex++)
            {
                string letter = row[keyIndex];
                CreateKeyButton(letter, rowObject.transform);
            }
            
            // Reset scale and position
            RectTransform rowRect = rowObject.GetComponent<RectTransform>();
            rowRect.localScale = Vector3.one;
            rowRect.localPosition = Vector3.zero;
        }
    }
    
    void CreateKeyButton(string letter, Transform parent)
    {
        GameObject buttonObj = Instantiate(keyButtonPrefab, parent);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null)
        {
            buttonText.text = letter;
        }
        
        if (button != null)
        {
            // Capture the letter value for the lambda
            string capturedLetter = letter;
            button.onClick.AddListener(() => OnKeyPressed(capturedLetter));
            letterButtons.Add(button);
        }
        
        // Set button name for easier identification
        buttonObj.name = $"Key_{letter}";
    }
    
    void SetupSpecialButtons()
    {
        if (spaceButton != null)
        {
            spaceButton.onClick.AddListener(() => OnKeyPressed(" "));
        }
        
        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(OnBackspacePressed);
        }
        
        if (capsLockButton != null)
        {
            capsLockButton.onClick.AddListener(ToggleCapsLock);
            UpdateCapsLockVisual();
        }
    }
    
    public void OnKeyPressed(string key)
    {
        if (targetInputField == null) return;
        
        // Play sound effect
        PlayKeySound();
        
        // Apply caps lock if enabled
        string finalKey = isCapsLockOn ? key.ToUpper() : key.ToLower();
        
        // Add character to input field
        targetInputField.text += finalKey;
        
        // Keep input field focused (important for VR)
        targetInputField.ActivateInputField();
    }
    
    public void OnBackspacePressed()
    {
        if (targetInputField == null || targetInputField.text.Length == 0) return;
        
        PlayKeySound();
        
        // Remove last character
        targetInputField.text = targetInputField.text.Substring(0, targetInputField.text.Length - 1);
        targetInputField.ActivateInputField();
    }
    
    public void ToggleCapsLock()
    {
        isCapsLockOn = !isCapsLockOn;
        UpdateCapsLockVisual();
        UpdateKeyboardCase();
        PlayKeySound();
    }
    
    void UpdateCapsLockVisual()
    {
        if (capsLockButton != null)
        {
            Image buttonImage = capsLockButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = isCapsLockOn ? Color.yellow : Color.white;
            }
        }
    }
    
    void UpdateKeyboardCase()
    {
        foreach (Button button in letterButtons)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isCapsLockOn ? buttonText.text.ToUpper() : buttonText.text.ToLower();
            }
        }
    }
    
    void PlayKeySound()
    {
        if (audioSource != null && keyPressSound != null)
        {
            audioSource.PlayOneShot(keyPressSound);
        }
    }
    
    public void SetTargetInputField(TMP_InputField inputField)
    {
        targetInputField = inputField;
    }
    
    public void ClearInput()
    {
        if (targetInputField != null)
        {
            targetInputField.text = "";
            targetInputField.ActivateInputField();
        }
    }
    
    // Public method to reset caps lock state
    public void ResetCapsLock()
    {
        isCapsLockOn = false;
        UpdateCapsLockVisual();
        UpdateKeyboardCase();
    }
}