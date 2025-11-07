using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

namespace LJ2025
{
    public class VendingMachine : MonoBehaviour
    {
        [SerializeField] private int _initialStock = 0;
        
        private List<GameObject> _items = new();
        private int _itemsStocked = 0;

        public bool IsStocked() => _itemsStocked == _items.Count;

        private void Awake()
        {
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
            _items[_itemsStocked++].SetActive(true);
        }

        public void RemoveItem()
        {
            if (_itemsStocked == 0) return;
            _items[--_itemsStocked].SetActive(false);
        }
    }
}
