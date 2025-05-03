using UnityEngine;
using TMPro;
using System.Collections;

public class GestureDisplayManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private Color successColor = Color.green;
    private Coroutine fadeOutCoroutine;

    public void ShowRecognitionResult(string gestureName, float confidence)
    {
        if (displayText == null) return;
        
        // Stop any ongoing fade out
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);

        // Set the text and make it fully visible
        displayText.text = $"Gesture: {gestureName}\nConfidence: {confidence:P1}";
        displayText.color = new Color(successColor.r, successColor.g, successColor.b, 1f);
        
        // Start fade out
        fadeOutCoroutine = StartCoroutine(FadeOutText());
    }

    private IEnumerator FadeOutText()
    {
        yield return new WaitForSeconds(displayDuration * 0.5f); // Hold full visibility

        float elapsedTime = 0f;
        float fadeTime = displayDuration * 0.5f; // Fade during second half
        Color startColor = displayText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            displayText.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
            yield return null;
        }

        displayText.color = endColor;
    }
} 