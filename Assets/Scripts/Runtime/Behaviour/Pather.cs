using System;
using System.Collections.Generic;
using LJ2025;
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
        }
        
        [SerializeField] private Transform _pathContainer;
        [SerializeField] private float _turnSpeed = 140;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private NavMeshSurface _navMesh;
        private MoveState _moveState = MoveState.Stopped;
        private float _targetRot;

        private MathExt.Transform _initialTrans;

        private Promise _promise;

        private List<Transform> _spots = new();
        [SerializeField, ReadOnly] private int _curPath = -1;


        public Promise Next()
        {
            _curPath += 1;
            return Goto(new MathExt.Transform(_spots[_curPath]));
        }

        public Promise Goto(MathExt.Transform trans)
        {
            _targetRot = trans.rotation.eulerAngles.y;
            Vector3 pos = trans.position;
            _agent.SetDestination(pos);
            _moveState = MoveState.Moving;
            _promise = new Promise();
            return _promise;
        }

        private bool IsAgentStopped() => _agent.pathPending == false && _agent.hasPath == false;

        private void Update()
        {
            if (_moveState == MoveState.Stopped) return;
            if (_moveState == MoveState.Moving && IsAgentStopped())
            {
                _moveState = MoveState.Turning;
            }

            if (_moveState == MoveState.Turning)
            {
                float dir = MathExt.AngleToShortestDirection(transform.eulerAngles.y, _targetRot);
                transform.eulerAngles = transform.eulerAngles.AddY(dir * Time.deltaTime * _turnSpeed);
                float dirNow = MathExt.AngleToShortestDirection(transform.eulerAngles.y, _targetRot);
                if (System.Math.Sign(dir) != System.Math.Sign(dirNow))
                {
                    transform.eulerAngles = transform.eulerAngles.SetY(_targetRot);
                    _moveState = MoveState.Stopped;
                    Promise tmp = _promise;
                    _promise = null;
                    tmp?.Resolve();
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
            _moveState = MoveState.Stopped;
            _curPath = -1;
            LoadSpots(_pathContainer.GetChild(GameManager.GetMonoSystem<IGameLogicMonoSystem>().Act()));
        }
    }
}
