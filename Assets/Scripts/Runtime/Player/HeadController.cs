using LJ2025.MonoSystems;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;

namespace LJ2025.Player
{
    public class HeadController : MonoBehaviour
    {
        [SerializeField] private PlayerSettings _settings;

        [SerializeField, ReadOnly] private Vector3 _startPos;
        private IInputMonoSystem _input;

        private void StartHeadBob()
        {
            Vector3 pos = Vector3.zero;
            pos.x += Mathf.Lerp(pos.x, Mathf.Sin(Time.time * _settings.HeadBobFreqency) * _settings.HeadBobAmount * 1.4f, _settings.HeadBobSmoothing * Time.deltaTime);
            pos.y += Mathf.Lerp(pos.y, Mathf.Sin(Time.time * _settings.HeadBobFreqency) * _settings.HeadBobAmount * 1.4f, _settings.HeadBobSmoothing * Time.deltaTime);
            transform.localPosition += pos;
        }

        private void CheckForHeadMovement()
        {
            float movementAmount = new Vector3(_input.RawMovement.x, 0f, _input.RawMovement.y).magnitude;
            if (movementAmount > 0f)
            {
                StartHeadBob();
            }
        }

        private void StopHeadMovement()
        {
            if (transform.localPosition == _startPos) return;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _startPos, Time.deltaTime);
        }

        private void Awake()
        {
            _startPos = transform.localPosition;
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();
        }

        private void Update()
        {
            if (!_settings.EnableHeadMotion || LJ2025GameManager.IsPaused || LJ2025GameManager.LockMovement || LJ2025GameManager.Inspector.IsInspecting()) return;
            CheckForHeadMovement();
            StopHeadMovement();
        }
    }
}
