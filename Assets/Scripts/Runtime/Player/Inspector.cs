using System;
using LJ2025.MonoSystems;
using LJ2025.UI;
using PlazmaGames.Core;
using PlazmaGames.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using MathExt = LJ2025.MathExt;


namespace LJ2025.Player
{
    public class InspectorProfile
    {
        public Transform obj;
        public MathExt.Transform startTransform;
        public InspectableSettings settings;

        public InspectorProfile(Transform obj, MathExt.Transform startTransform, InspectableSettings settings)
        {
            this.obj = obj;
            this.startTransform = startTransform;
            this.settings = settings;
        }
    }
    
    public class Inspector : MonoBehaviour
    {
        private InspectorProfile _profile = null;

        private Player.Controller _player;
        private Transform _head;

        private MathExt.Transform _pickupTarget = new();
        private MathExt.Transform _lookAtTarget = new();
        private MathExt.Transform _headStart = new();

        private float _transition = 0;

        private enum State
        {
            Starting,
            Inspecting,
            Stopping,
        }

        private State _state;
        [SerializeField] private LayerMask _inspectorButtonLayer;

        public bool IsInspecting() => _profile != null;
        
        public void Inspect(InspectorProfile profile)
        {
            if (IsInspecting()) return;
            _profile = profile;
            _state = State.Starting;
            _headStart = new MathExt.Transform(_head);
            switch (_profile.settings.lookType)
            {
                case InspectableLookType.Pickup:
                    _pickupTarget =
                        new MathExt.Transform(
                            _head.position + _head.forward * _profile.settings.offsetDistance,
                            Quaternion.AngleAxis(_profile.settings.pickupRotation.y, _head.up) *
                            Quaternion.AngleAxis(_profile.settings.pickupRotation.x, _head.right) *
                            Quaternion.AngleAxis(_profile.settings.pickupRotation.z, _head.forward) *
                            (Quaternion.AngleAxis(180, _head.up) * _head.rotation));
                        _lookAtTarget = new MathExt.Transform(_head);
                    break;
                case InspectableLookType.LookAt:
                    if (_profile.settings.lookAtTarget)
                    {
                        _lookAtTarget = new MathExt.Transform(_profile.settings.lookAtTarget);
                    }
                    else
                    {
                        Vector3 dir = Vector3.Normalize(_head.position - _profile.obj.position);
                        _lookAtTarget = new MathExt.Transform(
                            _profile.obj.position + dir * _profile.settings.offsetDistance,
                            Quaternion.LookRotation(-dir, Vector3.up));
                    }
                    break;
            }
        }
        
        public void StopInspecting()
        {
            if (!IsInspecting()) return;
            _state = State.Stopping;
            _pickupTarget.rotation = _profile.obj.rotation;
            _lookAtTarget = new MathExt.Transform(_head);
        }

        private void Start()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().InteractCallback.AddListener(HandleInteractPress);
            
            _player = GetComponent<Player.Controller>();
            _head = _player.Head();
        }

        private void HandleInteractPress()
        {
            if (IsInspecting() && _state == State.Inspecting) StopInspecting();
        }

        private void Update()
        {
            if (!IsInspecting()) return;
            if (_state is State.Starting or State.Stopping)
            {
                float dir = _state == State.Starting ? +1.0f : -1.0f;
                _transition = MathExt.Clamp01(_transition + Time.deltaTime / 0.3f * dir);
                HeadTransition();
                if (_profile.settings.lookType == InspectableLookType.Pickup) PickupTransition();

                if (_state == State.Starting && _transition >= 1) FinishTransition();
                if (_state == State.Stopping && _transition <= 0) EndInspection();
            }
            if (!IsInspecting()) return;

            if (_state == State.Inspecting && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (VirtualCaster.Instance.Raycast(Mouse.current.position.value, out IInspectorButton button, 10, _inspectorButtonLayer))
                {
                    button?.Click();
                }
            }

            if (_profile.settings.allowRotate && _state == State.Inspecting)
            {
                float value = Mouse.current.scroll.value.y;
                MathExt.Transform trans = new MathExt.Transform(_head);
                trans.Translate(Vector3.forward * (value * 0.1f));
                float dist = Vector3.Distance(_profile.obj.position, trans.position);
                if (
                    dist > _profile.settings.BoundingRadius() &&
                    dist <= Vector3.Distance(_lookAtTarget.position, _profile.obj.position) 
                )
                {
                    _head.position = trans.position;
                }
            }

            if (!String.IsNullOrWhiteSpace(_profile.settings.readText) && _state == State.Inspecting)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    GameManager.GetMonoSystem<IUIMonoSystem>().GetView<InspectorView>().ToggleReadPanel();
                }
            }

            if (_profile.settings.allowRotate && _state == State.Inspecting)
            {
                if (Mouse.current.rightButton.isPressed)
                {
                    Vector2 d = Mouse.current.delta.value / 3.0f;
                    switch (_profile.settings.lookType)
                    {
                        case InspectableLookType.Pickup: RotatePickup(d); break;
                        case InspectableLookType.LookAt: RotateLookAt(d); break;
                    }
                }
            }
        }

        private void RotatePickup(Vector2 d)
        {
            Quaternion rot = Quaternion.AngleAxis(-d.x, _head.up) * Quaternion.AngleAxis(d.y, _head.right);
            _profile.obj.rotation = rot * _profile.obj.rotation;
        }

        private void RotateLookAt(Vector2 d)
        {
            _head.RotateAround(_profile.obj.position, Vector3.up, d.x);
            _head.RotateAround(_profile.obj.position, _head.right, -d.y);
        }
        
        private void HeadTransition()
        {
            MathExt.Transform.Slerp(_headStart, _lookAtTarget, _transition)
                .ApplyTo(_head);
        }

        private void PickupTransition()
        {
            _profile.obj.position = Vector3.Slerp(_profile.startTransform.position, _pickupTarget.position, _transition);
            if (_profile.settings.rotatePickup || _profile.settings.allowRotate)
            {
                _profile.obj.rotation = Quaternion.Slerp(_profile.startTransform.rotation, _pickupTarget.rotation, _transition);
            }
        }

        private void EndInspection()
        {
            LJ2025GameManager.HideCursor();
            _profile = null;
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
        }

        private void FinishTransition()
        {
            _state = State.Inspecting;
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<InspectorView>();
            InspectorView iv = GameManager.GetMonoSystem<IUIMonoSystem>().GetView<InspectorView>();
            iv.SetTitle(!String.IsNullOrWhiteSpace(_profile.settings.title));
            iv.SetTitleText(!String.IsNullOrWhiteSpace(_profile.settings.title) ? _profile.settings.title : _profile.obj.GetComponent<IInteractable>().GetHintAction());
            iv.SetInteractButton(_profile.settings.hasInteractions);
            iv.SetRotateButton(_profile.settings.allowRotate);
            iv.SetZoomButton(_profile.settings.allowRotate);
            iv.SetReadPanel(false);
            iv.SetReadButton(!String.IsNullOrWhiteSpace(_profile.settings.readText));
            iv.SetReadText(_profile.settings.readText);
        }
    }
}
