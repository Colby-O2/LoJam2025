using LJ2025.Player;
using LJ2025;
using NUnit.Framework.Internal.Commands;
using UnityEngine;

namespace LJ2025
{
    public class DoorOpen : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool _interactable = true;
        [SerializeField] private float _openSpeed = 1.6f;
        [SerializeField] private bool _startOpen = false;
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _pivotOpen;
        [SerializeField] private SphereCollider _boundingRadius;

        private MathExt.Transform _pivotOpenTrans;
        private MathExt.Transform _pivotClosedTrans;

        private bool _open = false;
        private float _t = 0;
        
        public bool IsOpen() => _open;
        
        public bool IsInteractable() => _interactable;
        public string GetHintName() => "Door";
        public string GetHintAction() => "Open";
        
        public SphereCollider BoundingRadius() => _boundingRadius;

        public bool Interact(Interactor interactor)
        {
            ToggleDoor();
            return true;
        }

        public void ToggleDoor()
        {
            if (_open) CloseDoor();
            else OpenDoor();
        }

        public void OpenDoor()
        {
            _open = true;
        }
        
        public void CloseDoor()
        {
            _open = false;
        }

        public void AddOutline()
        {
        }

        public void RemoveOutline()
        {
        }

        public void Restart()
        {
        }

        private void Awake()
        {
            _pivotOpenTrans = MathExt.Transform.FromLocal(_pivotOpen);
            _pivotClosedTrans = MathExt.Transform.FromLocal(_pivot);
        }

        private void Update()
        {
            if (_open && _t >= 1) return;
            if (!_open && _t <= 0) return;
            float dir = _open ? 1 : -1;
            _t = MathExt.Clamp01(_t + Time.deltaTime * _openSpeed * dir);
            MathExt.Transform.Slerp(_pivotClosedTrans, _pivotOpenTrans, _t)
                .ApplyToLocal(_pivot);
        }
    }
}
