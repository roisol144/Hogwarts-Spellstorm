using System.Collections;
using System.Collections.Generic;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.Data.Intents;
using UnityEngine;
using System;

public class WitAiAgent : MonoBehaviour
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform wandTip; // Reference to the wand's tip transform
    private const float CONFIDENCE_THRESHOLD = 0.85f;

    // Add this event for SpellCastingManager
    public event Action<string> OnIntentRecognized;
  
    public void OnResponse(WitResponseNode witResponse)
    {
        Debug.Log("Wit response received:" + witResponse.ToString());
        
        // Get the first intent and its confidence
        WitIntentData intentData = witResponse.GetFirstIntentData();
        
        if (intentData != null)
        {
            string intentName = intentData.name;
            float confidence = intentData.confidence;
            
            Debug.Log($"Intent: {intentName}, Confidence: {confidence}");

            // Check if confidence is above threshold
            if (confidence >= CONFIDENCE_THRESHOLD)
            {
                Debug.Log($"Intent recognized with high confidence: {intentName}");
                // Fire the event for SpellCastingManager
                OnIntentRecognized?.Invoke(intentName);
            }
            else
            {
                Debug.Log($"Intent confidence too low: {confidence}");
            }
        }
        else
        {
            Debug.Log("No intent recognized");
        }
    }
} 