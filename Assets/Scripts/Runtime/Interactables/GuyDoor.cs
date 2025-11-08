using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class GuyDoor : TwoWayDoor
    {
        [SerializeField] private string _id;
        [SerializeField] private string _lockedDialogue;
        [SerializeField] private string _lockedAfterDialogue;

        private bool _knocked = false;
        
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
