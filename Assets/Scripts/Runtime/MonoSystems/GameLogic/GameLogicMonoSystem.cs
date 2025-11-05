using System;
using PlazmaGames.Core;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LJ2025
{
    public enum GameState
    {
    }

    public static class GameStateExt
    {
        public static bool IsBefore(this GameState state, GameState to)
        {
            return (int)state < (int)to;
        }
        public static bool IsAfter(this GameState state, GameState to)
        {
            return (int)state > (int)to;
        }

        public static bool IsEqual(this GameState state, GameState to)
        {
            return (int)state == (int)to;
        }
    }
    
    public class GameLogicMonoSystem : MonoBehaviour, IGameLogicMonoSystem
    {
        [SerializeField] private float _globalTimeScale = 1;

        private IDialogueMonoSystem _dialogueMs;
        private DateTime _date = new DateTime(2012, 3, 3, 17, 0, 0);
        private float _timeScale = 10;

        private ResetableState[] _states;

        private Scheduler _scheduler = new();

        private int _act = 0;
        private GameState _gameState;

        private Player.Controller _player;
        private Player.Interactor _playerInteractor;
        private Player.Inspector _playerInspector;
        private Player.ObjectMover _playerMover;

        private HashSet<string> _inRange = new();

        private class Refs
        {
        }

        private void Start()
        {
            _dialogueMs = GameManager.GetMonoSystem<IDialogueMonoSystem>();
            _player = GameObject.FindAnyObjectByType<Player.Controller>();
            _playerInteractor = _player.GetComponent<Player.Interactor>();
            _playerInspector = _player.GetComponent<Player.Inspector>();
            _playerMover = _player.GetComponent<Player.ObjectMover>();
            
            _states = GameObject.FindObjectsByType<ResetableState>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var rs in _states) rs.InitState();
        }


        private Refs _refs = new();

        public Player.ObjectMover GetObjectMover() => _playerMover;
        
        public int Act() => _act;
        public GameState GameState() => _gameState;

        public System.DateTime CurrentDate() => _date;
        public int CurrentYear() => 2012 + _act - 1;
        public int CurrentDay() => 3;
        public int CurrentHour() => _date.Hour;
        public string CurrentTime() => _date.ToString("hh:mm");
        
        private void SetDate() => _date = new DateTime(2012, 3, 3 + _act, 17, 0, 0);
        
        public void TriggerEvent(string eventName, Transform by)
        {
            Debug.Log("Event: '" + eventName + "'");
        }
        

        private void Update()
        {
            _scheduler.Tick(Time.deltaTime);
        }

        private bool _tmpTestStart = false;
        private void FixedUpdate()
        {
            if (Time.time > 1) Begin();
            _date = _date.Add(TimeSpan.FromSeconds(Time.fixedDeltaTime * _timeScale * _globalTimeScale));
        }

        public void Begin()
        {
            if (_tmpTestStart) return;
            _tmpTestStart = true;
            TriggerEvent("Begin", transform);
        }

        public void SetInRange(string id, bool state)
        {
            if (state) _inRange.Add(id);
            else _inRange.Remove(id);
        }

        public bool IsInRange(string id) => _inRange.Contains(id);
    }
}
