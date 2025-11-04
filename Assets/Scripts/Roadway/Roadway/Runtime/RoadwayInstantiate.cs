using System.Collections.Generic;
using UnityEngine;
using Roadway.Attribute;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine.Splines;
using System.Linq;
using Roadway.Helper;


#if UNITY_EDITOR
using UnityEditor.Splines;
#endif

namespace Roadway
{
    [ExecuteInEditMode]
    public class RoadwayInstantiate : MonoBehaviour
    {
        private float _lastViewDistanceCheck = 0;

        [System.Serializable]
        class Section
        {
            public SplineContainer container;
            public int spline;
            public int knotStart, knotEnd;
            public List<SectionOverride> instanceOverrides = new();

            public Section(SplineContainer container, int spline, int knotStart, int knotEnd)
            {
                this.container = container;
                this.spline = spline;
                this.knotStart = knotStart;
                this.knotEnd = knotEnd;
            }
        }

        [System.Serializable]
        public class InstanceInfo
        {
            public int seed;
            public GameObject prefab;
            public bool left, right;
            public float step;
            public bool align;
            public bool randomOffset;
            public Vector3 offsetFrom;
            public Vector3 offsetTo;
            public bool randomScale;
            public float scaleFrom = 1f;
            public float scaleTo = 1f;
            public bool randomRotation;
        }

        [System.Serializable]
        public class InstanceOverrideInfo
        {
            public bool oSeed;
            public int seed;

            public bool oLeft;
            public bool left;

            public bool oRight;
            public bool right;

            public bool oStep;
            public float step;

            public bool oOffset;
            public Vector3 offsetFrom;
            public Vector3 offsetTo;

            public bool oScale;
            public float scaleFrom = 1f;
            public float scaleTo = 1f;

            public bool spawnComponents = true;
        }

        [System.Serializable]
        public class SectionOverride
        {
            public int instanceId;
            public InstanceOverrideInfo overrides;
        }

        [SerializeField] private GameObject _currentControllable;
        [SerializeField] private float _viewDst = 1000f;
        [SerializeField] private float _componentVewDst = 1000f;
        [SerializeField] private List<InstanceInfo> _instances = new();
        [SerializeField] private List<Section> _sections = new();

        class DrawnPart
        {
            public Matrix4x4 localMatrix;
            public Mesh mesh;
            public Material[] materials;
            public List<Matrix4x4> matrices = new();
        }

        class DrawnInstance
        {
            public Matrix4x4[] matrices;
            public List<DrawnPart> parts = new();
        }

        class DrawnSection
        {
            public MinMaxAABB bounds = new();
            public List<DrawnInstance> instances = new();
            public bool inViewDistance;
        }

        class MeshComponent
        {
            public GameObject go;

            public MeshComponent(GameObject go)
            {
                this.go = go;
            }
        }

        [SerializeField, ReadOnly] List<GameObject> _generateNearObjects;

        private List<MeshComponent> _components;
        private List<DrawnSection> _drawnSections = new();
        private Dictionary<Material, Material> _materialClones = new();

        private bool _hasRanFirstTime = false;

        public void SetCurrentControllable(GameObject controllable)
        {
            _currentControllable = controllable;
        }

#if UNITY_EDITOR
        [InspectorButton("CreateSection")] public bool buttonCreateSection = false;
        void CreateSection()
        {
            List<SelectableKnot> knots = RoadwayHelper.GetSelectedRoadwayKnots(false);
            if (knots.Count != 2) return;
            _sections.Add(new Section(knots[0].SplineInfo.Container as SplineContainer, knots[0].SplineInfo.Index, knots[0].KnotIndex, knots[1].KnotIndex));
        }

        [InspectorButton("Generate")] public bool buttonGenerate = false;
#endif
        void Generate()
        {
            if (_components != null)
            {
                for (int i = 0; i < _components.Count; i++)
                {
                    GameObject.DestroyImmediate(_components[i].go);
                }
            }

            while (transform.childCount > 0)
            {
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
            }

            int pSeed = Mathf.FloorToInt(UnityEngine.Random.value * int.MaxValue);

            _drawnSections = new();
            _components = new();

            foreach (Section s in _sections)
            {
                float roadWidth = RoadwayCreator.Instance.RoadWidth(s.spline);
                DrawnSection ds = new();
                _drawnSections.Add(ds);
                foreach (InstanceInfo inst in _instances)
                {
                    InstanceOverrideInfo overrides = null;
                    foreach (SectionOverride so in s.instanceOverrides)
                    {
                        int iid = so.instanceId;
                        if (iid >= 0 && iid < _instances.Count && _instances[iid] == inst)
                        {
                            overrides = so.overrides;
                            break;
                        }
                    }
                    int seed = inst.seed;
                    if (overrides is { oSeed: true }) seed = overrides.seed;
                    bool leftOn = inst.left;
                    if (overrides is { oLeft: true }) leftOn = overrides.left;
                    bool rightOn = inst.right;
                    if (overrides is { oRight: true }) rightOn = overrides.right;
                    float step = inst.step;
                    if (overrides is { oStep: true }) step = overrides.step;
                    Vector3 offsetFrom = inst.offsetFrom;
                    Vector3 offsetTo = inst.offsetTo;
                    if (overrides is { oOffset: true })
                    {
                        offsetFrom = overrides.offsetFrom;
                        offsetTo = overrides.offsetTo;
                    }
                    float scaleFrom = inst.scaleFrom;
                    float scaleTo = inst.scaleTo;
                    if (overrides is { oScale: true })
                    {
                        scaleFrom = overrides.scaleFrom;
                        scaleTo = overrides.scaleTo;
                    }
                    bool spawnComponents = true;
                    if (overrides is { spawnComponents: false }) spawnComponents = false;
                    UnityEngine.Random.InitState(seed);
                    float tStart = RoadwayHelper.GetKnotTInSpline(s.container, s.spline, s.knotStart);
                    float tLength = RoadwayHelper.GetTBetweenKnots(s.container, s.spline, s.knotStart, s.knotEnd);
                    int segmentCount = Mathf.FloorToInt(RoadwayHelper.GetDistanceBetweenKnots(s.container, s.spline, s.knotStart, s.knotEnd) / step);
                    DrawnInstance di = new();
                    ds.instances.Add(di);
                    LoadDrawnParts(inst, di);
                    di.matrices = new Matrix4x4[(leftOn ? segmentCount : 0) + (rightOn ? segmentCount : 0)];
                    for (int i = 0; i < segmentCount; i++)
                    {
                        float t = tStart + i / (float)segmentCount * tLength;
                        Vector3 leftEdge, rightEdge;
                        RoadwayHelper.GetRoadwayWidthAt(s.container, s.spline, t, roadWidth, out leftEdge, out rightEdge);
                        float3 tangentF3, upF3;
                        s.container[s.spline].Evaluate(t, out var _, out tangentF3, out upF3);
                        Vector3 forward = Vector3.Normalize(tangentF3);
                        Vector3 up = Vector3.Normalize(upF3);
                        Vector3 right = Vector3.Cross(forward, up);
                        Vector3 offset;
                        Vector3 rotation = inst.align ? Quaternion.LookRotation(forward, up).eulerAngles : Vector3.zero;

                        if (inst.randomOffset)
                        {
                            offset =
                                right * UnityEngine.Random.Range(offsetFrom.x, offsetTo.x) +
                                up * UnityEngine.Random.Range(offsetFrom.y, offsetTo.y) +
                                forward * UnityEngine.Random.Range(offsetFrom.z, offsetTo.z);
                        }
                        else
                        {
                            offset =
                                right * offsetFrom.x +
                                up * offsetFrom.y +
                                forward * offsetFrom.z;
                        }

                        if (leftOn)
                        {
                            Vector3 leftPos = leftEdge + offset;
                            ds.bounds.Encapsulate(leftPos);
                            float scaleFactor = inst.randomScale ? UnityEngine.Random.Range(scaleFrom, scaleTo) : 1f;
                            float rot = inst.align ? 180f : (inst.randomRotation ? UnityEngine.Random.Range(0f, 360f) : 0f);

                            Matrix4x4 mat = Matrix4x4.TRS(
                                leftPos,
                                Quaternion.AngleAxis(rot, up) * Quaternion.Euler(rotation),
                                scaleFactor * inst.prefab.transform.localScale
                            );
                            di.matrices[i] = mat;

                            if (spawnComponents)
                            {
                                for (int j = 0; j < inst.prefab.transform.childCount; j++)
                                {
                                    if (inst.prefab.transform.GetChild(j).TryGetComponent<Light>(out Light light))
                                    {
                                        GameObject go = GameObject.Instantiate(light, leftPos, Quaternion.Euler(rotation), transform).gameObject;
                                        SetChildMatrix(go.transform, inst.prefab.transform, light.transform, mat);
                                        _components.Add(new MeshComponent(go));
                                    }
                                    if (inst.prefab.transform.GetChild(j).TryGetComponent<BoxCollider>(out BoxCollider collider))
                                    {
                                        GameObject go = GameObject.Instantiate(collider, leftPos, Quaternion.Euler(rotation), transform).gameObject;
                                        SetChildMatrix(go.transform, inst.prefab.transform, collider.transform, mat);
                                        _components.Add(new MeshComponent(go));
                                    }
                                }
                            }
                        }

                        if (inst.randomOffset)
                        {
                            offset =
                                -right * UnityEngine.Random.Range(offsetFrom.x, offsetTo.x) +
                                up * UnityEngine.Random.Range(offsetFrom.y, offsetTo.y) +
                                forward * UnityEngine.Random.Range(offsetFrom.z, offsetTo.z);
                        }
                        else
                        {
                            offset =
                                -right * offsetFrom.x +
                                up * offsetFrom.y +
                                forward * offsetFrom.z;
                        }

                        if (rightOn)
                        {
                            Vector3 rightPos = rightEdge + offset;
                            ds.bounds.Encapsulate(rightPos);
                            int index = i;
                            if (leftOn) index += segmentCount;

                            float scaleFactor = inst.randomScale ? UnityEngine.Random.Range(scaleFrom, scaleTo) : 1f;
                            float rot = inst.randomRotation ? UnityEngine.Random.Range(0f, 360f) : 0f;

                            Matrix4x4 mat = Matrix4x4.TRS(
                                rightPos,
                                Quaternion.AngleAxis(rot, up) * Quaternion.Euler(rotation),
                                scaleFactor * inst.prefab.transform.localScale
                            );

                            di.matrices[index] = mat;

                            if (spawnComponents)
                            {
                                for (int j = 0; j < inst.prefab.transform.childCount; j++)
                                {
                                    if (inst.prefab.transform.GetChild(j).TryGetComponent<Light>(out Light light))
                                    {
                                        GameObject go = GameObject.Instantiate(light, rightPos, Quaternion.Euler(rotation), transform).gameObject;
                                        SetChildMatrix(go.transform, inst.prefab.transform, light.transform, mat);
                                        _components.Add(new MeshComponent(go));
                                    }
                                    if (inst.prefab.transform.GetChild(j).TryGetComponent<BoxCollider>(out BoxCollider collider))
                                    {
                                        GameObject go = GameObject.Instantiate(collider, rightPos, Quaternion.Euler(rotation), transform).gameObject;
                                        SetChildMatrix(go.transform, inst.prefab.transform, collider.transform, mat);
                                        _components.Add(new MeshComponent(go));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            UnityEngine.Random.InitState(pSeed);
        }

        private void SetChildMatrix(Transform set, Transform parent, Transform child, Matrix4x4 mat)
        {
            Matrix4x4 m = mat * (parent.worldToLocalMatrix * child.transform.localToWorldMatrix);
            set.position = m.ExtractPosition();
            set.rotation = m.ExtractRotation();
            set.localScale = m.ExtractScale();
        }

        private void HideUnusedComponents()
        {
            float viewDistSq = MathExt.Square(_componentVewDst);

            if (_components == null) return;

            foreach (MeshComponent component in _components)
            {
                float minDst = float.MaxValue;

                Vector3 compDst = component.go.transform.position;
                foreach (GameObject obj in _toCheck)
                {
                    if (!obj || !obj.activeSelf) continue;

                    float distanceSq = (obj.transform.position - compDst).sqrMagnitude;
                    if (minDst > distanceSq)
                    {
                        minDst = distanceSq;
                        if (minDst < viewDistSq) break;
                    }
                }

                bool isActive = minDst < viewDistSq;
                if (isActive != component.go.activeSelf) component.go.SetActive(isActive);
            }
        }

        private void LoadDrawnParts(InstanceInfo inst, DrawnInstance di)
        {
            foreach (MeshRenderer mr in inst.prefab.GetComponentsInChildren<MeshRenderer>())
            {
                DrawnPart p = new();
                di.parts.Add(p);
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                p.mesh = mf.sharedMesh;
                p.localMatrix = inst.prefab.transform.worldToLocalMatrix * mr.localToWorldMatrix;
                p.materials = mr.sharedMaterials;
                for (int i = 0; i < p.materials.Length; i++)
                {
                    if (p.materials[i].enableInstancing) continue;
                    if (_materialClones.TryGetValue(p.materials[i], out Material clone))
                    {
                        p.materials[i] = clone;
                    }
                    else
                    {
                        clone = new Material(p.materials[i]);
                        clone.enableInstancing = true;
                        _materialClones.Add(p.materials[i], clone);
                        p.materials[i] = clone;
                    }
                }
            }
        }

        private void Start()
        {
            Generate();
            _hasRanFirstTime = false;

            _generateNearObjects = GameObject.FindGameObjectsWithTag("RoadwayGenerateNear").ToList();
        }

        private bool CheckForOverlap(DrawnSection s)
        {
            float d = _viewDst;
            foreach (GameObject obj in _toCheck)
            {
                if (!obj.activeSelf) continue;
                if (s.bounds.Overlaps(MinMaxAABB.CreateFromCenterAndHalfExtents(obj.transform.position, new float3(d, d, d))))
                {
                    return true;
                }
            }

            return _toCheck.Count == 0;
        }

        private bool GetClosestObject(Vector3 pos, out float distanceSq)
        {
            float minDst = float.MaxValue;
            GameObject nearbyObj = null;

            foreach (GameObject obj in _toCheck)
            {
                if (!obj.activeSelf) continue;
                float dst = (obj.transform.position - pos).sqrMagnitude;
                if (dst < minDst)
                {
                    nearbyObj = obj;
                    minDst = dst;
                }
            }

            if (!nearbyObj) distanceSq = 0;
            else distanceSq = minDst;
            return true;
        }

        private void MarkInViewDistance()
        {
            float viewDistSq = MathExt.Square(_viewDst);
            foreach (DrawnSection s in _drawnSections)
            {
                if (!CheckForOverlap(s))
                {
                    s.inViewDistance = false;
                    continue;
                }
                s.inViewDistance = true;
                foreach (DrawnInstance inst in s.instances)
                {
                    foreach (DrawnPart part in inst.parts)
                    {
                        part.matrices.Clear();
                        foreach (Matrix4x4 matrix in inst.matrices)
                        {
                            Vector3 pos = matrix.GetColumn(3);
                            if (GetClosestObject(pos, out float distanceSq))
                            {
                                if (distanceSq >= viewDistSq) continue;
                            }
                            part.matrices.Add(matrix * part.localMatrix);
                        }
                    }
                }
            }
        }

        private List<Matrix4x4> _drawn = new(256);
        private List<GameObject> _toCheck = new();
        private void Update()
        {
            if (Time.time - _lastViewDistanceCheck >= _viewDst || !_hasRanFirstTime)
            {
                _toCheck.Clear();
                _toCheck.AddRange(_generateNearObjects);

                if (_currentControllable) _toCheck.Add(_currentControllable);

                _hasRanFirstTime = true;
                _lastViewDistanceCheck = Time.time;
                HideUnusedComponents();
                MarkInViewDistance();
            }

            foreach (DrawnSection s in _drawnSections)
            {
                if (!s.inViewDistance) continue;
                foreach (DrawnInstance inst in s.instances)
                {
                    foreach (DrawnPart part in inst.parts)
                    {
                        for (int i = 0; i < part.materials.Length; i++)
                        {
                            Graphics.DrawMeshInstanced(part.mesh, i, part.materials[i], part.matrices);
                        }
                    }
                }
            }
        }
    }
}