using System.Collections.Generic;
using System.Linq;
using PlazmaGames.Attribute;
using UnityEngine;

namespace LJ2025
{
    public struct None{}
    
    public enum PlayerAnimationState
    {
        Idle,
        Walking,
        Driving,
        Sitting,
    }

    public class PlayerAnimationController : MonoBehaviour
    {
        class Rig
        {
            private List<Transform> _locations = new();
            
            public List<Transform> Locations() => _locations;
            
            public Rig(Transform root)
            {
                _locations.Add(root);
                _locations.AddRange(root.FindAllWithTag("RigPart"));
            }
            
            public void Lerp(RigState from, RigState to, float t)
            {
                for (int i = 0; i < _locations.Count; i++)
                {
                    MathExt.Transform me = from.GetLocation(i);
                    MathExt.Transform you = to.GetLocation(i);
                    Transform apply = _locations[i];
                    MathExt.Transform.Slerp(me, you, t)
                        .ApplyToLocal(apply);
                }
            }
        }
            
        class RigState
        {
            private List<MathExt.Transform> _locations = new();

            public MathExt.Transform GetLocation(int loc) => _locations[loc];

            public RigState(Transform root)
            {
                _locations.Add(root);
                _locations.AddRange(
                    root.FindAllWithTag("RigPart")
                        .Select(MathExt.Transform.FromLocal));
            }
            
            public RigState(Rig root)
            {
                _locations.AddRange(root.Locations().Select(MathExt.Transform.FromLocal));
            }
        }

        private Rig _myRig;
        private RigState _fromRigState;
        private RigState _toRigState;
        private Promise<None> _sitPromise;

        [SerializeField] private GameObject _sittingRig;
        private RigState _sittingRigState;
        [SerializeField] private GameObject _sittingSideRig;
        private RigState _sittingSideRigState;
        
        [SerializeField] private LayerMask _walkableLayerMask;

        [SerializeField] private PlayerAnimationState _state;
        
        [Header("IK Bones")]
        [SerializeField] private Transform _lHand;
        [SerializeField] private Transform _rHand;
        [SerializeField] private Transform _lFoot;
        [SerializeField] private Transform _rFoot;

        [Header("Walking Settings")]
        [SerializeField] private float _stepLength = 0.2f;
        [SerializeField] private float _stepHeight = 0.1f;
        [SerializeField] private float _armSwingAmount = 0.15f;
        [SerializeField] private float _armSwingHeight = 0.05f;
        [SerializeField] private float _stepSpeed = 2f;
        [SerializeField] private float _footRaycastHeight = 1f;
        [SerializeField] private float _footGroundOffset = 0.02f;

        [Header("Idle Settings")]
        [SerializeField] private float _idleLerpSpeed = 5f;

        [Header("Crouch")]
        [SerializeField] private float _crouchHeight = 0.5f;

        [Header("Driving")]
        [SerializeField] private Transform _leftGrip;
        [SerializeField] private Transform _rightGrip;
        [SerializeField] private Transform _wheelCenter;
        [SerializeField] private float _seatHeight = -1f;
        [SerializeField] private float _offsetInSeat = -0.2f;
        [SerializeField] private float _feetOffset = 0;

        private Vector3 _leftFootStartPos, _rightFootStartPos;
        private Vector3 _leftHandStartPos, _rightHandStartPos;

        [SerializeField, ReadOnly] private bool _isCrouching;
        [SerializeField, ReadOnly] private float _timer;

        public void SetCrouchState(bool isCrouching)
        {
            _isCrouching = isCrouching;
        }

        public void SetAnimationState(PlayerAnimationState state)
        {
            _state = state;
            if (state != PlayerAnimationState.Walking)
            {
                _timer = 0f;
            }
        }

        private void SetFootPosition(Transform footTransform, Vector3 localTargetPos)
        {
            if (!footTransform) return;

            if (_feetOffset != 0) localTargetPos = localTargetPos.SetZ(_feetOffset);

            Vector3 worldTarget = footTransform.parent.TransformPoint(localTargetPos);

            Vector3 rayStart = worldTarget + Vector3.up * _footRaycastHeight;
            Debug.DrawLine(rayStart, rayStart  + _footRaycastHeight * 2f * Vector3.down, Color.red, 1f);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, _footRaycastHeight * 2f, _walkableLayerMask))
            {
                Debug.Log("Here!");
                worldTarget.y = hit.point.y + _footGroundOffset;
            }

            footTransform.localPosition = footTransform.parent.InverseTransformPoint(worldTarget);
        }

        private void GripWheel(Transform handTransform, Transform gripTransform, Transform center)
        { 
            if (!handTransform || !gripTransform || !center) return;

            handTransform.position = gripTransform.position;
        }

        private void Walking()
        {
            _timer += Time.deltaTime * _stepSpeed;

            Vector3 leftFootOffset = new Vector3(0, Mathf.Max(0, Mathf.Sin(_timer)) * _stepHeight, Mathf.Sin(_timer) * _stepLength);
            Vector3 rightFootOffset = new Vector3(0, Mathf.Max(0, Mathf.Sin(_timer + Mathf.PI)) * _stepHeight, Mathf.Sin(_timer + Mathf.PI) * _stepLength);
            Vector3 leftHandOffset = new Vector3(0, Mathf.Sin(_timer + Mathf.PI) * _armSwingHeight, Mathf.Sin(_timer + Mathf.PI) * _armSwingAmount);
            Vector3 rightHandOffset = new Vector3(0, Mathf.Sin(_timer) * _armSwingHeight, Mathf.Sin(_timer) * _armSwingAmount);

            SetFootPosition(_lFoot, _leftFootStartPos + leftFootOffset);
            SetFootPosition(_rFoot, _rightFootStartPos + rightFootOffset);

            if (_lHand) _lHand.localPosition = _leftHandStartPos + leftHandOffset;
            if (_rHand) _rHand.localPosition = _rightHandStartPos + rightHandOffset;
        }

        private void ReturnToIdle()
        {
            _lFoot.localPosition = Vector3.Lerp(_lFoot.localPosition, _leftFootStartPos, Time.deltaTime * _idleLerpSpeed);
            _rFoot.localPosition = Vector3.Lerp(_rFoot.localPosition, _rightFootStartPos, Time.deltaTime * _idleLerpSpeed);
            _lHand.localPosition = Vector3.Lerp(_lHand.localPosition, _leftHandStartPos, Time.deltaTime * _idleLerpSpeed);
            _rHand.localPosition = Vector3.Lerp(_rHand.localPosition, _rightHandStartPos, Time.deltaTime * _idleLerpSpeed);

            SetFootPosition(_lFoot, _lFoot.localPosition);
            SetFootPosition(_rFoot, _rFoot.localPosition);
        }

        private void Crouch()
        {
            //transform.localPosition = new Vector3(transform.localPosition.x, (_isCrouching) ? -_crouchHeight : 0f, transform.localPosition.z);
        }
        
        private void SitDown()
        {
            if (_timer >= 1) return;
            _timer += Time.deltaTime / LJ2025GameManager.PlayerSettings.SitTime;
            _myRig.Lerp(_fromRigState, _toRigState, _timer);
            if (_timer >= 1)
            {
                Promise<None> tmp = _sitPromise;
                _sitPromise = null;
                tmp?.Resolve();
            }
        }

        void Awake()
        {
            _myRig = new Rig(transform);
            _sittingRigState = new RigState(_sittingRig.transform);
            _sittingSideRigState = new RigState(_sittingSideRig.transform);
            if (_lFoot) _leftFootStartPos = _lFoot.localPosition;
            if (_rFoot) _rightFootStartPos = _rFoot.localPosition;
            if (_lHand) _leftHandStartPos = _lHand.localPosition;
            if (_rHand) _rightHandStartPos = _rHand.localPosition;

            _timer = 0f;
        }

        public Promise<None> Sit()
        {
            _fromRigState = new RigState(_myRig);
            _toRigState = _sittingRigState;
            _timer = 0;
            _sitPromise = new Promise<None>();
            _state = PlayerAnimationState.Sitting;
            return _sitPromise;
        }
        
        public Promise<None> SitSide()
        {
            _fromRigState = new RigState(_myRig);
            _toRigState = _sittingSideRigState;
            _timer = 0;
            _sitPromise = new Promise<None>();
            _state = PlayerAnimationState.Sitting;
            return _sitPromise;
        }
        
        public Promise<None> Unsit()
        {
            _toRigState = _fromRigState;
            _fromRigState = new RigState(_myRig);
            _timer = 0;
            _sitPromise = new Promise<None>();
            _state = PlayerAnimationState.Sitting;
            return _sitPromise;
        }

        private void Update()
        {
            switch (_state)
            {
                case PlayerAnimationState.Walking:
                    Crouch();
                    Walking();
                    break;
                case PlayerAnimationState.Idle:
                    Crouch();
                    ReturnToIdle();
                    break;
                case PlayerAnimationState.Sitting:
                    SitDown();
                    break;
                case PlayerAnimationState.Driving:
                {
                    if (_leftGrip && _rightGrip)
                    {
                        GripWheel(_lHand, _leftGrip, _wheelCenter);
                        GripWheel(_rHand, _rightGrip, _wheelCenter);

                        Vector3 palmDirR = (_wheelCenter.position - _rHand.position).normalized;
                        Vector3 forwardR = transform.forward;
                        Vector3 upR = Vector3.Cross(palmDirR, forwardR);

                        Quaternion rightHandRot = Quaternion.LookRotation(forwardR, upR);

                        Vector3 palmDirL = (_wheelCenter.position - _lHand.position).normalized;
                        Vector3 forwardL = transform.forward;
                        Vector3 upL = Vector3.Cross(palmDirL, forwardL);

                        Quaternion leftHandRot = Quaternion.LookRotation(forwardL, upL);

                        _lHand.rotation = leftHandRot;
                        _rHand.rotation = leftHandRot;
                    }

                    transform.localPosition = new Vector3(transform.localPosition.x, _seatHeight, _offsetInSeat);

                    SetFootPosition(_lFoot, _lFoot.localPosition);
                    SetFootPosition(_rFoot, _rFoot.localPosition);
                    break;
                }
            }
        }

    }
}
