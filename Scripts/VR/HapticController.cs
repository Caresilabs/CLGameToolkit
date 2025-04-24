using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Haptics;

#if XR_INPUT_DEVICES_AVAILABLE
    using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace CLGameToolkit.Input
{
    public class HapticController
    {
        public static float HapticsScale = 1f;
        private static Dictionary<IHaptics, Tween> gamepadTweens = new();

#if XR_INPUT_DEVICES_AVAILABLE
        private static XRBaseController left;
        private static XRBaseController right;

        public static void Init(XRBaseController left, XRBaseController right, float strengthScale = 1f)
        {
            HapticController.left = left;
            HapticController.right = right;

            HapticsScale = strengthScale;
        }
#endif

        public static void PlayHaptics(float strength = 0.2f, float duration = 0.5f, InputDevice device = null)
        {
            float strengthNormalized = Mathf.Clamp01(strength * HapticsScale);

#if XR_INPUT_DEVICES_AVAILABLE
            if (left != null)
            {
                left.SendHapticImpulse(strengthNormalized, duration);
                right.SendHapticImpulse(strengthNormalized, duration);
                return;
            }
#endif

            if ((device ?? Gamepad.current) is IDualMotorRumble gamepad)
            {
                // TODO: Support dual motor config
                float motorPower = strengthNormalized * 0.5f;
                gamepad.SetMotorSpeeds(motorPower, motorPower);
                gamepadTweens.GetValueOrDefault(gamepad)?.Kill(false);
                gamepadTweens[gamepad] = DOVirtual.DelayedCall(duration, gamepad.ResetHaptics);
            }
        }

        public static void StopAll()
        {
#if XR_INPUT_DEVICES_AVAILABLE
            if (left != null)
            {
                left.SendHapticImpulse(0, 0);
                right.SendHapticImpulse(0, 0);
            }
#endif

            StopGamepad();
        }

        public static void StopGamepad()
        {
            Gamepad gamepad = Gamepad.current;
            gamepad?.ResetHaptics();
        }
    }
}
