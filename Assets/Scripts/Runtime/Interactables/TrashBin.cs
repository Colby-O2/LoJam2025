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
        [SerializeField] private string _tag;

        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _clip;

        [SerializeField, ReadOnly] private int _trashCount = 0;

        public bool TaskEnabledRoom = false;
        public int RequiredTrash => _requiredTrashCount;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_tag)) return;
            if (_as && _clip) _as.PlayOneShot(_clip);
            other.gameObject.SetActive(false);
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().GetObjectMover().EndMove();
            if (TaskEnabledRoom) GameManager.GetMonoSystem<ITaskMonoSystem>().UpdateTask();
            _trashCount += 1;
            if (_trashCount >= _requiredTrashCount)
            {
                GameManager.GetMonoSystem<IGameLogicMonoSystem>().TriggerEvent(_id, transform);
            }
        }
    }
}
