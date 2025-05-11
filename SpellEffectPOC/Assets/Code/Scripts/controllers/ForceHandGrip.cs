using UnityEngine;

public class ForceHandGrip : MonoBehaviour
{
    public Animator handAnimator;

    void Start()
    {
        if (handAnimator != null)
            handAnimator.SetFloat("Grip", 1.0f);
    }
} 