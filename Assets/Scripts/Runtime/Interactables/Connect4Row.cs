using UnityEngine;

namespace LJ2025
{
    public class Connect4Row : MonoBehaviour, IInspectorButton
    {
        [SerializeField] private Connect4Controller _controller;
        [SerializeField] private int _column;
        public void Click()
        {
            if (_controller) _controller.Move(_column);
        }

        private void Awake()
        {
            if (!_controller) _controller.GetComponentInParent<Connect4Controller>(); 
        }
    }
}
