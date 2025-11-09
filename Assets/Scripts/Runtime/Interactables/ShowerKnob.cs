using LJ2025.Player;
using PlazmaGames.Attribute;
using UnityEngine;

namespace LJ2025
{
    public class ShowerKnob : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool _isInteractable = false;
        [SerializeField] private SphereCollider _radius;
        [SerializeField] private ShowerController _controller;

        [SerializeField, ReadOnly] private bool _isOn = false;

        public void SetInteractable()
        {
            _isInteractable = true;
        }

        public void AddOutline()
        {

        }

        public SphereCollider BoundingRadius()
        {
            return _radius;
        }

        public string GetHintAction()
        {
            return $"Turn {(_isOn ? "Off" : "On")}";
        }

        public string GetHintName()
        {
            return "Shower";
        }

        public bool Interact(Interactor interactor)
        {
            _controller.Toggle();
            _isOn = !_isOn;
            return true;
        }

        public bool IsInteractable()
        {
            return _isInteractable;
        }

        public void RemoveOutline()
        {

        }

        public void Restart()
        {
            if (_isOn) _controller.Toggle();
        }
    }
}
