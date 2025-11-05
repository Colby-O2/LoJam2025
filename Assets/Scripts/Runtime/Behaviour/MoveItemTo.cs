using LJ2025;
using UnityEngine;

namespace LJ2025
{
    public class MoveItemTo : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _bounceHeight = 0.25f;
        [SerializeField] private float _moveTime = 1.0f;
        
        private bool _moving = false;
        private float _t = 0;
        private Vector3 _startPos;
        private Vector3 _endPos;

        private Promise _promise = null;

        public void SetTarget(Transform target) => _target = target;

        public Promise Move()
        {
            _promise = new Promise();
            _moving = true;
            _t = 0;
            _startPos = transform.position;
            _endPos = _target.position;
            return _promise;
        }

        private void Update()
        {
            if (!_moving) return;
            _t = Mathf.Clamp01(_t + Time.deltaTime * _moveTime);
            float y;
            if (_t < 0.5f)
            {
                y = Mathf.SmoothStep(_startPos.y, _startPos.y + _bounceHeight, _t * 2.0f);
            }
            else
            {
                y = Mathf.SmoothStep(_startPos.y + _bounceHeight, _endPos.y, (_t - 0.5f) * 2.0f);
            }

            transform.position = new Vector3(
                Mathf.SmoothStep(_startPos.x, _endPos.x, _t),
                y,
                Mathf.SmoothStep(_startPos.z, _endPos.z, _t));

            if (_t >= 1)
            {
                _moving = false;
                _promise?.Resolve();
                _promise = null;
            }
        }
    }
}
