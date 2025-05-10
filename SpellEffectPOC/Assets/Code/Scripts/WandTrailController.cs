using UnityEngine;
using UnityEngine.XR;

public class WandTrailController : MonoBehaviour
{
    public TrailRenderer wandTrail;

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool triggerValue;
        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue) && triggerValue)
        {
            if (!wandTrail.emitting)
                wandTrail.emitting = true;
        }
        else
        {
            if (wandTrail.emitting)
                wandTrail.emitting = false;
        }
    }
}