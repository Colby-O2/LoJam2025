using LJ2025.Player;
using UnityEngine;

namespace LJ2025
{
    public class MoveableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private MoveableSettings _settings;

        public bool IsInteractable() => true;
        public string GetHintName() => _settings.name;
        public string GetHintAction() => "Move";
        public SphereCollider BoundingRadius() => _settings.bounds;
        
        public bool Interact(Interactor interactor)
        {
            if (interactor.TryGetComponent(out Player.ObjectMover mover))
            {
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
