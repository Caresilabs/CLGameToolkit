#if ENABLE_VR
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HapticController
{
    private static XRBaseController left;
    private static XRBaseController right;

    private static float HapticsScale = 1.0f;
    private static Tween gamepadTween;

    public static void Init(XRBaseController left, XRBaseController right, float strengthScale)
    {
        HapticController.left = left;
        HapticController.right = right;

        HapticsScale = strengthScale;
    }

    public static void SendHaptics(float strength = 0.2f, float duration = 0.5f)
    {
        float strengthNormalized = Mathf.Clamp01(strength * HapticsScale);

        if (left != null)
        {
            left.SendHapticImpulse(strengthNormalized, duration);
            right.SendHapticImpulse(strengthNormalized, duration);
            return;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float motorPower = strengthNormalized * 0.5f;
            gamepad.SetMotorSpeeds(motorPower, motorPower); // TODO test out the rumble
            gamepadTween?.Kill(false);
            gamepadTween = DOVirtual.DelayedCall(duration, StopGamepad);
        }
    }

    public static void StopAll()
    {
        if (left != null)
        {
            left.SendHapticImpulse(0, 0);
            right.SendHapticImpulse(0, 0);
        }

        StopGamepad();
    }

    public static void StopGamepad()
    {
        var gamepad = Gamepad.current;
        gamepad?.ResetHaptics();
    }
}

#endif
