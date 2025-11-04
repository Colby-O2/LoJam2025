using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Roadway.Attribute;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Roadway
{
    [ExecuteInEditMode]
    public class RoadwayMover : MonoBehaviour
    {
        public SplineContainer splineContainer;
        public int[] splineIndicesToMove;
        public Vector3 offset = new Vector3(-3000f, -3000f, -3000f);
        [HideInInspector] public bool offsetApplied = false;
        [SerializeField, InspectorButton("ApplyOffset")] private bool _applyOffset;

        public void ApplyOffset()
        {
            if (splineContainer == null) return;

            foreach (int splineIndex in splineIndicesToMove)
            {
                if (splineIndex >= 0 && splineIndex < splineContainer.Splines.Count)
                {
                    var spline = splineContainer.Splines[splineIndex];
                    for (int i = 0; i < spline.Count; i++)
                    {
                        var knot = spline[i];
                        knot.Position += (float3)offset;
                        spline[i] = knot;
                    }
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(splineContainer);
            EditorSceneManager.MarkSceneDirty(splineContainer.gameObject.scene);
#endif

            offsetApplied = true;
        }
    }
}
