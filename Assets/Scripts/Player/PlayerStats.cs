using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        public float speed = 8;
        public float bufferJumpTime = 0.15f;
        public float coyoteTime = 0.1f;
        public float runSpeedMultiplier = 1.75f;
        public float accelTime = 0.08f;
        public float accelTimeOnAir = 0.3f;
        public float decelTime = 0.08f;
        public float decelTimeOnAir = 0.3f;

        [Header("Jump & Gravity")]

        public float gravity = -35f;

        public float jumpHeight = 2.5f;
        public float groundedGravity = -2f;

        [Header("Shooting")]
        public float shotStartupTime = 0.05f;
        public LayerMask portalAbleLayer;
        public float recoilTime = 0.2f;
        public int bulletCount;

        [Header("Camera Look")]
        [Range(0f, 1f)]
        public float sensitivity = 0.0345f;

        public bool invertPitch = false;
        public float runFov = 70f;
        public float smoothFovTime = 0.12f;

        [Header("Bob")]
        public float bobYAmount = 0.1f;
        public float bobXAmount = 0.025f;
        public float bobSmoothTime = 0.1f;
        public float bobYFrequency = 12f;
        public float bobXFrequency = 5f;

        [Header("Tuning")] 
        public float sprintSpeedThresholdFactor = 0.5f;
        public float movingSpeedThresholdFactor = 0.5f;
        public float sprintBobSpeedMultiplier = 1.25f;

        [Header("Leaning")] 
        public float maxLeanRollDegrees = 5f;
        public float maxLeanPitchDegrees = 3f;
        public float leanSmoothTime = 0.1f;
        public float maxMouseLeanRollDegrees = 2f;
        public float maxMouseLeanPitchDegrees = 1f;
        public float mouseLeanTime = 0.2f;
        public float verticalLeanMultiplier = 0.7f;
        public float landingLeanMaxDegrees = 10f;
        public float landingLeanVelocityScale = 2f;
        public float landingLeanSmoothTime = 0.2f;
        public float jumpLeanImpulseDegrees = 2f;

        [Header("Recoil")]
        public Vector3 recoilPositionKickback = new Vector3(0f, 0f, -0.1f);
        public Vector3 recoilRotationKickback = new Vector3(-5f, 0f, 0f);
        public float recoilVerticalRotation = 3f;
        public float recoilSnapTime = 0.05f;
        public float recoilReturnTime = 0.2f;

        [Header("Ground stats")]
        public float groundCheckExtraDistance = 0.05f;
        public float groundCheckRadius = 0.25f;
    }
}