using UnityEngine;
using UnityEngine.XR;

public class GripTrailController : MonoBehaviour
{
    [Header("Trail and Sparkles")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem magicSparkles;
    
    private bool isTrailActive = false;
    private bool isSparklesActive = false;

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool gripValue;
        if (device.TryGetFeatureValue(CommonUsages.gripButton, out gripValue) && gripValue)
        {
            // Enable trail
            if (trailRenderer != null && !isTrailActive)
            {
                trailRenderer.emitting = true;
                isTrailActive = true;
                Debug.Log("GripTrailController: Trail enabled");
            }
            
            // Enable sparkles
            if (magicSparkles != null && !isSparklesActive)
            {
                magicSparkles.Play();
                isSparklesActive = true;
                Debug.Log("GripTrailController: Sparkles enabled");
            }
        }
        else
        {
            // Disable trail
            if (trailRenderer != null && isTrailActive)
            {
                trailRenderer.emitting = false;
                isTrailActive = false;
                Debug.Log("GripTrailController: Trail disabled");
            }
            
            // Disable sparkles
            if (magicSparkles != null && isSparklesActive)
            {
                magicSparkles.Stop();
                isSparklesActive = false;
                Debug.Log("GripTrailController: Sparkles disabled");
            }
        }
    }
}
