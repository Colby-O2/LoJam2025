using PlazmaGames.Core.MonoSystem;
using UnityEngine;
using UnityEngine.Events;

namespace LJ2025.MonoSystems
{
    public interface IInputMonoSystem : IMonoSystem
    {
        public UnityEvent JumpAction { get; }
        public UnityEvent InteractCallback { get; }
        public Vector2 RawMovement { get; }
        public Vector2 RawLook { get; }
    }
}
