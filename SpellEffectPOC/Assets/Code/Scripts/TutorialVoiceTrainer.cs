using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.WitAi.Data.Intents;
using Oculus.Voice;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialVoiceTrainer : MonoBehaviour
{
    [Header("Voice Recognition")]
    [SerializeField] private AppVoiceExperience appVoiceExperience;
    [SerializeField] private AudioSource voiceRecognitionAudioSource; // For voice feedback sounds
    
    [Header("Spell Casting Integration")]
    [SerializeField] private SpellCastingManager spellCastingManager;
    [SerializeField] private MovementRecognizer movementRecognizer;
    
    [Header("Tutorial Audio")]
    [SerializeField] private AudioSource tutorialAudioSource; // Separate audio source for tutorial sounds
    [SerializeField] private AudioClip welcomeMessage;
    [SerializeField] private AudioClip successRewardSound; // Reward sound for 3 consecutive successes
    [SerializeField] private bool playWelcomeOnStart = true;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentSpellText;
    [SerializeField] private TextMeshProUGUI recognizedSpeechText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    
    [Header("Spell Configuration")]
    [SerializeField] private string[] spellNames = new string[] 
    {
        "cast_bombardo",
        "cast_expecto_patronum", 
        "cast_stupefy",
        "cast_protego",
        "cast_accio"
    };
    
    [Header("Input")]
    [SerializeField] private InputAction triggerAction = new InputAction("Trigger", InputActionType.Button);
    [SerializeField] private InputAction spellSwitchAction = new InputAction("Spell Switch", InputActionType.Button);
    [SerializeField] private InputAction returnToMenuAction = new InputAction("Return to Menu", InputActionType.Button);
    
    [Header("Feedback")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private float feedbackDuration = 2f;
    
    [Header("Wit.ai Settings")]
    [SerializeField] private float confidenceThreshold = 0.85f; // Same as main game
    
    [Header("Progress Tracking")]
    [SerializeField] private int consecutiveSuccessTarget = 3; // Number of consecutive successes needed for reward
    
    // Private variables
    private int currentSpellIndex = 0;
    private string lastTranscription = "";
    private bool isListening = false;
    private Coroutine feedbackCoroutine;
    
    // Progress tracking
    private int consecutiveSuccesses = 0;
    
    // Spell casting tracking
    private string lastRecognizedVoice = null;
    private float lastVoiceTime = -10f;
    private string lastRecognizedGesture = null;
    private float lastGestureTime = -10f;
    private float spellMatchWindow = 3f; // Time window for voice + gesture matching
    
    // Spell name mappings for user-friendly display
    private Dictionary<string, string> spellDisplayNames = new Dictionary<string, string>
    {
        { "cast_bombardo", "Bombardo" },
        { "cast_expecto_patronum", "Expecto Patronum" },
        { "cast_stupefy", "Stupefy" },
        { "cast_protego", "Protego" },
    };

    void Start()
    {
        // Ensure spell array is properly initialized (fix for Quest 3 serialization issue)
        ValidateSpellArray();
        
        InitializeVoiceRecognition();
        InitializeSpellCasting();
        InitializeUI();
        SetupInputActions();
        LoadWelcomeMessage();
        LoadSuccessRewardSound();
        
        // Play welcome message after a short delay
        if (playWelcomeOnStart)
        {
            StartCoroutine(PlayWelcomeMessageDelayed(1f));
        }
    }
    
    private void ValidateSpellArray()
    {
        // Fix for Quest 3 serialization issue - ensure all spells are present
        if (spellNames == null || spellNames.Length != 5)
        {
            spellNames = new string[]
            {
                "cast_bombardo",
                "cast_expecto_patronum", 
                "cast_stupefy",
                "cast_protego"
            };
        }
    }

    void InitializeVoiceRecognition()
    {
        if (appVoiceExperience == null)
        {
            appVoiceExperience = FindObjectOfType<AppVoiceExperience>();
        }
        
        if (appVoiceExperience != null)
        {
            // Subscribe to voice events
            appVoiceExperience.VoiceEvents.OnStartListening.AddListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
            appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
            appVoiceExperience.VoiceEvents.OnResponse.AddListener(OnWitResponse); // Add response handler for intents
        }
        else
        {
            Debug.LogWarning("[TutorialVoiceTrainer] No AppVoiceExperience component found!");
        }
    }
    
    void InitializeSpellCasting()
    {
        // Auto-find SpellCastingManager if not assigned
        if (spellCastingManager == null)
        {
            spellCastingManager = FindObjectOfType<SpellCastingManager>();
        }
        
        // Auto-find MovementRecognizer if not assigned
        if (movementRecognizer == null)
        {
            movementRecognizer = FindObjectOfType<MovementRecognizer>();
        }
        
        if (movementRecognizer != null)
        {
            movementRecognizer.OnRecognized.AddListener(OnGestureRecognized);
            Debug.Log("[TutorialVoiceTrainer] Connected to MovementRecognizer");
        }
        else
        {
            Debug.LogWarning("[TutorialVoiceTrainer] No MovementRecognizer found!");
        }
        
        if (spellCastingManager != null)
        {
            Debug.Log("[TutorialVoiceTrainer] Connected to SpellCastingManager");
        }
        else
        {
            Debug.LogWarning("[TutorialVoiceTrainer] No SpellCastingManager found!");
        }
    }
    
    void InitializeUI()
    {
        UpdateCurrentSpellDisplay();
        
        if (recognizedSpeechText != null)
        {
            recognizedSpeechText.text = "Say the spell name...";
            recognizedSpeechText.color = neutralColor;
        }
        
        if (statusText != null)
        {
            statusText.text = "Ready to listen";
        }
    }
    
    void SetupInputActions()
    {
        // Setup Quest 3 right trigger for voice recognition
        if (triggerAction.bindings.Count == 0)
        {
            triggerAction.AddBinding("<XRController>{RightHand}/triggerPressed");
            triggerAction.AddBinding("<OculusTouchController>{RightHand}/triggerPressed");
        }
        triggerAction.performed += OnTriggerPressed;
        triggerAction.Enable();
        
        // Setup A/X button for spell switching
        if (spellSwitchAction.bindings.Count == 0)
        {
            spellSwitchAction.AddBinding("<XRController>{RightHand}/primaryButton");
            spellSwitchAction.AddBinding("<OculusTouchController>{RightHand}/primaryButton");
        }
        spellSwitchAction.performed += OnSpellSwitchPressed;
        spellSwitchAction.Enable();
        
        // Setup B button for returning to main menu
        if (returnToMenuAction.bindings.Count == 0)
        {
            returnToMenuAction.AddBinding("<XRController>{RightHand}/secondaryButton");
            returnToMenuAction.AddBinding("<OculusTouchController>{RightHand}/secondaryButton");
        }
        returnToMenuAction.performed += OnReturnToMenuPressed;
        returnToMenuAction.Enable();
    }

    void OnEnable()
    {
        triggerAction?.Enable();
        spellSwitchAction?.Enable();
        returnToMenuAction?.Enable();
    }

    void OnDisable()
    {
        triggerAction?.Disable();
        spellSwitchAction?.Disable();
        returnToMenuAction?.Disable();
    }

    private void OnDestroy()
    {
        if (appVoiceExperience != null)
        {
            // Unsubscribe from voice events
            appVoiceExperience.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
            appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
            appVoiceExperience.VoiceEvents.OnResponse.RemoveListener(OnWitResponse);
        }
        
        if (movementRecognizer != null)
        {
            movementRecognizer.OnRecognized.RemoveListener(OnGestureRecognized);
        }
    }

    // Input handlers
    private void OnTriggerPressed(InputAction.CallbackContext context)
    {
        if (!isListening)
        {
            StartVoiceRecognition();
        }
    }
    
    private void OnSpellSwitchPressed(InputAction.CallbackContext context)
    {
        CycleSpell();
    }
    
    private void OnReturnToMenuPressed(InputAction.CallbackContext context)
    {
        Debug.Log("[TutorialVoiceTrainer] B button pressed - returning to main menu");
        SceneManager.LoadScene("MainMenu");
    }
    
    private void OnGestureRecognized(string gestureName)
    {
        lastRecognizedGesture = gestureName;
        lastGestureTime = Time.time;
        
        Debug.Log($"[TutorialVoiceTrainer] Gesture recognized: {gestureName}");
        
        // Check if we can complete a spell
        CheckSpellCompletion();
    }

    // Voice recognition methods
    public void StartVoiceRecognition()
    {
        if (appVoiceExperience != null && !isListening)
        {
            lastTranscription = "";
            appVoiceExperience.Activate();
            
            // Only play voice recognition feedback sound, not the welcome message
            if (voiceRecognitionAudioSource != null && voiceRecognitionAudioSource.clip != welcomeMessage)
            {
                voiceRecognitionAudioSource.Play();
            }
            
            Debug.Log("[TutorialVoiceTrainer] Voice recognition started");
        }
    }

    private void OnStartListening()
    {
        isListening = true;
        
        if (statusText != null)
        {
            statusText.text = "Listening...";
        }
        
        if (recognizedSpeechText != null)
        {
            recognizedSpeechText.text = "Listening for spell...";
            recognizedSpeechText.color = neutralColor;
        }
        
        Debug.Log("[TutorialVoiceTrainer] Started listening for voice input");
    }

    private void OnStoppedListening()
    {
        isListening = false;
        
        if (statusText != null)
        {
            statusText.text = "Processing...";
        }
        
        Debug.Log($"[TutorialVoiceTrainer] Stopped listening. Transcription: {lastTranscription}");
    }

    private void OnFullTranscription(string transcription)
    {
        lastTranscription = transcription;
        
        if (recognizedSpeechText != null)
        {
            recognizedSpeechText.text = $"You said: \"{transcription}\"";
        }
        
        Debug.Log($"[TutorialVoiceTrainer] Transcription: {transcription}");
    }
    
    // New wit.ai response handler for intent-based recognition (like main game)
    private void OnWitResponse(WitResponseNode witResponse)
    {
        Debug.Log($"[TutorialVoiceTrainer] Wit response received: {witResponse.ToString()}");
        
        // Get the first intent and its confidence
        WitIntentData intentData = witResponse.GetFirstIntentData();
        
        if (intentData != null)
        {
            string intentName = intentData.name;
            float confidence = intentData.confidence;
            
            Debug.Log($"[TutorialVoiceTrainer] Intent: {intentName}, Confidence: {confidence}");

            // Check if confidence is above threshold (same as main game)
            if (confidence >= confidenceThreshold)
            {
                Debug.Log($"[TutorialVoiceTrainer] Intent recognized with high confidence: {intentName}");
                
                // Store voice recognition for spell matching
                lastRecognizedVoice = intentName;
                lastVoiceTime = Time.time;
                
                CheckSpellMatchByIntent(intentName, confidence);
                CheckSpellCompletion();
            }
            else
            {
                Debug.Log($"[TutorialVoiceTrainer] Intent confidence too low: {confidence} (threshold: {confidenceThreshold})");
                ShowLowConfidenceFeedback(intentName, confidence);
            }
        }
        else
        {
            Debug.Log("[TutorialVoiceTrainer] No intent recognized");
            ShowNoIntentFeedback();
        }
        
        if (statusText != null)
        {
            statusText.text = "Ready to listen";
        }
    }

    // Spell management methods
    private void CycleSpell()
    {
        currentSpellIndex = (currentSpellIndex + 1) % spellNames.Length;
        UpdateCurrentSpellDisplay();
        
        // Reset spell tracking when switching spells
        lastRecognizedVoice = null;
        lastRecognizedGesture = null;
        
        // Reset feedback when switching spells
        if (recognizedSpeechText != null)
        {
            recognizedSpeechText.text = "Say the spell name...";
            recognizedSpeechText.color = neutralColor;
        }
        
        Debug.Log($"[TutorialVoiceTrainer] Switched to spell: {spellNames[currentSpellIndex]}");
    }

    private void UpdateCurrentSpellDisplay()
    {
        if (currentSpellText != null)
        {
            string currentSpell = spellNames[currentSpellIndex];
            string displayName = spellDisplayNames.ContainsKey(currentSpell) ? 
                spellDisplayNames[currentSpell] : currentSpell;
            
            currentSpellText.text = $"Current Spell: {displayName}";
        }
    }
    
    private void UpdateInstructions()
    {
        if (instructionsText != null)
        {
            string currentSpell = spellNames[currentSpellIndex];
            string displayName = spellDisplayNames.ContainsKey(currentSpell) ? 
                spellDisplayNames[currentSpell] : currentSpell;
            
            instructionsText.text = $"Practice saying: \"{displayName}\"\n\nPress trigger to start voice recognition\nPress spell switch button to change spells";
        }
    }

    // Intent-based spell matching (like main game)
    private void CheckSpellMatchByIntent(string intentName, float confidence)
    {
        string currentSpell = spellNames[currentSpellIndex];
        string expectedSpellName = spellDisplayNames.ContainsKey(currentSpell) ? 
            spellDisplayNames[currentSpell] : currentSpell;
        
        Debug.Log($"[TutorialVoiceTrainer] Checking intent '{intentName}' against current spell '{currentSpell}'");
        
        // Check if the recognized intent matches the current spell
        if (intentName == currentSpell)
        {
            ShowCorrectFeedback(expectedSpellName, confidence);
        }
        else
        {
            // Show what intent was recognized vs what was expected
            string recognizedSpellName = spellDisplayNames.ContainsKey(intentName) ? 
                spellDisplayNames[intentName] : intentName;
            ShowIncorrectIntentFeedback(expectedSpellName, recognizedSpellName, confidence);
        }
    }
    
    // Fallback spell matching for transcription (keep for backward compatibility)
    private void CheckSpellMatch(string transcription)
    {
        string currentSpell = spellNames[currentSpellIndex];
        string expectedSpellName = spellDisplayNames.ContainsKey(currentSpell) ? 
            spellDisplayNames[currentSpell] : currentSpell;
        
        // Normalize both strings for comparison (remove case sensitivity and extra spaces)
        string normalizedTranscription = transcription.Trim().ToLowerInvariant();
        string normalizedExpected = expectedSpellName.ToLowerInvariant();
        
        // Check for various matching criteria
        bool isMatch = IsSpellMatch(normalizedTranscription, normalizedExpected, currentSpell);
        
        if (isMatch)
        {
            ShowCorrectFeedback(expectedSpellName, 1.0f);
        }
        else
        {
            ShowIncorrectFeedback(expectedSpellName, transcription);
        }
    }
    
    private bool IsSpellMatch(string transcription, string expectedName, string spellIntent)
    {
        // Direct match with display name
        if (transcription.Contains(expectedName))
        {
            return true;
        }
        
        // Match specific spell variations
        switch (spellIntent)
        {
            case "cast_bombardo":
                return transcription.Contains("bombardo") || transcription.Contains("bombard");
                
            case "cast_expecto_patronum":
                return transcription.Contains("expecto patronum") || 
                       transcription.Contains("expecto") || 
                       transcription.Contains("patronum");
                       
            case "cast_stupefy":
                return transcription.Contains("stupefy") || transcription.Contains("stupify");
                
            case "cast_protego":
                return transcription.Contains("protego") || transcription.Contains("protect");
                
            case "cast_accio":
                return transcription.Contains("accio") || transcription.Contains("acio");
                
            default:
                return false;
        }
    }
    
    private void CheckSpellCompletion()
    {
        string currentSpell = spellNames[currentSpellIndex];
        bool hasValidVoice = lastRecognizedVoice != null && (Time.time - lastVoiceTime) <= spellMatchWindow;
        bool hasValidGesture = lastRecognizedGesture != null && (Time.time - lastGestureTime) <= spellMatchWindow;
        bool voiceMatches = hasValidVoice && lastRecognizedVoice == currentSpell;
        bool gestureMatches = hasValidGesture && lastRecognizedGesture == currentSpell;
        
        Debug.Log($"[TutorialVoiceTrainer] CheckSpellCompletion - Current: {currentSpell}, Voice: {lastRecognizedVoice} ({voiceMatches}), Gesture: {lastRecognizedGesture} ({gestureMatches})");
        
        // Case 1: Both voice and gesture match the SELECTED spell - Success!
        if (voiceMatches && gestureMatches)
        {
            consecutiveSuccesses++;
            string displayName = spellDisplayNames.ContainsKey(currentSpell) ? spellDisplayNames[currentSpell] : currentSpell;
            ShowSpellSuccessFeedback(displayName);
            
            // Clear the tracking variables
            lastRecognizedVoice = null;
            lastRecognizedGesture = null;
            
            if (consecutiveSuccesses >= consecutiveSuccessTarget)
            {
                StartCoroutine(PlaySuccessRewardDelayed());
                consecutiveSuccesses = 0;
            }
            return;
        }
        
        // TUTORIAL-SPECIFIC: If player casts a different spell correctly, give feedback but don't cast
        if (hasValidVoice && hasValidGesture && lastRecognizedVoice == lastRecognizedGesture)
        {
            // Player performed a valid spell combination, but is it the selected one?
            if (lastRecognizedVoice != currentSpell)
            {
                consecutiveSuccesses = 0;
                string attemptedSpellName = spellDisplayNames.ContainsKey(lastRecognizedVoice) ? spellDisplayNames[lastRecognizedVoice] : lastRecognizedVoice;
                string currentSpellName = spellDisplayNames.ContainsKey(currentSpell) ? spellDisplayNames[currentSpell] : currentSpell;
                ShowSpellFeedback($"Good {attemptedSpellName}! Now try {currentSpellName}");
                
                // Clear tracking to prevent confusion
                lastRecognizedVoice = null;
                lastRecognizedGesture = null;
                return;
            }
        }
        
        // Case 2: Has valid voice but wrong/no gesture (for selected spell)
        if (hasValidVoice && lastRecognizedVoice == currentSpell && (!hasValidGesture || !gestureMatches))
        {
            consecutiveSuccesses = 0;
            ShowSpellFeedback("Check your gesture");
            return;
        }
        
        // Case 3: Has valid gesture but wrong/no voice (for selected spell)
        if (hasValidGesture && lastRecognizedGesture == currentSpell && (!hasValidVoice || !voiceMatches))
        {
            consecutiveSuccesses = 0;
            ShowSpellFeedback("Say the spell");
            return;
        }
        
        // Case 4: Voice doesn't match selected spell but gesture might
        if (hasValidVoice && lastRecognizedVoice != currentSpell)
        {
            consecutiveSuccesses = 0;
            string currentSpellName = spellDisplayNames.ContainsKey(currentSpell) ? spellDisplayNames[currentSpell] : currentSpell;
            ShowSpellFeedback($"Say {currentSpellName}");
            return;
        }
        
        // Case 5: Gesture doesn't match selected spell but voice might
        if (hasValidGesture && lastRecognizedGesture != currentSpell)
        {
            consecutiveSuccesses = 0;
            string currentSpellName = spellDisplayNames.ContainsKey(currentSpell) ? spellDisplayNames[currentSpell] : currentSpell;
            ShowSpellFeedback($"Draw {currentSpellName} gesture");
            return;
        }
        
        // Case 6: Both attempted but neither matches selected spell
        if (hasValidVoice && hasValidGesture && !voiceMatches && !gestureMatches)
        {
            consecutiveSuccesses = 0;
            string currentSpellName = spellDisplayNames.ContainsKey(currentSpell) ? spellDisplayNames[currentSpell] : currentSpell;
            ShowSpellFeedback($"Try {currentSpellName}");
            return;
        }
    }
    
    private void ShowSpellSuccessFeedback(string spellName)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            $"✓ Perfect! {spellName} cast successfully!", 
            correctColor
        ));
        
        Debug.Log($"[TutorialVoiceTrainer] Spell successfully cast: {spellName}");
    }
    
    private void ShowSpellFeedback(string message)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            message, 
            incorrectColor
        ));
        
        Debug.Log($"[TutorialVoiceTrainer] Spell feedback: {message}");
    }
    
    private void ShowCorrectFeedback(string spellName, float confidence)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            $"✓ Correct! You said \"{spellName}\" perfectly!\nConfidence: {confidence:F2}", 
            correctColor
        ));
        
        Debug.Log($"[TutorialVoiceTrainer] Correct spell recognition: {spellName} (confidence: {confidence:F2})");
    }
    
    private void ShowIncorrectIntentFeedback(string expectedSpell, string recognizedSpell, float confidence)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            $"✗ Wrong spell! Expected: \"{expectedSpell}\"\nRecognized: \"{recognizedSpell}\"\nConfidence: {confidence:F2}", 
            incorrectColor
        ));
        
        Debug.Log($"[TutorialVoiceTrainer] Wrong spell intent. Expected: {expectedSpell}, Got: {recognizedSpell} (confidence: {confidence:F2})");
    }
    
    private void ShowLowConfidenceFeedback(string intentName, float confidence)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        string recognizedSpellName = spellDisplayNames.ContainsKey(intentName) ? 
            spellDisplayNames[intentName] : intentName;
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            $"⚠ Low confidence: \"{recognizedSpellName}\"\nConfidence: {confidence:F2} (need {confidenceThreshold:F2})\nTry speaking more clearly!", 
            incorrectColor
        ));
        
        Debug.Log($"[TutorialVoiceTrainer] Low confidence recognition: {intentName} ({confidence:F2})");
    }
    
    private void ShowNoIntentFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            $"❓ No spell recognized\nTry speaking more clearly or closer to the microphone!", 
            incorrectColor
        ));
        
        Debug.Log("[TutorialVoiceTrainer] No intent recognized");
    }
    
    private void ShowIncorrectFeedback(string expectedSpell, string actualTranscription)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        
        feedbackCoroutine = StartCoroutine(ShowFeedbackCoroutine(
            $"✗ Try again! Expected: \"{expectedSpell}\"\nYou said: \"{actualTranscription}\"", 
            incorrectColor
        ));
        
        Debug.Log($"[TutorialVoiceTrainer] Incorrect spell recognition. Expected: {expectedSpell}, Got: {actualTranscription}");
    }
    
    private IEnumerator ShowFeedbackCoroutine(string message, Color color)
    {
        if (recognizedSpeechText != null)
        {
            recognizedSpeechText.text = message;
            recognizedSpeechText.color = color;
        }
        
        yield return new WaitForSeconds(feedbackDuration);
        
        // Reset to neutral state
        if (recognizedSpeechText != null)
        {
            recognizedSpeechText.text = "Say the spell name...";
            recognizedSpeechText.color = neutralColor;
        }
        
        feedbackCoroutine = null;
    }
    
    // Public methods for UI buttons (if needed)
    public void OnTriggerButtonPressed()
    {
        StartVoiceRecognition();
    }
    
    public void OnSpellSwitchButtonPressed()
    {
        CycleSpell();
    }
    
    // Getter methods for external access
    public string GetCurrentSpellName()
    {
        string currentSpell = spellNames[currentSpellIndex];
        return spellDisplayNames.ContainsKey(currentSpell) ? 
            spellDisplayNames[currentSpell] : currentSpell;
    }
    
    public bool IsCurrentlyListening()
    {
        return isListening;
    }
    
    // Welcome message methods
    private void LoadWelcomeMessage()
    {
        if (welcomeMessage == null)
        {
            welcomeMessage = Resources.Load<AudioClip>("Sounds/TutorialWelcomeMsg");
            if (welcomeMessage != null)
            {
                Debug.Log("[TutorialVoiceTrainer] Welcome message loaded from Resources");
            }
            else
            {
                Debug.LogWarning("[TutorialVoiceTrainer] Could not load welcome message from Resources/Sounds/TutorialWelcomeMsg");
            }
        }
    }
    
    private IEnumerator PlayWelcomeMessageDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayWelcomeMessage();
    }
    
    public void PlayWelcomeMessage()
    {
        if (welcomeMessage != null && tutorialAudioSource != null)
        {
            // Update status text while playing welcome message
            if (statusText != null)
            {
                statusText.text = "Welcome to Spell Training!";
            }
            
            // Ensure tutorial audio source is properly configured
            tutorialAudioSource.clip = welcomeMessage;
            tutorialAudioSource.volume = 1f; // Ensure volume is at maximum
            tutorialAudioSource.spatialBlend = 0f; // 2D audio (not 3D positioned)
            tutorialAudioSource.Play();
            
            Debug.Log($"[TutorialVoiceTrainer] Playing welcome message - Duration: {welcomeMessage.length}s, Volume: {tutorialAudioSource.volume}");
            
            // Start coroutine to reset status after audio finishes
            StartCoroutine(ResetStatusAfterWelcome());
        }
        else
        {
            Debug.LogWarning("[TutorialVoiceTrainer] Cannot play welcome message - missing audio clip or tutorial audio source");
            if (welcomeMessage == null) Debug.LogWarning("  - Welcome message AudioClip is null");
            if (tutorialAudioSource == null) Debug.LogWarning("  - Tutorial AudioSource is null");
        }
    }
    
    private IEnumerator ResetStatusAfterWelcome()
    {
        // Wait for the audio to finish playing
        yield return new WaitForSeconds(welcomeMessage.length);
        
        // Reset status text
        if (statusText != null)
        {
            statusText.text = "Ready to listen";
        }
    }
    
    // Success reward methods
    private void LoadSuccessRewardSound()
    {
        if (successRewardSound == null)
        {
            // Try to load from Resources if not manually assigned
            successRewardSound = Resources.Load<AudioClip>("Sounds/SuccessReward");
            if (successRewardSound != null)
            {
                Debug.Log("[TutorialVoiceTrainer] Success reward sound loaded from Resources");
            }
            else
            {
                Debug.LogWarning("[TutorialVoiceTrainer] No success reward sound found. Assign manually in Inspector or place in Resources/Sounds/SuccessReward");
            }
        }
    }
    
    private IEnumerator PlaySuccessRewardDelayed()
    {
        // Wait a moment after the success feedback before playing reward
        yield return new WaitForSeconds(1.5f);
        PlaySuccessReward();
    }
    
    public void PlaySuccessReward()
    {
        if (successRewardSound != null && tutorialAudioSource != null)
        {
            // Update status text during reward
            if (statusText != null)
            {
                statusText.text = "🎉 Excellent! 3 in a row!";
            }
            
            // Save current clip to restore later
            AudioClip previousClip = tutorialAudioSource.clip;
            
            // Play reward sound
            tutorialAudioSource.clip = successRewardSound;
            tutorialAudioSource.volume = 1f;
            tutorialAudioSource.spatialBlend = 0f; // 2D audio
            tutorialAudioSource.Play();
            
            Debug.Log($"[TutorialVoiceTrainer] Playing success reward! Consecutive successes reset to 0");
            
            // Start coroutine to reset status and restore previous clip after reward finishes
            StartCoroutine(ResetAfterSuccessReward(previousClip));
        }
        else
        {
            Debug.LogWarning("[TutorialVoiceTrainer] Cannot play success reward - missing audio clip or audio source");
            if (successRewardSound == null) Debug.LogWarning("  - Success reward AudioClip is null");
            if (tutorialAudioSource == null) Debug.LogWarning("  - Tutorial AudioSource is null");
        }
    }
    
    private IEnumerator ResetAfterSuccessReward(AudioClip previousClip)
    {
        // Wait for the reward audio to finish playing
        yield return new WaitForSeconds(successRewardSound.length);
        
        // Restore previous clip
        if (tutorialAudioSource != null && previousClip != null)
        {
            tutorialAudioSource.clip = previousClip;
        }
        
        // Reset status text
        if (statusText != null)
        {
            statusText.text = "Ready to listen";
        }
    }
}