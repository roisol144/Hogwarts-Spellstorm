using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR;

public class GestureTrainingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI currentGestureNameText;
    
    [Header("Training Settings")]
    [SerializeField] private MovementRecognizer movementRecognizer;
    [SerializeField] private string[] predefinedGestureNames;
    
    [Header("Input Settings")]
    [SerializeField] private XRNode inputSource = XRNode.RightHand;
    [SerializeField] private InputHelpers.Button toggleButton = InputHelpers.Button.PrimaryButton;
    [SerializeField] private float inputThreshold = 0.1f;
    
    private int currentGestureIndex = 0;
    private bool isTrainingMode = false;
    private bool wasToggleButtonPressed = false;

    void Start()
    {
        if (predefinedGestureNames.Length == 0)
        {
            // Default gesture names if none are provided
            predefinedGestureNames = new string[] 
            { 
                "cast_accio", "cast_bombardo", "cast_expecto_patronum", "cast_stupefy"
            };
        }
        
        UpdateUI();
        if (movementRecognizer != null && predefinedGestureNames.Length > 0)
        {
            Debug.Log($"[GestureTrainingUI] Setting initial gesture name: {predefinedGestureNames[currentGestureIndex]}");
            movementRecognizer.SetGestureName(predefinedGestureNames[currentGestureIndex]);
        }
    }

    void Update()
    {
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), toggleButton, out bool isPressed, inputThreshold);

        // Handle toggle training mode (on button press, not hold)
        if (isPressed && !wasToggleButtonPressed)
        {
            ToggleTrainingMode();
        }
        else if (isPressed && wasToggleButtonPressed && isTrainingMode)
        {
            // If holding the button in training mode, cycle through names
            CycleGestureName();
        }

        wasToggleButtonPressed = isPressed;
    }

    public void ToggleTrainingMode()
    {
        isTrainingMode = !isTrainingMode;
        if (isTrainingMode)
        {
            statusText.text = "TRAINING MODE\nPress Trigger to record gesture\nPress A/X to cycle names";
            movementRecognizer.SetTrainingMode(true, predefinedGestureNames[currentGestureIndex]);
        }
        else
        {
            statusText.text = "RECOGNITION MODE";
            movementRecognizer.SetTrainingMode(false, "");
        }
        UpdateUI();
    }

    public void CycleGestureName()
    {
        if (!isTrainingMode) return;
        
        currentGestureIndex = (currentGestureIndex + 1) % predefinedGestureNames.Length;
        movementRecognizer.SetGestureName(predefinedGestureNames[currentGestureIndex]);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentGestureNameText != null)
        {
            currentGestureNameText.text = isTrainingMode ? 
                $"Current Gesture: {predefinedGestureNames[currentGestureIndex]}" : 
                "Recognition Mode";
        }
    }
} 