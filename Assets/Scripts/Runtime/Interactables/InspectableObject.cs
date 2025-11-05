using LJ2025.Player;
using LJ2025;
using UnityEngine;

namespace LJ2025
{
    public class InspectableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private InspectableSettings _settings;
        private InspectorProfile _profile;

        private void Start()
        {
            _profile = new InspectorProfile(transform, new MathExt.Transform(transform), _settings);
        }
        
        public bool IsInteractable() => true;

        public string GetHintName() => _settings.name;
        public string GetHintAction() => "Inspect";
        public SphereCollider BoundingRadius() => _settings.bounds;
        
        public bool Interact(Interactor interactor)
        {
            if (interactor.TryGetComponent(out Inspector inspector))
            {
                inspector.Inspect(_profile);
            }
            return true;
        }

        public void AddOutline()
        {
        }

        public void RemoveOutline()
        {
        }

        public void Restart()
        {
        }
    }
}
