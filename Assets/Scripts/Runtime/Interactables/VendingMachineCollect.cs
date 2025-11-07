using System;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class VendingMachineCollect : MonoBehaviour
    {
        private VendingMachine _parent;

        private void Awake()
        {
            _parent = transform.parent.GetComponent<VendingMachine>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Snack")) return;
            _parent.AddItem();
            other.gameObject.SetActive(false);
            GameManager.GetMonoSystem<IGameLogicMonoSystem>().GetObjectMover().EndMove();
        }
    }
}
