using LJ2025.MonoSystems;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _gravityMul = 1f;
    [SerializeField, ReadOnly] private float _gravity = -9.81f;

    private CharacterController _controller;
    private Vector3 _velocity;

    private IInputMonoSystem _input;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GameManager.GetMonoSystem<IInputMonoSystem>();
    }

    void Update()
    {
        Vector3 move = transform.right * _input.RawMovement.x + transform.forward * _input.RawMovement.y;
        _controller.Move(move * _walkSpeed * Time.deltaTime);

        if (!_controller.isGrounded)
            _velocity.y += _gravityMul * _gravity * Time.deltaTime;
        else if (_velocity.y < 0)
            _velocity.y = -2f;

        _controller.Move(_velocity * Time.deltaTime);
    }
}