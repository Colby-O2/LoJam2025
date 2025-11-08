using LJ2025.UI;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LJ2025.MonoSystems
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputMonoSystem : MonoBehaviour, IInputMonoSystem
    {
        [SerializeField] private PlayerInput _input;

        private IUIMonoSystem _ui;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _interactAction;
        public UnityEvent JumpAction { get; private set; }
        public UnityEvent InteractCallback { get; private set; }

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

        private void HandleInteractAction(InputAction.CallbackContext e)
        {
            InteractCallback.Invoke();
        }

        private void HandleJumpAction(InputAction.CallbackContext e)
        {
            JumpAction.Invoke();
        }

        private void Awake()
        {
            if (!_input) _input = GetComponent<PlayerInput>();

            JumpAction       = new UnityEvent();
            InteractCallback = new UnityEvent();

            _moveAction       = _input.actions["Move"];
            _lookAction       = _input.actions["Look"];
            _jumpAction       = _input.actions["Jump"];
            _interactAction   = _input.actions["Interact"];

            _moveAction.performed       += HandleMoveAction;
            _lookAction.performed       += HandleLookAction;
            _jumpAction.performed       += HandleJumpAction;
            _interactAction.performed   += HandleInteractAction;
        }

        private void Start()
        {
            _ui = LJ2025GameManager.GetMonoSystem<IUIMonoSystem>();
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_ui.GetCurrentViewIs<GameView>())
                {
                    _ui.Show<PauseView>();
                }
                else if (_ui.GetCurrentViewIs<InspectorView>())
                {
                    LJ2025GameManager.Inspector.StopInspecting();
                }
                else if (_ui.GetCurrentViewIs<PauseView>())
                {
                    _ui.GetView<PauseView>().Resume();
                }
                else if (_ui.GetCurrentViewIs<SettingsView>())
                {
                    _ui.GetView<SettingsView>().Back();
                }
            }
        }

        private void OnDestroy()
        {
            _moveAction.performed       -= HandleMoveAction;
            _lookAction.performed       -= HandleLookAction;
            _jumpAction.performed       -= HandleJumpAction;
            _interactAction.performed   -= HandleInteractAction;
        }
    }
}
