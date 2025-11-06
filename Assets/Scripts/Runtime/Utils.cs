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
        public static List<Transform> FindAllWithTag(this Transform t, string tag)
        {
            List<Transform> found = new();
            t.FindAllWithTag(tag, found);
            return found;
        }
        
        public static void FindAllWithTag(this Transform t, string tag, List<Transform> results)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Transform c = t.GetChild(i);
                if (c.CompareTag(tag)) results.Add(c);
                c.FindAllWithTag(tag, results);
            }
        }
    }
}
