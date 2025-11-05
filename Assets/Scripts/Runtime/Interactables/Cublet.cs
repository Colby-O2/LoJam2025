using UnityEngine;

namespace LJ2025
{

    public class Cubelet : MonoBehaviour
    {
        public int ix, iy, iz; 
        [HideInInspector] public Transform cubelet; 

        private void Awake()
        {
            cubelet = transform;
        }
    }
}
