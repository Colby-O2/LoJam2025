using LJ2025.UI;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LJ2025.MonoSystems
{
    public class TaskMonoSystem : MonoBehaviour, ITaskMonoSystem
    {
        [SerializeField, ReadOnly] private bool _hasTask;
        [SerializeField, ReadOnly] private string _taskMsg;
        [SerializeField, ReadOnly] private int _maxCount;
        [SerializeField, ReadOnly] private int _count;

        private UnityAction _onTaskCompelete;
        private GameView _gameView;

        private string GetTaskString()
        {
            return (_maxCount > 0) ? $"{_taskMsg} {_count}/{_maxCount}" : _taskMsg;
        }

        public void StartTask(string msg, int maxCount = -1, UnityAction onTaskCompelete = null)
        {
            _hasTask = true;
            _maxCount = maxCount;
            _taskMsg = msg;
            _count = 0;
            _onTaskCompelete = onTaskCompelete;
            _gameView.ShowTask(GetTaskString());
        }

        public void UpdateTask(bool preventAutoEnding = false)
        {
            _count++;
            _gameView.UpdateTask(GetTaskString());
            if (!preventAutoEnding && _maxCount <= _count) EndTask();
        }

        public void EndTask()
        {
            _gameView.HideTask();
            _onTaskCompelete?.Invoke();
            _hasTask = false;
            _maxCount = -1;
            _count = 0;
            _taskMsg = string.Empty;
            _onTaskCompelete = null;
        }

        private void Start()
        {
            _gameView = GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>();
        }
    }
}
