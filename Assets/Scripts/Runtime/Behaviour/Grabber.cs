using PlazmaGames.Animation;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025
{
    public class Grabber : MonoBehaviour
    {
        [SerializeField] private Transform _hand;
        [SerializeField] private float _grabTime = 1.0f;
        
        public Promise Grab(Transform obj)
        {
            Promise p = new Promise();
            Vector3 startPos = obj.position;
            GameManager.GetMonoSystem<IAnimationMonoSystem>()
                .RequestAnimation(this, _grabTime,
                    t => obj.position = Vector3.Lerp(startPos, _hand.position, t),
                    () =>
                    {
                        obj.transform.parent = _hand;
                        p.Resolve();
                    });
            return p;
        }
    }
}
