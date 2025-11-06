using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace LJ2025
{
    public static class QuaternionExtension
    {
        public static Quaternion Slerp(Quaternion from, Quaternion to, float t, bool useShortestPath = true)
        {
            if (useShortestPath && Quaternion.Dot(from, to) < 0.0f)
            {
                to = new Quaternion(-to.x, -to.y, -to.z, -to.w);
            }

            return Quaternion.Slerp(from, to, t);
        }

        public static Quaternion Lerp(Quaternion from, Quaternion to, float t, bool useShortestPath = true)
        {
            if (useShortestPath && Quaternion.Dot(from, to) < 0.0f)
            {
                to = new Quaternion(-to.x, -to.y, -to.z, -to.w);
            }

            return Quaternion.Lerp(from, to, t);
        }
    }

    public class MoveAlongSpline : MonoBehaviour
    {
        public static bool IsTriggering = false;

        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private Animator _animator;
        [SerializeField] private int _splineIndex = 0;
        [SerializeField] private Vector2 _travelRange = new Vector2Int(0, 1);
        [SerializeField] private float _speed = 0.2f;
        [SerializeField] private bool _useSpeedUpRange;
        [SerializeField] private Vector2 _speedUpRange = new Vector2Int(0, 0);
        [SerializeField] private float _speedUpValue = 0.2f;
        [SerializeField] private bool _canTurn = true;
        [SerializeField] private bool _isLoop = false;
        [SerializeField] private float _turnSpeed = 3f;
        [SerializeField] private float _offsetRight = 0f;
        [SerializeField] private float _offsetUp= 0f;
        [SerializeField] private Vector3 _rotationOffset;

        [Range(0f, 1f)]
        [SerializeField] private float t;

        private bool _movingForward = true;
        [SerializeField] private bool _isWaiting = false;
        private bool _isTurning = false;
        private float _tolerance = 0.01f;
        private HashSet<int> _activatedKnotIndices = new HashSet<int>();
        
        public enum Direction { Forward, Backward }

        [System.Serializable]
        public class KnotWait
        {
            [Range(0f, 1f)] public float positionT;
            public float waitTime;
            public Direction direction;
            public Vector3 lookDirection;
        }

        [SerializeField] private List<KnotWait> _knotWaits = new List<KnotWait>();

        public bool IsMoving() => !_isWaiting;

        public void Stop()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            _isWaiting = true;
            MoveAlongSpline.IsTriggering = true;
        }

        public void Continue()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            _isWaiting = false;
            MoveAlongSpline.IsTriggering = false;
        }

        private void Update()
        {
            if (_animator && !LJ2025GameManager.IsPaused && !MoveAlongSpline.IsTriggering) _animator.SetBool("IsWalking", !_isWaiting || (_isWaiting && _isTurning));

            if (_splineContainer == null || _splineContainer.Splines.Count <= _splineIndex || _isWaiting || LJ2025GameManager.IsPaused || MoveAlongSpline.IsTriggering)
                return;

            float length = _splineContainer.Splines[_splineIndex].GetLength();
            float delta = (((_useSpeedUpRange && t >= _speedUpRange.x && t < _speedUpRange.y) ? _speedUpValue : _speed) / length) * Time.deltaTime;
            float previousT = t;

            t += (_movingForward ? delta : -delta);
            t = Mathf.Clamp01(t);

            for (int i = 0; i < _knotWaits.Count; i++)
            {
                var knot = _knotWaits[i];
                bool isInRange = Mathf.Abs(t - knot.positionT) < _tolerance;
                bool cameFromCorrectDir =
                    (_movingForward && knot.direction == Direction.Forward) ||
                    (!_movingForward && knot.direction == Direction.Backward);

                if (isInRange && cameFromCorrectDir && !_activatedKnotIndices.Contains(i))
                {
                    _activatedKnotIndices.Add(i);

                    _isWaiting = true;
                    return;
                    _isTurning = true;
                    Quaternion fromRot = transform.rotation;
                    Quaternion toRot = Quaternion.LookRotation(knot.lookDirection.normalized, transform.up);
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _canTurn ? _turnSpeed : 0f,
                        (float t) =>
                        {
                            if (_canTurn) transform.rotation = QuaternionExtension.Slerp(fromRot, toRot, t, true);
                        },
                        () =>
                        {
                            _isTurning = false;
                            
                            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                this,
                                knot.waitTime,
                                (float t) =>
                                {
                                },
                                () =>
                                {
                                    _splineContainer.Evaluate(_splineIndex, t, out float3 _, out float3 tangent, out float3 upVector);
                                    Vector3 resumeDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
                                    Quaternion reusmeStartRot = transform.rotation;
                                    Quaternion resumeRot = Quaternion.LookRotation(resumeDir, upVector);
                                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                        this,
                                        _canTurn ? _turnSpeed : 0f,
                                        (float t) =>
                                        {
                                            if (_canTurn) transform.rotation = QuaternionExtension.Slerp(reusmeStartRot, resumeRot, t, true);
                                        },
                                        () =>
                                        {
                                            _isWaiting = false;
                                        }
                                    );
                                }
                            );
                        }
                    );
                    return;
                }
            }

            if (t >= _travelRange.y || t <= _travelRange.x)
            {
                if (_isLoop)
                {
                    _movingForward = true;
                    t = 0f;
                    return;
                }

                _movingForward = t <= _travelRange.x;
                t = _movingForward ? _travelRange.x : _travelRange.y;

                _activatedKnotIndices.Clear();

                if (!_canTurn) return;

                _isWaiting = true;

                _splineContainer.Evaluate(_splineIndex, t, out float3 _, out float3 tangent, out float3 upVector);
                Quaternion fromRot = transform.rotation;
                Vector3 newDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
                Quaternion toRot = Quaternion.LookRotation(newDir, upVector);

                _isTurning = true;

                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                    this,
                    _turnSpeed,
                    (float t) =>
                    {
                        transform.rotation = QuaternionExtension.Slerp(fromRot, toRot, t, true);
                    },
                    () =>
                    {
                        GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                            this,
                            0.1f,
                            (float t) =>
                            {
                            },
                            () =>
                            {
                                _isTurning = false;
                                _isWaiting = false;
                            }
                        );
                    }
                );

                return;
            }

            UpdateTransformAlongSpline();
        }

        private void UpdateTransformAlongSpline()
        {
            _splineContainer.Evaluate(_splineIndex, t, out float3 position, out float3 tangent, out float3 upVector);
            float3 right = Vector3.Cross(Vector3.Normalize(tangent), upVector);

            Vector3 lookDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, upVector);

            transform.position = position + _offsetRight * right + _offsetUp * upVector;
            transform.rotation = targetRotation * Quaternion.Euler(_rotationOffset);
        }

        private void OnEnable()
        {
            Stop();
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}
