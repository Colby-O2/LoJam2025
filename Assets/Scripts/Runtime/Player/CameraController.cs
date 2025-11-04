using LJ2025.MonoSystems;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LJ2025
{
    public class CameraLook : MonoBehaviour
    {
        [SerializeField] private float _sensitivity = 3f;
        [SerializeField] private Transform _playerBody;

        private float _pitch = 0f;

        private PlayerController _pc;

        private IInputMonoSystem _input;

        void Awake()
        {
            _pc = _playerBody.GetComponent<PlayerController>();
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();
        }

        void Update()
        {
            float mx = _input.RawLook.x * _sensitivity;
            float my = _input.RawLook.y * _sensitivity;

            _playerBody.Rotate(Vector3.up * mx);

            _pitch -= my;
            _pitch = Mathf.Clamp(_pitch, -80f, 80f);
            transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}