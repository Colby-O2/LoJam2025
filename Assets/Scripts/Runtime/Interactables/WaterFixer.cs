using LJ2025.Player;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class WaterFixer : MonoBehaviour, IInteractable
    {
        [SerializeField] private SphereCollider _bounds;

        [SerializeField, ReadOnly] private bool _isInteractable = false;

        public void SetInteractable(bool state)
        {
            _isInteractable = state;
        }

        public bool IsInteractable() => _isInteractable;

        public string GetHintName() => "Water Valve";

        public string GetHintAction() => "Increase Temperature";

        public SphereCollider BoundingRadius() => _bounds;
        
        public bool Interact(Interactor interactor)
        {
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent("WaterFixed", transform);
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
