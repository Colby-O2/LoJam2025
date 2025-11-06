using System.Collections.Generic;
using UnityEngine;

namespace System.Runtime.CompilerServices
{
    class IsExternalInit
    {
  
    }
}

namespace LJ2025
{
    public static class Utils
    {
        public static List<Transform> FindAllWithTag(this Transform t, string tag, bool recurse = true)
        {
            List<Transform> found = new();
            t.FindAllWithTag(tag, found, recurse);
            return found;
        }
        
        public static void FindAllWithTag(this Transform t, string tag, List<Transform> results, bool recurse = true)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                if (c.CompareTag(tag)) results.Add(c);
                if (recurse) c.FindAllWithTag(tag, results);
            }
        }
        
        public static Transform FindWithTag(this Transform t, string tag, bool recurse = true)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                if (c.CompareTag(tag)) return c;
                if (recurse)
                {
                    Transform down = FindWithTag(c, tag, true);
                    if (down != null) return down;
                }
            }
            return null;
        }
    }
}
