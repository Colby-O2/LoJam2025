using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace Roadway
{
    struct Edge
    {
        public Vector3 left;
        public Vector3 right;
        public Vector3 center;

        public Edge(Vector3 left, Vector3 right)
        {
            this.left = left;
            this.right = right;
            this.center = (right + left) / 2.0f;
        }
    }

    public class RoadwayMeshGenerator
    {
        public static List<GameObject> Meshes { get; private set; }

        public static Transform Parent { get; set; }
        public static Material RoadMat { get; set; }
        public static Material IntersectionMat { get; set; }
        public static Material CurbMat { get; set; }

        public static void SetMaterials(RoadwayData mat)
        {
            RoadMat = mat.RoadMat;
            IntersectionMat = mat.IntersectionMat;
            CurbMat = mat.CurbMat;
        }

        public static void Clear()
        {
            if (Meshes == null) return;
            if (Meshes.Count > 0) foreach (GameObject go in Meshes) if (go != null) GameObject.DestroyImmediate(go);
            Meshes.Clear();
        }

        private static Vector3 GetIntersectionInnerEdges(RoadwayIntersection intersection, float roadWidth, ref List<Edge> edges, ref List<Vector3> points)
        {
            Vector3 center = Vector3.zero;

            foreach (JunctionInfo junction in intersection.GetJunctions())
            {
                float t = junction.knotIndex == 0 ? 0f : 1f;

                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), junction.splineIndex, t, roadWidth, out Vector3 rightPT, out Vector3 leftPT);

                edges.Add(junction.knotIndex == 0 ? new Edge(leftPT, rightPT) : new Edge(rightPT, leftPT));
                center += rightPT;
                center += leftPT;
            }

            center = center / (2 * edges.Count);

            edges.Sort((x, y) => {

                Vector3 xDir = x.center - center;
                Vector3 yDir = y.center - center;

                float xAngle = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
                float yAngle = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

                if (xAngle > yAngle) return 1;
                else if (xAngle < yAngle) return -1;
                else return 0;
            });

            for (int i = 1; i <= edges.Count; i++)
            {
                Vector3 a = edges[i - 1].left;
                points.Add(a);
                Vector3 b = (i < edges.Count) ? edges[i].right : edges[0].right;
                Vector3 mid = Vector3.Lerp(a, b, 0.5f);

                Vector3 dir = center - mid;
                mid = mid - dir;
                Vector3 c = Vector3.Lerp(mid, center, intersection.curveWeights[i - 1]);

                BezierCurve curve = new BezierCurve(a, c, b);
                for (float t = 0.0f; t < 1.0f; t += 0.1f)
                {
                    Vector3 pos = CurveUtility.EvaluatePosition(curve, t);
                    points.Add(pos);
                }

                points.Add(b);
            }

            return center;
        }

        private static Vector3 GetIntersectionOuterEdges(RoadwayIntersection intersection, float roadWidth, float curveWidth, ref List<Edge> edges, ref List<Vector3> points)
        {
            Vector3 center = Vector3.zero;

            foreach (JunctionInfo junction in intersection.GetJunctions())
            {
                float t = junction.knotIndex == 0 ? 0f : 1f;

                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), junction.splineIndex, t, roadWidth + curveWidth, out Vector3 rightPT, out Vector3 leftPT);

                edges.Add(junction.knotIndex == 0 ? new Edge(leftPT, rightPT) : new Edge(rightPT, leftPT));
                center += rightPT;
                center += leftPT;
            }

            center = center / (2 * edges.Count);

            edges.Sort((x, y) => {

                Vector3 xDir = x.center - center;
                Vector3 yDir = y.center - center;

                float xAngle = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
                float yAngle = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

                if (xAngle > yAngle) return 1;
                else if (xAngle < yAngle) return -1;
                else return 0;
            });

            for (int i = 1; i <= edges.Count; i++)
            {
                Vector3 a = edges[i - 1].left;
                points.Add(a);
                Vector3 b = (i < edges.Count) ? edges[i].right : edges[0].right;
                Vector3 mid = Vector3.Lerp(a, b, 0.5f);

                Vector3 dir = center - mid;
                mid = mid - dir;
                Vector3 c = Vector3.Lerp(mid, center, intersection.curveWeights[i - 1]);

                BezierCurve curve = new BezierCurve(a, c, b);
                for (float t = 0.0f; t < 1.0f; t += 0.1f)
                {
                    Vector3 pos = CurveUtility.EvaluatePosition(curve, t);
                    points.Add(pos);
                }

                points.Add(b);
            }

            return center;
        }

        private static void GenerateIntersection(Vector3 center, List<Vector3> points, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            int vOffset = vertices.Count;

            Vector2 centerUV = new Vector2(0.5f, 0.5f);

            for (int j = 1; j <= points.Count; j++)
            {
                vertices.Add(center);
                vertices.Add(points[j - 1]);

                uvs.Add(centerUV);
                uvs.Add(new Vector2((points[j - 1].x - center.x) * 0.5f + 0.5f, (points[j - 1].z - center.z) * 0.5f + 0.5f));

                if (j == points.Count)
                {
                    vertices.Add(points[0]);
                    uvs.Add(new Vector2((points[0].x - center.x) * 0.5f + 0.5f, (points[0].z - center.z) * 0.5f + 0.5f));
                }
                else
                {
                    vertices.Add(points[j]);
                    uvs.Add(new Vector2((points[j].x - center.x) * 0.5f + 0.5f, (points[j].z - center.z) * 0.5f + 0.5f));
                }

                triangles.Add(vOffset + ((j - 1) * 3) + 0);
                triangles.Add(vOffset + ((j - 1) * 3) + 1);
                triangles.Add(vOffset + ((j - 1) * 3) + 2);
            }
        }

        private static void GenerateIntersectionCurve(RoadwayIntersection intersection, Vector3 center, List<Vector3> pointsInner, List<Vector3> pointsOuter, List<Vector3> pointsOuter2, float roadWidth, float curveHeight, float curveWidth, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            if (curveHeight < 0.001f || curveWidth < 0.001f) return;

            int vOffset = vertices.Count;
            float uvScale = 0.1f;

            for (int j = 1; j <= pointsInner.Count; j++)
            {
                Vector3 p1 = pointsInner[j - 1];
                Vector3 p2 = pointsInner[j - 1] + Vector3.up * curveHeight;
                Vector3 p3 = pointsOuter[j - 1] + Vector3.up * curveHeight;
                //Vector3 p4 = pointsOuter[j - 1];
                Vector3 p5 = pointsOuter2[j - 1] - Vector3.up * 5f * curveHeight;
                Vector3 p6;
                Vector3 p7;
                Vector3 p8;
                //Vector3 p9;
                Vector3 p10;
                if (j == pointsInner.Count)
                {
                    p6 = pointsInner[0];
                    p7 = pointsInner[0] + Vector3.up * curveHeight;
                    p8 = pointsOuter[0] + Vector3.up * curveHeight;
                    //p9 = pointsOuter[0];
                    p10 = pointsOuter2[0] - Vector3.up * 5f * curveHeight;
                }
                else
                {
                    p6 = pointsInner[j];
                    p7 = pointsInner[j] + Vector3.up * curveHeight;
                    p8 = pointsOuter[j] + Vector3.up * curveHeight;
                    //p9 = pointsOuter[j];
                    p10 = pointsOuter2[j] - Vector3.up * 5f * curveHeight;
                }

                if (intersection.HasJunction(p1, p6, roadWidth)) continue;

                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                //vertices.Add(p4);
                vertices.Add(p5);
                vertices.Add(p6);
                vertices.Add(p7);
                vertices.Add(p8);
                //vertices.Add(p9);
                vertices.Add(p10);

                uvs.Add(new Vector2(p1.x * uvScale, p1.z * uvScale));
                uvs.Add(new Vector2(p2.x * uvScale, p2.z * uvScale));
                uvs.Add(new Vector2(p3.x * uvScale, p3.z * uvScale));
                //uvs.Add(new Vector2(p4.x * uvScale, p4.z * uvScale));
                uvs.Add(new Vector2(p5.x * uvScale, p5.z * uvScale));
                uvs.Add(new Vector2(p6.x * uvScale, p6.z * uvScale));
                uvs.Add(new Vector2(p7.x * uvScale, p7.z * uvScale));
                uvs.Add(new Vector2(p8.x * uvScale, p8.z * uvScale));
                //uvs.Add(new Vector2(p9.x * uvScale, p9.z * uvScale));
                uvs.Add(new Vector2(p10.x * uvScale, p10.z * uvScale));

                // Curve Side Inner
                triangles.Add(vOffset + 0);
                triangles.Add(vOffset + 1);
                triangles.Add(vOffset + 4);

                triangles.Add(vOffset + 4);
                triangles.Add(vOffset + 1);
                triangles.Add(vOffset + 5);

                // Curve Top
                triangles.Add(vOffset + 2);
                triangles.Add(vOffset + 5);
                triangles.Add(vOffset + 1);

                triangles.Add(vOffset + 5);
                triangles.Add(vOffset + 2);
                triangles.Add(vOffset + 6);

                // Curve Side Outer
                //triangles.Add(vOffset + 7);
                //triangles.Add(vOffset + 2);
                //triangles.Add(vOffset + 3);

                //triangles.Add(vOffset + 7);
                //triangles.Add(vOffset + 3);
                //triangles.Add(vOffset + 8);

                // Curve Side Outer
                triangles.Add(vOffset + 2);
                triangles.Add(vOffset + 3);
                triangles.Add(vOffset + 6);

                triangles.Add(vOffset + 3);
                triangles.Add(vOffset + 7);
                triangles.Add(vOffset + 6);

                vOffset = vertices.Count;
            }
        }

        private static void CleanMesh(ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            Dictionary<Vector3, int>  vertexMap = new Dictionary<Vector3, int>();

            List <Vector3> uniqueVerts = new List<Vector3>();
            List<Vector2> uniqueUvs = new List<Vector2>();

            int[] remapped = new int[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                if (vertexMap.TryGetValue(v, out int existingIndex))
                {
                    remapped[i] = existingIndex;
                }
                else
                {
                    int newIndex = uniqueVerts.Count;
                    uniqueVerts.Add(v);
                    uniqueUvs.Add(uvs[i]);
                    vertexMap[v] = newIndex;
                    remapped[i] = newIndex;
                }
            }

            int[] outTriangles = new int[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                outTriangles[i] = remapped[triangles[i]];
            }

            vertices = uniqueVerts;
            triangles = outTriangles.ToList();
            uvs = uniqueUvs;
        }

        public static void GenerateIntersectionMesh(RoadwayIntersection intersection, float roadWidth, float curveWidth, float curveHeight)
        {
            GameObject roadwayGameObject = new GameObject("intersection");
            roadwayGameObject.layer = LayerMask.NameToLayer("Roadway");
            roadwayGameObject.transform.parent = Parent;
            MeshRenderer mr = roadwayGameObject.AddComponent<MeshRenderer>();
            MeshFilter mf = roadwayGameObject.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> trianglesRoad = new List<int>();
            List<int> trianglesCurb = new List<int>();

            List<Edge> edgesInner = new List<Edge>();
            List<Vector3> pointsInner = new List<Vector3>();
            List<Edge> edgesOuter = new List<Edge>();
            List<Vector3> pointsOuter= new List<Vector3>();
            List<Edge> edgesOuter2 = new List<Edge>();
            List<Vector3> pointsOuter2 = new List<Vector3>();

            Vector3 center = GetIntersectionInnerEdges(intersection, roadWidth, ref edgesInner, ref pointsInner);
            GetIntersectionOuterEdges(intersection, roadWidth, curveWidth, ref edgesOuter, ref pointsOuter);
            GetIntersectionOuterEdges(intersection, roadWidth, 2f * curveWidth, ref edgesOuter2, ref pointsOuter2);

            GenerateIntersectionCurve(intersection, center, pointsInner, pointsOuter, pointsOuter2, roadWidth, curveHeight, curveWidth, ref vertices, ref trianglesCurb, ref uvs);
            CleanMesh(ref vertices, ref trianglesCurb, ref uvs);
            GenerateIntersection(center, pointsInner, ref vertices, ref trianglesRoad, ref uvs);

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.subMeshCount = 2;
            mesh.SetTriangles(trianglesRoad, 0);
            mesh.SetTriangles(trianglesCurb, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mr.materials = new Material[] { IntersectionMat, CurbMat };
            mf.mesh = mesh;

            roadwayGameObject.AddComponent<MeshCollider>();

            if (Meshes == null) Meshes = new List<GameObject>();
            Meshes.Add(roadwayGameObject);
        }

        private static void CreateRoad(Roadway roadway, float roadWidth, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            int vOffset = vertices.Count;

            for (int i = 0; i < roadway.segments.Count; i++)
            {
                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), roadway.splineIndex, roadway.segments[i], roadWidth, out Vector3 rightPT, out Vector3 leftPT);
                vertices.Add(rightPT);
                vertices.Add(leftPT);
            }

            int idx = 0;
            float uvOffset = 0;
            for (int i = 0; i < roadway.segments.Count - 1; i++)
            {
                idx = vOffset + i * 2;

                triangles.Add(idx);
                triangles.Add(idx + 2);
                triangles.Add(idx + 1);

                triangles.Add(idx + 1);
                triangles.Add(idx + 2);
                triangles.Add(idx + 3);

                Vector3 p1 = vertices[idx];
                Vector3 p3 = vertices[idx + 2];
                float dist = Vector3.Distance(p1, p3);
                float uvDist = uvOffset + dist;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1) });
                uvOffset += dist;
            }

            uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1) });

            if (RoadwayCreator.Instance.GetContainer().Splines[roadway.splineIndex].Closed)
            {
                idx = (roadway.segments.Count - 1) * 2;

                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(idx);

                triangles.Add(1);
                triangles.Add(idx + 1);
                triangles.Add(idx);
            }
        }

        private static void CreateCurve(Roadway roadway, float roadWidth, float curveHeight, float curveWidth, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            if (curveHeight < 0.001f || curveWidth < 0.001f) return;

            int vOffset = vertices.Count;

            float uvScale = 0.1f;

            for (int i = 0; i < roadway.segments.Count; i++)
            {
                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), roadway.splineIndex, roadway.segments[i], roadWidth, out Vector3 rightPT, out Vector3 leftPT);
                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), roadway.splineIndex, roadway.segments[i], roadWidth + curveWidth, out Vector3 curbRightPT, out Vector3 curbLeftPT);
                RoadwayHelper.GetRoadwayWidthAt(RoadwayCreator.Instance.GetContainer(), roadway.splineIndex, roadway.segments[i], roadWidth + 2f * curveWidth, out Vector3 edgeRightPT, out Vector3 edgeLeftPT);

                Vector3[] points = new Vector3[]
                {
                    rightPT,
                    rightPT + Vector3.up * curveHeight,
                    curbRightPT + Vector3.up * curveHeight,
                    //curbRightPT,
                    leftPT,
                    leftPT + Vector3.up * curveHeight,
                    curbLeftPT + Vector3.up * curveHeight,
                    //curbLeftPT,
                    edgeRightPT - Vector3.up * 5f * curveHeight,
                    edgeLeftPT - Vector3.up * 5f * curveHeight,
                };

                foreach (var pt in points)
                {
                    vertices.Add(pt);
                    uvs.Add(new Vector2(pt.x * uvScale, pt.z * uvScale));
                }
            }

            int idx = 0;
            for (int i = 0; i < roadway.segments.Count - 1; i++)
            {
                idx = vOffset + i * 8;

                // Right Curb Inner Side
                triangles.Add(idx + 8); //*
                triangles.Add(idx);
                triangles.Add(idx + 1);

                triangles.Add(idx + 8); //*
                triangles.Add(idx + 1);
                triangles.Add(idx + 9); //*

                //// Right Curb
                triangles.Add(idx + 2);
                triangles.Add(idx + 9); //*
                triangles.Add(idx + 1);

                triangles.Add(idx + 9); //*
                triangles.Add(idx + 2);
                triangles.Add(idx + 10); //*

                ////// Right Curb Outer Side
                //triangles.Add(idx + 12);
                //triangles.Add(idx + 2);
                //triangles.Add(idx + 3);

                //triangles.Add(idx + 12);
                //triangles.Add(idx + 3);
                //triangles.Add(idx + 13);

                // Right Siding
                triangles.Add(idx + 10); //*
                triangles.Add(idx + 6); //*
                triangles.Add(idx + 14); //*

                triangles.Add(idx + 6); //*
                triangles.Add(idx + 10); //*
                triangles.Add(idx + 2);

                //// Left Curb Inner Side
                triangles.Add(idx + 3); //*
                triangles.Add(idx + 11); //*
                triangles.Add(idx + 4); //*

                triangles.Add(idx + 4); //*
                triangles.Add(idx + 11); //*
                triangles.Add(idx + 12); //*

                //// Left Curb
                triangles.Add(idx + 12); //*
                triangles.Add(idx + 5); //*
                triangles.Add(idx + 4); //*

                triangles.Add(idx + 13); //*
                triangles.Add(idx + 5); //*
                triangles.Add(idx + 12); //*

                //// Left Curb Outer Side
                //triangles.Add(idx + 6);
                //triangles.Add(idx + 16);
                //triangles.Add(idx + 7);

                //triangles.Add(idx + 7);
                //triangles.Add(idx + 16);
                //triangles.Add(idx + 17);

                // Left Siding
                triangles.Add(idx + 7); //*
                triangles.Add(idx + 13); //*
                triangles.Add(idx + 15); //*

                triangles.Add(idx + 13); //*
                triangles.Add(idx + 7); //*
                triangles.Add(idx + 5); //*

            }
        }

        public static void GenerateRoadMesh(Roadway roadway, float roadWidth, float curveWidth, float curveHeight)
        {
            if (roadway.segments.Count == 0) return;

            GameObject roadwayGameObject = new GameObject("Road");
            roadwayGameObject.layer = LayerMask.NameToLayer("Roadway");
            roadwayGameObject.transform.parent = Parent;
            MeshRenderer mr = roadwayGameObject.AddComponent<MeshRenderer>();
            MeshFilter mf = roadwayGameObject.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> trianglesRoad = new List<int>();
            List<int> trianglesCurb = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            CreateRoad(roadway, roadWidth, ref vertices, ref trianglesRoad, ref uvs);
            CreateCurve(roadway, roadWidth, curveHeight, curveWidth, ref vertices, ref trianglesCurb, ref uvs);

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.subMeshCount = 2;
            mesh.SetTriangles(trianglesRoad, 0);
            mesh.SetTriangles(trianglesCurb, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mr.materials = new Material[] { RoadMat, CurbMat };
            mf.mesh = mesh;

            roadwayGameObject.AddComponent<MeshCollider>();

            if (Meshes == null) Meshes = new List<GameObject>();
            Meshes.Add(roadwayGameObject);
        }
    }
}

