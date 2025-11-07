using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class Phone : MonoBehaviour, IInteractable
    {
        [SerializeField] private AudioSource _ringSource;
        [SerializeField] private SphereCollider _bounds;

        private Promise _promise = null;
        
        public Promise Ring()
        {
            _ringSource.Play();
            _promise = new Promise();
            return _promise;
        }

        public void StopRinging()
        {
            if (!IsRinging()) return;
            Promise tmp = _promise;
            _promise = null;
            tmp.Resolve();
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
                StopRinging();
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
