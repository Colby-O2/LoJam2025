using UnityEngine;

namespace LJ2025
{
    public class PoliceSiren : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light _redLight;
        [SerializeField] private Light _blueLight;

        [Header("Settings")]
        [SerializeField] float _speed;

        private Vector3 _rot;

        private void Awake()
        {
            _rot = transform.localRotation.eulerAngles;
        }

        private void Update() 
        {
            _rot.y += _speed * Time.deltaTime;
            transform.localRotation = Quaternion.Euler( _rot);
        }
    }
}
