using UnityEngine;

namespace LJ2025.Player
{
    [CreateAssetMenu(fileName = "DefaultPlayerMovementSettings", menuName = "Player/MovementSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Movement")]
        public float Speed;
        public float WalkingForwardSpeed = 1f;
        public float WalkingBackwardSpeed = 0.5f;
        public float WalkingStrideSpeed = 1f;
        public float MovementSmoothing = 0.5f;

        [Header("Jumping")]
        public float GravityMultiplier = 1.0f;
        public float JumpForce = 1f;

        [Header("Crouching")]
        public float CrouchHeight = 1.2f;
        public float CrouchSpeedMul = 0.5f;

        [Header("Heading Bobing")]
        public bool EnableHeadMotion = true;
        [Range(0.001f, 0.01f)] public float HeadBobAmount;
        [Range(1f, 30f)] public float HeadBobFreqency;
        [Range(10f, 100f)] public float HeadBobSmoothing;

        [Header("Look")]
        public Vector2 Sensitivity = Vector2.one;
        public Vector2 YLookLimit;
        public bool InvertLookY = false;
        public bool InvertLookX = false;
        public float DetachedHeadTransitionTime = 1.5f;

        [Header("Interactions")] 
        public float MoveObjectRadius = 0.2f;
        public float MoveableObjectSpeed = 1.5f;
        public float ThrowForce = 10;
        public float DadThrowForce = 10;
        public float BallDamping = 0.2f;
        public float DadHoldBallDistance = 0.6f;
        public float DadThrowBallAngle = 30;
        public float DadHoldBallTime = 2;
        public float DadCatchDistance = 1;
    }
}
