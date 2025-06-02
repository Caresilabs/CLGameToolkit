using UnityEngine;

namespace CLGameToolkit.Player
{
    public class CameraMotionController : MonoBehaviour
    {
        public float AllMotionScale { get; set; } = 1f;
        public float ShakeScale { get; set; } = 1f;

        [Header("Movement Speed Settings")]
        [SerializeField] private Rigidbody Body;
        [SerializeField] private float MaxSpeed = 6f;

        [Header("Position Sway (Idle + Movement)")]
        [SerializeField] private float BaseSwaySpeed = 1f;
        [SerializeField] private float BaseSwayAmount = 0.01f;
        [SerializeField] private float MoveSwaySpeed = 2.5f;
        [SerializeField] private float MoveSwayAmount = 0.05f;
        [SerializeField] private float SwaySmooth = 5f;

        [Header("Rotation Sway (Idle + Movement Tilt)")]
        [SerializeField] private float BaseTiltAmount = 0.25f;
        [SerializeField] private float MoveTiltAmount = 2f;
        [SerializeField] private float TiltSpeed = 1.5f;

        [Header("Shake")]
        [SerializeField] private AnimationCurve ShakeCurve;
        [SerializeField] private float BaseShakeAmount = 0.2f;

        private Vector3 originalLocalPos;
        private Quaternion originalLocalRot;
        private float swayTimer;
        private float smoothedSpeed;

        private bool isShaking;
        private float shakeTimer;
        private float shakeSpeed = 15f;
        private float shakeDuration;
        private float shakeStrength;

        private void Awake()
        {
            originalLocalPos = transform.localPosition;
            originalLocalRot = transform.localRotation;
            swayTimer = Random.Range(0f, Mathf.PI * 2f); // Randomize idle start phase
        }


        public void Shake(float duration, float strength = 1f, float speed = 15)
        {
            isShaking = true;
            shakeTimer = 0f;

            shakeDuration = duration;
            shakeStrength = strength * ShakeScale;
            shakeSpeed = speed;
        }

        private void LateUpdate()
        {
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, Body.linearVelocity.magnitude, Time.deltaTime * SwaySmooth);

            Vector3 swayOffset = CalculateSway();
            Vector3 shakeOffset = CalculateShake();
            Quaternion tiltRotation = CalculateTilt();

            transform.SetLocalPositionAndRotation(
                originalLocalPos + swayOffset + shakeOffset,
                Quaternion.Slerp(transform.localRotation, tiltRotation, Time.deltaTime * SwaySmooth)
                );
        }

        private Vector3 CalculateSway()
        {
            float normalizedSpeed = Mathf.Clamp01(smoothedSpeed / MaxSpeed);

            float swayAmt = AllMotionScale * Mathf.Lerp(BaseSwayAmount, MoveSwayAmount, normalizedSpeed);
            float swaySpd = AllMotionScale * Mathf.Lerp(BaseSwaySpeed, MoveSwaySpeed, normalizedSpeed);

            swayTimer += Time.deltaTime * swaySpd;

            float swayX = Mathf.Sin(swayTimer) * swayAmt;
            float swayY = Mathf.Cos(swayTimer * 2f) * swayAmt * 0.5f;

            // Smooth Perlin noise for micro-drift
            float noiseTime = Time.time * .5f;
            float noiseX = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * swayAmt * 0.6f;
            float noiseY = (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * swayAmt * 0.6f;

            return new Vector3(swayX + noiseX, swayY + noiseY, 0f);
        }

        private Quaternion CalculateTilt()
        {
            float normalizedSpeed = Mathf.Clamp01(smoothedSpeed / MaxSpeed);
            float tiltAmount = AllMotionScale * Mathf.Lerp(BaseTiltAmount, MoveTiltAmount, normalizedSpeed);
            float time = Time.time * TiltSpeed;

            Vector3 localVelocity = transform.InverseTransformDirection(Body.linearVelocity);
            float roll = (Mathf.Sin(time) * tiltAmount * .3f) + -localVelocity.x * tiltAmount * .7f;
            float pitch = Mathf.Cos(time * 1.5f) * tiltAmount * 0.5f;

            Quaternion tilt = Quaternion.Euler(pitch, 0f, roll);
            return originalLocalRot * tilt;
        }

        private Vector3 CalculateShake()
        {
            if (!isShaking) return Vector3.zero;

            shakeTimer += Time.deltaTime;
            float progress = shakeTimer / shakeDuration;

            if (progress >= 1f)
            {
                isShaking = false;
                shakeTimer = 0f;
                return Vector3.zero;
            }

            float strength = ShakeCurve.Evaluate(progress) * BaseShakeAmount * shakeStrength;
            float noiseTime = Time.time * shakeSpeed;
            float noiseX = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * 2f;

            return new Vector3(noiseX, noiseY, 0f) * strength;
        }

    }
}
