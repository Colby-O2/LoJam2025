using LJ2025.UI;
using PlazmaGames.Core.MonoSystem;
using UnityEngine;
using UnityEngine.Events;

namespace LJ2025.MonoSystems
{
   public interface ITaskMonoSystem : IMonoSystem
    {
        public void StartTask(string msg, int maxCount = -1, UnityAction onTaskCompelete = null);
        public void UpdateTask(bool preventAutoEnding = false);
        public void EndTask();
    }
}
