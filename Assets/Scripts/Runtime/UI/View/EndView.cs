using PlazmaGames.UI;
using UnityEngine;

namespace LJ2025.UI
{
    public class EndView : View
    {
        [SerializeField] private GameObject _mainMenuScene;
        [SerializeField] private GameObject _policeScene;
        [SerializeField] private GameObject _mainMenuCamera;
        [SerializeField] private GameObject _playerView;
        [SerializeField] private GameObject _hintView;

        public override void Init()
        {

        }

        public override void Show()
        {
            base.Show();
            _mainMenuScene.SetActive(true);
            _policeScene.SetActive(true);
            _mainMenuCamera.SetActive(false);
            _playerView.SetActive(false);
            _hintView.SetActive(false);
        }

        public override void Hide()
        {
            base.Hide();
            _policeScene.SetActive(false);
        }
    }
}
