using LJ2025.Player;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class MasterKey : MonoBehaviour, IInteractable
    {
        [SerializeField] private SphereCollider _bounds;

        [SerializeField, ReadOnly] private bool _isInteractable = true;

        public void SetIsInteractable(bool state)
        {
            _isInteractable = state;
        }

        public bool IsInteractable() => _isInteractable;

        public string GetHintName() => "Master Key";

        public string GetHintAction() => "Take";

        public SphereCollider BoundingRadius() => _bounds;

        public bool Interact(Interactor interactor)
        {
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().SetInRange("MasterKey", true);
            gameObject.SetActive(false);
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
