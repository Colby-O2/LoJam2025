using UnityEngine;
using PlazmaGames.Core;

namespace LJ2025
{
    public class HomeFrontDoor : TwoWayDoor
    {
        private IGameLogicMonoSystem _gameLogic;
        private IDialogueMonoSystem _dialogueMs;
        
        protected override void Start()
        {
            base.Start();
            _gameLogic = GameManager.GetMonoSystem<IGameLogicMonoSystem>();
            _dialogueMs = GameManager.GetMonoSystem<IDialogueMonoSystem>();
            this.OnOpen.AddListener(() =>
            {
                _gameLogic.TriggerEvent("LeaveForWork", transform);
            });
        }
        
        public override bool IsLocked()
        {
            return _gameLogic.GameState() != GameState.LeaveForWork;
        }

        protected override void OpenedWhenLocked()
        {
            _dialogueMs.StartDialogue("NoReasonToLeave");
        }
    }
}
