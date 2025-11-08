using System;
using LJ2025.MonoSystems;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEditor;
using UnityEngine;

namespace LJ2025.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Controller : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController _controller;
        [SerializeField] private AudioSource _as;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _feet;
        [SerializeField] private PlayerSettings _settings;

        [Header("Grounded Check")]
        [SerializeField] private LayerMask _groundedCheckIgnoreLayer;
        [SerializeField] private float _groundedCheckDst = 0.1f;
        [SerializeField] private float _groundedCheckRadius = 0.1f;

        [Header("Debug -- Movement")]
        [SerializeField, ReadOnly] private Vector3 _movementSpeed;
        [SerializeField, ReadOnly] private Vector3 _currentVel;
        [SerializeField, ReadOnly] private float _velY;

        enum DetachedHeadState
        {
            Attached,
            Entering,
            Looking,
            Returning,
        }
        
        [Header("Debug -- Detached Head")]
        [SerializeField, ReadOnly] private DetachedHeadState _detachedHeadState = DetachedHeadState.Attached;
        [SerializeField, ReadOnly] private Transform _detachedHeadTargetObject;
        [SerializeField, ReadOnly] private MathExt.Transform _detachedHeadTarget;
        [SerializeField, ReadOnly] private MathExt.Transform _detachedHeadFromTrans;
        [SerializeField, ReadOnly] private float _detachedHeadTransition = 0;
        [SerializeField, ReadOnly] private bool _lockHead = false;

        private Inspector _inspector;
        private ObjectMover _objectMover;

        private float gravity = -9.81f;

        private IInputMonoSystem _input;

        private Vector3 _groundPoint;
        [SerializeField] private AudioClip _indoorsClip;
        [SerializeField] private AudioClip _outdoorsClip;

        public Transform Head() => _head;

        public bool Occupied() => LJ2025GameManager.IsPaused || LJ2025GameManager.LockMovement || _inspector.IsInspecting() || _objectMover.IsMoving();
        
        public bool HasDetachedHead() => _detachedHeadState == DetachedHeadState.Looking;

        public void LockHead() => _lockHead = true;
        public void UnlockHead() => _lockHead = false;

        public void DetachHead(Transform target)
        {
            DetachHead(target, target);
        }
        
        public void DetachHead(MathExt.Transform target, Transform obj)
        {
            if (_detachedHeadState != DetachedHeadState.Attached) return;
            _detachedHeadTargetObject = obj;
            _detachedHeadTarget = target;
            _detachedHeadTransition = 0;
            _detachedHeadState = DetachedHeadState.Entering;
            _detachedHeadFromTrans = _head;
        }
        
        public void DetachHeadLookAt(Vector3 targetPos)
        {
            MathExt.Transform pos = _head;
            pos.rotation = Quaternion.LookRotation(targetPos - pos.position);
            DetachHead(pos, null);
        }
        
        public void AttachHead()
        {
            if (_detachedHeadState != DetachedHeadState.Looking) return;
            _detachedHeadTarget = _head;
            _detachedHeadState = DetachedHeadState.Returning;
            _detachedHeadTransition = 0;
            _detachedHeadTargetObject = null;
        }
        
        public void AttachHeadImmediately()
        {
            _detachedHeadState = DetachedHeadState.Attached;
            _detachedHeadFromTrans.ApplyTo(_head);
            _detachedHeadTargetObject = null;
        }


        private bool IsGrounded()
        {
            bool isGrounded = Physics.CheckSphere(_feet.position, _groundedCheckRadius, ~_groundedCheckIgnoreLayer);
            return isGrounded;
        }

        private void Jump()
        {
            if (_detachedHeadState != DetachedHeadState.Attached || Occupied()) return;
            if (IsGrounded()) _velY = _settings.JumpForce;
        }

        private void ProcessGravity()
        {
            bool isGrounded = IsGrounded();
            if (isGrounded && _velY < 0.0f) _velY = 0.0f;
            else _velY += _settings.GravityMultiplier * gravity * Time.deltaTime;

            _movementSpeed.y = _velY;
        }

        private void ProcessMovement()
        {
            float dirSpeed = (_input.RawMovement.y == 1) ? _settings.WalkingForwardSpeed : _settings.WalkingBackwardSpeed;
            float forwardSpeed = _input.RawMovement.y * _settings.Speed * dirSpeed;
            float rightSpeed = _input.RawMovement.x * _settings.Speed * _settings.WalkingStrideSpeed;

            _movementSpeed = Vector3.SmoothDamp(
                _movementSpeed,
                new Vector3(
                    rightSpeed,
                    0,
                    forwardSpeed),
                ref _currentVel,
                _settings.MovementSmoothing
            );
        }

        private void ProcessLook()
        {
            float x = _head.localEulerAngles.AngleAs180().x;
            x -= (_settings.InvertLookY ? -1 : 1) * _settings.Sensitivity.y * _input.RawLook.y;
            x = Mathf.Clamp(x , _settings.YLookLimit.x, _settings.YLookLimit.y);
            _head.localEulerAngles = _head.localEulerAngles.SetX(x);
            
            float y = transform.localEulerAngles.y;
            y += (_settings.InvertLookX ? -1 : 1) * _settings.Sensitivity.x * _input.RawLook.x;
            transform.localEulerAngles = transform.localEulerAngles.SetY(y);
        }
        
        private void ProcessDetachedHeadLook()
        {
            if (LJ2025GameManager.LockMovement) return;
            Vector3 rot = _head.localEulerAngles.AngleAs180();
            rot.x -= (_settings.InvertLookY ? -1 : 1) * _settings.Sensitivity.y * _input.RawLook.y;
            rot.x = Mathf.Clamp(rot.x , _settings.YLookLimit.x, _settings.YLookLimit.y);
            rot.y += (_settings.InvertLookX ? -1 : 1) * _settings.Sensitivity.x * _input.RawLook.x;

            _head.localEulerAngles = rot;
        }

        private void Awake()
        {
            LJ2025GameManager.PlayerSettings = _settings;
            _inspector = GetComponent<Inspector>();
            _objectMover = GetComponent<ObjectMover>();
            if (!_controller) _controller = GetComponent<CharacterController>();
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();

            _input.JumpAction.AddListener(Jump);
            _input.JumpAction.AddListener(() =>
            {
                if (_lockHead || LJ2025GameManager.IsPaused || LJ2025GameManager.LockMovement || _inspector.IsInspecting() || _objectMover.IsMoving()) return;
                AttachHead();
            });
        }

        private void Update()
        {
            if (LJ2025GameManager.IsPaused || _inspector.IsInspecting())
            {
                if (_as.isPlaying) _as.Stop();
                return;
            }

            if (_detachedHeadState != DetachedHeadState.Attached)
            {
                if (_as.isPlaying) _as.Stop();
                if (_detachedHeadState == DetachedHeadState.Looking)
                {
                    ProcessDetachedHeadLook();
                }
                else if (_detachedHeadState == DetachedHeadState.Entering)
                {
                    _detachedHeadTransition = MathExt.Clamp01(
                        _detachedHeadTransition +
                        Time.deltaTime * LJ2025GameManager.PlayerSettings.DetachedHeadTransitionTime);
                    MathExt.Transform.Slerp(_detachedHeadFromTrans, _detachedHeadTarget, _detachedHeadTransition)
                        .ApplyTo(_head);

                    if (_detachedHeadTransition >= 1) _detachedHeadState = DetachedHeadState.Looking;
                }
                else if (_detachedHeadState == DetachedHeadState.Returning)
                {
                    _detachedHeadTransition = MathExt.Clamp01(
                        _detachedHeadTransition +
                        Time.deltaTime * LJ2025GameManager.PlayerSettings.DetachedHeadTransitionTime);
                    MathExt.Transform.Slerp(_detachedHeadTarget, _detachedHeadFromTrans, _detachedHeadTransition)
                        .ApplyTo(_head);

                    if (_detachedHeadTransition >= 1) _detachedHeadState = DetachedHeadState.Attached;
                }
                return;
            }

            if (LJ2025GameManager.LockMovement)
            {
                if (_as.isPlaying) _as.Stop();
                return;
            }
            ProcessLook();
            ProcessMovement();
            ProcessGravity();

            Vector3 speed = _movementSpeed;
            speed.y = 0;
            if (speed.magnitude > 0.01f && IsGrounded())
            {
                if (_as.clip && !_as.isPlaying)
                {
                    _as.time = UnityEngine.Random.Range(0, _as.clip.length);
                    _as.Play();
                }
            }
            else
            {
                _as.Stop();
            }

            _controller.Move(transform.TransformDirection(_movementSpeed * Time.deltaTime));
        }

        public void TeleportToChair(Chair chair)
        {
            if (_detachedHeadState != DetachedHeadState.Attached)
            {
                AttachHeadImmediately();
            }
            Teleport(chair.ExitLocation());
            _detachedHeadState = DetachedHeadState.Looking;
            _detachedHeadFromTrans = _head;
            _detachedHeadTarget = chair.TargetLocation();
            _detachedHeadTarget.ApplyTo(_head);
            _detachedHeadTransition = 1;
        }

        public void Teleport(MathExt.Transform loc)
        {
            bool prevState = _controller.enabled;
            _controller.enabled = false;
            loc.ApplyTo(transform);
            _controller.enabled = prevState;
        }

        public bool IsInChair(Chair chair)
        {
            return _detachedHeadTargetObject == chair.TargetLocation() && _detachedHeadState == DetachedHeadState.Looking;
        }

        public Transform GetMouth()
        {
            return _head;
        }

        public Vector3 Velocity() => _controller.velocity;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.name);
        }

        public void SetIndoors(bool state)
        {
            if (state)
            {
                _as.clip = _indoorsClip;
            }
            else
            {
                _as.clip = _outdoorsClip;
            }
        }
    }
}
