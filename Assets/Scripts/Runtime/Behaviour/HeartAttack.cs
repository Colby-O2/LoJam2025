using PlazmaGames.Animation;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class HeartAttack : MonoBehaviour
    {
        [SerializeField] private Transform _spine;
        [SerializeField] private float _endSpineRot;
        [SerializeField] private float _attackFreq = 10;
        [SerializeField] private float _attackAmp = 20;
        
        private float _startSpineRot;

        private void Awake()
        {
            _startSpineRot = _spine.localEulerAngles.x;
        }
        
        public void StartAttack(float duration)
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>()
                .RequestAnimation(
                    this,
                    duration,
                    t =>
                    {
                        _spine.localEulerAngles = _spine.localEulerAngles.SetX(
                            _startSpineRot + Mathf.Sin(t * duration * 2 * Mathf.PI * _attackFreq) * _attackAmp
                        );
                    },
                    () =>
                    {
                        _spine.localEulerAngles = _spine.localEulerAngles.SetX(
                            _endSpineRot
                        );
                    });
        }
    }
}