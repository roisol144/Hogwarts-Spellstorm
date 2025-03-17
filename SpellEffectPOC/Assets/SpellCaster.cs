using UnityEngine;
using UnityEngine.InputSystem; // Only needed if you're using the new Input System
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using SpellEffectPOC;

public class SpellCaster : MonoBehaviour
{
    [Header("References")]
    // Assign your wand tip Transform in the Inspector (where the fireball should appear)
    public Transform wandTip;

    // Assign your Fireball prefab (created earlier) here
    public GameObject fireballPrefab;

    [Header("Input Settings")]
    // If using the new Input System, assign the Input Action that corresponds to your controller trigger
    public InputActionProperty triggerAction;

    // Threshold for detecting a press; adjust as needed
    public float triggerThreshold = 0.1f;

    // For detecting the "rising edge" of a trigger press (so you only cast once per press)
    private float previousTriggerValue = 0f;

    [Header("Voice Recognition")]
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string witAccessToken = "23E6X2QB4WOQMCXQLY32EX4YWYR7F2VG"; // Replace with your Wit.ai token
    private string witApiUrl = "https://api.wit.ai/speech?v=20230222";

    void Update()
    {
        // Check if the new Input System action is assigned and valid
        if (triggerAction != null && triggerAction.action != null)
        {
            // Read the trigger value (typically a float from 0 to 1)
            float triggerValue = triggerAction.action.ReadValue<float>();

            // Start recording when trigger is pressed
            if (triggerValue > triggerThreshold && previousTriggerValue <= triggerThreshold)
            {
                StartRecording();
            }
            // Stop recording when trigger is released
            else if (triggerValue <= triggerThreshold && previousTriggerValue > triggerThreshold)
            {
                StopRecording();
            }

            previousTriggerValue = triggerValue;
        }
        else
        {
            // Fallback: if not using the new Input System, use a key press (for example, the Space key)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CastFireball();
            }
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 10, 16000);
            Debug.Log("Started recording spell...");
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    async void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(null);
            Debug.Log("Recording stopped. Processing spell...");

            AudioClip trimmedClip = TrimSilence(recordedClip);
            AudioClip paddedClip = AddSilence(trimmedClip);

            await SendToWitAi(paddedClip);
        }
    }

    AudioClip TrimSilence(AudioClip clip, float silenceThreshold = 0.01f)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int start = 0;
        int end = samples.Length - 1;

        while (start < samples.Length && Mathf.Abs(samples[start]) <= silenceThreshold)
            start++;

        while (end > start && Mathf.Abs(samples[end]) <= silenceThreshold)
            end--;

        int trimmedLength = end - start + 1;
        if (trimmedLength <= 0)
        {
            Debug.LogWarning("No significant audio detected!");
            return null;
        }

        float[] trimmedSamples = new float[trimmedLength];
        System.Array.Copy(samples, start, trimmedSamples, 0, trimmedLength);

        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", trimmedSamples.Length, clip.channels, clip.frequency, false);
        trimmedClip.SetData(trimmedSamples, 0);

        return trimmedClip;
    }

    AudioClip AddSilence(AudioClip clip, float silenceDuration = 0.5f)
    {
        if (clip == null) return null;

        int samplesToAdd = Mathf.CeilToInt(silenceDuration * clip.frequency);
        int totalSamples = clip.samples + (2 * samplesToAdd);

        float[] originalSamples = new float[clip.samples * clip.channels];
        float[] newSamples = new float[totalSamples * clip.channels];

        clip.GetData(originalSamples, 0);

        int offset = samplesToAdd * clip.channels;
        for (int i = 0; i < originalSamples.Length; i++)
        {
            newSamples[i + offset] = originalSamples[i];
        }

        AudioClip newClip = AudioClip.Create("PaddedClip", totalSamples, clip.channels, clip.frequency, false);
        newClip.SetData(newSamples, 0);
        return newClip;
    }

    async Task SendToWitAi(AudioClip clip)
    {
        if (clip == null) return;

        byte[] audioData = ConvertAudioClipToWav(clip);

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + witAccessToken);

            using (var content = new ByteArrayContent(audioData))
            {
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/wav");

                Debug.Log("Processing spell recognition...");
                var response = await client.PostAsync(witApiUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();
                Debug.Log($"Spell recognition response: {responseText}");

                ProcessSpellResponse(responseText);
            }
        }
    }

    void ProcessSpellResponse(string jsonResponse)
    {
        Debug.Log($"Raw JSON response: {jsonResponse}");
        
        try
        {
            // Try to parse the entire response as a single JSON object first
            var witResponse = JsonUtility.FromJson<WitResponse>(jsonResponse);
            
            if (witResponse != null && witResponse.intents != null && witResponse.intents.Length > 0)
            {
                ProcessIntent(witResponse.intents[0]);
            }
            else
            {
                // If that fails, try the line-by-line approach
                List<string> jsonObjects = SplitJsonResponse(jsonResponse);
                Debug.Log($"Processing {jsonObjects.Count} JSON objects from response");
                
                foreach (var jsonObject in jsonObjects)
                {
                    try
                    {
                        witResponse = JsonUtility.FromJson<WitResponse>(jsonObject);
                        
                        if (witResponse != null && witResponse.intents != null && witResponse.intents.Length > 0)
                        {
                            ProcessIntent(witResponse.intents[0]);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to parse JSON object: {jsonObject}\nError: {ex.Message}");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to process response: {ex.Message}");
            
            // Let's try a more manual approach to extract the intent
            if (jsonResponse.Contains("\"name\":"))
            {
                int nameIndex = jsonResponse.IndexOf("\"name\":");
                if (nameIndex > 0)
                {
                    int startQuote = jsonResponse.IndexOf("\"", nameIndex + 7);
                    int endQuote = jsonResponse.IndexOf("\"", startQuote + 1);
                    
                    if (startQuote > 0 && endQuote > startQuote)
                    {
                        string intentName = jsonResponse.Substring(startQuote + 1, endQuote - startQuote - 1);
                        Debug.Log($"Manually extracted intent: {intentName}");
                        
                        // Create a mock intent
                        Intent mockIntent = new Intent { name = intentName, confidence = 1.0f };
                        ProcessIntent(mockIntent);
                    }
                }
            }
        }
    }

    // Helper method to process an intent
    void ProcessIntent(Intent intent)
    {
        string spellName = intent.name;
        float confidence = intent.confidence;
        Debug.Log($"Processing intent: {spellName} with confidence: {confidence}");
        
        // Cast the appropriate spell based on the recognition
        if (spellName.ToLower().Contains("fireball") || 
            spellName.ToLower().Contains("bombardo") || 
            spellName.ToLower().Equals("cast_bombardo"))
        {
            Debug.Log("Bombardo spell detected! Attempting to cast fireball...");
            CastFireball();
        }
        else
        {
            Debug.Log($"Spell {spellName} not recognized as a fireball spell");
        }
    }

    List<string> SplitJsonResponse(string jsonResponse)
    {
        List<string> jsonObjects = new List<string>();
        
        // Try to find complete JSON objects
        int depth = 0;
        int startIndex = -1;
        
        for (int i = 0; i < jsonResponse.Length; i++)
        {
            char c = jsonResponse[i];
            
            if (c == '{')
            {
                if (depth == 0)
                {
                    startIndex = i;
                }
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0 && startIndex != -1)
                {
                    string jsonObject = jsonResponse.Substring(startIndex, i - startIndex + 1);
                    jsonObjects.Add(jsonObject);
                    startIndex = -1;
                }
            }
        }
        
        return jsonObjects;
    }

    void CastFireball()
    {
        Debug.Log("CastFireball method called");
        if (fireballPrefab != null && wandTip != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, wandTip.position, wandTip.rotation);
            Debug.Log($"Fireball instantiated at position: {fireball.transform.position}");
        }
        else
        {
            if (fireballPrefab == null)
                Debug.LogError("Fireball prefab is not assigned in the Unity Inspector!");
            if (wandTip == null)
                Debug.LogError("Wand Tip transform is not assigned in the Unity Inspector!");
        }
    }

    // Helper method from VoiceRecorder
    byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Write WAV header
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(IntToBytes(36 + samples.Length * 2), 0, 4);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            memoryStream.Write(IntToBytes(16), 0, 4);
            memoryStream.Write(ShortToBytes(1), 0, 2);
            memoryStream.Write(ShortToBytes((short)clip.channels), 0, 2);
            memoryStream.Write(IntToBytes(clip.frequency), 0, 4);
            memoryStream.Write(IntToBytes(clip.frequency * clip.channels * 2), 0, 4);
            memoryStream.Write(ShortToBytes((short)(clip.channels * 2)), 0, 2);
            memoryStream.Write(ShortToBytes(16), 0, 2);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            memoryStream.Write(IntToBytes(samples.Length * 2), 0, 4);

            foreach (var sample in samples)
            {
                short rescaled = (short)(sample * 32767);
                memoryStream.Write(ShortToBytes(rescaled), 0, 2);
            }

            return memoryStream.ToArray();
        }
    }

    byte[] IntToBytes(int value)
    {
        return new byte[]
        {
            (byte)(value & 0xFF),
            (byte)((value >> 8) & 0xFF),
            (byte)((value >> 16) & 0xFF),
            (byte)((value >> 24) & 0xFF)
        };
    }

    byte[] ShortToBytes(short value)
    {
        return new byte[]
        {
            (byte)(value & 0xFF),
            (byte)((value >> 8) & 0xFF)
        };
    }
}
