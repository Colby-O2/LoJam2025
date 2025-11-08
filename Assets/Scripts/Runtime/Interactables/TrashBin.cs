using System;
using LJ2025.MonoSystems;
using LJ2025.Player;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class TrashBin : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private int _requiredTrashCount = 10;

        [SerializeField, ReadOnly] private int _trashCount = 0;

        public bool TaskEnabledRoom = false;
        public int RequiredTrash => _requiredTrashCount;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Trash") && !other.CompareTag("TrashBag")) return;
            other.gameObject.SetActive(false);
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().GetObjectMover().EndMove();
            if (other.CompareTag("Trash") && TaskEnabledRoom) GameManager.GetMonoSystem<ITaskMonoSystem>().UpdateTask();
            _trashCount += 1;
            if (_trashCount >= _requiredTrashCount)
            {
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent(_id, transform);
            }
        }
    }
}
