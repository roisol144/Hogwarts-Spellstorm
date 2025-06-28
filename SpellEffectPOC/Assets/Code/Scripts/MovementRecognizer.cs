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
        float scale = Mathf.Min(28 / width, 28 / height) * 0.8f; // 0.8 to add some padding

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

        float segmentLength = totalLength / (28 - 1);
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

        // Ensure we have exactly 28 points
        while (resampledPoints.Count < 28)
        {
            resampledPoints.Add(resampledPoints[resampledPoints.Count - 1]);
        }

        return resampledPoints;
    }

    void EndMovement()
    {
        isMoving = false;

        // Convert VR world positions to consistent 2D plane (XY plane for front-facing gestures)
        // This avoids screen coordinate variations between different VR headsets/FOV
        var points = new List<Vector2>();
        foreach (var pos in positionsList)
        {
            // Project to XY plane (front-facing gestures) instead of XZ plane (top-down)
            Vector2 planePoint = new Vector2(pos.x, pos.y);
            points.Add(planePoint);
        }

        // DEBUG: Log the raw 2D points to see the shape
        Debug.Log($"[MovementRecognizer] Raw 2D points count: {points.Count}");
        for (int i = 0; i < Mathf.Min(points.Count, 10); i++)
        {
            Debug.Log($"[MovementRecognizer] Point {i}: {points[i]}");
        }

        if (isInTrainingMode && !string.IsNullOrEmpty(currentGestureName))
        {
            // For training, keep normalization and resampling for XML
            var processedPoints = NormalizeAndResamplePoints(points);
            string fileName = Application.persistentDataPath + "/" + currentGestureName + ".xml";
            SaveGestureToXML(processedPoints, currentGestureName, fileName);
            if (displayManager != null)
            {
                displayManager.ShowRecognitionResult($"Saved: {currentGestureName}", 1.0f);
            }
        }
        else
        {
            // For recognition, pass raw 2D points to Sentis (let it handle normalization)
            string recognizedGesture = sentisRecognizer.RecognizeGesture(points);
            if (!string.IsNullOrEmpty(recognizedGesture))
            {
                OnRecognized.Invoke(recognizedGesture);
                if (displayManager != null)
                {
                    displayManager.ShowRecognitionResult(recognizedGesture, 1.0f);
                }
                // --- DEBUG: Automatically save image for cast_bombardo gesture ---
                // REMOVE THIS BLOCK AFTER TESTING
                if (recognizedGesture == "cast_bombardo")
                {
                    SaveGestureImage(points, 28, 28, "bombardo_debug.png");
                    Debug.Log("[DEBUG] Saved bombardo_debug.png to persistentDataPath. Remove this code after testing.");
                }
                // --- END DEBUG BLOCK ---
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

    // --- DEBUG: Utility to save gesture as PNG. REMOVE AFTER TESTING ---
    private void SaveGestureImage(List<Vector2> points, int width, int height, string filename)
    {
        // Normalize and scale points to fit in image
        float minX = points.Min(p => p.x);
        float maxX = points.Max(p => p.x);
        float minY = points.Min(p => p.y);
        float maxY = points.Max(p => p.y);
        float scale = Mathf.Min((width - 2) / (maxX - minX), (height - 2) / (maxY - minY));
        var normPoints = points.Select(p => new Vector2(
            (p.x - minX) * scale + 1,
            (p.y - minY) * scale + 1
        )).ToList();

        // Flip Y to match image convention (Y=0 at top)
        //for (int i = 0; i < normPoints.Count; i++)
        //{
        //    normPoints[i] = new Vector2(normPoints[i].x, height - 1 - normPoints[i].y);
        //}

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        Color32 black = new Color32(0, 0, 0, 255);
        Color32 white = new Color32(255, 255, 255, 255);
        Color32[] pixels = Enumerable.Repeat(black, width * height).ToArray();

        // Draw lines between points
        for (int i = 1; i < normPoints.Count; i++)
        {
            DrawLineOnArray((int)normPoints[i - 1].x, (int)normPoints[i - 1].y, (int)normPoints[i].x, (int)normPoints[i].y, width, height, pixels, white);
        }
        tex.SetPixels32(pixels);
        tex.Apply();

        // Save to persistent data path
        byte[] png = tex.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, png);
        Debug.Log("[DEBUG] Saved gesture image to: " + path);
    }

    // Bresenham's line algorithm for pixel array
    // --- DEBUG: Utility for image saving. REMOVE AFTER TESTING ---
    private void DrawLineOnArray(int x0, int y0, int x1, int y1, int width, int height, Color32[] pixels, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;
        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                pixels[y0 * width + x0] = color;
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
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