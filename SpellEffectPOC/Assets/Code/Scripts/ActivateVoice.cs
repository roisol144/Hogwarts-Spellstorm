using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;
using UnityEngine.InputSystem;

public class ActivateVoice : MonoBehaviour
{
    [SerializeField]
    private Wit wit;

    [SerializeField]
    private AudioSource audioSource;

    private string lastTranscription = "";

    // Start is called before the first frame update
    void Start()
    {
        if (wit != null)
        {
            // Subscribe to voice events
            wit.VoiceEvents.OnStartListening.AddListener(OnStartListening);
            wit.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
            wit.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
        }
    }

    private void OnDestroy()
    {
        if (wit != null)
        {
            // Unsubscribe from voice events
            wit.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
            wit.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
            wit.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (wit == null)
        {
            wit = GetComponent<Wit>();
        }
    }

    public void TriggerPressed()
    { 
        lastTranscription = ""; // Reset transcription when starting new recording
        wit.Activate();
        audioSource.Play();
        Debug.Log("Trigger pressed - Starting voice recording");
    }

    private void OnStartListening()
    {
        Debug.Log("Voice recording started");
    }

    private void OnStoppedListening()
    {
        Debug.Log($"Voice recording stopped. Recorded text: {(string.IsNullOrEmpty(lastTranscription) ? "No text recorded" : lastTranscription)}");
    }

    private void OnFullTranscription(string transcription)
    {
        lastTranscription = transcription;
    }
}
