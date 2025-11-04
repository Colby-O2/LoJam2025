using System;
using System.Collections.Generic;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor.Splines;
#endif
using UnityEngine;
using UnityEngine.Splines;

namespace Roadway
{
    [Serializable]
    public struct Roadway
    {
        public int splineIndex;
        public List<float> segments;
    }

    public class RoadwayHelper : MonoBehaviour
    {
        public static void GetRoadwayWidthAt(SplineContainer roadway, int splineIndex, float t, float width, out Vector3 p1, out Vector3 p2)
        {
            roadway.Evaluate(splineIndex, t, out float3 position, out float3 forward, out float3 up);
            float3 right = Vector3.Cross(Vector3.Normalize(forward), up);
            p1 = position + width * right;
            p2 = position - width * right;
        }
        
        public static float GetKnotTInSpline(SplineContainer container, int spline, int knot)
        {
            return container[spline].ConvertIndexUnit(knot, PathIndexUnit.Knot, PathIndexUnit.Normalized);
        }
        
        public static float GetKnotDistanceInSpline(SplineContainer container, int spline, int knot)
        {
            return container[spline].ConvertIndexUnit(knot, PathIndexUnit.Knot, PathIndexUnit.Distance);
        }
        
        public static float GetTBetweenKnots(SplineContainer container, int spline, int knotFrom, int knotTo)
        {
            float distance = 0;
            for (int k = knotFrom; k < knotTo; k++)
            {
                distance += GetKnotTInSpline(container, spline, k + 1) - GetKnotTInSpline(container, spline, k);
            }

            return distance;
        }

        public static float GetDistanceBetweenKnots(SplineContainer container, int spline, int knotFrom, int knotTo)
        {
            float distance = 0;
            for (int k = knotFrom; k < knotTo; k++)
            {
                distance += container[spline].GetCurveLength(k);
            }

            return distance;
        }

        //public static SplineContainer GetRoadwayContainer()
        //{
        //    GameObject paths = GameObject.FindWithTag("RoadwayPath");
        //    if (paths.TryGetComponent(out SplineContainer splineContainer))
        //    {
        //        return splineContainer;
        //    }

        //    return null;
        //}

        public static List<Roadway> GetRoadways(SplineContainer roadwayContainer)
        {
            if (!roadwayContainer) return new List<Roadway>();

            List<Roadway> roadways = new List<Roadway>();
            Debug.Log(roadwayContainer);

            for (int i = 0; i < roadwayContainer.Splines.Count; i++)
            {
                Roadway roadway = new Roadway();
                roadway.splineIndex = i;
                roadways.Add(roadway);
            }

            return roadways;
        }

        //public static List<Roadway> GetRoadways()
        //{
        //    List<Roadway> roadways = new List<Roadway>();

        //    SplineContainer roadwayContainer = GetRoadwayContainer();
        //    Debug.Log(roadwayContainer);

        //    for (int i = 0; i < roadwayContainer.Splines.Count; i++)
        //    {
        //        Roadway roadway = new Roadway();
        //        roadway.splineIndex = i;
        //        roadways.Add(roadway);
        //    }

        //    return roadways;
        //}

#if UNITY_EDITOR

        public static List<SelectableKnot> GetSelectedRoadwayKnots(bool onlyEnds = true)
        {
            SplineContainer roadwayContainers = RoadwayCreator.Instance.GetContainer();
            List<SelectableKnot> selectedKnots = new List<SelectableKnot>();

            if (SplineSelection.Count > 0)
            {
                for (int i = 0; i < roadwayContainers.Splines.Count; i++)
                {
                    List<SelectableKnot> selected = new List<SelectableKnot>();
                    SplineInfo info = new SplineInfo(roadwayContainers, i);
                    SplineSelection.GetElements(info, selected);

                    for (int j = selected.Count - 1; j >= 0; j--)
                    {
                        SelectableKnot knot = selected[j];
                        if (onlyEnds && knot.KnotIndex != 0 && knot.KnotIndex != knot.SplineInfo.Spline.Count - 1) selected.RemoveAt(j);
                    }

                    selectedKnots.AddRange(selected);
                }
            }

            return selectedKnots;
        }
#endif
    }
}
