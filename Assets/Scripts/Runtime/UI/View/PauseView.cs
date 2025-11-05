using LJ2025;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Xml.Serialization;
using UnityEngine;

namespace LJ2025
{
    public class PauseView : View
    {
        [SerializeField] private EventButton _resume;
        [SerializeField] private EventButton _settings;
        [SerializeField] private EventButton _quit;
        
        [SerializeField] private GameObject _screen;
        [SerializeField] private GameObject _gameScreen;

        public void Resume()
        {
            _screen.SetActive(false);
            _gameScreen.SetActive(true);
            LJ2025GameManager.IsPaused = false;
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
        }

        private void Settings()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<SettingsView>();
        }

        private void Quit()
        {
            LJ2025GameManager.QuitGame();
        }

        public override void Init()
        {
            _resume.onPointerDown.AddListener(Resume);
            _settings.onPointerDown.AddListener(Settings);
            _quit.onPointerDown.AddListener(Quit);
            _screen.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            LJ2025GameManager.IsPaused = true;
            LJ2025GameManager.ShowCursor();
            _screen.SetActive(true);
            _gameScreen.SetActive(false);
            _resume.HideOverlay();
            _settings.HideOverlay();
            _quit.HideOverlay();
        }
    }
}
