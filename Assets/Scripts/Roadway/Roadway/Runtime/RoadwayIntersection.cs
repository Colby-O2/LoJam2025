using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Roadway.Attribute;


#if UNITY_EDITOR
using UnityEditor.Splines;
#endif

namespace Roadway
{
    [Serializable]
    public struct JunctionInfo
    {
        public int splineIndex;
        public int knotIndex;

        public JunctionInfo(int splineIndex, int knotIndex) 
        { 
            this.splineIndex = splineIndex;
            this.knotIndex = knotIndex;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is JunctionInfo otherJunction)
            {
                return otherJunction.splineIndex == this.splineIndex && otherJunction.knotIndex == this.knotIndex;
            }
            return false;
        }
    }

    [Serializable]
    public class RoadwayIntersection
    {
        [SerializeField, ReadOnly] private List<JunctionInfo> _junctions;
        [SerializeField] public List<float> curveWeights;

        public void AddJunction(int splineIndex, int knotIndex, List<float> curveWeights)
        {
            this.curveWeights = curveWeights;
            _junctions ??= new List<JunctionInfo>();
            _junctions.Add(new JunctionInfo(splineIndex, knotIndex));
        }

        public List<JunctionInfo> GetJunctions()
        {
            return _junctions;
        }

        public bool HasJunction(Vector3 leftPT, Vector3 rightPT, float roadWidth)
        {
            foreach (JunctionInfo junction in _junctions)
            {
                float t = junction.knotIndex == 0 ? 0f : 1f;

                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), junction.splineIndex, t, roadWidth, out Vector3 p1, out Vector3 p2);

                if ((p1 == leftPT && p2 == rightPT) || (p1 == rightPT && p2 == leftPT)) return true;
            }

            return false;
        }

#if UNITY_EDITOR
        public bool HasJunction(SelectableKnot knot)
        {
            return HasJunction(new JunctionInfo(
                    knot.SplineInfo.Index,
                    knot.KnotIndex
            ));
        }
#endif

        public bool HasJunction(JunctionInfo junction)
        {
            for (int i = 0; i < _junctions.Count; i++)
            {
                bool hasJunction = _junctions.Contains(junction);
                if (hasJunction) return true;
            }
            return false;
        }
#if UNITY_EDITOR
        public bool HasJunctions(List<SelectableKnot> knots, bool checkIfSame)
        {
            if (knots == null || knots.Count == 0) return false;

            List<JunctionInfo> junctions = new List<JunctionInfo>();

            foreach (SelectableKnot knot in knots)
            {
                junctions.Add(new JunctionInfo(
                    knot.SplineInfo.Index,
                    knot.KnotIndex
                ));
            }

            return HasJunctions(junctions, checkIfSame);
        }
#endif
        public bool HasJunctions(List<JunctionInfo> junctions, bool checkIfSame)
        {
            if (junctions == null || junctions.Count == 0) return false;

            bool isSame = (junctions != null && _junctions != null && _junctions.Count == junctions.Count) || !checkIfSame;

            for (int i = 0; i < junctions.Count; i++)
            {
                isSame &= _junctions.Contains(junctions[i]);
                if (!isSame) return false;
            }

            return isSame;
        }
#if UNITY_EDITOR
        public bool HasAtLeastOneJunction(List<SelectableKnot> knots)
        {
            if (knots == null || knots.Count == 0) return false;

            List<JunctionInfo> junctions = new List<JunctionInfo>();

            foreach (SelectableKnot knot in knots)
            {
                junctions.Add(new JunctionInfo(
                    knot.SplineInfo.Index,
                    knot.KnotIndex
                ));
            }

            return HasAtLeastOneJunction(junctions);
        }
#endif
        public bool HasAtLeastOneJunction(List<JunctionInfo> junctions)
        {
            if (junctions == null || junctions.Count == 0) return false;

            for (int i = 0; i < junctions.Count; i++)
            {
                bool hasJunction= _junctions.Contains(junctions[i]);
                if (hasJunction) return true;
            }

            return false;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is RoadwayIntersection otherIntersection) return HasJunctions(otherIntersection._junctions, true);
            return false;
        }
    }
}
