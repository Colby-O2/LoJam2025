using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Linq;
using Roadway.Attribute;



#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor.Splines;
#endif

namespace Roadway
{
    [System.Serializable]
    public class RoadwayData 
    {
        [SerializeField] public List<int> SplineIndices = new List<int>();
        [SerializeField] public List<int> Intersections = new List<int>();
        [SerializeField] public Material RoadMat;
        [SerializeField] public Material IntersectionMat;
        [SerializeField] public Material CurbMat;
        [SerializeField] public float RoadWidth;
        [SerializeField] public float CurbWidth;
        [SerializeField] public float CurbHeight;
        [SerializeField] public bool TerrainIgnore = false;
    }

    [ExecuteInEditMode]
    public class RoadwayCreator : MonoBehaviour
    {
        [Header("Generation Parameters")]
        [SerializeField, InspectorButton("GenerateRoadway")] private bool _regenerate = false;
        [SerializeField, InspectorButton("ClearIntersections")] private bool _removeAllIntersections = false;
        [SerializeField, InspectorButton("ClearRoadways")] private bool _destroy = false;
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private bool _debugMode = false;
        [SerializeField] private float _resolution = 1;
        [SerializeField, ReadOnly] private List<Roadway> _roadways;
        [SerializeField] private RoadwaySO _data;

        [Header("Road Parameters")]
        //[SerializeField] private float _roadWidth;
        //[SerializeField] private float _curveWidth;
        //[SerializeField] private float _curveHeight;

        [Header("Mesh Parameters")]
        [SerializeField] private RoadwayData _defaultData;
        [SerializeField] private List<RoadwayData> _overridesPresets;

        public static RoadwayCreator Instance { get; private set; }


        public SplineContainer GetContainer() => _splineContainer; 

        private GameObject _roadwayHolder;

        public List<Roadway> GetRoadways() => _roadways;

        public List<RoadwayIntersection> GetIntersections()
        {
            return _data.intersections;
        }

        public Transform GetRoadwayHolder() 
        {
            if (_roadwayHolder == null) _roadwayHolder = GameObject.Find("Roadway");
            if (_roadwayHolder == null) _roadwayHolder = new GameObject("Roadway");
            return _roadwayHolder.transform; 
        }

#if UNITY_EDITOR
        public RoadwayIntersection HasIntersectionWithAtLeastOneJunction(List<SelectableKnot> knots)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasAtLeastOneJunction(knots)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithAtLeastOneJunction(List<JunctionInfo> junctions)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasAtLeastOneJunction(junctions)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithJunctions(List<JunctionInfo> junctions, bool checkIfSame = true)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasJunctions(junctions, checkIfSame)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithJunctions(List<SelectableKnot> knots, bool checkIfSame = true)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasJunctions(knots, checkIfSame)) return intersection;
            }
            return null;
        }


        public void AddIntersection(RoadwayIntersection intersection)
        {
            RemoveIntersection(intersection);
            GetIntersections().Add(intersection);
        }

        public void RemoveIntersection(RoadwayIntersection intersection)
        {
            if (GetIntersections().Contains(intersection)) GetIntersections().Remove(intersection);
        }

        public void ClearRoadways()
        {
            RoadwayMeshGenerator.Clear();
            if (_roadways == null) _roadways = new List<Roadway>();
            else _roadways.Clear();
            if (_roadwayHolder != null)
            {
                while (_roadwayHolder.transform.childCount > 0) DestroyImmediate(_roadwayHolder.transform.GetChild(0).gameObject);
            }
        }

        public void GenerateRoadway()
        {
            ClearRoadways();

            _roadways = RoadwayHelper.GetRoadways(_splineContainer);

            for (int i = 0; i < _roadways.Count; i++)
            {
                Roadway roadway = _roadways[i];
                roadway.segments = new List<float>();
                float length = _splineContainer.Splines[roadway.splineIndex].GetLength();
                int numberOfSegments = Mathf.CeilToInt(length / _resolution);
                for (float j = 0.0f; j <= numberOfSegments; j++) roadway.segments.Add(j / numberOfSegments);



                RoadwayData data = null;
                if (_overridesPresets != null && _overridesPresets.Any(e => e.SplineIndices.Contains(roadway.splineIndex)))
                {
                    data = _overridesPresets.First(e => e.SplineIndices.Contains(roadway.splineIndex));
                }
                else
                {
                    data = _defaultData;
                }

                RoadwayMeshGenerator.SetMaterials(data);
                RoadwayMeshGenerator.GenerateRoadMesh(roadway, data.RoadWidth, data.CurbWidth, data.CurbHeight);
            }

            List<RoadwayIntersection> intersections = GetIntersections();
            for (int i = 0; i < intersections.Count; i++) 
            {
                RoadwayIntersection intersection = intersections[i];

                RoadwayData data = null;
                if (_overridesPresets != null && _overridesPresets.Any(e => e.Intersections.Contains(i)))
                {
                    data = _overridesPresets.First(e => e.Intersections.Contains(i));
                }
                else
                {
                    data = _defaultData;
                }

                RoadwayMeshGenerator.SetMaterials(data);
                RoadwayMeshGenerator.GenerateIntersectionMesh(intersection, data.RoadWidth, data.CurbWidth, data.CurbHeight);
            }
        }

        public void ClearIntersections()
        {
            GetIntersections().Clear();
            GenerateRoadway();
        }

        private void OnSplineChanged(Spline _, int __, SplineModification ___)
        {
            GenerateRoadway();
            //if (TerrainRoadwayConformer.Instance != null) TerrainRoadwayConformer.Instance.ConformTerrainToRoadway();
        }
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            RoadwayMeshGenerator.Parent = GetRoadwayHolder();
            RoadwayMeshGenerator.SetMaterials(_defaultData);
            Spline.Changed += OnSplineChanged;
#endif
            Instance = this;
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            Spline.Changed -= OnSplineChanged;
#endif
            Instance = null;
        }

#if UNITY_EDITOR
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //if (Instance != null) Instance.GenerateRoadway();
            //if (TerrainRoadwayConformer.Instance != null && TerrainRoadwayConformer.Instance.enabled) TerrainRoadwayConformer.Instance.ConformTerrainToRoadway();
        }
#endif
        public float RoadWidth(int splineIndex) 
        { 
            return (_overridesPresets != null && _overridesPresets.Any(e => e.SplineIndices.Contains(splineIndex))) ? _overridesPresets.First(e => e.SplineIndices.Contains(splineIndex)).RoadWidth : _defaultData.RoadWidth;
        }

        public bool IsOnTerrainIgnoreLayer(int splineIndex)
        {
            
            return (_overridesPresets != null && _overridesPresets.Any(e => e.SplineIndices.Contains(splineIndex))) ? _overridesPresets.First(e => e.SplineIndices.Contains(splineIndex)).TerrainIgnore : _defaultData.TerrainIgnore;
        }
    }
}

