using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class MasterKey : MonoBehaviour, IInteractable
    {
        [SerializeField] private SphereCollider _bounds;

        public bool IsInteractable() => true;

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
