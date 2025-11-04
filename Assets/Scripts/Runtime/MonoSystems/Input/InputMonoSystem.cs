using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LJ2025.MonoSystems
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputMonoSystem : MonoBehaviour, IInputMonoSystem
    {
        [SerializeField] private PlayerInput _input;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _leftClickAction;
        private InputAction _rightClickAction;
        public UnityEvent<InputAction.CallbackContext> LeftClickAction { get; private set; }
        public UnityEvent<InputAction.CallbackContext> RightClickCallback { get; private set; }

        public Vector2 RawMovement { get; private set; }
        public Vector2 RawLook { get; private set; }

        private void HandleMoveAction(InputAction.CallbackContext e)
        {
            RawMovement = e.ReadValue<Vector2>();
        }

        private void HandleLookAction(InputAction.CallbackContext e)
        {
            RawLook = e.ReadValue<Vector2>();
        }

        private void HandleRightClickAction(InputAction.CallbackContext e)
        {
            RightClickCallback.Invoke(e);
        }

        private void HandleLeftClickAction(InputAction.CallbackContext e)
        {
            LeftClickAction.Invoke(e);
        }

        private void Awake()
        {
            if (!_input) _input = GetComponent<PlayerInput>();

            LeftClickAction             = new UnityEvent<InputAction.CallbackContext>();
            RightClickCallback          = new UnityEvent<InputAction.CallbackContext>();

            _moveAction                 = _input.actions["Move"];
            _lookAction                 = _input.actions["Look"];
            _leftClickAction            = _input.actions["LeftClick"];
            _rightClickAction           = _input.actions["RightClick"];

            _moveAction.performed       += HandleMoveAction;
            _lookAction.performed       += HandleLookAction;
            _leftClickAction.performed  += HandleLeftClickAction;
            _rightClickAction.performed += HandleRightClickAction;
            _leftClickAction.canceled   += HandleLeftClickAction;
            _rightClickAction.canceled  += HandleRightClickAction;
        }

        private void OnDestroy()
        {
            _moveAction.performed       -= HandleMoveAction;
            _lookAction.performed       -= HandleLookAction;
            _leftClickAction.performed  -= HandleLeftClickAction;
            _rightClickAction.performed -= HandleRightClickAction;
            _leftClickAction.canceled   -= HandleLeftClickAction;
            _rightClickAction.canceled  -= HandleRightClickAction;
        }
    }
}
