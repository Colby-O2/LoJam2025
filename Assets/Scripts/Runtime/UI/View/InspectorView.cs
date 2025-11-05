using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace LJ2025
{
    public class InspectorView : View
    {
        [SerializeField] private GameObject _rotateButton;
        [SerializeField] private GameObject _zoomButton;
        [SerializeField] private GameObject _interactButton;
        [SerializeField] private GameObject _readButton;
        
        [SerializeField] private GameObject _readPanel;
        [SerializeField] private TMPro.TMP_InputField _readText;
        
        [SerializeField] private GameObject _titlePanel;
        [SerializeField] private TMPro.TMP_Text _titleText;

        public void ToggleReadPanel() => _readPanel.SetActive(!_readPanel.activeSelf);
        public void SetReadPanel(bool state) => _readPanel.SetActive(state);
        public void SetReadText(string text) => _readText.text = text;

        public void SetTitle(bool state) => _titlePanel.SetActive(state);
        public void SetTitleText(string text) => _titleText.text = text;

        public void SetRotateButton(bool state) => _rotateButton.SetActive(state);
        public void SetZoomButton(bool state) => _zoomButton.SetActive(state);
        public void SetReadButton(bool state) => _readButton.SetActive(state);
        public void SetInteractButton(bool state) => _interactButton.SetActive(state);
        
        public override void Init()
        {
        }

        public override void Show()
        {
            base.Show();
            LJ2025GameManager.ShowCursor();
        }
    }
}
