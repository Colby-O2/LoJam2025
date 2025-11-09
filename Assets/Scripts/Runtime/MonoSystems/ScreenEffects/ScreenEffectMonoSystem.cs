using LJ2025;
using PlazmaGames.Core.Debugging;
using PlazmaGames.Rendering.CRT;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LJ2025
{
    public class ScreenEffectMonoSystem : MonoBehaviour, IScreenEffectMonoSystem
    {
        [SerializeField] private UniversalRendererData _rendererData;

        [Header("Default CRT Values")]
        [SerializeField] private float _defaultNoiseScale = 0f;
        [SerializeField] private float _defaultChromicOffset = 0f;
        [SerializeField] private float _defaultScreenRoundness= 0f;
        [SerializeField] private float _defaultVignette = 1f;
        [SerializeField] private float _defaultRedShift = 0f;
        [SerializeField] private float _defaultContrast = 0.5f;

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

        private T GetRendererFeature<T>(ScriptableRendererData rendererData, string featureName) where T : ScriptableRendererFeature
        {
            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature != null && feature.name == featureName)
                {
                    return feature as T;
                }
            }

            return null;
        }

        public void ToggleRendererFeature(ScriptableRendererData rendererData, string featureName, bool state)
        {
            ScriptableRendererFeature feature = GetRendererFeature<ScriptableRendererFeature>(rendererData, featureName);
            if (feature != null) feature.SetActive(state);
        }

        public void ToggleRendererFeature(string featureName, bool state)
        {
            PlazmaDebug.Log($"Setting renderer feature '{featureName}' to an {state} state.", "Screen Effect", verboseLevel: 2, color: Color.green);
            ToggleRendererFeature(_rendererData, featureName, state);
        }

        public void SetStaticLevel(float scale)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' noise scale to {scale}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetNoiseLevel(scale);
            else PlazmaDebug.LogWarning($"Setting renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void SetChromicOffset(float level)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' chromic offset to {level}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetChromicOffset(level);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void SetScreenRoundness(float roudness)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' screen roudness to {roudness}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetRoundness(roudness);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }
        public void SetScreenVignette(float value)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' screen vignette to {value}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetVignetteOpacity(value);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void SetRedShift(float value)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' red shift to {value}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetRedShift(value);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void SetContrast(float value)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' contrast to {value}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetContrast(value);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void RestoreDefaults()
        {
            SetStaticLevel(_defaultNoiseScale);
            SetChromicOffset(_defaultChromicOffset);
            SetScreenRoundness(_defaultScreenRoundness);
            SetScreenVignette(_defaultVignette);
            SetRedShift(_defaultRedShift);
            SetContrast(_defaultContrast);
        }
    }
}
