using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using TMPro;

public class SpellCastingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementRecognizer movementRecognizer;
    [SerializeField] private WitAiAgent witAiAgent; // Or your voice handler script
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject stupefyPrefab; // Yellow lightning for Stupefy
    [SerializeField] private GameObject bombardoPrefab; // Red lightning for Bombardo
    [SerializeField] private GameObject expectoPatronumPrefab; // Blue lightning for Expecto Patronum
    [SerializeField] private GameObject accioPrefab; // Default to Stupefy for Accio (or create another)
    [SerializeField] private Transform wandTip;
    [SerializeField] private float matchWindowSeconds = 7f;

    [Header("Input")]
    [SerializeField] private InputAction triggerAction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gripCastSound;

    [Header("UI - Legacy (now handled by MagicalDebugUI)")]
    [SerializeField] private TextMeshProUGUI spellCastText; // Keep for backward compatibility but use MagicalDebugUI instead

    private string lastRecognizedGesture = null;
    private float lastGestureTime = -10f;
    private string lastRecognizedIntent = null;
    private float lastIntentTime = -10f;

    private Dictionary<string, string> intentToGesture = new Dictionary<string, string>
    {
        { "cast_accio", "cast_accio" },
        { "cast_bombardo", "cast_bombardo" },
        { "cast_expecto_patronum", "cast_expecto_patronum" },
        { "cast_stupefy", "cast_stupefy" }
    };

    void Start()
    {
        Debug.Log($"[SpellCastingManager] Start called. movementRecognizer: {movementRecognizer}, witAiAgent: {witAiAgent}");
        Debug.Log($"[SpellCastingManager] References - fireballPrefab: {fireballPrefab}, stupefyPrefab: {stupefyPrefab}, bombardoPrefab: {bombardoPrefab}, expectoPatronumPrefab: {expectoPatronumPrefab}, accioPrefab: {accioPrefab}, wandTip: {wandTip}");
        
        // Validate prefab references
        if (fireballPrefab == null)
        {
            Debug.LogError("[SpellCastingManager] CRITICAL: Fireball prefab is null on Start!");
            Debug.LogError("[SpellCastingManager] Please assign the FireBall prefab from Assets/Spell Effects/FireBall.prefab");
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Fireball prefab assigned successfully: {fireballPrefab.name}");
        }
        
        if (stupefyPrefab == null)
        {
            Debug.LogError("[SpellCastingManager] CRITICAL: Stupefy prefab is null on Start!");
            Debug.LogError("[SpellCastingManager] Please assign the Stupefy prefab from Assets/Spell Effects/Stupefy.prefab");
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Stupefy prefab assigned successfully: {stupefyPrefab.name}");
        }
        
        if (bombardoPrefab == null)
        {
            Debug.LogError("[SpellCastingManager] CRITICAL: Bombardo prefab is null on Start!");
            Debug.LogError("[SpellCastingManager] Please assign the Bombardo prefab from Assets/Spell Effects/Bombardo.prefab");
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Bombardo prefab assigned successfully: {bombardoPrefab.name}");
        }
        
        if (expectoPatronumPrefab == null)
        {
            Debug.LogError("[SpellCastingManager] CRITICAL: Expecto Patronum prefab is null on Start!");
            Debug.LogError("[SpellCastingManager] Please assign the Expecto Patronum prefab from Assets/Spell Effects/ExpectoPatronum.prefab");
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Expecto Patronum prefab assigned successfully: {expectoPatronumPrefab.name}");
        }
        
        if (accioPrefab == null)
        {
            Debug.LogWarning("[SpellCastingManager] Accio prefab is not assigned. Defaulting to Stupefy.");
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Accio prefab assigned successfully: {accioPrefab.name}");
        }

        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[SpellCastingManager] Added AudioSource component");
            }
        }

        // Load grip cast sound if not assigned
        if (gripCastSound == null)
        {
            gripCastSound = Resources.Load<AudioClip>("Sounds/magic_spell");
            if (gripCastSound != null)
            {
                Debug.Log("[SpellCastingManager] Loaded grip cast sound from Resources");
            }
            else
            {
                Debug.LogWarning("[SpellCastingManager] Could not load grip cast sound from Resources/Sounds/magic_spell");
            }
        }
        
        if (movementRecognizer != null)
            movementRecognizer.OnRecognized.AddListener(OnGestureRecognized);
        if (witAiAgent != null)
            witAiAgent.OnIntentRecognized += OnIntentRecognized;

        // Setup trigger input
        triggerAction.performed += OnTriggerPressed;
    }

    void OnEnable()
    {
        triggerAction.Enable();
    }

    void OnDisable()
    {
        triggerAction.Disable();
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[SpellCastingManager] Trigger pressed, casting default fireball!");
        CastSpellEffect("default");
    }

    private void OnGestureRecognized(string gestureName)
    {
        Debug.Log($"[SpellCastingManager] Gesture recognized: {gestureName} at {Time.time}");
        lastRecognizedGesture = gestureName;
        lastGestureTime = Time.time;
        TryCastSpell();
    }

    private void OnIntentRecognized(string intentName)
    {
        Debug.Log($"[SpellCastingManager] Intent recognized: {intentName} at {Time.time}");
        lastRecognizedIntent = intentName;
        lastIntentTime = Time.time;
        TryCastSpell();
    }

    private void TryCastSpell()
    {
        Debug.Log($"[SpellCastingManager] TryCastSpell called. Gesture: {lastRecognizedGesture} (t={lastGestureTime}), Intent: {lastRecognizedIntent} (t={lastIntentTime})");
        if (lastRecognizedGesture == null || lastRecognizedIntent == null)
        {
            Debug.Log("[SpellCastingManager] Missing gesture or intent, cannot cast.");
            return;
        }
        if (Time.time - lastGestureTime > matchWindowSeconds || Time.time - lastIntentTime > matchWindowSeconds)
        {
            Debug.Log("[SpellCastingManager] Time window expired, cannot cast.");
            return;
        }
        if (intentToGesture.TryGetValue(lastRecognizedIntent, out string requiredGesture))
        {
            if (requiredGesture == lastRecognizedGesture)
            {
                Debug.Log($"[SpellCastingManager] Match found! Casting spell effect for intent '{lastRecognizedIntent}' and gesture '{lastRecognizedGesture}'.");
                
                // Play grip cast sound effect
                PlayGripCastSound();
                
                CastSpellEffect(lastRecognizedIntent);
                // Reset so it doesn't double-fire
                lastRecognizedGesture = null;
                lastRecognizedIntent = null;
            }
            else
            {
                Debug.Log($"[SpellCastingManager] Gesture '{lastRecognizedGesture}' does not match required '{requiredGesture}' for intent '{lastRecognizedIntent}'.");
            }
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Intent '{lastRecognizedIntent}' not found in mapping.");
        }
    }

    private void PlayGripCastSound()
    {
        if (audioSource != null && gripCastSound != null)
        {
            audioSource.PlayOneShot(gripCastSound);
            Debug.Log("[SpellCastingManager] Playing grip cast sound effect");
        }
        else
        {
            Debug.LogWarning("[SpellCastingManager] Cannot play grip cast sound - missing AudioSource or AudioClip");
        }
    }

    private void CastSpellEffect(string spellIntent)
    {
        GameObject prefabToSpawn;
        string spellName;
        
        // Determine which prefab to use based on the specific spell
        switch (spellIntent)
        {
            case "default":
                prefabToSpawn = fireballPrefab;
                spellName = "Fireball";
                break;
            case "cast_stupefy":
                prefabToSpawn = stupefyPrefab;
                spellName = "Stupefy";
                break;
            case "cast_bombardo":
                prefabToSpawn = bombardoPrefab;
                spellName = "Bombardo";
                break;
            case "cast_expecto_patronum":
                prefabToSpawn = expectoPatronumPrefab;
                spellName = "Expecto Patronum";
                break;
            case "cast_accio":
                prefabToSpawn = accioPrefab ?? stupefyPrefab; // Fallback to Stupefy if Accio prefab not set
                spellName = "Accio";
                break;
            default:
                // Fallback to fireball for unknown spells
                prefabToSpawn = fireballPrefab;
                spellName = "Unknown Spell (Fireball)";
                break;
        }
        
        Debug.Log($"[SpellCastingManager] CastSpellEffect called for '{spellIntent}' using {spellName}");
        
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[SpellCastingManager] Prefab for {spellName} is null!");
            return;
        }
        
        if (wandTip == null)
        {
            Debug.LogError("[SpellCastingManager] Wand tip is null!");
            return;
        }
        
        Vector3 spawnPos = wandTip.position + wandTip.forward * 0.5f;
        Debug.Log($"[SpellCastingManager] Spawning {spellName} at position: {spawnPos}, rotation: {wandTip.rotation}");
        
        GameObject spawnedEffect = Instantiate(prefabToSpawn, spawnPos, wandTip.rotation);
        
        if (spawnedEffect != null)
        {
            // Add SpellCasted component to track which spell was cast
            SpellCasted spellCasted = spawnedEffect.AddComponent<SpellCasted>();
            spellCasted.Initialize(spellName, spellIntent);
            
            // Update the magical debug UI with the spell name (new system)
            MagicalDebugUI.NotifySpellCast(spellName);
            
            // Also update legacy text for backward compatibility
            if (spellCastText != null)
            {
                spellCastText.text = $"Spell Casted: {spellName}";
            }
            
            // Ensure the effect is active
            if (!spawnedEffect.activeInHierarchy)
            {
                spawnedEffect.SetActive(true);
                Debug.Log($"[SpellCastingManager] Activated {spellName} GameObject");
            }
            
            Debug.Log($"[SpellCastingManager] {spellName} instantiated successfully! Name: {spawnedEffect.name}, Active: {spawnedEffect.activeInHierarchy}");
            Debug.Log($"[SpellCastingManager] {spellName} position: {spawnedEffect.transform.position}, scale: {spawnedEffect.transform.localScale}");
            
            // Get all particle systems in the spawned effect (including child objects)
            ParticleSystem[] particleSystems = spawnedEffect.GetComponentsInChildren<ParticleSystem>();
            Debug.Log($"[SpellCastingManager] Found {particleSystems.Length} particle system(s) in {spellName}");
            
            // Debug each particle system's state
            for (int i = 0; i < particleSystems.Length; i++)
            {
                Debug.Log($"[SpellCastingManager] Particle System {i}: {particleSystems[i].name}, Playing: {particleSystems[i].isPlaying}, Emission: {particleSystems[i].emission.enabled}");
                
                // Force play the particle system if it's not playing
                if (!particleSystems[i].isPlaying)
                {
                    particleSystems[i].Play();
                    Debug.Log($"[SpellCastingManager] Started particle system: {particleSystems[i].name}");
                }
            }
        }
        else
        {
            Debug.LogError($"[SpellCastingManager] {spellName} failed to instantiate!");
        }
        
        Debug.Log($"[SpellCastingManager] {spellName} cast complete!");
    }
    
    private string GetFriendlySpellName(string spellIntent)
    {
        switch (spellIntent)
        {
            case "cast_stupefy":
                return "Stupefy";
            case "cast_accio":
                return "Accio";
            case "cast_bombardo":
                return "Bombardo";
            case "cast_expecto_patronum":
                return "Expecto Patronum";
            default:
                return spellIntent.Replace("cast_", "").Replace("_", " ");
        }
    }

    // Keep the old FireFireball method for backward compatibility (in case it's called elsewhere)
    private void FireFireball()
    {
        CastSpellEffect("default");
    }
} 