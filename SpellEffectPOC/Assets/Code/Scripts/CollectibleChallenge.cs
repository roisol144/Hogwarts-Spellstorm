using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages a single collectible challenge - tracks timer, collection progress, and scoring
/// </summary>
public class CollectibleChallenge : MonoBehaviour
{
    // Private variables
    private List<GameObject> challengeItems = new List<GameObject>();
    private List<bool> itemsCollected = new List<bool>();
    private float timeLimit;
    private float timeRemaining;
    private int successPoints;
    private int failurePoints;
    private bool isActive = false;
    private bool challengeCompleted = false;
    private Coroutine timerCoroutine;
    
    // Properties
    public bool IsActive => isActive;
    public float TimeRemaining => timeRemaining;
    public int ItemsCollected => GetCollectedCount();
    public int TotalItems => challengeItems.Count;
    public float ProgressPercentage => challengeItems.Count > 0 ? (float)GetCollectedCount() / challengeItems.Count : 0f;
    
    // Events
    public System.Action<int, int> OnItemCollected; // collected, total
    public System.Action<bool, int, int> OnChallengeCompleted; // success, collected, total
    public System.Action<float> OnTimerUpdate; // time remaining
    
    /// <summary>
    /// Initializes the challenge with items and settings
    /// </summary>
    public void Initialize(List<GameObject> items, float timeLimitSeconds, int successPts, int failurePts)
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogError("[CollectibleChallenge] Cannot initialize with empty items list!");
            return;
        }
        
        challengeItems = new List<GameObject>(items);
        itemsCollected = new List<bool>(new bool[items.Count]);
        timeLimit = timeLimitSeconds;
        timeRemaining = timeLimitSeconds;
        successPoints = successPts;
        failurePoints = failurePts;
        isActive = true;
        challengeCompleted = false;
        
        // Set up collectible items
        SetupCollectibleItems();
        
        // Start timer
        StartTimer();
        
        Debug.Log($"[CollectibleChallenge] Challenge initialized with {items.Count} items, {timeLimitSeconds} second time limit.");
    }
    
    /// <summary>
    /// Sets up the collectible component on each item
    /// </summary>
    private void SetupCollectibleItems()
    {
        for (int i = 0; i < challengeItems.Count; i++)
        {
            GameObject item = challengeItems[i];
            if (item == null) continue;
            
            Collectible collectible = item.GetComponent<Collectible>();
            if (collectible == null)
            {
                collectible = item.AddComponent<Collectible>();
            }
            
            // Set up the collection callback with the item's index
            int itemIndex = i; // Capture the index for the closure
            collectible.OnCollected += () => OnItemCollectedInternal(itemIndex);
            
            // Initialize the collectible
            collectible.Initialize(this, itemIndex);
        }
    }
    
    /// <summary>
    /// Starts the challenge timer
    /// </summary>
    private void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }
    
    /// <summary>
    /// Timer coroutine that counts down and ends the challenge
    /// </summary>
    private IEnumerator TimerCoroutine()
    {
        while (timeRemaining > 0 && isActive && !challengeCompleted)
        {
            timeRemaining -= Time.deltaTime;
            OnTimerUpdate?.Invoke(timeRemaining);
            yield return null;
        }
        
        // Time's up!
        if (isActive && !challengeCompleted)
        {
            EndChallenge(false); // Failed due to timeout
        }
    }
    
    /// <summary>
    /// Called when an item is collected
    /// </summary>
    private void OnItemCollectedInternal(int itemIndex)
    {
        if (!isActive || challengeCompleted) return;
        
        if (itemIndex < 0 || itemIndex >= itemsCollected.Count)
        {
            Debug.LogError($"[CollectibleChallenge] Invalid item index: {itemIndex}");
            return;
        }
        
        if (itemsCollected[itemIndex])
        {
            Debug.LogWarning($"[CollectibleChallenge] Item {itemIndex} already collected!");
            return;
        }
        
        // Mark as collected
        itemsCollected[itemIndex] = true;
        
        int collectedCount = GetCollectedCount();
        
        Debug.Log($"[CollectibleChallenge] Item {itemIndex} collected! Progress: {collectedCount}/{challengeItems.Count}");
        
        // Notify listeners
        OnItemCollected?.Invoke(collectedCount, challengeItems.Count);
        
        // Check if challenge is complete
        if (collectedCount >= challengeItems.Count)
        {
            EndChallenge(true); // Success!
        }
    }
    
    /// <summary>
    /// Ends the challenge with success or failure
    /// </summary>
    private void EndChallenge(bool success)
    {
        if (challengeCompleted) return;
        
        challengeCompleted = true;
        isActive = false;
        
        // Stop timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        int collectedCount = GetCollectedCount();
        
        Debug.Log($"[CollectibleChallenge] Challenge ended. Success: {success}, Collected: {collectedCount}/{challengeItems.Count}");
        
        // Clean up remaining items
        CleanupRemainingItems();
        
        // Notify completion
        OnChallengeCompleted?.Invoke(success, collectedCount, challengeItems.Count);
        
        // Clean up this challenge object after a short delay
        StartCoroutine(CleanupAfterDelay(2f));
    }
    
    /// <summary>
    /// Cleans up any remaining uncollected items
    /// </summary>
    private void CleanupRemainingItems()
    {
        for (int i = 0; i < challengeItems.Count; i++)
        {
            if (!itemsCollected[i] && challengeItems[i] != null)
            {
                // Add a fade-out effect before destroying
                StartCoroutine(FadeAndDestroy(challengeItems[i], 1f));
            }
        }
    }
    
    /// <summary>
    /// Fades out and destroys an item
    /// </summary>
    private IEnumerator FadeAndDestroy(GameObject item, float fadeDuration)
    {
        if (item == null) yield break;
        
        Renderer renderer = item.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            Color originalColor = material.color;
            
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                if (item == null) yield break;
                
                float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);
                Color newColor = originalColor;
                newColor.a = alpha;
                material.color = newColor;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
        if (item != null)
        {
            Destroy(item);
        }
    }
    
    /// <summary>
    /// Cleans up this challenge object after a delay
    /// </summary>
    private IEnumerator CleanupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log("[CollectibleChallenge] Challenge cleanup completed.");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Forces the challenge to end (used when system is stopped)
    /// </summary>
    public void ForceEndChallenge()
    {
        if (!challengeCompleted)
        {
            EndChallenge(false);
        }
    }
    
    /// <summary>
    /// Gets the number of items collected
    /// </summary>
    private int GetCollectedCount()
    {
        int count = 0;
        for (int i = 0; i < itemsCollected.Count; i++)
        {
            if (itemsCollected[i]) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Gets the time remaining as a formatted string
    /// </summary>
    public string GetFormattedTimeRemaining()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    /// <summary>
    /// Gets challenge status information
    /// </summary>
    public string GetStatusInfo()
    {
        int collected = GetCollectedCount();
        return $"Collected: {collected}/{challengeItems.Count} | Time: {GetFormattedTimeRemaining()}";
    }
    
    void OnDestroy()
    {
        // Clean up any remaining coroutines
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }
} 