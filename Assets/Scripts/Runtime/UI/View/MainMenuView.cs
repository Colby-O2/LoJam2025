using LJ2025.UI;
using LJ2025;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System;
using UnityEngine;

namespace LJ2025
{
    public class MainMenuView : View
    {
        [SerializeField] private GameObject _mainMenuScene;
        [SerializeField] private EventButton _play;
        [SerializeField] private EventButton _settings;
        [SerializeField] private EventButton _exit;

        [SerializeField] private GameObject _screen;

        private void Quit()
        {
            LJ2025GameManager.QuitGame();
        }

        private void Settings()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<SettingsView>();
        }

        private void Play()
        {
            _mainMenuScene.SetActive(false);
            _screen.SetActive(false);
            LJ2025GameManager.IsPaused = false;
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
        }

        public override void Init()
        {
            _play.onPointerDown.AddListener(Play);
            _settings.onPointerDown.AddListener(Settings);
            _exit.onPointerDown.AddListener(Quit);
        }

        public override void Show()
        {
            base.Show();
            _mainMenuScene.SetActive(true);
            _screen.SetActive(true);
            _play.HideOverlay();
            _settings.HideOverlay();
            _exit.HideOverlay();
            LJ2025GameManager.IsPaused = true;
            LJ2025GameManager.ShowCursor();
        }
    }
}
