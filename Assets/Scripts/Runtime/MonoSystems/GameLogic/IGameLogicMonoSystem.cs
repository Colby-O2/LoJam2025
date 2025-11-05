using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace LJ2025
{
    public interface IGameLogicMonoSystem : IMonoSystem
    {
        public int Act();
        public GameState GameState();
        
        Player.ObjectMover GetObjectMover();

        public void Begin();
        
        public int CurrentYear();
        public int CurrentDay();
        public int CurrentHour();
        public System.DateTime CurrentDate();
        public string CurrentTime();
        public void TriggerEvent(string eventName, Transform by);
        public void SetInRange(string id, bool state);
        public bool IsInRange(string id);
    }
}
