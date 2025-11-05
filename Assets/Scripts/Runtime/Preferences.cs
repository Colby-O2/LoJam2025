using UnityEngine;

namespace LJ2025
{
    [CreateAssetMenu(fileName = "DefaultPreferences", menuName = "Preferences")]
    public class Preferences : ScriptableObject
    {
        public Texture2D Cursor;
        public float DialogueSpeedMul;
    }
}
