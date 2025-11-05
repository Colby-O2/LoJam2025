using LJ2025.Player;
using UnityEngine;

namespace LJ2025
{
    public class Chair : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _exitLocation;
        [SerializeField] private SphereCollider _boundingRadius;

        public Transform ExitLocation() => _exitLocation;
        public Transform TargetLocation() => _target;
        
        public string GetHintName() => "Chair";
        public string GetHintAction() => "Sit";
        public bool IsInteractable() => !LJ2025GameManager.Player.IsInChair(this);
        public SphereCollider BoundingRadius() => _boundingRadius;
        
        public bool Interact(Interactor interactor)
        {
            if (interactor.TryGetComponent(out Player.Controller player))
            {
                player.DetachHead(_target);
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
