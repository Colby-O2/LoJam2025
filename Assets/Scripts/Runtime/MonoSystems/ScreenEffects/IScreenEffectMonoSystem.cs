using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace  LJ2025
{
    public interface IScreenEffectMonoSystem : IMonoSystem
    {
        public Promise Fadeout(float duration);
        public Promise FadeoutText(string text, float duration);
        public Promise FadeinText(float duration);
        public Promise Fadein(float duration);
        public void SetFadeoutText(string text);
    }
}
