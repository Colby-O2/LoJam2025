using LJ2025.Player;
using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LJ2025
{
    public class BusDoor : MonoBehaviour, IInteractable, IResetState
    {
        [SerializeField] private SphereCollider _bounds;
        [SerializeField] private DoorOpen _leftDoor;
        [SerializeField] private DoorOpen _rightDoor;
        public bool IsInteractable() => true;

        public string GetHintName() => "Bus";

        public string GetHintAction() => "Leave";
        public SphereCollider BoundingRadius() => _bounds;

        public void Open()
        {
            _leftDoor.OpenDoor();
            _rightDoor.OpenDoor();
        }
        public bool Interact(Interactor interactor)
        {
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent("GoOnBus", transform);
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

        public void InitState()
        {
        }

        public void ResetState()
        {
        }
    }
}
