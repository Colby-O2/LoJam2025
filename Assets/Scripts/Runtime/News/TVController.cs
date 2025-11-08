using LJ2025.UI;
using PlazmaGames.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public string Title;
        public bool CanSwitchTo;
        public Texture2D Background;
        public float Duration;
        public GameObject Data;
    }

    public class TVController : MonoBehaviour
    {
        [SerializeField] private List<TVSegment> _segments;
        [SerializeField] private int _start;
        [SerializeField] private RawImage _background;
        [SerializeField] private ScrolllingText _newsScroll;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _date;

        [SerializeField, ReadOnly] private TVSegment _segment;
        [SerializeField] private int _segmentIdx;
        [SerializeField, ReadOnly] private float _timeElasped;
        [SerializeField] private List<string> _newsScrollSegments;

        [Range(0, 23)] public int StartHour = 11;
        public int StartMinute = 0;

        [SerializeField, ReadOnly] private int _currentHour;
        [SerializeField, ReadOnly] private int _currentMinute;

        private string GetDaySuffix(int day)
        {
            if (day is >= 11 and <= 13)
                return "th";

            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        public string GetDateTime()
        {
            DateTime now = DateTime.Now;
            DateTime time = new DateTime(now.Year, now.Month, now.Day, _currentHour, _currentMinute, 0);
            string daySuffix = GetDaySuffix(time.Day);
            string datePart = time.ToString($"MMM d'{daySuffix}' yyyy");
            string timePart = time.ToString("hh:mmtt").ToLower();
            return $"{datePart}\n{timePart}";
        }

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
            scroll.text = $"{_newsScrollSegments.OrderBy(_ => UnityEngine.Random.value).FirstOrDefault()}\t";
            _newsScroll.OnTextUpdate();
        }

        private void OnSegmentStart()
        {
            _timeElasped = 0f;
            _background.texture = _segment.Background;
            _title.text = _segment.Title;
            if (_segment.Data) _segment.Data.SetActive(true);
            switch (_segment.Type)
            {
                case TVSegmnetType.Weather:
                case TVSegmnetType.Debate:
                case TVSegmnetType.Story:
                    UpdateNewsScroll();
                    break;
                default:
                    HideNewsScroll();
                    break;
            }
        }

        private void OnEndStart()
        {
            if (_segment.Data) _segment.Data.SetActive(false);
            switch (_segment.Type)
            {
                case TVSegmnetType.Weather:
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
                SwitchSegment(_segments.Where(s => s.CanSwitchTo).OrderBy(s => UnityEngine.Random.value).FirstOrDefault());
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
            foreach (TVSegment s in _segments) if (s.Data) s.Data.SetActive(false);
            SwitchSegment(_segments[0]);
        }

        private void Update()
        {
            if (_segment != null) UpdateSegment();
            float t = Time.time;
            int passedMinutes = (int)(t / 60f);
            int totalMinutes = StartMinute + passedMinutes;
            int minute = totalMinutes % 60;
            int hourCarry = totalMinutes / 60;
            int hour24 = (StartHour + hourCarry) % 24;
            int hour12 = hour24 % 12;
            if (hour12 == 0) hour12 = 12;
            _currentHour = hour12;
            _currentMinute = minute;
            _date.text = GetDateTime();
        }
    }
}
