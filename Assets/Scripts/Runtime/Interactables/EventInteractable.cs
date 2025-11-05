using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class EventInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _event;
        [SerializeField] private string _name;
        [SerializeField] private string _action;
        [SerializeField] private SphereCollider _boundingRadius;
        
        public bool IsInteractable() => true;

        public string GetHintName() => _name;

        public string GetHintAction() => _action;

        public SphereCollider BoundingRadius() => _boundingRadius;

        public bool Interact(Interactor interactor)
        {
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent(_event, transform);
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
