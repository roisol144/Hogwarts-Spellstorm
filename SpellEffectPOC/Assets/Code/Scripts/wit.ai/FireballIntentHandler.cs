using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.Data.Intents;
using Meta.WitAi.CallbackHandlers;
using System.Collections.Generic;

public class FireballIntentHandler : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform wandTip;
    private const float CONFIDENCE_THRESHOLD = 0.85f;

    // List of valid spell intents that should trigger the fireball
    private readonly HashSet<string> validSpellIntents = new HashSet<string>
    {
        "cast_accio",
        "cast_bombardo",
        "cast_depulso",
        "cast_expecto_patronum",
        "cast_stupefy",
        "cast_protego"
    };

    public void OnResponse(WitResponseNode response)
    {
        WitIntentData intentData = response.GetFirstIntentData();
        
        if (intentData != null)
        {
            string intentName = intentData.name;
            float confidence = intentData.confidence;
            
            Debug.Log($"Detected intent: {intentName} with confidence: {confidence}");

            if (confidence >= CONFIDENCE_THRESHOLD && validSpellIntents.Contains(intentName))
            {
                Debug.Log($"Spell intent recognized: {intentName}");
                FireFireball();
            }
            else
            {
                Debug.Log($"Intent not recognized or confidence too low. Intent: {intentName}, Confidence: {confidence}");
            }
        }
        else
        {
            Debug.Log("No intent detected");
        }
    }

    private void FireFireball()
    {
        if (fireballPrefab != null && wandTip != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, wandTip.position, wandTip.rotation);
            Debug.Log($"Fireball fired!");
        }
        else
        {
            Debug.LogError("Fireball prefab or wand tip not assigned!");
        }
    }
} 