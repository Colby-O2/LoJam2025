using LJ2025.MonoSystems;
using PlazmaGames.Core;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

namespace LJ2025
{
    public class VendingMachine : MonoBehaviour
    {
        [SerializeField] private GameObject _snackPrefab;
        [SerializeField] private Transform _exitLocation;
        [SerializeField] private int _initialStock = 0;
        
        private List<GameObject> _items = new();
        private int _itemsStocked = 0;

        public bool TaskEnabled { get; set; }

        public bool IsStocked() => _itemsStocked == _items.Count;

        public int GetMaxCount()
        {
            return _items.Count - _initialStock;
        }

        public int GetCount()
        {
            return _itemsStocked;
        }

        private void Awake()
        {
            _itemsStocked = _initialStock;
            Transform items = transform.Find("Items");
            foreach (Transform item in items)
            {
                _items.Add(item.gameObject);
                if (_items.Count > _initialStock)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
        
        public void AddItem()
        {
            if (_itemsStocked == _items.Count) return;
            if (TaskEnabled) GameManager.GetMonoSystem<ITaskMonoSystem>().UpdateTask();
            _items[_itemsStocked++].SetActive(true);
        }

        public Transform RemoveItem()
        {
            if (_itemsStocked == 0) return null;
            _items[--_itemsStocked].SetActive(false);
            Transform t = GameObject.Instantiate(_snackPrefab).transform;
            t.position = _exitLocation.position;
            return t;
        }
    }
}
