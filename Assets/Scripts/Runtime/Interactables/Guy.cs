using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class Guy : MonoBehaviour, IInteractable
    {
        [SerializeField] private SphereCollider _bounds;
        [SerializeField] private string _name;
        [SerializeField] private string _eventName;

        private IGameLogicMonoSystem _logic;

        [SerializeField] private bool _isInteractable = true;

        public void SetIsInteractable(bool state)
        {
            _isInteractable = state;
        }

        private void Start()
        {
            _logic = GameManager.GetMonoSystem<IGameLogicMonoSystem>();
        }

        public bool IsInteractable()
        {
            return _isInteractable;
        }

        public string GetHintName() => _name;

        public string GetHintAction() => "Talk to";

        public SphereCollider BoundingRadius() => _bounds;

        public bool Interact(Interactor interactor)
        {
            if (_logic.GameState() == GameState.ServeGuest)
            {
                _logic.TriggerEvent($"{_eventName}Talk", transform);
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
