using System;
using PlazmaGames.Attribute;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace LJ2025
{
    public class LightFlicker2 : MonoBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private Vector2 _flickerInterval;
        [SerializeField, Range(0f, 1f)] private float _flickerChance;
        [SerializeField] private Vector2 _flickerDuration;
        [SerializeField, Range(0f, 1f)] private float _glitchChance;
        [SerializeField] private Vector2 _glitchDuration;

        [SerializeField, ReadOnly] private bool _isOn = true;
        private Material[] _materials;

        private IEnumerator Flicker()
        {
            while (this)
            {
                float wait = Random.Range(_flickerInterval.x, _flickerInterval.y);
                yield return new WaitForSeconds(wait);

                if (Random.value <= _glitchChance)
                {
                    _isOn = false;
                    _light.enabled = false;
                    float offTime = Random.Range(_glitchDuration.x, _glitchDuration.y);
                    yield return new WaitForSeconds(offTime);
                    _light.enabled = true;
                    _isOn = true;
                }
                if (Random.value <= _flickerChance)
                {
                    _isOn = false;
                    _light.enabled = false;
                    float offTime = Random.Range(_flickerDuration.x, _flickerDuration.y);
                    yield return new WaitForSeconds(offTime);
                    _light.enabled = true;
                    _isOn = true;
                }
            }
        }

        private void Awake()
        {
            if (_light == null) _light = GetComponent<Light>();
            if (enabled)StartCoroutine(Flicker());
        }

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(Flicker());
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
            _light.enabled = true;
        }
    }
}
