using System.Collections.Generic;
using System.Linq;
using Roadway.Attribute;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Roadway
{
    [ExecuteInEditMode]
    public class TerrainRoadwayConformer : MonoBehaviour
    {
        [System.Serializable]
        private class ShoulderWidthOverride
        {
            public int splineId;
            public float shoulderWidth;
        }
        
        [Header("References")]
        [SerializeField] private Terrain _terrain;
        [Header("Settings")]
        [SerializeField, Min(0.00001f)] float _resolution = 1f;
        [SerializeField] float _heightOffset = 1f;
        [SerializeField] float _shoulderWidth = 0f;
        [SerializeField] private AnimationCurve _falloff = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private int _brushResolution = 3;

        [SerializeField, InspectorButton("ConformTerrainToRoadway")] private bool _conformTerrainToRoadway = false;
        [SerializeField, InspectorButton("ResetTerrainHeight")] private bool _resetTerrain = false;
        [SerializeField] private bool _autoReset = true;
        [SerializeField] private List<ShoulderWidthOverride> _shoulderWidthOverrides = new();

        public static TerrainRoadwayConformer Instance { get; private set; }

        public void ResetTerrainHeight()
        {
            float baseHeight = 0;
            TerrainData tData = _terrain.terrainData;

            int width = tData.heightmapResolution;
            int height = tData.heightmapResolution;

            float[,] flatHeights = new float[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flatHeights[y, x] = baseHeight; 
                }
            }

            tData.SetHeights(0, 0, flatHeights);
        }

        public void ConformTerrainToRoadway()
        {
            if (!RoadwayCreator.Instance) return;
            if (_autoReset) ResetTerrainHeight();
            Debug.Log("Here!");

            TerrainData tData = _terrain.terrainData;
            Vector3 tPos = _terrain.transform.position;

            int tRes = tData.heightmapResolution;

            float[,] heights = tData.GetHeights(0, 0, tRes, tRes);

            float terrainWidth = tData.size.x;
            float terrainHeight = tData.size.y;
            float terrainLength = tData.size.z;

            List<Roadway> roadways = RoadwayHelper.GetRoadways(RoadwayCreator.Instance.GetContainer());

            foreach (Roadway roadway in roadways)
            {
                if (RoadwayCreator.Instance.IsOnTerrainIgnoreLayer(roadway.splineIndex)) continue;
                float shoulderWidth = _shoulderWidth;
                ShoulderWidthOverride swo = _shoulderWidthOverrides.FirstOrDefault(o => o.splineId == roadway.splineIndex);
                if (swo != null)
                {
                    shoulderWidth = swo.shoulderWidth;
                }
                    
                float width = RoadwayCreator.Instance.RoadWidth(roadway.splineIndex) + shoulderWidth;
                Spline spline = RoadwayCreator.Instance.GetContainer().Splines[roadway.splineIndex];
                int numSamples = Mathf.CeilToInt(spline.GetLength() / _resolution);

                for (int i = 0; i <= numSamples; i++)
                {
                    float t = i / (float)numSamples;
                    RoadwayCreator.Instance.GetContainer().Evaluate(roadway.splineIndex, t, out float3 worldPos, out float3 tangent, out float3 upVector);
                    float3 normal = Vector3.Normalize(tangent);
                    float3 up = Vector3.Normalize(upVector);
                    float3 right = Vector3.Cross(up, normal);

                    worldPos = worldPos - _heightOffset * up;

                    int samplesPerSide = Mathf.CeilToInt(width);
                    for (int j = -samplesPerSide; j <= samplesPerSide; j++)
                    {
                        float offset = (j / (float)samplesPerSide) * width;
                        float3 samplePos = worldPos + right * offset;

                        int mapX = Mathf.RoundToInt((samplePos.x - tPos.x) / terrainWidth * tRes);
                        int mapZ = Mathf.RoundToInt((samplePos.z - tPos.z) / terrainLength * tRes);

                        if (mapX < 0 || mapX >= tRes || mapZ < 0 || mapZ >= tRes) continue;

                        float height01 = (samplePos.y - tPos.y) / terrainHeight;
                        float blend = 1f - Mathf.Abs(offset) / width; 
                        float newHeight = Mathf.Lerp(heights[mapZ, mapX], height01, blend);

                        heights[mapZ, mapX] = Mathf.Max(heights[mapZ, mapX], newHeight);
                    }
                }
            }

            tData.SetHeights(0, 0, heights);
        }

        private void Awake()
        {
            Instance = this;
            //ConformTerrainToRoadway();
        }
        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            Instance = null;
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
