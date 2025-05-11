using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MagicSparkleController : MonoBehaviour
{
    [Header("Magic Sparkles")]
    [SerializeField] private ParticleSystem magicSparkles; // Assign your Particle System in the Inspector

    private bool isSparkling = false;

    // Call this from a UnityEvent (e.g., On Press)
    public void PlaySparkles()
    {
        if (magicSparkles != null && !isSparkling)
        {
            magicSparkles.Play();
            isSparkling = true;
            Debug.Log("MagicSparkleController: PlaySparkles called.");
        }
    }

    // Call this from a UnityEvent (e.g., On Release)
    public void StopSparkles()
    {
        if (magicSparkles != null && isSparkling)
        {
            magicSparkles.Stop();
            isSparkling = false;
            Debug.Log("MagicSparkleController: StopSparkles called.");
        }
    }
} 