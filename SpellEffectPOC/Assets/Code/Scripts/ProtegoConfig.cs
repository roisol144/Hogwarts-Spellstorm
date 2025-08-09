using UnityEngine;

/// <summary>
/// Per-enemy configuration for Protego effects (radius and visual sizing)
/// Attach this to any enemy GameObject to override default Protego settings
/// </summary>
public class ProtegoConfig : MonoBehaviour
{
    [Header("Protego Settings")]
    [Tooltip("Shield radius for this enemy when Protego is applied")]
    public float shieldRadius = 5f;

    [Header("Particle Start Size Override")]
    [Tooltip("If enabled, all ParticleSystems in the Protego prefab instance will use the specified Start Size for this enemy")]
    public bool overrideStartSize = true;

    [Tooltip("Particle System 'Start Size' value to apply to the Protego prefab instance for this enemy")]
    public float startSize = 5f;
}

