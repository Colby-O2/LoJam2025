using PlazmaGames.Attribute;
using UnityEngine;
using System.Collections;

namespace LJ2025
{
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private Material _onMat;
        [SerializeField] private Material _offMat;
        [SerializeField] private int _matIdx;
        [SerializeField] private Renderer _renderer;
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
                    _materials = _renderer.materials;
                    _materials[_matIdx] = _offMat;
                    _renderer.materials = _materials;
                    float offTime = Random.Range(_glitchDuration.x, _glitchDuration.y);
                    yield return new WaitForSeconds(offTime);
                    _isOn = true;
                    _materials = _renderer.materials;
                    _materials[_matIdx] = _offMat;
                    _renderer.materials = _materials;
                }
                if (Random.value <= _flickerChance)
                {
                    _isOn = false;
                    _materials = _renderer.materials;
                    _materials[_matIdx] =  _offMat;
                    _renderer.materials = _materials;
                    float offTime = Random.Range(_flickerDuration.x, _flickerDuration.y);
                    yield return new WaitForSeconds(offTime);
                    _isOn = true;
                    _materials = _renderer.materials;
                    _materials[_matIdx] = _onMat;
                    _renderer.materials = _materials;
                }
            }
        }

        private void Awake()
        {
            if (!_renderer) _renderer = GetComponent<Renderer>();
            _renderer.materials[_matIdx] = _onMat;
            StartCoroutine(Flicker());
        }
    }
}
