using LJ2025.UI;
using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LJ2025
{
    public class Globe : MonoBehaviour
    {
        [Header("Refernces")]
        [SerializeField] private Transform _globe;
        [SerializeField] private float _sensitivity;

        [Header("Input")]
        [SerializeField] private InputAction _clickAction;

        [SerializeField, ReadOnly] private bool _isRotating;
        [SerializeField, ReadOnly] private Vector2 _startMousePos;

        private void OnClickPerformed(InputAction.CallbackContext ctx)
        {
            _startMousePos = Mouse.current.position.ReadValue();
            _isRotating = VirtualCaster.Instance.Raycast(_startMousePos, out RaycastHit _);
        }

        private void OnClickCancelPerformed(InputAction.CallbackContext ctx)
        {
             _isRotating = false;
        }

        private void OnEnable()
        {
            _clickAction.Enable();
            _clickAction.performed += OnClickPerformed;
            _clickAction.canceled += OnClickCancelPerformed;
        }

        private void OnDisable()
        {
            _clickAction.performed -= OnClickPerformed;
            _clickAction.canceled -= OnClickCancelPerformed;
            _clickAction.Disable();
        }

        private void Update()
        {
            if (_isRotating)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector2 diff = mousePos - _startMousePos;
                _globe.Rotate(Vector3.forward, diff.x * _sensitivity, Space.Self);
                _startMousePos = mousePos;
            }
        }
    }
}
