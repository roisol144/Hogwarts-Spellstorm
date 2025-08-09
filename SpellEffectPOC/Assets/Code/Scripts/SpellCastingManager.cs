using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SpellCastingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementRecognizer movementRecognizer;
    [SerializeField] private WitAiAgent witAiAgent; // Or your voice handler script
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject stupefyPrefab; // Yellow lightning for Stupefy
    [SerializeField] private GameObject bombardoPrefab; // Red lightning for Bombardo
    [SerializeField] private GameObject expectoPatronumPrefab; // Blue lightning for Expecto Patronum
    [SerializeField] private GameObject protegoPrefab; // Blue shield for Protego
    [SerializeField] private GameObject accioPrefab; // Default to Stupefy for Accio (or create another)
    [SerializeField] private Transform wandTip;
    [SerializeField] private float matchWindowSeconds = 7f;

    [Header("Input")]
    [SerializeField] private InputAction triggerAction;
    [SerializeField] private InputAction gripAction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gripCastSound;
    
    [Header("Individual Spell Sounds")]
    [SerializeField] private AudioClip protegoCastSound;
    [SerializeField] private AudioClip stupefyCastSound;
    [SerializeField] private AudioClip bombardoCastSound;
    [SerializeField] private AudioClip expectoPatronumCastSound;
    [SerializeField] private AudioClip accioCastSound;

    [Header("Haptics")]
    [SerializeField] private XRBaseController rightHandController;
    [SerializeField, Range(0f, 1f)] private float weakHapticAmplitude = 0.25f;
    [SerializeField, Range(0f, 1f)] private float strongHapticAmplitude = 0.75f;
    [SerializeField, Min(0f)] private float hapticDuration = 0.1f;
    [SerializeField, Min(0.01f)] private float gripHapticInterval = 0.1f;
    [SerializeField, Range(0f, 1f)] private float gripHoldAmplitude = 0.10f;
    [SerializeField, Min(0f)] private float specialHapticDuration = 0.2f;
    [SerializeField, Min(0f)] private float gripSuppressAfterSpecialSeconds = 0.2f;
    private bool _isGripHeld = false;
    private Coroutine _gripHapticsRoutine = null;
    private float _gripSuppressUntilTime = 0f;

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
        { "cast_protego", "cast_protego" },
        { "cast_stupefy", "cast_stupefy" }
    };

    void Start()
    {
        Debug.Log($"[SpellCastingManager] Start called. movementRecognizer: {movementRecognizer}, witAiAgent: {witAiAgent}");
        Debug.Log($"[SpellCastingManager] References - fireballPrefab: {fireballPrefab}, stupefyPrefab: {stupefyPrefab}, bombardoPrefab: {bombardoPrefab}, expectoPatronumPrefab: {expectoPatronumPrefab}, protegoPrefab: {protegoPrefab}, accioPrefab: {accioPrefab}, wandTip: {wandTip}");
        
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
        
        if (protegoPrefab == null)
        {
            Debug.LogError("[SpellCastingManager] CRITICAL: Protego prefab is null on Start!");
            Debug.LogError("[SpellCastingManager] Please assign the Protego prefab from Assets/Spell Effects/Protego.prefab");
        }
        else
        {
            Debug.Log($"[SpellCastingManager] Protego prefab assigned successfully: {protegoPrefab.name}");
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

        // Try to auto-assign right-hand controller if not set
        if (rightHandController == null)
        {
            // Prefer legacy XRController with RightHand node
            var legacyControllers = FindObjectsOfType<XRController>(true);
            foreach (var ctrl in legacyControllers)
            {
                if (ctrl.controllerNode == XRNode.RightHand)
                {
                    rightHandController = ctrl;
                    Debug.Log("[SpellCastingManager] Auto-assigned rightHandController from XRController (RightHand)");
                    break;
                }
            }

            // Fallback: pick any XRBaseController with name hint
            if (rightHandController == null)
            {
                var controllers = FindObjectsOfType<XRBaseController>(true);
                foreach (var ctrl in controllers)
                {
                    var nameLower = ctrl.name.ToLowerInvariant();
                    if (nameLower.Contains("right"))
                    {
                        rightHandController = ctrl;
                        Debug.Log("[SpellCastingManager] Auto-assigned rightHandController by name hint: " + ctrl.name);
                        break;
                    }
                }
            }

            if (rightHandController == null)
            {
                Debug.LogWarning("[SpellCastingManager] rightHandController not assigned. Assign an XR controller in the inspector for haptics.");
            }
        }
        
        if (movementRecognizer != null)
            movementRecognizer.OnRecognized.AddListener(OnGestureRecognized);
        if (witAiAgent != null)
            witAiAgent.OnIntentRecognized += OnIntentRecognized;

        // Setup trigger input
        triggerAction.performed += OnTriggerPressed;

        // Setup grip input for continuous weak haptics while held
        if (gripAction != null)
        {
            // Provide common default bindings if none set
            if (gripAction.bindings.Count == 0)
            {
                gripAction.AddBinding("<XRController>{RightHand}/gripPressed");
                gripAction.AddBinding("<OculusTouchController>{RightHand}/gripPressed");
            }
            gripAction.started += OnGripStarted;
            gripAction.canceled += OnGripCanceled;
        }
    }

    void OnEnable()
    {
        triggerAction.Enable();
        if (gripAction != null) gripAction.Enable();
    }

    void OnDisable()
    {
        triggerAction.Disable();
        if (gripAction != null) gripAction.Disable();
        if (_gripHapticsRoutine != null)
        {
            StopCoroutine(_gripHapticsRoutine);
            _gripHapticsRoutine = null;
        }
        _isGripHeld = false;
    }

    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[SpellCastingManager] Trigger pressed, casting default fireball!");
        CastSpellEffect("default");
        TriggerHaptics(weakHapticAmplitude, hapticDuration);
    }

    private void OnGripStarted(InputAction.CallbackContext context)
    {
        _isGripHeld = true;
        if (_gripHapticsRoutine == null)
        {
            _gripHapticsRoutine = StartCoroutine(GripHapticsLoop());
        }
    }

    private void OnGripCanceled(InputAction.CallbackContext context)
    {
        _isGripHeld = false;
        if (_gripHapticsRoutine != null)
        {
            StopCoroutine(_gripHapticsRoutine);
            _gripHapticsRoutine = null;
        }
    }

    private IEnumerator GripHapticsLoop()
    {
        // Gentle continuous pulses while grip is held
        var wait = new WaitForSeconds(gripHapticInterval);
        while (_isGripHeld)
        {
            if (Time.time >= _gripSuppressUntilTime)
            {
                TriggerHaptics(gripHoldAmplitude, hapticDuration);
            }
            yield return wait;
        }
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
                PlayGripCastSound(lastRecognizedIntent);
                
                CastSpellEffect(lastRecognizedIntent);
                
                // Strong haptics for successful special spell
                TriggerHaptics(strongHapticAmplitude, specialHapticDuration);
                _gripSuppressUntilTime = Time.time + specialHapticDuration + gripSuppressAfterSpecialSeconds;
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

    private bool _hapticsWarnedOnce = false;
    private void TriggerHaptics(float amplitude, float duration)
    {
        if (rightHandController != null)
        {
            var clamped = Mathf.Clamp01(amplitude);
            try
            {
                HapticController.SendHaptics(rightHandController, clamped, duration);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SpellCastingManager] Failed to send haptics: {ex.Message}");
            }
        }
        else if (!_hapticsWarnedOnce)
        {
            _hapticsWarnedOnce = true;
            Debug.LogWarning("[SpellCastingManager] No right-hand XR controller assigned; haptics disabled.");
        }
    }

    private void PlayGripCastSound(string spellIntent = null)
    {
        AudioClip soundToPlay = gripCastSound; // Default sound
        
        // Check if we have a specific sound for this spell
        if (!string.IsNullOrEmpty(spellIntent))
        {
            switch (spellIntent)
            {
                case "cast_protego":
                    soundToPlay = protegoCastSound ?? gripCastSound;
                    break;
                case "cast_stupefy":
                    soundToPlay = stupefyCastSound ?? gripCastSound;
                    break;
                case "cast_bombardo":
                    soundToPlay = bombardoCastSound ?? gripCastSound;
                    break;
                case "cast_expecto_patronum":
                    soundToPlay = expectoPatronumCastSound ?? gripCastSound;
                    break;
                case "cast_accio":
                    soundToPlay = accioCastSound ?? gripCastSound;
                    break;
                default:
                    soundToPlay = gripCastSound;
                    break;
            }
        }
        
        if (audioSource != null && soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
            Debug.Log($"[SpellCastingManager] Playing cast sound for spell: {spellIntent ?? "default"}");
        }
        else
        {
            Debug.LogWarning($"[SpellCastingManager] Cannot play cast sound for {spellIntent ?? "default"} - missing AudioSource or AudioClip");
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
            case "cast_protego":
                prefabToSpawn = protegoPrefab;
                spellName = "Protego";
                
                // Create Protego shield around the enemy
                CreateProtegoShield();
                
                // Set prefab to null so it doesn't get spawned again at wand tip
                prefabToSpawn = null;
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
            Debug.Log($"[SpellCastingManager] No prefab to spawn for {spellName} (handled by special logic)");
            
            // Update the magical debug UI anyway
            // COMMENTED OUT: Regular spell casting debug text
            // MagicalDebugUI.NotifySpellCast(spellName);
            
            // Also update legacy text for backward compatibility
            if (spellCastText != null)
            {
                spellCastText.text = $"Spell Casted: {spellName}";
            }
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
            // COMMENTED OUT: Regular spell casting debug text
            // MagicalDebugUI.NotifySpellCast(spellName);
            
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
            case "cast_protego":
                return "Protego";
            default:
                return spellIntent.Replace("cast_", "").Replace("_", " ");
        }
    }

    // Keep the old FireFireball method for backward compatibility (in case it's called elsewhere)
    private void FireFireball()
    {
        CastSpellEffect("default");
    }
    
    // Create Protego shield around the player
    private void CreateProtegoShield()
    {
        // Find the closest enemy to target with the shield
        GameObject targetEnemy = FindClosestEnemy();
        
        if (targetEnemy == null)
        {
            Debug.LogWarning("[SpellCastingManager] No enemy found to target with Protego shield!");
            return;
        }
        
        // Check if there's already an active Protego shield on this enemy
        ProtegoShield existingShield = targetEnemy.GetComponent<ProtegoShield>();
        if (existingShield != null && existingShield.IsActive())
        {
            Debug.Log("[SpellCastingManager] Protego shield already active on enemy, refreshing duration...");
            // Destroy the existing shield to create a new one (refreshes duration)
            Destroy(existingShield);
        }
        
        // Spawn protego prefab at enemy location for visual effects
        GameObject protegoPrefabInstance = null;
        if (protegoPrefab != null && wandTip != null)
        {
            Vector3 spawnPos = targetEnemy.transform.position;
            protegoPrefabInstance = Instantiate(protegoPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"[SpellCastingManager] Protego prefab spawned at enemy location: {spawnPos}");
        }
        
        // Add ProtegoShield component to the target enemy
        ProtegoShield protegoShield = targetEnemy.AddComponent<ProtegoShield>();
        
        // Pass the prefab instance to the shield so it can destroy it when done
        if (protegoPrefabInstance != null)
        {
            protegoShield.SetProtegoPrefabInstance(protegoPrefabInstance);
        }

        // Apply per-enemy Protego configuration if available
        ProtegoConfig protegoConfig = targetEnemy.GetComponent<ProtegoConfig>();
        if (protegoConfig != null)
        {
            // Set logical radius for shield logic & gizmos
            protegoShield.SetShieldRadius(protegoConfig.shieldRadius);

            // Adjust particle systems' Start Size on the instantiated prefab, if present
            if (protegoPrefabInstance != null)
            {
                var particleSystems = protegoPrefabInstance.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var ps in particleSystems)
                {
                    // Skip any particle system that is on or under a GameObject named "Trails"
                    Transform t = ps.transform;
                    bool isUnderTrails = false;
                    while (t != null)
                    {
                        if (t.name == "Trails")
                        {
                            isUnderTrails = true;
                            break;
                        }
                        t = t.parent;
                    }
                    if (isUnderTrails) continue;

                    var main = ps.main;
                    // Only override if flagged to do so, otherwise leave prefab default
                    if (protegoConfig.overrideStartSize)
                    {
                        main.startSize = protegoConfig.startSize;
                    }
                }
            }
        }
        
        Debug.Log($"[SpellCastingManager] Protego shield created around enemy {targetEnemy.name} at {targetEnemy.transform.position}");
    }
    
    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            // Try alternative method - find by EnemyMovement component
            EnemyMovement[] enemyMovements = FindObjectsOfType<EnemyMovement>();
            if (enemyMovements.Length == 0) 
            {
                Debug.LogWarning("[SpellCastingManager] No enemies found!");
                return null;
            }
            
            // Convert to GameObject array
            enemies = new GameObject[enemyMovements.Length];
            for (int i = 0; i < enemyMovements.Length; i++)
            {
                enemies[i] = enemyMovements[i].gameObject;
            }
        }
        
        // Find closest enemy to the player
        Transform player = Camera.main?.transform;
        if (player == null) 
        {
            Debug.LogError("[SpellCastingManager] Player camera not found!");
            return enemies[0]; // Return first enemy as fallback
        }
        
        GameObject closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(player.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        Debug.Log($"[SpellCastingManager] Found closest enemy: {closestEnemy?.name} at distance {closestDistance}");
        return closestEnemy;
    }
} 