using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using LJ2025.MonoSystems;
using LJ2025.Player;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LJ2025
{
    public class LJ2025GameManager : GameManager
    {
        [SerializeField] GameObject _monoSystemHolder;

        [Header("MonoSystems")]
        [SerializeField] private UIMonoSystem _uiSystem;
        [SerializeField] private AnimationMonoSystem _animSystem;
        [SerializeField] private AudioMonoSystem _audioSystem;
        [SerializeField] private InputMonoSystem _inputSystem;
        [SerializeField] private DialogueMonoSystem _dialogueSystem;
        [SerializeField] private GameLogicMonoSystem _gameLogicSystem;
        [SerializeField] private ScreenEffectMonoSystem _screenEffectSystem;
        public static bool IsPaused = false;


        private static bool _lockMovement = false;
        public static bool LockMovement
        {
            get { return _lockMovement; }
            set
            {
                _lockMovement = value;
            }
        }
        public static PlayerSettings PlayerSettings;
        public static Player.Inspector Inspector;
        public static Player.Controller Player;
        public static Preferences Preferences { get => (Instance as LJ2025GameManager)._preferences; }
        [SerializeField] private Preferences _preferences;

        public static void HideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void ShowCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void AttachMonoSystems()
        {
            AddMonoSystem<UIMonoSystem, IUIMonoSystem>(_uiSystem);
            AddMonoSystem<AnimationMonoSystem, IAnimationMonoSystem>(_animSystem);
            AddMonoSystem<AudioMonoSystem, IAudioMonoSystem>(_audioSystem);
            AddMonoSystem<InputMonoSystem, IInputMonoSystem>(_inputSystem);
            AddMonoSystem<DialogueMonoSystem, IDialogueMonoSystem>(_dialogueSystem);
            AddMonoSystem<GameLogicMonoSystem, IGameLogicMonoSystem>(_gameLogicSystem);
            AddMonoSystem<ScreenEffectMonoSystem, IScreenEffectMonoSystem>(_screenEffectSystem);
        }

        public override string GetApplicationName()
        {
            return nameof(LJ2025GameManager);
        }

        public override string GetApplicationVersion()
        {
            return "v0.0.1";
        }

        protected override void OnInitalized()
        {
            AttachMonoSystems();

            _monoSystemHolder.SetActive(true);
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {

        }

        private void OnSceneUnload(Scene scene)
        {
            RemoveAllEventListeners();
        }

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            Cursor.lockState= CursorLockMode.Locked;
            Cursor.visible = false;
            Inspector = FindAnyObjectByType<Player.Inspector>();
            Player = FindAnyObjectByType<Player.Controller>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }
        
        public static void QuitGame()
        {
            Application.Quit();
        }
    }
}