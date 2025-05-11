using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class WandController : MonoBehaviour
{
    [Header("Wand Settings")]
    [SerializeField] private Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 gripPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 gripRotation = new Vector3(0f, 0f, 0f);
    [SerializeField] private Transform handController; // Reference to the hand controller

    private void Start()
    {
        if (handController == null)
        {
            Debug.LogError("Please assign the hand controller in the inspector!");
            return;
        }

        // Make the wand a child of the hand controller
        transform.SetParent(handController);
        
        // Apply the grip position and rotation
        transform.localPosition = gripPosition;
        transform.localRotation = Quaternion.Euler(gripRotation);
        
        // Apply the default scale
        transform.localScale = defaultScale;
    }

    // Call this method to adjust the wand's size
    public void AdjustWandSize(float scaleMultiplier)
    {
        defaultScale = Vector3.one * scaleMultiplier;
        transform.localScale = defaultScale;
    }

    // Call this method to adjust the grip position
    public void AdjustGripPosition(Vector3 newPosition)
    {
        gripPosition = newPosition;
        transform.localPosition = gripPosition;
    }

    // Call this method to adjust the grip rotation
    public void AdjustGripRotation(Vector3 newRotation)
    {
        gripRotation = newRotation;
        transform.localRotation = Quaternion.Euler(gripRotation);
    }
}

