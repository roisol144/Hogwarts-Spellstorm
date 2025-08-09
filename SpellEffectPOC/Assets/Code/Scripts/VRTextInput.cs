using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class VRTextInput : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button[] keyboardButtons;
    
    [Header("Keyboard Layout")]
    [SerializeField] private Transform keyboardParent;
    [SerializeField] private Button keyButtonPrefab;
    
    [Header("Special Buttons")]
    [SerializeField] private Button spaceButton;
    [SerializeField] private Button backspaceButton;
    [SerializeField] private Button enterButton;
    
    private string[] letters = {
        "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P",
        "A", "S", "D", "F", "G", "H", "J", "K", "L",
        "Z", "X", "C", "V", "B", "N", "M"
    };
    
    void Start()
    {
        CreateVRKeyboard();
        SetupSpecialButtons();
    }
    
    void CreateVRKeyboard()
    {
        if (keyboardParent == null || keyButtonPrefab == null) return;
        
        int letterIndex = 0;
        
        // Create 3 rows
        for (int row = 0; row < 3; row++)
        {
            GameObject rowObj = new GameObject($"Row_{row}");
            rowObj.transform.SetParent(keyboardParent);
            
            HorizontalLayoutGroup layout = rowObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            
            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            rowRect.localScale = Vector3.one;
            rowRect.anchorMin = Vector2.zero;
            rowRect.anchorMax = Vector2.one;
            
            int lettersInRow = (row == 0) ? 10 : (row == 1) ? 9 : 7;
            
            for (int col = 0; col < lettersInRow && letterIndex < letters.Length; col++)
            {
                CreateKeyButton(letters[letterIndex], rowObj.transform);
                letterIndex++;
            }
        }
    }
    
    void CreateKeyButton(string letter, Transform parent)
    {
        Button btn = Instantiate(keyButtonPrefab, parent);
        btn.name = $"Key_{letter}";
        
        TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = letter;
            btnText.fontSize = 24;
        }
        
        // Make button VR-friendly
        btn.transform.localScale = Vector3.one;
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(60, 60);
        
        string capturedLetter = letter;
        btn.onClick.AddListener(() => AddLetter(capturedLetter));
    }
    
    void SetupSpecialButtons()
    {
        if (spaceButton != null)
        {
            spaceButton.onClick.AddListener(() => AddLetter(" "));
        }
        
        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(Backspace);
        }
        
        if (enterButton != null)
        {
            enterButton.onClick.AddListener(FinishInput);
        }
    }
    
    public void AddLetter(string letter)
    {
        if (inputField != null)
        {
            inputField.text += letter;
        }
    }
    
    public void Backspace()
    {
        if (inputField != null && inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }
    
    public void FinishInput()
    {
        if (inputField != null)
        {
            inputField.DeactivateInputField();
        }
        
        // Notify MainMenuManager that input is complete
        MainMenuManager mainMenu = FindObjectOfType<MainMenuManager>();
        if (mainMenu != null && !string.IsNullOrEmpty(inputField.text))
        {
            // Trigger save and play
            mainMenu.GetComponent<MainMenuManager>().SendMessage("OnSaveAndPlay", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    public void ClearInput()
    {
        if (inputField != null)
        {
            inputField.text = "";
        }
    }
    
    public void SetInputField(TMP_InputField field)
    {
        inputField = field;
    }
}