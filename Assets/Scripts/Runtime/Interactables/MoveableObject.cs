using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class MoveableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private MoveableSettings _settings;

        [SerializeField] private bool _enabled = true;
        [SerializeField] private bool _detachFromParent = false;
        public void SetEnabled(bool val) => _enabled = val;

        public bool IsInteractable() => _enabled;
        public string GetHintName() => _settings.name;
        public string GetHintAction() => "Move";
        public SphereCollider BoundingRadius() => _settings.bounds;
        
        public bool Interact(Interactor interactor)
        {
            if (interactor.TryGetComponent(out Player.ObjectMover mover))
            {
                if (_detachFromParent) transform.parent = null;
                mover.Move(transform, _settings);
            }
            return true;
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
    }
}
