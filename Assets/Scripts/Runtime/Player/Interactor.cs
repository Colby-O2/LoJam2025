using LJ2025.MonoSystems;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace LJ2025.Player
{
    public class Interactor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _head;
        private IInputMonoSystem _input;

        [Header("Settings")]
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private float _interactionRadius = 0.1f;
        [SerializeField] private float _spehreCastRadius = 0.1f;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private IInteractable _possibleInteractable = null;
        [SerializeField, ReadOnly] private Transform _possibleInteractableTrans = null;

        [SerializeField] private TMPro.TMP_Text _hint;
        [SerializeField] private TMPro.TMP_Text _hintAction;
        [SerializeField] private LineRenderer _hintLine;
        [SerializeField] private Transform _hintDot;
        
        private Player.Controller _player;

        private void StartInteraction(IInteractable interactable)
        {
            Debug.Log("Interact");
            interactable.Interact(this);
        }
        
        RaycastHit[] _tmpHits = new RaycastHit[64];
        
        private (Transform, IInteractable) GetPossibleInteractable()
        {
            int size = Physics.RaycastNonAlloc(_head.position, (_interactionPoint.position - _head.position).normalized, _tmpHits, _interactionRadius, _interactionLayer);
            (Transform, IInteractable, float dist)? outt = null;
            for (int i = 0; i < size; i++)
            {
                RaycastHit hit = _tmpHits[i];
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.IsInteractable())
                {
                    if (outt == null || hit.distance < outt.Value.Item3)
                    {
                        outt = (hit.transform, interactable, hit.distance);
                    }
                }
            }

            if (outt != null) return (outt.Value.Item1, outt.Value.Item2);
            size = Physics.SphereCastNonAlloc(_head.position, _spehreCastRadius, (_interactionPoint.position - _head.position).normalized, _tmpHits, _interactionRadius, _interactionLayer);
            for (int i = 0; i < size; i++)
            {
                RaycastHit hit = _tmpHits[i];
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.IsInteractable())
                {
                    if (outt == null || hit.distance < outt.Value.Item3)
                    {
                        outt = (hit.transform, interactable, hit.distance);
                    }
                }
            }

            if (outt != null) return (outt.Value.Item1, outt.Value.Item2);
            return (null, null);
        }

        private void CheckForInteractionInteract()
        {
            if (_player.Occupied()) return;
            var (_, interactable) = GetPossibleInteractable();
            if (interactable != null) StartInteraction(interactable);
        }

        private void CheckForPossibleInteractionInteract()
        {
            if (_player.Occupied())
            {
                if (_hint.gameObject.activeSelf)
                {
                    _possibleInteractable = null;
                    _hint.gameObject.SetActive(false);
                }
                return;
            }
            
            var (obj, possibleInteractable) = GetPossibleInteractable();
            if (possibleInteractable == _possibleInteractable) return;
            if (possibleInteractable != null)
            {
                possibleInteractable.AddOutline();
                _hint.gameObject.SetActive(true);
                _hint.text = possibleInteractable.GetHintName();
                _hintAction.text = possibleInteractable.GetHintAction();
            }
            //if (interactable.GetHint() != string.Empty) GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SetHint(interactable.GetHint());
            
            _possibleInteractable?.RemoveOutline();
            _possibleInteractable = possibleInteractable;
            _possibleInteractableTrans = obj;

            if (_possibleInteractable != null)
            {
                MoveHint();
            }
            
            if (_possibleInteractable == null && _hint.gameObject.activeSelf) _hint.gameObject.SetActive(false);
        }

        private void Start()
        {
            _player = GetComponent<Player.Controller>();
            _hintAction = _hint.transform.Find("Action").GetComponent<TMPro.TMP_Text>();
            _hintLine = _hint.transform.GetChild(0).GetComponent<LineRenderer>();
            _hintDot = _hint.transform.GetChild(1);
            _hintLine.positionCount = 2;
            _hintLine.useWorldSpace = true;
            _hintLine.SetPositions(new Vector3[2]);
            _hint.gameObject.SetActive(false);
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();
            _input.InteractCallback.AddListener(CheckForInteractionInteract);
        }

        private void OnDisable()
        {
            _possibleInteractable?.RemoveOutline();
            _possibleInteractable = null;
        }

        private void MoveHint()
        {
            if (!_possibleInteractableTrans) return;
            Vector3 forward = Vector3.Normalize(_head.position - _possibleInteractableTrans.position);
            Vector3 right = Vector3.Cross(forward, _head.up);
            SphereCollider br = _possibleInteractable.BoundingRadius();
            Vector3 position = br.transform.TransformPoint(br.center);
            float radius = br.radius * br.transform.lossyScale.x;
            _hint.transform.position = position + (_head.up + right).normalized * radius;
            _hint.transform.rotation = _head.rotation;
            _hintLine.SetPosition(0, _hintLine.transform.position);
            _hintLine.SetPosition(1, position);
            _hintDot.position = position;
        }

        private void Update()
        {
            if (_hint.gameObject.activeSelf)
            {
                MoveHint();
            }
        }

        private void LateUpdate()
        {
            CheckForPossibleInteractionInteract();
        }
    }
}