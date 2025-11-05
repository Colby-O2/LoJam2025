using LJ2025.Player;
using LJ2025;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Utils;
using PlazmaGames.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LJ2025
{
    public enum SettingsTab
    {
        Gameplay,
        Sound,
        Graphics 
    }

    [System.Serializable]
    public struct ResolutionType
    {
        public int width;
        public int height;

        public string GetName()
        {
            return $"{width}x{height}";
        }
    }

    public class SettingsView : View
    {
        [SerializeField] private EventButton _gameplay;
        [SerializeField] private EventButton _sound;
        [SerializeField] private EventButton _graphics;
        [SerializeField] private EventButton _back;

        [SerializeField] private GameObject _gameplayUI;
        [SerializeField] private GameObject _soundUI;
        [SerializeField] private GameObject _graphicsUI;

        [Header("Gameplay Controls")]
        [SerializeField] private PlayerSettings _playerSettings;
        [SerializeField] private Slider _sensitivity;
        [SerializeField] private Toggle _invertX;
        [SerializeField] private Toggle _invertY;
        [SerializeField] private Toggle _headMotion;
        [SerializeField] private Slider _dialogueSpeed;

        [Header("Audio Controls")]
        [SerializeField] private Slider _overall;
        [SerializeField] private Slider _sfx;
        [SerializeField] private Slider _music;

        [Header("Video Controls")]
        [SerializeField] private List<ResolutionType> _resolutionTypes;
        [SerializeField] private TMP_Dropdown _resolution;
        [SerializeField] private Toggle _fullscreen;
        [SerializeField] private Toggle _vsync;
        [SerializeField] private EventButton _applyVideo;

        [SerializeField, ReadOnly] private int _currentResType = 0;

        private SettingsTab _currentTab;

        private void OpenGameplayTab()
        {
            _currentTab = SettingsTab.Gameplay;
            _gameplay.ShowOverlay();
            _sound.HideOverlay();
            _graphics.HideOverlay();

            _gameplayUI.SetActive(true);
            _soundUI.SetActive(false);
            _graphicsUI.SetActive(false);
        }

        private void OpenSoundTab()
        {
            _currentTab = SettingsTab.Sound;
            _gameplay.HideOverlay();
            _sound.ShowOverlay();
            _graphics.HideOverlay();

            _gameplayUI.SetActive(false);
            _soundUI.SetActive(true);
            _graphicsUI.SetActive(false);
        }

        private void OpenGraphicsTab()
        {
            _currentTab = SettingsTab.Graphics;
            _gameplay.HideOverlay();
            _sound.HideOverlay();
            _graphics.ShowOverlay();

            _gameplayUI.SetActive(false);
            _soundUI.SetActive(false);
            _graphicsUI.SetActive(true);
        }

        public void Back()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
        }

        private float GetSensitivityAdjustedValue(float input, float exp = 2f)
        {
            return Mathf.Pow(input, exp);
        }

        private void OnOverallChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetOverallVolume(val);
        }

        private void OnMusicChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetMusicVolume(val);
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetAmbientVolume(val);
        }

        private void OnSfXChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetSfXVolume(val);
        }

        private void OnSensitivityChanged(float val)
        {
            float sens = Mathf.Lerp(0.01f, 1f, GetSensitivityAdjustedValue(val));
            _playerSettings.Sensitivity = new Vector2(sens, sens);
        }

        public void OnHeadMotionChanged(bool val)
        {
            _playerSettings.EnableHeadMotion = val;
        }

        private void OnXInvertChanged(bool val)
        {
            _playerSettings.InvertLookX = val;
        }

        private void OnYInvertChanged(bool val)
        {
            _playerSettings.InvertLookY = val;
        }

        private void OnDialogueSpeedChanged(float val)
        {
            LJ2025GameManager.Preferences.DialogueSpeedMul = Mathf.Lerp(2f, 0f, val);
        }

        private void OnFullscreenChanged(bool val)
        {
            if (Screen.fullScreen != val) Screen.fullScreen = val;
        }

        private void OnVSyncChanged(bool val)
        {
            QualitySettings.vSyncCount = val ? 1 : 0;
        }

        private void ApplyVideoSettings()
        {
            _currentResType = _resolution.value;
            Screen.SetResolution(_resolutionTypes[_resolution.value].width, _resolutionTypes[_resolution.value].height, _fullscreen.isOn);
        }


        public override void Init()
        {
            _gameplay.onPointerDown.AddListener(OpenGameplayTab);
            _sound.onPointerDown.AddListener(OpenSoundTab);
            _graphics.onPointerDown.AddListener(OpenGraphicsTab);

            _gameplay.DisableAutoIconToggle = true;
            _sound.DisableAutoIconToggle = true;
            _graphics.DisableAutoIconToggle = true;

            _back.onPointerDown.AddListener(Back);

            _overall.onValueChanged.AddListener(OnOverallChanged);
            _music.onValueChanged.AddListener(OnMusicChanged);
            _sfx.onValueChanged.AddListener(OnSfXChanged);

            _sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
            _invertX.onValueChanged.AddListener(OnXInvertChanged);
            _invertY.onValueChanged.AddListener(OnYInvertChanged);
            _headMotion.onValueChanged.AddListener(OnHeadMotionChanged);
            _dialogueSpeed.onValueChanged.AddListener(OnDialogueSpeedChanged);

            _fullscreen.onValueChanged.AddListener(OnFullscreenChanged);
            _vsync.onValueChanged.AddListener(OnVSyncChanged);
            List<string> resTypes = new List<string>();
            foreach (ResolutionType res in _resolutionTypes) resTypes.Add(res.GetName());
            DropdownUtilities.SetDropdownOptions(ref _resolution, resTypes);
            _applyVideo.onPointerDown.AddListener(ApplyVideoSettings);

            _overall.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume();
            _music.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetMusicVolume();
            _sfx.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetSfXVolume();

            _playerSettings.Sensitivity = new Vector2(GetSensitivityAdjustedValue(0.5f), GetSensitivityAdjustedValue(0.5f));
            _sensitivity.value = 0.5f;
            _invertX.isOn = _playerSettings.InvertLookX;
            _invertY.isOn = _playerSettings.InvertLookY;
            _headMotion.isOn = _playerSettings.EnableHeadMotion;
            _dialogueSpeed.value = Mathf.InverseLerp(0f, 2f, LJ2025GameManager.Preferences.DialogueSpeedMul);

            _fullscreen.isOn = true;
            _vsync.isOn = false;
        }

        public override void Show()
        {
            base.Show();
            _back.HideOverlay();
            OpenGameplayTab();
        }
    }
}
