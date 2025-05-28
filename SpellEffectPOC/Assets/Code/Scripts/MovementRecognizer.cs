using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.IO;
using UnityEngine.Events;
using System.Linq;

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

    private const int GRID_SIZE = 28; // Match the model's expected input size
    private const int NUM_POINTS = 28; // Number of points to resample to

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

    private List<Vector2> NormalizeAndResamplePoints(List<Vector2> points)
    {
        if (points.Count < 2) return points;

        // Find bounding box
        float minX = points.Min(p => p.x);
        float maxX = points.Max(p => p.x);
        float minY = points.Min(p => p.y);
        float maxY = points.Max(p => p.y);

        // Calculate scale to fit in 28x28 grid while maintaining aspect ratio
        float width = maxX - minX;
        float height = maxY - minY;
        float scale = Mathf.Min(GRID_SIZE / width, GRID_SIZE / height) * 0.8f; // 0.8 to add some padding

        // Normalize points to 0-1 range
        List<Vector2> normalizedPoints = points.Select(p => new Vector2(
            (p.x - minX) * scale,
            (p.y - minY) * scale
        )).ToList();

        // Resample to fixed number of points
        List<Vector2> resampledPoints = new List<Vector2>();
        float totalLength = 0;
        for (int i = 1; i < normalizedPoints.Count; i++)
        {
            totalLength += Vector2.Distance(normalizedPoints[i], normalizedPoints[i - 1]);
        }

        float segmentLength = totalLength / (NUM_POINTS - 1);
        float currentLength = 0;
        resampledPoints.Add(normalizedPoints[0]);

        for (int i = 1; i < normalizedPoints.Count; i++)
        {
            float segmentDistance = Vector2.Distance(normalizedPoints[i], normalizedPoints[i - 1]);
            while (currentLength + segmentDistance >= segmentLength)
            {
                float t = (segmentLength - currentLength) / segmentDistance;
                Vector2 newPoint = Vector2.Lerp(normalizedPoints[i - 1], normalizedPoints[i], t);
                resampledPoints.Add(newPoint);
                currentLength = 0;
                segmentDistance -= (segmentLength - currentLength);
            }
            currentLength += segmentDistance;
        }

        // Ensure we have exactly NUM_POINTS
        while (resampledPoints.Count < NUM_POINTS)
        {
            resampledPoints.Add(resampledPoints[resampledPoints.Count - 1]);
        }

        return resampledPoints;
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

        // Normalize and resample points
        var processedPoints = NormalizeAndResamplePoints(points);

        if (isInTrainingMode && !string.IsNullOrEmpty(currentGestureName))
        {
            // Save gesture for training
            string fileName = Application.persistentDataPath + "/" + currentGestureName + ".xml";
            SaveGestureToXML(processedPoints, currentGestureName, fileName);
            
            if (displayManager != null)
            {
                displayManager.ShowRecognitionResult($"Saved: {currentGestureName}", 1.0f);
            }
        }
        else
        {
            // Use Sentis for recognition
            string recognizedGesture = sentisRecognizer.RecognizeGesture(processedPoints);
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