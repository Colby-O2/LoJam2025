using LJ2025.Player;
using UnityEngine;

namespace LJ2025
{
    public class SuppliesBox  : MonoBehaviour, IInteractable
    {
        [SerializeField] private MoveableSettings _settings;
        [SerializeField] private Transform _itemsContainer;

        private bool _hasMoved = false;
        
        public bool IsInteractable() => true;

        public string GetHintName() => "Vending Machine Stock";

        public string GetHintAction() => "Pickup";

        public SphereCollider BoundingRadius() => _settings.bounds;

        public bool Interact(Interactor interactor)
        {
            if (interactor.TryGetComponent(out Player.ObjectMover mover))
            {
                mover.Move(transform, _settings);

                if (!_hasMoved)
                {
                    _hasMoved = true;
                    foreach (Transform t in _itemsContainer)
                    {
                        t.GetComponent<MoveableObject>().SetEnabled(true);
                    }
                }
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
