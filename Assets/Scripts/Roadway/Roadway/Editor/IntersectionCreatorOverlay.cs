#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;
using Roadway;

namespace Roadway.Editor
{
    [Overlay(typeof(SceneView), "Intersection Creator", true)]
    public class IntersectionCreatorOverlay : Overlay
    {
        private ScrollView _root;

        public List<SelectableKnot> _selectedKnots { get; private set; }
        public List<float> _selectedCurveWeights { get; private set; }

        public override VisualElement CreatePanelContent()
        {
            _root = new ScrollView();
            _root.style.flexGrow = 1;
            _root.style.paddingTop = 4;
            _root.style.paddingBottom = 4;

            _selectedCurveWeights = new List<float>();

            Update();
            SplineSelection.changed += Update;

            return _root;
        }

        RoadwayIntersection GetIntersectionFrom(List<SelectableKnot> knots)
        {
            RoadwayIntersection intersection = null;
            if (RoadwayCreator.Instance)
            {
                intersection = RoadwayCreator.Instance.HasIntersectionWithJunctions(knots);
            }
            return intersection;
        }

        private List<SelectableKnot> GetSelectedKnots(out RoadwayIntersection currentIntersection)
        {
            currentIntersection = null;

            List<SelectableKnot> newKnots = RoadwayHelper.GetSelectedRoadwayKnots();
            if (_selectedKnots != null && newKnots != null && RoadwayCreator.Instance)
            {
                currentIntersection = RoadwayCreator.Instance.HasIntersectionWithAtLeastOneJunction(newKnots);
                if (currentIntersection != null)
                {
                    _selectedKnots = new List<SelectableKnot>();
                    foreach (JunctionInfo info in currentIntersection.GetJunctions())
                    {
                        _selectedKnots.Add(new SelectableKnot(new SplineInfo(RoadwayCreator.Instance.GetContainer(), info.splineIndex), info.knotIndex));
                    }

                    foreach (SelectableKnot knot in newKnots)
                    {
                        if (!currentIntersection.HasJunction(knot)) _selectedKnots.Add(knot);
                    }

                    return newKnots;
                }
            }

            _selectedKnots = newKnots;
            return newKnots;
        }

        private void ShowSelectedKnots()
        {
            Label knotSectionLabel = new Label("Selected Knots");
            knotSectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _root.Add(knotSectionLabel);

            int id = 0;
            foreach (SelectableKnot knot in _selectedKnots)
            {
                Label label = new Label($"{id++}\tSpline ID: {knot.SplineInfo.Index} Knot ID: {knot.KnotIndex}");
                _root.Add(label);
            }
        }

        private void CreateCurveWeightControls(RoadwayIntersection intersection)
        {
            Label curveWeightSectionLabel = new Label("Curve Weights");
            curveWeightSectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _root.Add(curveWeightSectionLabel);

            _selectedCurveWeights = new List<float>();

            for (int i = 0; i < _selectedKnots.Count; i++)
            {
                SelectableKnot knot = _selectedKnots[i];

                if (intersection != null) _selectedCurveWeights.Add(intersection.curveWeights[i]);
                else _selectedCurveWeights.Add(0.5f);

                Slider slider = new Slider(0f, 1f)
                {
                    value = (intersection != null) ? intersection.curveWeights[i] : 0.5f,
                    label = $"{i}"
                };

                int knotIndex = i;

                slider.RegisterValueChangedCallback(evt =>
                {
                    _selectedCurveWeights[knotIndex] = evt.newValue;

                    if (RoadwayCreator.Instance)
                    {
                        if (intersection != null) intersection.curveWeights[knotIndex] = evt.newValue;
                        RoadwayCreator.Instance.GenerateRoadway();
                    }
                });

                _root.Add(slider);
            }
        }

        private void CreateControls(RoadwayIntersection intersection, RoadwayIntersection currentIntersection, List<SelectableKnot> newKnots)
        {
            if (currentIntersection != null)
            {
                Button removeButton = new UnityEngine.UIElements.Button(() => RemoveIntersection(intersection))
                {
                    text = "Remove"
                };
                _root.Add(removeButton);

                if (_selectedKnots != null && newKnots != null && newKnots.Count > 1)
                {
                    Button recreateButton = new UnityEngine.UIElements.Button(() => RecreateIntersection(currentIntersection))
                    {
                        text = "Recreate"
                    };
                    _root.Add(recreateButton);
                }
            }
            else
            {
                if (_selectedKnots != null && _selectedKnots.Count > 1)
                {
                    Button createButton = new UnityEngine.UIElements.Button(() => CreateIntersection(intersection))
                    {
                        text = "Create"
                    };
                    _root.Add(createButton);
                }
            }
        }

        public void Update()
        {
            _root.Clear();

            List<SelectableKnot> newKnots = GetSelectedKnots(out RoadwayIntersection currentIntersection);

            ShowSelectedKnots();

            RoadwayIntersection intersection = GetIntersectionFrom(_selectedKnots);
            CreateCurveWeightControls(intersection);

            CreateControls(intersection, currentIntersection, newKnots);
        }

        private void CreateIntersection(RoadwayIntersection intersection)
        {
            if (_selectedKnots == null || _selectedKnots.Count <= 1) return;

            if (intersection == null)
            {
                intersection = new RoadwayIntersection();
                foreach (SelectableKnot knot in _selectedKnots)
                {
                    intersection.AddJunction(
                        knot.SplineInfo.Index,
                        knot.KnotIndex,
                        _selectedCurveWeights
                    );
                }
            }


            if (RoadwayCreator.Instance)
            {
                Debug.Log("Creating Intersection!");
                RoadwayCreator.Instance.AddIntersection(intersection);
                RoadwayCreator.Instance.GenerateRoadway();
                Update();
            }
        }

        private void RemoveIntersection(RoadwayIntersection intersection)
        {
            if (RoadwayCreator.Instance && intersection != null)
            {
                RoadwayCreator.Instance.RemoveIntersection(intersection);
                RoadwayCreator.Instance.GenerateRoadway();
                Update();
            }
        }

        private void RecreateIntersection(RoadwayIntersection intersection)
        {
            RemoveIntersection(intersection);
            CreateIntersection(null);
        }

        public override void OnWillBeDestroyed()
        {
            SplineSelection.changed -= Update;
            base.OnWillBeDestroyed();
        }
    }
}
#endif