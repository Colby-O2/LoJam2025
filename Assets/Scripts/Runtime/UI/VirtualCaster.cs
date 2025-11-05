using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace LJ2025.UI
{
    public class VirtualCaster : GraphicRaycaster
    {
        [SerializeField] private Camera _screenCamera;

        public static VirtualCaster Instance { get; private set; }

        private bool ScreenSpaceCast<T>(Vector3 pos, out T output, float maxDst = Mathf.Infinity, int layer = ~0)
        {
            output = default;
            Ray ray = _screenCamera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDst, layer))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.TryGetComponent<T>(out output))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Raycast<T>(Vector3 mousePosition, out T output, float maxDst = Mathf.Infinity, int layer = ~0)
        {
            output = default;

            Ray ray = eventCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.transform == transform)
                {
                    Vector3 virtualPos = new Vector3(hit.textureCoord.x, hit.textureCoord.y);
                    virtualPos.x *= _screenCamera.targetTexture.width;
                    virtualPos.y *= _screenCamera.targetTexture.height;

                    return ScreenSpaceCast<T>(virtualPos, out output, maxDst, layer);
                }
            }

            return false;
        }

        private bool ScreenSpaceCast(Vector3 pos, out RaycastHit hit, float maxDst = Mathf.Infinity, int layer = ~0)
        {
            hit = default;
            Ray ray = _screenCamera.ScreenPointToRay(pos);
            bool hasHit = Physics.Raycast(ray, out hit, maxDst, layer);
            return hasHit;
        }

        public bool Raycast(Vector3 mousePosition, out RaycastHit hit, float maxDst = Mathf.Infinity, int layer = ~0)
        {
            hit = default;
            Ray ray = eventCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit _hit))
            {
                if (_hit.collider.transform == transform)
                {
                    Vector3 virtualPos = new Vector3(_hit.textureCoord.x, _hit.textureCoord.y);
                    virtualPos.x *= _screenCamera.targetTexture.width;
                    virtualPos.y *= _screenCamera.targetTexture.height;

                    return ScreenSpaceCast(virtualPos, out hit, maxDst, layer);
                }
            }

            return false;
        }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }
    }
}
