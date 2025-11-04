using System.Collections.Generic;
using UnityEngine;

namespace Roadway
{
    [CreateAssetMenu(fileName = "RoadwaySO", menuName = "Roadway/RoadwaySO")]
    public class RoadwaySO : ScriptableObject
    {
        public List<RoadwayIntersection> intersections = new List<RoadwayIntersection>();
    }
}
