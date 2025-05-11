using UnityEngine;

public class MagicTrailController : MonoBehaviour
{
    [Header("Trail Renderer")]
    [SerializeField] private TrailRenderer trailRenderer; // Assign your Trail Renderer in the Inspector

    void Start()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    // Call this from a UnityEvent (e.g., On Press)
    public void EnableTrail()
    {
        if (trailRenderer != null && !trailRenderer.emitting)
        {
            trailRenderer.Clear(); // Optional: clear old trail
            trailRenderer.emitting = true;
            Debug.Log("MagicTrailController: EnableTrail called.");
        }
    }

    // Call this from a UnityEvent (e.g., On Release)
    public void DisableTrail()
    {
        if (trailRenderer != null && trailRenderer.emitting)
        {
            trailRenderer.emitting = false;
            Debug.Log("MagicTrailController: DisableTrail called.");
        }
    }
} 