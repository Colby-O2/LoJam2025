using UnityEngine;

namespace LJ2025
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using System.Collections.Generic;
    using UnityEngine.InputSystem;

    public class RaycastDebugger : MonoBehaviour
    {
        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                PointerEventData data = new PointerEventData(EventSystem.current);
                data.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(data, results);

                Debug.Log("Raycast hits:");
                foreach (var r in results)
                    Debug.Log(" • " + r.gameObject.name);
            }
        }
    }
}
