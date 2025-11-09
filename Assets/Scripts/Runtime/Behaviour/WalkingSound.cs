using UnityEngine;

namespace LJ2025
{
    public class WalkingSound : MonoBehaviour
    {
        [SerializeField] private VelocityTracker _vel;

        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _indoorsClip;
        [SerializeField] private AudioClip _outdoorClip;

        [SerializeField] private bool _isOutDoorsToStart = true;

        public void SetIndoors()
        {
            _as.clip = _indoorsClip;    
        }

        public void SetOutdoors()
        {
            _as.clip = _outdoorClip;
        }

        private void Awake()
        {
            _as.clip = _isOutDoorsToStart ? _outdoorClip : _indoorsClip;
        }

        private void Update()
        {
            if (_as.isPlaying && _vel.SpeedInPlane < 0.01f) _as.Stop();
            else if (!_as.isPlaying && _vel.SpeedInPlane > 0.01f) _as.Play();
        }

    }
}
