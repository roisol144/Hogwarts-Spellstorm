using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using System.IO;
using TMPro;
using PDollarGestureRecognizer;
using System;
using System.Collections;

public class GestureTrainingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI currentSpellText;
    [SerializeField] private GameObject spellSelectionPanel;
    
    [Header("Training Settings")]
    [SerializeField] private Transform movementSource;
    [SerializeField] private float newPositionThresholdDistance = 0.05f;
    [SerializeField] private float recognitionThreshold = 0.9f;
    
    [Header("Visual Feedback")]
    [SerializeField] private LineRenderer gestureLineRenderer;
    [SerializeField] private Material recordingMaterial;
    [SerializeField] private Material previewMaterial;
    
    [Header("Spell List")]
    [SerializeField] private string[] spellNames = new string[] 
    {
        "cast_bombardo",
        "cast_expecto_patronum",
        "cast_stupefy",
        "cast_protego"
    };

    [Header("File Transfer Settings")]
    [SerializeField] private string transferDirectory = "GestureTraining";
    [SerializeField] private bool useExternalStorage = true;

    private List<Vector3> positionsList = new List<Vector3>();
    private bool isRecording = false;
    private int currentSpellIndex = 0;
    private string saveDirectory;
    private string transferPath;

    void Start()
    {
        InitializeDirectories();
        UpdateUI();
    }

    void InitializeDirectories()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, transferDirectory);
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    void Update()
    {
        // Check for trigger press
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), 
            InputHelpers.Button.Trigger, out bool isPressed, 0.1f);

        if (isPressed && !isRecording)
        {
            StartRecording();
        }
        else if (!isPressed && isRecording)
        {
            StopRecording();
        }
        else if (isRecording)
        {
            UpdateRecording();
        }

        // Check for spell selection (using A/X button)
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), 
            InputHelpers.Button.PrimaryButton, out bool isSelectPressed, 0.1f);
        
        if (isSelectPressed)
        {
            CycleSpell();
        }

        // Check for file transfer (using B/Y button)
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), 
            InputHelpers.Button.SecondaryButton, out bool isTransferPressed, 0.1f);
        
        if (isTransferPressed)
        {
            TransferFiles();
        }
    }

    void StartRecording()
    {
        isRecording = true;
        positionsList.Clear();
        positionsList.Add(movementSource.position);
        
        gestureLineRenderer.material = recordingMaterial;
        gestureLineRenderer.positionCount = 1;
        gestureLineRenderer.SetPosition(0, movementSource.position);
    }

    void UpdateRecording()
    {
        Vector3 lastPosition = positionsList[positionsList.Count - 1];
        if (Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
        {
            positionsList.Add(movementSource.position);
            gestureLineRenderer.positionCount = positionsList.Count;
            gestureLineRenderer.SetPosition(positionsList.Count - 1, movementSource.position);
        }
    }

    void StopRecording()
    {
        isRecording = false;
        Point[] pointArray = new Point[positionsList.Count];
        for (int i = 0; i < positionsList.Count; i++)
        {
            // Use consistent world coordinates (XY plane) instead of screen coordinates
            // This matches the fix applied to MovementRecognizer.cs
            Vector2 worldPoint = new Vector2(positionsList[i].x, positionsList[i].y);
            pointArray[i] = new Point(worldPoint.x, worldPoint.y, 0);
        }
        string spellName = spellNames[currentSpellIndex];
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{spellName}_{timestamp}.xml";
        string localPath = Path.Combine(saveDirectory, fileName);
        GestureIO.WriteGesture(pointArray, spellName, localPath);
        gestureLineRenderer.positionCount = 0;
        UpdateUI($"Saved gesture for {spellName} locally!");
    }

    void TransferFiles()
    {
        try
        {
            // Copy all files from local directory to transfer directory
            string[] files = Directory.GetFiles(saveDirectory, "*.xml");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(transferPath, fileName);
                File.Copy(file, destFile, true);
            }
            
            UpdateUI($"Files transferred to: {transferPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error transferring files: {e.Message}");
            UpdateUI("Error transferring files");
        }
    }

    void CycleSpell()
    {
        currentSpellIndex = (currentSpellIndex + 1) % spellNames.Length;
        UpdateUI();
    }

    void UpdateUI(string message = null)
    {
        if (statusText != null)
        {
            statusText.text = message ?? (isRecording ? "Recording..." : "Ready to record");
        }
        
        if (currentSpellText != null)
        {
            currentSpellText.text = $"Current Spell: {spellNames[currentSpellIndex]}";
        }
    }
} 