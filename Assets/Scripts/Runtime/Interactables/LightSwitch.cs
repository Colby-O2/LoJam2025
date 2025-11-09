using UnityEngine;
using System.Collections.Generic;
using LJ2025.Player;
using PlazmaGames.Attribute;

namespace LJ2025
{
    public class LightSwitch : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<Light> _linkedLights;
        [SerializeField] private bool _isEnabled = true;
        [SerializeField] private bool _defaultState = true;
        [SerializeField, ReadOnly] private bool _isOn;
        [SerializeField] private SphereCollider _boundingRadius;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _onClip;
        [SerializeField] private AudioClip _offClip;

        public void UpdateOnStatus()
        {
            _isOn = true;
            foreach (Light light in _linkedLights) _isOn &= light.gameObject.activeSelf;
        }

        public void On(bool playAudio = true)
        {
            _isOn = true;
            if (playAudio && _audioSource) _audioSource.PlayOneShot(_onClip);
            foreach (Light light in _linkedLights) light.gameObject.SetActive(true);
        }

        public void Off(bool playAudio = true)
        {
            _isOn = false;
            if (playAudio && _audioSource) _audioSource.PlayOneShot(_offClip);
            foreach (Light light in _linkedLights) light.gameObject.SetActive(false);
        }

        public bool IsInteractable()
        {
            return _isEnabled;
        }

        public string GetHintName()
        {
            return $"Lights";
        }

        public string GetHintAction()
        {
            return (_isOn ? "Off" : "On");
        }

        public SphereCollider BoundingRadius() => _boundingRadius;

        public bool Interact(Interactor interactor)
        {
            UpdateOnStatus();

            if (_isOn) Off();
            else On();

            return true;
        }

        public void AddOutline()
        {

        }

        public void RemoveOutline()
        {

        }

        public void Restart()
        {
            if (_defaultState) On(false);
            else Off(false);
        }

        private void Awake()
        {
            Restart();
        }
    }
}
