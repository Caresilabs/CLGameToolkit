#if ENABLE_VR
using UnityEngine.XR.Interaction.Toolkit;

public class HapticController
{
    private static XRBaseController left;
    private static XRBaseController right;

    public static void Init(XRBaseController left, XRBaseController right)
    {
        HapticController.left = left;
        HapticController.right = right;
    }

    public static void SendHaptics(float strength = 0.2f, float duration = 0.5f)
    {
        left.SendHapticImpulse(strength, duration);
        right.SendHapticImpulse(strength, duration);
    }


}

#endif
