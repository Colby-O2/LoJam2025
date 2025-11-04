using PlazmaGames.Core.MonoSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LJ2025.MonoSystems
{
    public interface IInputMonoSystem : IMonoSystem
    {
        public UnityEvent<InputAction.CallbackContext> LeftClickAction { get; }
        public UnityEvent<InputAction.CallbackContext> RightClickCallback { get; }
        public Vector2 RawMovement { get; }
        public Vector2 RawLook { get; }
    }
}
