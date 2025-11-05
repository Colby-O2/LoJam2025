using UnityEngine;

namespace LJ2025
{
    public class ResetableState : MonoBehaviour
    {
        private IResetState[] _resetState;
        
        public void InitState()
        {
            _resetState = GetComponents<IResetState>();
            foreach (IResetState rs in _resetState) rs.InitState();
        }

        public void ResetState()
        {
            foreach (IResetState rs in _resetState) rs.ResetState();
        }
    }
}
