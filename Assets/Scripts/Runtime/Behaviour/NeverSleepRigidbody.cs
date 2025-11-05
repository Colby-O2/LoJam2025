using UnityEngine;

namespace LJ2025
{
    public class NeverSleepRigidbody : MonoBehaviour
    {
        void Awake()
        {
            Rigidbody rig = GetComponent<Rigidbody>();
            if (rig != null) rig.sleepThreshold = 0;
        }

    }
}
