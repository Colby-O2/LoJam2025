using LJ2025;
using UnityEngine;

namespace LJ2025
{
    public class GenericResetter : MonoBehaviour, IResetState
    {
        [SerializeField] private bool _startDisabled = false;
        
        private MathExt.Transform _initialTrans;
        private bool _initialActiveState;
        
        public void InitState()
        {
            _initialTrans = transform;
            _initialActiveState = (gameObject.activeSelf == false) ? false : _startDisabled;
            if (_startDisabled) gameObject.SetActive(false);
        }
        
        public void ResetState()
        {
            _initialTrans.ApplyTo(transform);
            gameObject.SetActive(_initialActiveState);
        }
    }
}
