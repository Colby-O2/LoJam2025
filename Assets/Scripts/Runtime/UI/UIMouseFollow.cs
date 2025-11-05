using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace LJ2025
{
    public class UIMouseFollow : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _parentRect;
        [SerializeField] private Camera _camera;

        private Vector2 _localPos;

        public void Reset()
        {
            _localPos = _parentRect.rect.size / 2.0f;
        }

        private void Awake()
        {
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
            if (_parentRect == null) _parentRect = _rectTransform.parent as RectTransform;
        }

        private void Update()
        {
            _localPos += Mouse.current.delta.value;

            _localPos = new Vector2(
                Mathf.Clamp(_localPos.x, 0, _parentRect.rect.width),
                Mathf.Clamp(_localPos.y, 0, _parentRect.rect.height));

            _rectTransform.localPosition = _parentRect.rect.min + _localPos;
        }
    }
}
