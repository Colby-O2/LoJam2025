using PlazmaGames.Attribute;
using UnityEngine;

namespace LJ2025
{
    public class ShowerController : MonoBehaviour
    {
        [SerializeField] private GameObject _ps;
        [SerializeField] private bool _defaultState = false;
        [SerializeField, ReadOnly] private bool _isOn;

        public void Toggle()
        {
            _isOn = !_isOn;
            _ps.SetActive(_isOn);
        }
        private void Awake()
        {
            _ps.SetActive(_defaultState);
        }
    }
}
