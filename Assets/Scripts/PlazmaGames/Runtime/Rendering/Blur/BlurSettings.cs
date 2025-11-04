using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PlazmaGames.Rendering.Blur
{
    public class BlurSettings : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter strength = new ClampedFloatParameter(0f, 0f, 15f);

        public bool IsActive()
        {
            return strength.value > 0;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}
