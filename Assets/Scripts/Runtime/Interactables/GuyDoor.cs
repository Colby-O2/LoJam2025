using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class GuyDoor : TwoWayDoor
    {
        [SerializeField] private string _id;
        [SerializeField] private string _lockedDialogue;
        [SerializeField] private string _lockedAfterDialogue;
        [SerializeField] private string _openDialogue;

        private bool _knocked = false;
        private bool _hasOpened = false;

        public override Promise Open(Transform from, bool overrideAudio = false)
        {
            if (GameManager.GetMonoSystem<IGameLogicMonoSystem>().GameState() != GameState.CheckOnGuests || _hasOpened || IsLocked())
            {
                return base.Open(from, overrideAudio);
            }
            else
            {
                _hasOpened = true;
                return GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialoguePromise(_openDialogue);
            }
        }
        
        protected override void OpenedWhenLocked()
        {
            base.OpenedWhenLocked();
            if (_knocked == false) GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialogue(_lockedDialogue);
            else GameManager.GetMonoSystem<IDialogueMonoSystem>().StartDialogue(_lockedAfterDialogue);
            _knocked = true;
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().SetInRange($"Door{_id}Knocked", true);
        }
    }
}
