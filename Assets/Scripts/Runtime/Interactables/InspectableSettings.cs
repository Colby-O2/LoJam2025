using UnityEngine;

namespace LJ2025
{
    public enum InspectableLookType
    {
        Pickup,
        LookAt,
    }
    [System.Serializable]
    public class InspectableSettings
    {
        public string title = "";
        public string name;
        public InspectableLookType lookType;
        public Transform lookAtTarget;
        public float offsetDistance;
        public bool rotatePickup;
        public Vector3 pickupRotation;
        public string readText;
        public bool allowRotate;
        public SphereCollider bounds;
        public bool hasInteractions = false;

        public float BoundingRadius() => this.bounds.radius;
    }
}
