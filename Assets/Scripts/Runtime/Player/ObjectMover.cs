using System;
using System.Collections.Generic;
using System.Linq;
using LJ2025.MonoSystems;
using LJ2025;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025.Player
{
    class MoveableProfile
    {
        public Transform obj;
        public MoveableSettings settings;
        public List<Collider> colliders;
        public List<Rigidbody> rigs;
        public Quaternion rotationOffset;

        public MoveableProfile(Transform mover, Transform obj, MoveableSettings settings)
        {
            this.obj = obj;
            this.settings = settings;
            if (settings.ignoreChildren)
            {
                this.rigs = obj.GetComponents<Rigidbody>()
                    .ToList();
            }
            else
            {
                this.rigs = obj.GetComponentsInChildren<Rigidbody>(false)
                    .ToList();
            }
            this.colliders = obj.GetComponentsInChildren<Collider>(false)
                .Where(c => c.enabled)
                .Where(c => (c.excludeLayers & LayerMask.GetMask("KeepOn")) == 0)
                .ToList();
            this.rotationOffset = Quaternion.Inverse(mover.rotation) * obj.rotation;
        }
    }
    
    public class ObjectMover : MonoBehaviour
    {
        private MoveableProfile _profile = null;

        private Transform _head;

        private Vector3 _target;
        
        private bool _endedThisFrame = false;
        private bool _startedThisFrame = false;

        public bool IsMoving() => _profile != null;
        
        public void Move(Transform obj, MoveableSettings settings)
        {
            if (IsMoving() || _endedThisFrame) return;
            _startedThisFrame = true;
            _profile = new MoveableProfile(_head, obj, settings);
            _profile.colliders.ForEach(c => c.enabled = false);
            _profile.rigs.ForEach(c => c.isKinematic = true);
            _target = _profile.obj.position;
        }

        public void EndMove()
        {
            if (!IsMoving() || _startedThisFrame) return;
            _profile.colliders.ForEach(c => c.enabled = true);
            _profile.rigs.ForEach(c => c.isKinematic = false);
            _profile = null;
            _endedThisFrame = true;
        }

        private void Start()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractCallback.AddListener(HandleInteractPress);
            _head = GetComponent<Player.Controller>().Head();
        }

        private void HandleInteractPress()
        {
            EndMove();
        }

        private RaycastHit[] _hits = new RaycastHit[12];

        private void FixedUpdate()
        {
            if (!IsMoving()) return;
            float distance = _profile.settings.holdDistance;
            int size = Physics.SphereCastNonAlloc(_head.position,
                LJ2025GameManager.PlayerSettings.MoveObjectRadius,
                _head.forward,
                _hits,
                _profile.settings.holdDistance,
                ~LayerMask.GetMask("Player") ^ LayerMask.GetMask("PlayerBounds") ^ LayerMask.GetMask("Interactable"));

            RaycastHit? hit =
                _hits
                    .Cast<RaycastHit?>()
                    .Take(size)
                    .FirstOrDefault(h => h?.transform != _profile.obj);
            if (hit.HasValue)
            {
                distance = hit.Value.distance;
            }

            _target = _head.position + _head.forward * distance;
        }

        private void Update()
        {
            if (!IsMoving()) return;
            _profile.obj.transform.position = Vector3.Lerp(
                _profile.obj.position,
                _target,
                Time.deltaTime * LJ2025GameManager.PlayerSettings.MoveableObjectSpeed);

            switch (_profile.settings.rotateType)
            {
                case MoveableSettings.RotateType.JustYAxis:
                    _profile.obj.eulerAngles = _profile.obj.eulerAngles.SetY((_head.rotation * _profile.rotationOffset).eulerAngles.y);
                    break;
                case MoveableSettings.RotateType.Full:
                    _profile.obj.rotation = _head.rotation * _profile.rotationOffset;
                    break;
                default: break;
            }
        }

        private void LateUpdate()
        {
            _startedThisFrame = false;
            _endedThisFrame = false;
        }

        public Transform GetMovingObject()
        {
            return _profile?.obj;
        }
    }
}
