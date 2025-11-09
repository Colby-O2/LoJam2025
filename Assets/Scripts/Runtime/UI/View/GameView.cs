using DialogueGraph.Data;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LJ2025.UI
{
    public class GameView : View
    {
        [SerializeField] private AudioSource _as;
        [SerializeField] private GameObject _holder;

        [Header("Dialogue")]
        [SerializeField] private GameObject _dialogueHolder;
        [SerializeField] private TMP_Text _dialogueAvatarName;
        [SerializeField] private TMP_Text _dialogue;
        [SerializeField] private GameObject _dialogueHint;
        [SerializeField] private float _typeSpeed = 0.1f;
        [SerializeField] private float _timeout = 4f;
        [SerializeField] private float _passiveTimeout = 3f;
        [SerializeField, ReadOnly] private bool _passive = false;
        [SerializeField, ReadOnly] private float _timeSinceMessageEnd = 0;

        [Header("Choice")]
        [SerializeField] private GameObject _choiceHolder;
        [SerializeField] private List<EventButton> _choiceButtons;
        [SerializeField] private List<TMP_Text> _choiceText;

        [Header("Task")]
        [SerializeField] private TMP_Text _task;

        [SerializeField, ReadOnly] private DialogueNodeData _currentDialogueNode;
        [SerializeField, ReadOnly] private float _timeSinceWriteStart = 0f;
        [SerializeField, ReadOnly] private string _currentMessage;
        [SerializeField, ReadOnly] private bool _showedMessage = false;
        [SerializeField, ReadOnly] private bool _isTyping = false;
        [SerializeField, ReadOnly] private bool _showingChoice = false;
        [SerializeField, ReadOnly] private int _selectedChoice = -1;

        private bool _initialInputLockState = false;

        private Coroutine _writeRoutine = null;
        
        private bool IsWriting() => _writeRoutine == null;

        private void StopWriteRoutine()
        {
            if (_writeRoutine == null) return;
            StopCoroutine(_writeRoutine);
            _writeRoutine = null;
        }

        public void SetPassive(bool state) => _passive = state;

        IEnumerator TypeDialogue(string msg, float typeSpeed, TMP_Text target, UnityAction onFinished = null, bool isDigloue = true)
        {
            if (isDigloue) _timeSinceMessageEnd = 0;
            if (isDigloue) _currentMessage = msg;
            if (isDigloue) _showedMessage = false;

            if (isDigloue) _timeSinceWriteStart = 0f;

            if (_as && isDigloue) _as.Play();

            target.text = string.Empty;

            if (!isDigloue) _isTyping = true;

            for (int i = 0; i < msg.Length; i++)
            {
                while (LJ2025GameManager.IsPaused)
                {
                    if (_as && isDigloue) _as.Stop();
                    yield return null;
                }

                if (_as && !_as.isPlaying && isDigloue) _as.Play();

                if (msg[i] == '<')
                {
                    int endIndex = msg.IndexOf('>', i);
                    if (endIndex != -1)
                    {
                        string fullTag = msg.Substring(i, endIndex - i + 1);
                        target.text += fullTag;
                        i = endIndex;
                        continue;
                    }
                }

                target.text += msg[i];
                yield return new WaitForSeconds(typeSpeed * LJ2025GameManager.Preferences.DialogueSpeedMul);
            }

            target.text = msg;
            if (!isDigloue) _isTyping = false;
            if (_as && isDigloue) _as.Stop();

            if (isDigloue) _showedMessage = true;
            if (isDigloue && !_passive) _dialogueHint.SetActive(true);
        }

        private IEnumerator WaitForTaskToStopTyping(string msg)
        {
            yield return new WaitWhile(() => _isTyping);
            _task.text = msg;
        }

        public void ShowTask(string msg)
        {
            _task.gameObject.SetActive(true);
            StartCoroutine(TypeDialogue(
                msg,
                _typeSpeed,
                _task,
                null,
                false
            ));
        }

        public void UpdateTask(string msg)
        {
            StartCoroutine(WaitForTaskToStopTyping(msg));
        }

        public void HideTask()
        {
            _task.gameObject.SetActive(false);
        }

        public bool IsShowingDialogue()
        {
            return _currentDialogueNode != null;
        }

        public bool IsDialogueWaiting()
        {
            return IsShowingDialogue() && _showedMessage;
        }

        public void ShowDialogue()
        {
            if (!_dialogueHolder.activeSelf)
            {
                _initialInputLockState = LJ2025GameManager.LockMovement;
            }
            _dialogueHolder.SetActive(true);
            _dialogueHint.SetActive(false);
            if (!_passive) LJ2025GameManager.LockMovement = true;
        }

        public void DisplayDialogue(DialogueNodeData dialogue)
        {
            StopWriteRoutine();
            _dialogueHint.SetActive(false);
            _currentDialogueNode = dialogue;
            _dialogueAvatarName.text = dialogue.ActorName;
            _writeRoutine = StartCoroutine(TypeDialogue(
                dialogue.Text,
                 _typeSpeed,
                _dialogue,
                Next,
                true)
            );
        }

        public void HideDialogue()
        {
            _dialogueAvatarName.text = string.Empty;
            _dialogue.text = string.Empty;
            _dialogueHolder.SetActive(false);
            _dialogueHint.SetActive(false);

            if (!_passive) LJ2025GameManager.LockMovement = _initialInputLockState;
        }

        public void GoToNext(int choice)
        {
            StopWriteRoutine();
            _showedMessage = false;
            GameManager.GetMonoSystem<IDialogueMonoSystem>().OnDialogueNodeFinish(choice);
        }

        private void Next()
        {
            if (_currentDialogueNode.Type == DialogueGraph.Enumeration.DialogueType.SingleChoice)
            {
                GoToNext(0);
            }
            else if (_currentDialogueNode.Type == DialogueGraph.Enumeration.DialogueType.MultipleChoice)
            {
                if (_currentDialogueNode.Choices.Count == 0 || !_currentDialogueNode.Choices.Any(e => e.Next != string.Empty && e.Next != null))
                {
                    GameManager.GetMonoSystem<IDialogueMonoSystem>().FinishDialogue();
                }
                else StartChoice();
            }
        }

        private void StartChoice()
        {
            ShowChoice();
            for (int i = 0; i < _choiceText.Count; i++)
            {
                TMP_Text text = _choiceText[i];
                text.color = Color.white;
                if (i < _currentDialogueNode.Choices.Count)
                {
                    int choice = i;
                    text.text = _currentDialogueNode.Choices[i].Text;
                    _choiceButtons[i].enabled = true;
                    _choiceButtons[i].onPointerDown.RemoveAllListeners();
                    _choiceButtons[i].onPointerDown.AddListener(() => SelectChoice(choice));
                    _choiceButtons[i].onPointerEnter.RemoveAllListeners();
                    _choiceButtons[i].onPointerEnter.AddListener(() => {
                        _selectedChoice = choice;
                        _choiceButtons[choice].GetOverlay().color = _choiceButtons[choice].GetOverlayColor();
                        _choiceText[choice].color = Color.black;
                        for (int j = 0; j < _choiceText.Count; j++)
                        {
                            if (j == choice) continue;
                            _choiceButtons[j].GetOverlay().color = Color.clear;
                            _choiceText[j].color = Color.white;
                        }
                    });
                }
                else
                {
                    text.text = string.Empty;
                    _choiceButtons[i].enabled = false;
                }
            }
        }

        private void SelectChoice(int choice)
        {
            _showedMessage = false;
            _dialogue.text = string.Empty;
            _as?.Stop();
            HideChoice();
            GoToNext(choice);
        }

        private void HideChoice()
        {
            _showingChoice = false;

            LJ2025GameManager.HideCursor();

            foreach (EventButton button in _choiceButtons)
            {
                button.onPointerEnter.RemoveAllListeners();
                button.onPointerDown.RemoveAllListeners();
            }
            foreach (TMP_Text text in _choiceText) text.text = string.Empty;
            _choiceHolder.SetActive(false);
        }

        private void ShowChoice()
        {
            _showingChoice = true;
            LJ2025GameManager.ShowCursor();
            _choiceHolder.SetActive(true);
        }

        private void HandleTimeout()
        {
            if (LJ2025GameManager.IsPaused) return;

            _timeSinceWriteStart += Time.deltaTime;

            if (
                (_timeSinceWriteStart > _timeout && !_showedMessage) ||
                (!_passive && Keyboard.current.spaceKey.wasPressedThisFrame && !_showedMessage))
            {
                StopWriteRoutine();
                _as?.Stop();
                _showedMessage = true;
                _dialogue.text = _currentMessage;
                if (!_passive) _dialogueHint.SetActive(true);
            }
            else if (_showedMessage)
            {
                if (_passive)
                {
                    _timeSinceMessageEnd += Time.deltaTime;

                    if (_timeSinceMessageEnd >= _passiveTimeout)
                    {
                        _timeSinceMessageEnd = -1;
                        StopWriteRoutine();
                        Next();
                    }
                }
                else if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    StopWriteRoutine();
                    Next();
                }
            }
        }

        public bool IsShowingChoice()
        {
            return _showingChoice;
        }

        public override void Show()
        {
            base.Show();
            _holder.gameObject.SetActive(true);
            if (!IsShowingChoice()) LJ2025GameManager.HideCursor();
        }

        public override void Hide()
        {
            _holder.gameObject.SetActive(false);
        }

        public override void Init()
        {
            HideTask();
            HideDialogue();
            HideChoice();
        }

        private void Update()
        {
            if (LJ2025GameManager.IsPaused) return;

            HandleTimeout();

            if (_showingChoice)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    if (_selectedChoice >= 0 && _choiceText[_selectedChoice].text != string.Empty) SelectChoice(_selectedChoice);
                }
            }
        }
    }
}
