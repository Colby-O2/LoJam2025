using UnityEngine;

namespace LJ2025
{
    public class Fan : MonoBehaviour
    {
        [SerializeField] private float _fanSpeed;
        [SerializeField] private Transform _blades;

        private void Update()
        {
            _blades.Rotate(Vector3.up, _fanSpeed * 100f * Time.deltaTime, Space.World);
        }
    }
}
