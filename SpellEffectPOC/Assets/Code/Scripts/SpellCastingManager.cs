using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class SpellCastingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MovementRecognizer movementRecognizer;
    [SerializeField] private WitAiAgent witAiAgent; // Or your voice handler script
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform wandTip;
    [SerializeField] private float matchWindowSeconds = 2f;

    [Header("Input")]
    [SerializeField] private InputAction triggerAction;

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
        Debug.Log("[SpellCastingManager] Trigger pressed, casting fireball!");
        FireFireball();
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
                Debug.Log($"[SpellCastingManager] Match found! Firing fireball for intent '{lastRecognizedIntent}' and gesture '{lastRecognizedGesture}'.");
                FireFireball();
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

    private void FireFireball()
    {
        if (fireballPrefab != null && wandTip != null)
        {
            Vector3 spawnPos = wandTip.position + wandTip.forward * 0.5f;
            Instantiate(fireballPrefab, spawnPos, wandTip.rotation);
            Debug.Log("[SpellCastingManager] Fireball cast!");
        }
        else
        {
            Debug.LogError("[SpellCastingManager] Fireball prefab or wand tip not assigned!");
        }
    }
} 