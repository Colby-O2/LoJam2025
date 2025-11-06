using System;
using System.Collections.Generic;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using Unity.AI.Navigation;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.AI;

namespace LJ2025
{
    public class Pather : MonoBehaviour, IResetState
    {
        enum MoveState
        {
            Stopped,
            Moving,
            Turning,
            Sitting,
        }
        
        [SerializeField] private Transform _pathContainer;
        [SerializeField] private float _turnSpeed = 140;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private NavMeshSurface _navMesh;
        [SerializeField] private PlayerAnimationController _animationController;
        [SerializeField] private MoveState _moveState = MoveState.Stopped;
        private string _targetTag = "";
        private float _targetRot;

        private MathExt.Transform _initialTrans;

        private Promise<None> _promise;

        private List<Transform> _spots = new();
        [SerializeField, ReadOnly] private int _curPath = -1;
        private bool _sitting = false;


        public Promise<None> Next()
        {
            _curPath += 1;
            return Goto(new MathExt.Transform(_spots[_curPath]), _spots[_curPath].tag);
        }

        public Promise<None> Goto(MathExt.Transform trans, string tag = "")
        {
            if (_sitting)
            {
                return _animationController
                    .Unsit()
                    .Then(_ => { _sitting = false; })
                    .Then(_ => Goto(trans, tag));
            }
            _targetTag = tag;
            _targetRot = trans.rotation.eulerAngles.y;
            Vector3 pos = trans.position;
            SetState(MoveState.Moving);
            _agent.SetDestination(pos);
            _promise = new Promise<None>();
            return _promise;
        }

        private bool IsAgentStopped() => _agent.pathPending == false && _agent.hasPath == false;

        private void SetAnimationState(MoveState state)
        {
            switch (state)
            {
                case MoveState.Moving:
                    _animationController.SetAnimationState(PlayerAnimationState.Walking);
                    break;
                case MoveState.Stopped:
                    if (_sitting) break;
                    _animationController.SetAnimationState(PlayerAnimationState.Idle);
                    break;
            }
        }

        private void SetState(MoveState state)
        {
            _moveState = state;
            _agent.enabled = state != MoveState.Stopped;
            if (_animationController) SetAnimationState(state);
        }

        private void Awake()
        {
            if (!_animationController) _animationController = GetComponentInChildren<PlayerAnimationController>();
        }

        private void Update()
        {
            if (_moveState == MoveState.Stopped) return;
            if (_moveState == MoveState.Moving && IsAgentStopped())
            {
                SetState(MoveState.Turning);
            }

            if (_moveState == MoveState.Turning)
            {
                float dir = MathExt.AngleToShortestDirection(transform.eulerAngles.y, _targetRot);
                transform.eulerAngles = transform.eulerAngles.AddY(dir * Time.deltaTime * _turnSpeed);
                float dirNow = MathExt.AngleToShortestDirection(transform.eulerAngles.y, _targetRot);
                if (System.Math.Sign(dir) != System.Math.Sign(dirNow))
                {
                    transform.eulerAngles = transform.eulerAngles.SetY(_targetRot);
                    if (_targetTag == "SitBack")
                    {
                        _sitting = true;
                        SetState(MoveState.Sitting);
                        _animationController.Sit()
                            .Then(_ =>
                            {
                                SetState(MoveState.Stopped);
                                Promise<None> tmp = _promise;
                                _promise = null;
                                tmp?.Resolve();
                            });
                    }
                    else if (_targetTag == "SitSide")
                    {
                        _sitting = true;
                        SetState(MoveState.Sitting);
                        _animationController.SitSide()
                            .Then(_ =>
                            {
                                SetState(MoveState.Stopped);
                                Promise<None> tmp = _promise;
                                _promise = null;
                                tmp?.Resolve();
                            });
                    }
                    else
                    {
                        SetState(MoveState.Stopped);
                        Promise<None> tmp = _promise;
                        _promise = null;
                        tmp?.Resolve();
                    }
                }
                
            }
        }

        private void LoadSpots(Transform pathContainer)
        {
            _spots.Clear();
            foreach (Transform t in pathContainer)
            {
                _spots.Add(t);
            }
        }

        public void InitState()
        {
            _navMesh = GameObject.FindAnyObjectByType<NavMeshSurface>();
            _initialTrans = transform;
            LoadSpots(_pathContainer.GetChild(GameManager.GetMonoSystem<IGameLogicMonoSystem>().Act()));
        }

        public void ResetState()
        {
            _agent.Warp(_initialTrans.position);
            SetState(MoveState.Stopped);
            _curPath = -1;
            LoadSpots(_pathContainer.GetChild(GameManager.GetMonoSystem<IGameLogicMonoSystem>().Act()));
        }
    }
}
