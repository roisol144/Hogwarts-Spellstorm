using UnityEngine;
using Unity.Sentis;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SentisGestureRecognizer : MonoBehaviour
{
    [Header("Model Settings")]
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private float confidenceThreshold = 0.3f; // Very low for testing VR gestures
    
    private Worker worker;
    private Model model;
    private bool isWarmedUp = false;
    
    public bool IsWarmedUp => isWarmedUp;
    
    // Gesture class names
    private readonly string[] gestureClasses = new string[] 
    {
        "cast_bombardo",    // Triangle
        "cast_protego",     // Circle
        "cast_stupefy",     // Zigzag
        "cast_expecto_patronum" // Square
    };

    void Start()
    {
        // Load the model
        if (modelAsset == null)
        {
            Debug.LogError("Model asset is not assigned!");
            return;
        }

        model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.CPU);
        
        // Check if MLWarmupManager has already started warmup
        if (MLWarmupManager.Instance != null && MLWarmupManager.Instance.IsWarmupCompleted)
        {
            Debug.Log("[SentisGestureRecognizer] Warmup already completed by MLWarmupManager");
            isWarmedUp = true;
        }
        else
        {
            // Fallback: Start warmup immediately if no MLWarmupManager
            // This ensures backward compatibility for scenes without scene transitions
            StartCoroutine(WarmUpModel());
        }
    }
    
    /// <summary>
    /// Public method to force start warmup (called by MLWarmupManager)
    /// </summary>
    public void ForceStartWarmup()
    {
        if (!isWarmedUp && worker != null)
        {
            Debug.Log("[SentisGestureRecognizer] Force starting warmup via MLWarmupManager");
            StartCoroutine(WarmUpModel());
        }
    }

    public string RecognizeGesture(List<Vector2> points)
    {
        if (worker == null)
        {
            Debug.LogError("Worker is not initialized!");
            return null;
        }
        
        if (!isWarmedUp)
        {
            Debug.LogWarning("Gesture recognition called before warm-up completed. This may result in slower performance.");
        }

        if (points.Count < 3)
        {
            Debug.LogWarning("Not enough points to recognize gesture");
            return null;
        }

        // Normalize points
        var normalizedPoints = NormalizePoints(points);
        
        // Convert 2D points to 28x28 image
        var imageData = PointsToImage(normalizedPoints, 28, 28);
        
        // Create tensor with proper shape (1, 28, 28, 1) for NHWC format
        var shape = new TensorShape(1, 28, 28, 1);
        using (var inputTensor = new Tensor<float>(shape, imageData))
        {
            // Run inference
            worker.Schedule(inputTensor);
            
            // Get output tensor
            var outputTensor = worker.PeekOutput() as Tensor<float>;
            
            // Download results to array
            var results = outputTensor.DownloadToArray();
            
            // Find the class with highest confidence
            var maxIndex = 0;
            var maxConfidence = results[0];
            
            for (int i = 1; i < results.Length; i++)
            {
                if (results[i] > maxConfidence)
                {
                    maxConfidence = results[i];
                    maxIndex = i;
                }
            }

            // Check if confidence is high enough
            if (maxConfidence >= confidenceThreshold)
            {
                Debug.Log($"Recognized gesture: {gestureClasses[maxIndex]} with confidence: {maxConfidence}");
                return gestureClasses[maxIndex];
            }
            else
            {
                Debug.Log($"Gesture confidence too low: {maxConfidence}");
            }
        }

        return null;
    }

    private float[] PointsToImage(List<Vector2> points, int width, int height)
    {
        var image = new float[width * height];
        
        // Points are already in pixel coordinates (0-28 range) from NormalizePoints
        // Draw lines between consecutive points
        for (int i = 1; i < points.Count; i++)
        {
            DrawLine(image, points[i-1], points[i], width, height);
        }
        
        return image;
    }

    private void DrawLine(float[] image, Vector2 p1, Vector2 p2, int width, int height)
    {
        // Points are already in pixel coordinates, just round and clamp
        int x1 = Mathf.RoundToInt(p1.x);
        int y1 = Mathf.RoundToInt(p1.y);
        int x2 = Mathf.RoundToInt(p2.x);
        int y2 = Mathf.RoundToInt(p2.y);
        
        // Clamp to image bounds
        x1 = Mathf.Clamp(x1, 0, width - 1);
        y1 = Mathf.Clamp(y1, 0, height - 1);
        x2 = Mathf.Clamp(x2, 0, width - 1);
        y2 = Mathf.Clamp(y2, 0, height - 1);
        
        // Bresenham's line algorithm
        int dx = Mathf.Abs(x2 - x1);
        int dy = Mathf.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;
        
        int x = x1, y = y1;
        
        while (true)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                image[y * width + x] = 1.0f; // White pixel
            }
            
            if (x == x2 && y == y2) break;
            
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

    private List<Vector2> NormalizePoints(List<Vector2> points)
    {
        if (points.Count < 2) return points;

        // Step 1: Get bounding box of raw points
        var bounds = GetBoundingBox(points);
        
        // Step 2: Center at origin (translate so center is at 0,0)
        var centeredPoints = new List<Vector2>();
        foreach (var point in points)
        {
            centeredPoints.Add(point - bounds.center);
        }
        
        // Step 3: Scale uniformly so largest dimension fits 27 pixels (preserving aspect ratio)
        // This leaves a 1-pixel border in the 28x28 image, matching Quick, Draw! preprocessing
        float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);
        float scale = 27.0f / Mathf.Max(bounds.size.x, bounds.size.y); // Scale to fit 27 pixels max
        
        var scaledPoints = new List<Vector2>();
        foreach (var point in centeredPoints)
        {
            scaledPoints.Add(point * scale);
        }
        
        // Step 4: Shift to center of 28x28 image (move from origin to pixel 14,14)
        var finalPoints = new List<Vector2>();
        foreach (var point in scaledPoints)
        {
            finalPoints.Add(point + new Vector2(14f, 14f));
        }
        
        return finalPoints;
    }

    private Rect GetBoundingBox(List<Vector2> points)
    {
        var minX = points.Min(p => p.x);
        var maxX = points.Max(p => p.x);
        var minY = points.Min(p => p.y);
        var maxY = points.Max(p => p.y);
        
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private IEnumerator WarmUpModel()
    {
        Debug.Log("[SentisGestureRecognizer] Starting ML model warm-up...");
        
        // Wait a frame to ensure everything is initialized
        yield return null;
        
        if (worker == null)
        {
            Debug.LogError("[SentisGestureRecognizer] Worker not initialized for warm-up!");
            yield break;
        }
        
        // Create dummy gesture data for warm-up - simulate simple shapes
        List<Vector2> dummyGestures = new List<Vector2>
        {
            // Dummy gesture 1: Simple triangle
            new Vector2(0f, 0f),
            new Vector2(5f, 0f),
            new Vector2(2.5f, 4.33f),
            new Vector2(0f, 0f)
        };
        
        // Perform 2 dummy inference calls to warm up the model
        for (int i = 0; i < 2; i++)
        {
            Debug.Log($"[SentisGestureRecognizer] Warm-up inference {i + 1}/2...");
            
            try
            {
                // Create slightly different dummy data for each warm-up call
                var warmupPoints = new List<Vector2>(dummyGestures);
                
                // Add some variation to each warm-up call
                for (int j = 0; j < warmupPoints.Count; j++)
                {
                    warmupPoints[j] += new Vector2(i * 0.1f, i * 0.1f);
                }
                
                // Normalize points
                var normalizedPoints = NormalizePoints(warmupPoints);
                
                // Convert to image data
                var imageData = PointsToImage(normalizedPoints, 28, 28);
                
                // Create tensor and run inference
                var shape = new TensorShape(1, 28, 28, 1);
                using (var inputTensor = new Tensor<float>(shape, imageData))
                {
                    worker.Schedule(inputTensor);
                    var outputTensor = worker.PeekOutput() as Tensor<float>;
                    var results = outputTensor.DownloadToArray();
                }
                
                Debug.Log($"[SentisGestureRecognizer] Warm-up inference {i + 1} completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SentisGestureRecognizer] Warm-up inference {i + 1} failed: {e.Message}");
            }
            
            // Small delay between warm-up calls (outside try-catch)
            yield return new WaitForSeconds(0.1f);
        }
        
        isWarmedUp = true;
        Debug.Log("[SentisGestureRecognizer] ML model warm-up completed! System ready for gesture recognition.");
    }

    void OnDestroy()
    {
        worker?.Dispose();
    }
} 