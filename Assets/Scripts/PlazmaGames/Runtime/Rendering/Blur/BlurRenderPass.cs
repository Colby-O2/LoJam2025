using PlazmaGames.Core.Debugging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PlazmaGames.Rendering.Blur
{
    public class BlurRenderPass : ScriptableRenderPass
    {
        private Material _mat;
        private BlurSettings _blurSettings;

        private RTHandle _rtHandle;

        public BlurRenderPass(Material mat) => _mat = mat;

        public bool Initialize()
        {
            _blurSettings = VolumeManager.instance.stack.GetComponent<BlurSettings>();

            if (_blurSettings != null && _blurSettings.IsActive()) 
            {
                return true;
            }

            PlazmaDebug.LogWarning("Blur render pass Initialize has failed. Blur setting is not active or null", "BlurRenderPass", 3);
            return false;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (_blurSettings == null || !_blurSettings.IsActive())
            {
                PlazmaDebug.LogWarning("Can't Configure blur renderer pass. Blur setting is not active or null", "BlurRenderPass", 3);
                return;
            }

            cameraTextureDescriptor.depthBufferBits = (int)DepthBits.None;
            RenderingUtils.ReAllocateIfNeeded(ref _rtHandle, cameraTextureDescriptor, wrapMode: TextureWrapMode.Clamp);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_blurSettings == null || !_blurSettings.IsActive())
            {
                PlazmaDebug.LogWarning("Skipping blur render pass. Blur setting is not active or null", "BlurRenderPass", 3);
                return;
            }

            if (_mat == null || _rtHandle == null || _rtHandle.rt == null || renderingData.cameraData.renderer.cameraColorTargetHandle == null || renderingData.cameraData.renderer.cameraColorTargetHandle.rt == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Blur Post Process");

            int gridSize = Mathf.CeilToInt(_blurSettings.strength.value * 3.0f);

            if (gridSize % 2 == 0) gridSize++;

            _mat.SetInteger("_GridSize", gridSize);
            _mat.SetFloat("_Spread", _blurSettings.strength.value);

            // Horiztional Blur Pass
            cmd.Blit(renderingData.cameraData.renderer.cameraColorTargetHandle, _rtHandle, _mat, 0);
            // Vertical Blur Pass
            cmd.Blit(_rtHandle, renderingData.cameraData.renderer.cameraColorTargetHandle, _mat, 1);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.Release();
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }

        public void Destroy()
        {
            _rtHandle?.Release();
            _rtHandle = null;
        }
    }
}
