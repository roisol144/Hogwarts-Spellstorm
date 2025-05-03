using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticController : MonoBehaviour
{
    public static void SendHaptics(XRBaseController controller, float intensity, float duration)
    {
        controller.SendHapticImpulse(intensity, duration);
    }

}
