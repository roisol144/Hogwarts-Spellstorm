using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using PDollarGestureRecognizer;
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

    public float recognitionThreshold = 0.9f;
    [SerializeField] private GestureDisplayManager displayManager;

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string> { }
    public UnityStringEvent OnRecognized;

    private List<Gesture> trainingSet = new List<Gesture>();
    private List<Vector3> positionsList = new List<Vector3>();
    public KeyCode debugKey = KeyCode.G;

    void Start()
    {
        trainingSet.Clear();
        // Load all gesture XMLs from Resources/Gestures
        TextAsset[] gestureFiles = Resources.LoadAll<TextAsset>("Gestures");
        foreach (var gestureFile in gestureFiles)
        {
            // Each file may have multiple strokes; load each as a separate Gesture
            var gestures = ReadAllStrokesFromXML(gestureFile.text);
            trainingSet.AddRange(gestures);
        }
        Debug.Log($"Loaded {trainingSet.Count} gestures from Resources/Gestures");
    }

    // Helper to parse all strokes in a gesture XML as separate Gesture objects
    private List<Gesture> ReadAllStrokesFromXML(string xml)
    {
        var gestures = new List<Gesture>();
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(xml);
        var gestureName = doc.DocumentElement.GetAttribute("Name");
        var strokeNodes = doc.DocumentElement.SelectNodes("Stroke");
        foreach (System.Xml.XmlNode strokeNode in strokeNodes)
        {
            var points = new List<Point>();
            foreach (System.Xml.XmlNode pointNode in strokeNode.ChildNodes)
            {
                float x = float.Parse(pointNode.Attributes["X"].Value, System.Globalization.CultureInfo.InvariantCulture);
                float y = float.Parse(pointNode.Attributes["Y"].Value, System.Globalization.CultureInfo.InvariantCulture);
                points.Add(new Point(x, y, 0));
            }
            gestures.Add(new Gesture(points.ToArray(), gestureName));
        }
        return gestures;
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

        Point[] pointArray = new Point[positionsList.Count];
        for(int i = 0; i < positionsList.Count; i++)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(positionsList[i]);
            pointArray[i] = new Point(screenPoint.x, screenPoint.y, 0);
        }

        Gesture newGesture = new Gesture(pointArray);
        
        if(isInTrainingMode && !string.IsNullOrEmpty(currentGestureName))
        {
            newGesture.Name = currentGestureName;
            trainingSet.Add(newGesture);

            string fileName = Application.persistentDataPath + "/" + currentGestureName + ".xml";
            GestureIO.WriteGesture(pointArray, currentGestureName, fileName);
            
            if (displayManager != null)
            {
                displayManager.ShowRecognitionResult($"Saved: {currentGestureName}", 1.0f);
            }
        }
        else
        {
            Result result = PointCloudRecognizer.Classify(newGesture, trainingSet.ToArray());
            Debug.Log("Result: " + result.GestureClass + " with score: " + result.Score);
            if(result.Score > recognitionThreshold)
            {
                OnRecognized.Invoke(result.GestureClass);
                if (displayManager != null)
                {
                    displayManager.ShowRecognitionResult(result.GestureClass, result.Score);
                }
            }
        }
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