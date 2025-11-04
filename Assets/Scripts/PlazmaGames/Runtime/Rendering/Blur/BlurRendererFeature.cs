using PlazmaGames.Core.Debugging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PlazmaGames.Rendering.Blur
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        [Header("References")]
        [SerializeField] private Shader _shader;

        [Header("Options")]
        [SerializeField] private RenderPassEvent _injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;


        private Material _mat = null;
        private BlurRenderPass _rp = null;

        public override void Create()
        {
            name = "Blur";

            if (_shader == null)
            {
                _shader = Shader.Find("Hidden/Blur");
            }

            if (_mat == null)
            {
                _mat = CoreUtils.CreateEngineMaterial(_shader);
            }

            if (_rp == null)
            {
                _rp = new BlurRenderPass(_mat);
                _rp.renderPassEvent = _injectionPoint;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_mat != null)
            {
                CoreUtils.Destroy(_mat);
                _mat = null;
            }

            if (_rp != null)
            {
                _rp.Destroy();
                _rp = null;
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_rp.Initialize())
            {
                _rp.ConfigureInput(ScriptableRenderPassInput.Color);
                renderer.EnqueuePass(_rp);
            }
            else
            {
                PlazmaDebug.LogWarning("Blur renderer feature failed to initialize.", "BlurRendererFeature", 3);
            }
        }

    }
}
