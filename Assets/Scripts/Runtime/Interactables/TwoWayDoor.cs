using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LJ2025
{
    public class TwoWayDoor : MonoBehaviour, IInteractable, IResetState
    {
        
        [Header("References")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _center;

        [Header("Settings")]
        [SerializeField] private float _openSpeed = 1.5f;
        [SerializeField] private int _directionOverride = 0;
        [SerializeField] private bool _flipDirection = false;
        [SerializeField] private bool _interactable = true;

        public void SetOpenSpeed(float val) => _openSpeed = val;

        [Header("Dialogue")]
        [SerializeField, ReadOnly] private bool _hasOpenedBefore = false;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _lockedSound;

        [SerializeField, ReadOnly] private bool _isOpen = false;
        [SerializeField, ReadOnly] private bool _inProgress = false;
        [SerializeField, ReadOnly] private Quaternion _startRot;

        [Header("Locked Settings")]
        [SerializeField] private bool _isLocked = false;

        public UnityEvent OnOpen = new UnityEvent();
        [SerializeField] private SphereCollider _boundingRadius;

        private Promise _promise = null;

        public void SetDirectionOverride(int dir)
        {
            _directionOverride = dir;
        }

        private float CurrentAngle()
        {
            float angle = _pivot.localRotation.eulerAngles.y;
            angle %= 360;
            angle = angle > 180 ? angle - 360 : angle;
            return angle;
        }

        public virtual bool IsLocked() => _isLocked;

        protected virtual void OpenedWhenLocked()
        {
        }

        public virtual void Lock()
        {
            _isLocked = true;
        }

        public virtual void Unlock()
        {
            _isLocked = false;
        }

        public SphereCollider BoundingRadius() => _boundingRadius;

        public bool Interact(Player.Interactor interactor)
        {
            if (_isOpen) Close();
            else Open(interactor.transform);
            return true;
        }

        public string GetHintName() => "Door";
        public string GetHintAction() => "Open";

        public virtual Promise Open(Transform from, bool overrideAudio = false)
        {
            if (_isOpen)
            {
                Promise p = new();
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().Scheduler().Wait(0.1f).Then(_ => p.Resolve());
                return p;
            }
            if (_inProgress) return _promise;
            _promise = new Promise();

            if (IsLocked())
            {
                if (!overrideAudio && _audioSource && _lockedSound) _audioSource.PlayOneShot(_lockedSound);

                OpenedWhenLocked();

                Promise p = new();
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().Scheduler().Wait(0.1f).Then(_ => p.Resolve());
                return p;
            }

            if (!_hasOpenedBefore)
            {
                _hasOpenedBefore = true;
            }

            if (!overrideAudio && _audioSource && _openSound) _audioSource.PlayOneShot(_openSound);
            OnOpen.Invoke();

            _isOpen = true;
            _inProgress = true;
            float start = CurrentAngle();
            
            float target;
            if (_directionOverride < 0) target = -90;
            else if (_directionOverride > 0) target = 90;
            else
            {
                float mag = Vector3.Dot(
                    _center.forward,
                    (_center.position - from.position).normalized);
                float flip = _flipDirection ? -1 : 1;
                if (mag * flip < 0) target = -90;
                else target = 90;
            }
            
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _openSpeed,
                (float progress) =>
                {
                    _pivot.localRotation = Quaternion.Euler(0, start + (target - start) * progress, 0);
                },
                () =>
                {
                    _inProgress = false;
                    Promise tmp = _promise;
                    _promise = null;
                    tmp.Resolve();
                }
            );

            return _promise;
        }

        public void Restart()
        {
            Close(true, true);
            _hasOpenedBefore = false;
        }

        public void Close(bool overrideAudio = false, bool force = false)
        {
            if (force)
            {
                GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
                _isOpen = false;
                _inProgress = false;
                _pivot.localRotation = _startRot;
                if (!overrideAudio && _audioSource && _closeSound) _audioSource.PlayOneShot(_closeSound);
                return;
            }

            if (!_isOpen) return;
            if (_inProgress) return;

            if (!overrideAudio && _audioSource && _closeSound) _audioSource.PlayOneShot(_closeSound);

            _inProgress = true;
            _isOpen = false;
            float start = CurrentAngle();
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _openSpeed,
                (float progress) =>
                {
                    _pivot.localRotation = Quaternion.Euler(0, start - progress * start, 0);
                },
                () =>
                {
                    _inProgress = false;
                }
            );
        }

        public void EndInteraction()
        {
        }

        public void AddOutline()
        {
        }

        public void RemoveOutline()
        {
        }

        public bool IsInteractable() => _interactable;

        protected virtual void Start()
        {
            
        }

        protected virtual void Awake()
        {
            _hasOpenedBefore = false;
            _startRot = _pivot.localRotation;
        }

        private void OnDisable()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            _pivot.localRotation = Quaternion.Euler(0, 0, 0);
            _inProgress = false;
            _isOpen = false;
        }

        public void InitState()
        {
        }

        public void ResetState()
        {
            _isOpen = false;
            _inProgress = false;
            _isLocked = false;
            _pivot.localRotation = Quaternion.identity;
        }

        public Promise OpenThenClose(Transform from, float delay)
        {
            Promise p = Open(from);
            GameManager.GetMonoSystem<IGameLogicMonoSystem>()
                .Scheduler()
                .Wait(delay)
                .Then(_ =>
                {
                    Close();
                });

            return p;
        }

        public bool IsOpen() => _isOpen;
    }
}
