// Updated VoiceRecorder.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class VoiceRecorder : MonoBehaviour
{
    public Button startButton;
    public Button stopButton;
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string witAccessToken = "23E6X2QB4WOQMCXQLY32EX4YWYR7F2VG"; // Replace with your Wit.ai access token
    private string witApiUrl = "https://api.wit.ai/speech?v=20230222";

    void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
    }

    void StartRecording()
    {
        if (Microphone.devices.Length > 0)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 10, 16000); // Lowered to 16kHz for optimal size and speed
            Debug.Log("Recording started...");
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
            Debug.Log("Recording stopped. Sending audio to Wit.ai...");

            // Trim silence and add artificial silence
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

        // Detect start of non-silence
        while (start < samples.Length && Mathf.Abs(samples[start]) <= silenceThreshold)
        {
            start++;
        }

        // Detect end of non-silence
        while (end > start && Mathf.Abs(samples[end]) <= silenceThreshold)
        {
            end--;
        }

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
        int totalSamples = clip.samples + (2 * samplesToAdd); // Add silence at start and end

        float[] originalSamples = new float[clip.samples * clip.channels];
        float[] newSamples = new float[totalSamples * clip.channels];

        clip.GetData(originalSamples, 0);

        // Insert silence at the start
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

            Debug.Log("Sending request to Wit.ai...");
            var startTime = Time.realtimeSinceStartup;
            var response = await client.PostAsync(witApiUrl, content);
            var endTime = Time.realtimeSinceStartup;

            string responseText = await response.Content.ReadAsStringAsync();
            Debug.Log($"Wit.ai Response: {responseText}");
            Debug.Log($"Request Time: {endTime - startTime} seconds");

            // Parse response and trigger effects
            ProcessResponse(responseText);
        }
    }
}



List<string> SplitJsonResponse(string jsonResponse)
{
    // Split the response by newlines or carriage returns
    string[] lines = jsonResponse.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
    var jsonObjects = new List<string>();

    foreach (var line in lines)
    {
        // Ensure that the line is a valid JSON object
        if (line.StartsWith("{") && line.EndsWith("}"))
        {
            jsonObjects.Add(line.Trim());
        }
    }

    return jsonObjects;
}



void ProcessResponse(string jsonResponse)
{
    List<string> jsonObjects = SplitJsonResponse(jsonResponse);

    foreach (var jsonObject in jsonObjects)
    {
        try
        {
            var witResponse = JsonUtility.FromJson<WitResponse>(jsonObject);

            if (witResponse != null && witResponse.intents.Length > 0)
            {
                string intentName = witResponse.intents[0].name;
                Debug.Log($"Recognized intent: {intentName}");

                EffectManager effectManager = FindObjectOfType<EffectManager>();
                if (effectManager != null)
                {
                    Vector3 effectPosition = new Vector3(0, 1, 0); // Example position
                    effectManager.TriggerEffect(intentName, effectPosition);
                }
                else
                {
                    Debug.LogError("EffectManager not found in the scene!");
                }
            }
            else
            {
                Debug.LogWarning("No intents found in JSON object.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse JSON object: {jsonObject}\nError: {ex.Message}");
        }
    }
}



    byte[] ConvertAudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Write WAV header
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            memoryStream.Write(IntToBytes(36 + samples.Length * 2), 0, 4); // Chunk size
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            memoryStream.Write(IntToBytes(16), 0, 4); // Subchunk1Size
            memoryStream.Write(ShortToBytes(1), 0, 2); // AudioFormat (PCM)
            memoryStream.Write(ShortToBytes((short)clip.channels), 0, 2);
            memoryStream.Write(IntToBytes(clip.frequency), 0, 4);
            memoryStream.Write(IntToBytes(clip.frequency * clip.channels * 2), 0, 4); // ByteRate
            memoryStream.Write(ShortToBytes((short)(clip.channels * 2)), 0, 2); // BlockAlign
            memoryStream.Write(ShortToBytes(16), 0, 2); // BitsPerSample
            memoryStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            memoryStream.Write(IntToBytes(samples.Length * 2), 0, 4); // Subchunk2Size

            // Write audio samples
            foreach (var sample in samples)
            {
                short rescaled = (short)(sample * 32767);
                memoryStream.Write(ShortToBytes(rescaled), 0, 2);
            }

            return memoryStream.ToArray();
        }
    }

    // Helper functions for byte conversion
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
