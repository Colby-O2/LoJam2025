using System;
using UnityEngine;

namespace LJ2025
{
    public class Indoors : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Player.Controller p))
            {
                p.SetIndoors(true);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Player.Controller p))
            {
                p.SetIndoors(false);
            }
        }
    }
}