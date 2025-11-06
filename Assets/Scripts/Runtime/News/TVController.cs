using LJ2025.UI;
using PlazmaGames.Attribute;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.HableCurve;

namespace LJ2025
{
    public enum TVSegmnetType
    {
        Begin,
        ComingUp,
        Weather,
        Debate,
        Story,
        WeridestAnimal,
        End
    }

    [System.Serializable]
    public class TVSegment
    {
        public TVSegmnetType Type;
        public bool CanSwitchTo;
        public Texture2D Background;
        public float Duration;
    }

    public class TVController : MonoBehaviour
    {
        [SerializeField] private List<TVSegment> _segments;
        [SerializeField] private int _start;
        [SerializeField] private RawImage _background;
        [SerializeField] private GameObject _weatherMap;
        [SerializeField] ScrolllingText _newsScroll;

        [SerializeField, ReadOnly] private TVSegment _segment;
        [SerializeField] private int _segmentIdx;
        [SerializeField, ReadOnly] private float _timeElasped;
        [SerializeField] private List<string> _newsScrollSegments;

        public void SwitchSegment(TVSegment segment)
        {
            OnEndStart();
            _segment = segment;
            if (_segment == null) _segment = _segments[_segments.Count - 1];
            OnSegmentStart();
        }

        private void HideNewsScroll()
        {
            _newsScroll.gameObject.SetActive(false);
        }

        private void UpdateNewsScroll()
        {
            _newsScroll.gameObject.SetActive(true);
            TMP_Text scroll = _newsScroll.GetFirst();
            scroll.text = $"{_newsScrollSegments.OrderBy(_ => Random.value).FirstOrDefault()}\t";
            _newsScroll.OnTextUpdate();
        }

        private void OnSegmentStart()
        {
            _timeElasped = 0f;
            _background.texture = _segment.Background;
            switch (_segment.Type)
            {
                case TVSegmnetType.Weather:
                    _weatherMap.SetActive(true);
                    UpdateNewsScroll();
                    break;
                default:
                    HideNewsScroll();
                    break;
            }
        }

        private void OnEndStart()
        {
            switch (_segment.Type)
            {
                case TVSegmnetType.Weather:
                    _weatherMap.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        private void UpdateSegment()
        {
            _timeElasped += Time.deltaTime;
            if (_timeElasped > _segment.Duration)
            {
                SwitchSegment(_segments.Where(s => s.CanSwitchTo).OrderBy(s => Random.value).FirstOrDefault());
                return;
            }

            switch (_segment.Type)
            {
                case TVSegmnetType.Begin:
                    break;
                default:
                    break;
            }
        }

        private void Awake()
        {
            SwitchSegment(_segments[0]);
        }

        private void Update()
        {
            if (_segment != null) UpdateSegment();
        }
    }
}
