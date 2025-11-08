using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class WaterFixer : MonoBehaviour, IInteractable
    {
        [SerializeField] private SphereCollider _bounds;

        public bool IsInteractable() => true;

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
