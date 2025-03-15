using UnityEngine;
using UnityEngine.InputSystem; // Only needed if you're using the new Input System

public class NewSpellCaster : MonoBehaviour
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

    void Update()
    {
        // Check if the new Input System action is assigned and valid
        if (triggerAction != null && triggerAction.action != null)
        {
            // Read the trigger value (typically a float from 0 to 1)
            float triggerValue = triggerAction.action.ReadValue<float>();

            // If the trigger has just been pressed this frame, cast the spell
            if (triggerValue > triggerThreshold && previousTriggerValue <= triggerThreshold)
            {
                CastSpell();
            }

            previousTriggerValue = triggerValue;
        }
        else
        {
            // Fallback: if not using the new Input System, use a key press (for example, the Space key)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CastSpell();
            }
        }
    }

    // The method that instantiates your Fireball prefab
    void CastSpell()
    {
        if (fireballPrefab != null && wandTip != null)
        {
            // Instantiate the Fireball prefab at the wand tip's position and with its rotation
            Instantiate(fireballPrefab, wandTip.position, wandTip.rotation);
        }
        else
        {
            Debug.LogWarning("Fireball prefab or Wand Tip reference is missing!");
        }
    }
}
