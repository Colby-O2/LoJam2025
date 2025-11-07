using System;
using LJ2025.Player;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class TrashBin : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Trash")) return;
            other.gameObject.SetActive(false);
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().GetObjectMover().EndMove();
        }
    }
}
