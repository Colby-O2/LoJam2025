using PlazmaGames.Attribute;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LJ2025.UI
{
    public class ScrolllingText : MonoBehaviour
    {
        [SerializeField] private float _scrollSpeed = 100f;

        [SerializeField] private int _maxClones = 10;

        [SerializeField, ReadOnly] float _textWidth;
        [SerializeField, ReadOnly] LinkedList<RectTransform> _texts = new LinkedList<RectTransform>();

        private int GetNecessaryClones()
        {
            int clones = 0;
            RectTransform mask = GetComponent<RectTransform>();

            do
            {
                clones++;
                if (clones == _maxClones) break;

            } while (mask.rect.width / (_textWidth * clones) >= 1);

            return clones;
        }

        private void AttachAtEnd(RectTransform clone)
        {
            float lastPos = _texts.Last.Value.localPosition.x;
            Vector3 newPos = clone.localPosition;
            newPos.x = lastPos + _textWidth;
            clone.localPosition = newPos;
        }

        private void CreateClones()
        {
            int numClones = GetNecessaryClones();

            for (int i = 0; i < numClones; i++)
            {
                RectTransform clone = Instantiate(_texts.First.Value, transform);
                clone.name = "Text";
                AttachAtEnd(clone);
                _texts.AddLast(clone);
            }

            CheckIfLeftMask();
        }

        private void MoveFirstToEnd()
        {
            float pos = _texts.Last.Value.localPosition.x;
            LinkedListNode<RectTransform> node = _texts.First;

            Vector3 newPos = node.Value.localPosition;
            newPos.x = pos + _textWidth;
            node.Value.localPosition = newPos;

            _texts.RemoveFirst();
            _texts.AddLast(node);
        }

        private void CheckIfLeftMask()
        {
            RectTransform mask = _texts.First.Value;
            if (mask.localPosition.x + _textWidth <= 0) MoveFirstToEnd();
        }

        private void Move()
        {
            float dst = _scrollSpeed * Time.deltaTime;
            foreach(RectTransform rectTransform in _texts)
            {
                Vector3 newPos = rectTransform.localPosition;
                newPos.x -= dst;
                rectTransform.localPosition = newPos;
            }

            CheckIfLeftMask();
        }

        public TMP_Text GetFirst()
        {
            return _texts.First.Value.GetComponent<TMP_Text>();
        }

        public void OnTextUpdate()
        {
            RectTransform first = _texts.First.Value;
            _texts.RemoveFirst();

            foreach (RectTransform rect in _texts)
            {
                Destroy(rect.gameObject);
            }

            _texts.Clear();

            _texts.AddFirst(first);
            _textWidth = first.GetComponent<TextMeshProUGUI>().preferredWidth;
            CreateClones();
        }

        private void Awake()
        {
            _texts.AddFirst((RectTransform)transform.GetChild(0));
            _textWidth = _texts.First.Value.GetComponent<TextMeshProUGUI>().preferredWidth;
            CreateClones();
        }

        private void Update()
        {
            Move();
        }
    }
}
