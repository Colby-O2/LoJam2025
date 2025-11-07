using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PlazmaGames.ProGen.Sampling;

namespace LJ2025
{
    [ExecuteInEditMode]
    public class TreePoissonPlacer : MonoBehaviour
    {
        [Header("Sampling Area")]
        public Vector2 regionSize = new Vector2(50, 50);
        public float radius = 5f;

        [Header("Tree Settings")]
        public GameObject treePrefab;
        public Transform parent;
        public int maxTrees = 100;
        public Vector2 heightRange;
        public float scale;
       

        [Header("Seed Settings")]
        public int? seed = null;

        [ContextMenu("Generate Trees (Poisson)")]
        public void GenerateTrees()
        {
            if (treePrefab == null)
            {
                Debug.LogError("Tree prefab not assigned!");
                return;
            }

            if (parent != null)
            {
                while (parent.childCount > 0)
                {
#if UNITY_EDITOR
                    DestroyImmediate(parent.GetChild(0).gameObject);
#else
                Destroy(parent.GetChild(0).gameObject);
#endif
                }
            }

            PoissonSampler sampler = new PoissonSampler(regionSize.x, regionSize.y, radius, seed);
            List<Vector2> samples = sampler.Sample(maxTrees);

            foreach (var sample in samples)
            {
                Vector3 position = new Vector3(sample.x, 0f, sample.y);
                if (parent != null)
                    position += parent.position;

                GameObject tree = (GameObject)Instantiate(treePrefab);
                tree.transform.position = position;
                tree.transform.localScale = new Vector3(scale, Random.Range(heightRange.x, heightRange.y), scale);
                tree.transform.SetParent(parent);
            }

            Debug.Log($"Generated {samples.Count} trees via Poisson sampling!");
        }
    }
}
