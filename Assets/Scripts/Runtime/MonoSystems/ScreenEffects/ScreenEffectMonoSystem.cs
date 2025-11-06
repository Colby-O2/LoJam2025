using LJ2025;
using UnityEngine;

namespace LJ2025
{
    public class ScreenEffectMonoSystem : MonoBehaviour, IScreenEffectMonoSystem
    {
        private UnityEngine.UI.Image _fadeoutImage;
        private TMPro.TMP_Text _fadeoutText;
        private Promise _fadeoutPromise;
        private bool _fadeoutDoText = false;
        private float _fadeoutT = 0;
        private float _fadeoutDuration = 0;
        private float _fadeoutDir = -1;
        
        void Start()
        {
            _fadeoutImage = GameObject.FindWithTag("Fadeout").GetComponent<UnityEngine.UI.Image>();
            _fadeoutText = _fadeoutImage.GetComponentInChildren<TMPro.TMP_Text>();
        }

        void Update()
        {
            DoFadeout();
        }

        private void DoFadeout()
        {
            if (_fadeoutDir < 0 && _fadeoutT <= 0) return;
            if (_fadeoutDir > 0 && _fadeoutT >= 1) return;

            _fadeoutT = Mathf.Clamp01(_fadeoutT + Time.deltaTime / _fadeoutDuration * _fadeoutDir);
            if (_fadeoutDoText) _fadeoutText.color = _fadeoutText.color.SetA(_fadeoutT);
            else _fadeoutImage.color = _fadeoutImage.color.SetA(_fadeoutT);

            if ((_fadeoutDir < 0 && _fadeoutT <= 0) || (_fadeoutDir > 0 && _fadeoutT >= 1))
            {
                Promise tmp = _fadeoutPromise;
                _fadeoutPromise = null;
                tmp?.Resolve();
            }
        }

        public Promise Fadeout(float duration)
        {
            _fadeoutDoText = false;
            _fadeoutPromise = new Promise();
            _fadeoutDuration = duration;
            _fadeoutT = 0;
            _fadeoutDir = 1;
            return _fadeoutPromise;
        }
        
        public Promise FadeoutText(string text, float duration)
        {
            SetFadeoutText(text);
            _fadeoutDoText = true;
            _fadeoutPromise = new Promise();
            _fadeoutDuration = duration;
            _fadeoutT = 0;
            _fadeoutDir = 1;
            return _fadeoutPromise;
        }

        public Promise Fadein(float duration)
        {
            _fadeoutDoText = false;
            _fadeoutPromise = new Promise();
            _fadeoutDuration = duration;
            _fadeoutT = 1;
            _fadeoutDir = -1;
            return _fadeoutPromise;
        }
        
        public Promise FadeinText(float duration)
        {
            _fadeoutDoText = true;
            _fadeoutPromise = new Promise();
            _fadeoutDuration = duration;
            _fadeoutT = 1;
            _fadeoutDir = -1;
            return _fadeoutPromise;
        }

        public void SetFadeoutText(string text)
        {
            _fadeoutText.text = text;
        }
    }
}
