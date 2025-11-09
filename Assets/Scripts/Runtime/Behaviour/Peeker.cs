using PlazmaGames.Animation;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class Peeker : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset;
        private Vector3 _startPos;

        private void Awake()
        {
            _startPos = transform.position;
        }

        public void Peek(float peekSpeed, float peekDuration)
        {
            transform.position = _startPos;
            gameObject.SetActive(true);
            Vector3 end = _startPos + _offset;
            GameManager.GetMonoSystem<IAnimationMonoSystem>()
                .RequestAnimation(
                    this,
                    peekSpeed,
                    t =>
                    {
                        transform.position = Vector3.Lerp(_startPos, end, t);
                    },
                    () =>
                    {
                        GameManager.GetMonoSystem<IGameLogicMonoSystem>().Scheduler().Wait(peekDuration).Then(_ =>
                        {
                            gameObject.SetActive(false);
                        });
                    });
        }
    }
}