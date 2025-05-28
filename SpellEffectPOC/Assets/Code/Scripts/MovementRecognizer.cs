using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using UnityEngine.Events;

public class MovementRecognizer : MonoBehaviour
{
    public XRNode inputSource;
    public InputHelpers.Button inputButton;
    public Transform movementSource;
    public float inputThreshold = 0.1f;
    private bool isMoving = false;
    
    public float newPositionThresholdDistance = 0.05f;
    public GameObject debugCubePrefab;
    private bool isInTrainingMode = false;
    private string currentGestureName = "";

    [SerializeField] private SentisGestureRecognizer sentisRecognizer;
    [SerializeField] private GestureDisplayManager displayManager;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecognized;

    private List<Vector3> positionsList = new List<Vector3>();
    public KeyCode debugKey = KeyCode.G;

    void Start()
    {
        if (sentisRecognizer == null)
        {
            Debug.LogError("SentisGestureRecognizer reference is missing!");
        }
    }

    public void SetTrainingMode(bool enable, string gestureName)
    {
        isInTrainingMode = enable;
        currentGestureName = gestureName;
        Debug.Log($"[MovementRecognizer] SetTrainingMode: {enable}, Gesture: {gestureName}");
        if (displayManager != null)
        {
            displayManager.ShowRecognitionResult(
                enable ? $"Training Mode: {gestureName}" : "Recognition Mode",
                1.0f
            );
        }
    }

    public void SetGestureName(string newName)
    {
        currentGestureName = newName;
        Debug.Log($"[MovementRecognizer] SetGestureName: {newName}");
        if (displayManager != null)
        {
            displayManager.ShowRecognitionResult($"Selected: {newName}", 1.0f);
        }
    }

    void Update()
    {
        bool isPressed = false;
        InputHelpers.IsPressed(InputDevices.GetDeviceAtXRNode(inputSource), inputButton, out bool xrPressed, inputThreshold);
        bool keyPressed = Input.GetKey(debugKey);
        isPressed = xrPressed || keyPressed;

        if(!isMoving && isPressed)
        {
            StartMovement();
            Debug.Log("Start Movement");
        }
        else if(isMoving && !isPressed)
        {
            EndMovement();
            Debug.Log("End Movement");
        }
        else if(isMoving && isPressed)
        {
            UpdateMovement();
        }
    }

    void StartMovement()
    {
        isMoving = true;
        positionsList.Clear();
        positionsList.Add(movementSource.position);
    
        if(debugCubePrefab != null)
        {
            Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
        }
    }

    void EndMovement()
    {
        isMoving = false;

        // Convert positions to 2D points
        var points = new List<Vector2>();
        foreach (var pos in positionsList)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(pos);
            points.Add(screenPoint);
        }

        if (isInTrainingMode && !string.IsNullOrEmpty(currentGestureName))
        {
            // Save gesture for training
            string fileName = Application.persistentDataPath + "/" + currentGestureName + ".xml";
            SaveGestureToXML(points, currentGestureName, fileName);
            
            if (displayManager != null)
            {
                displayManager.ShowRecognitionResult($"Saved: {currentGestureName}", 1.0f);
            }
        }
        else
        {
            // Use Sentis for recognition
            string recognizedGesture = sentisRecognizer.RecognizeGesture(points);
            if (!string.IsNullOrEmpty(recognizedGesture))
            {
                OnRecognized.Invoke(recognizedGesture);
                if (displayManager != null)
                {
                    displayManager.ShowRecognitionResult(recognizedGesture, 1.0f);
                }
            }
        }
    }

    private void SaveGestureToXML(List<Vector2> points, string gestureName, string fileName)
    {
        var doc = new System.Xml.XmlDocument();
        var root = doc.CreateElement("Gesture");
        root.SetAttribute("Name", gestureName);
        doc.AppendChild(root);

        var stroke = doc.CreateElement("Stroke");
        root.AppendChild(stroke);

        foreach (var point in points)
        {
            var pointNode = doc.CreateElement("Point");
            pointNode.SetAttribute("X", point.x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            pointNode.SetAttribute("Y", point.y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            stroke.AppendChild(pointNode);
        }

        doc.Save(fileName);
    }

    void UpdateMovement()
    {
        Vector3 lastPosition = positionsList[positionsList.Count - 1];

        if(Vector3.Distance(movementSource.position, lastPosition) > newPositionThresholdDistance)
        {
            positionsList.Add(movementSource.position);
            if(debugCubePrefab != null)
            {
                Destroy(Instantiate(debugCubePrefab, movementSource.position, Quaternion.identity), 3);
            }
        }
    }
}