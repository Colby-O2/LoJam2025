using UnityEngine;
using UnityEngine.UIElements;

namespace LJ2025
{
    [System.Serializable]
    public class MoveableSettings
    {
        public enum RotateType
        {
            DoNot,
            JustYAxis,
            Full,
        }
        public string name;
        public float holdDistance = 0.4f;
        public RotateType rotateType = RotateType.JustYAxis;
        public SphereCollider bounds;

        public float BoundingRadius() => this.bounds.radius;
    }
}
