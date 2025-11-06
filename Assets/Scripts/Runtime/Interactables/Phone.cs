using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class Phone : MonoBehaviour, IInteractable
    {
        [SerializeField] private AudioSource _ringSource;
        [SerializeField] private SphereCollider _bounds;
        public void Ring()
        {
            _ringSource.Play();
        }

        public void StopRinging()
        {
            _ringSource.Stop();
        }

        public bool IsRinging() => _ringSource.isPlaying;

        public bool IsInteractable() => true;

        public string GetHintName() => "Phone";

        public string GetHintAction() => "Answer";

        public SphereCollider BoundingRadius() => _bounds;

        public bool Interact(Interactor interactor)
        {
            if (IsRinging())
            {
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent("AnswerHomePhone", transform);
            }
            else
            {
                GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialogue("NoOneToCall");
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
