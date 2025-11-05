using LJ2025.Player;
using UnityEngine;

namespace LJ2025
{
    public interface IInteractable 
    {
        public bool IsInteractable();
        
        public string GetHintName();
        
        public string GetHintAction();
        
        public SphereCollider BoundingRadius();
        
        public bool Interact(Interactor interactor);

        public void AddOutline();

        public void RemoveOutline();

        public void Restart();
    }
}