using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LJ2025
{
    public enum Connect4Teams 
    { 
        Red, 
        Yellow
    }

    public class Connect4Controller : MonoBehaviour
    {
        private struct Connect4Chip
        {
            public Connect4Teams Team;
            public Transform Transform;
        }

        [Header("Board")]
        [SerializeField] private Transform _boardParent;
        [SerializeField] private Transform _chipHolder;
        [SerializeField] private int _columns = 7;
        [SerializeField] private int _rows = 6;

        [Header("Chip")]
        [SerializeField] private GameObject _chipPrefab;
        [SerializeField] private float _dropHeight = 5f;
        [SerializeField] private float _dropSpeed = 2f;

        [Header("AI")]
        [Range(0f, 1f), SerializeField] private float _aiSmartness = 1f;

        private Promise _promise = null;

        [SerializeField, ReadOnly] private bool _moving = false;

        private Transform[,] _slotLocations;
        private Connect4Chip?[,] _slots;

        private Connect4Teams _playerTeam;
        private Connect4Teams _currentTurn = Connect4Teams.Red;
        [SerializeField, ReadOnly] private bool _gameOver = false;

        public Promise StartGame(Connect4Teams team = Connect4Teams.Red)
        {
            _promise = new Promise();
            _playerTeam = team;
            Restart();
            return _promise;
        }

        public void Restart()
        {
            _moving = false;
            _currentTurn = _playerTeam;
            _gameOver = false;
            _slots = new Connect4Chip?[_columns, _rows];
            for (int i = _chipHolder.childCount - 1; i >= 0; i--) Destroy(_chipHolder.GetChild(i).gameObject);
        }

        private void BuildSlotGrid()
        {
            var slots = new List<Transform>();
            foreach (Transform child in _boardParent)
                slots.Add(child);

            var spreads = new[]
            {
            new { Axis="x", Spread = slots.Max(t => t.localPosition.x) - slots.Min(t => t.localPosition.x), Get = new System.Func<Transform,float>(t=>t.localPosition.x) },
            new { Axis="y", Spread = slots.Max(t => t.localPosition.y) - slots.Min(t => t.localPosition.y), Get = new System.Func<Transform,float>(t=>t.localPosition.y) },
            new { Axis="z", Spread = slots.Max(t => t.localPosition.z) - slots.Min(t => t.localPosition.z), Get = new System.Func<Transform,float>(t=>t.localPosition.z) },
        }.OrderByDescending(s => s.Spread).ToList();

            var horiz = spreads[0];
            var vert = spreads[1];

            var ordered = slots.OrderBy(s => horiz.Get(s))
                               .ThenBy(s => vert.Get(s))
                               .ToList();

            _slotLocations = new Transform[_columns, _rows];
            int i = 0;
            for (int c = 0; c < _columns; c++)
                for (int r = 0; r < _rows; r++)
                    _slotLocations[c, r] = ordered[i++];
        }

        public void Move(int column)
        {
            if (_moving) return;
            if (_gameOver) return;
            Debug.Log($"Moving on Cloumn {column}");
            StartCoroutine(SpawnChip(column, _currentTurn, true));
        }

        private void OnWin(Connect4Teams team)
        {
            Debug.Log($"{team} Wins!");
            GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("PlayedConnect4", true);
            GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("DrawConnect4", false);
            GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("WonConnect4", team == _playerTeam);
            _promise?.Resolve();
            _promise = null;
        }

        private void OnDraw()
        {
            Debug.Log($"Draw!");
            GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("PlayedConnect4", true);
            GameManager.GetMonoSystem<IDialogueMonoSystem>().SetFlag("DrawConnect4", true);
            _promise?.Resolve();
            _promise = null;
        }

        private bool CheckDraw()
        {
            for (int c = 0; c < _columns; c++)
            {
                if (GetNextEmptyRow(c) != -1)
                    return false;
            }

            return true;
        }

        private IEnumerator SpawnChip(int column, Connect4Teams team, bool triggerNextTurn)
        {
            if (column < 0 || column >= _columns) yield break;

            int row = GetNextEmptyRow(column);
            if (row == -1) yield break;

            GameObject chip = Instantiate(_chipPrefab);
            chip.transform.localScale = transform.lossyScale;
            chip.transform.forward = transform.forward;
            chip.transform.parent = _chipHolder;
            chip.GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", (team == Connect4Teams.Red) ? Color.red : Color.yellow);

            Transform targetSlot = _slotLocations[column, row];
            Vector3 targetPos = targetSlot.position;
            Transform chipTf = chip.transform;
            chipTf.position = targetPos + Vector3.up * _dropHeight;
            
            yield return null;

            _moving = true;

            float t = 0f;
            Vector3 startPos = chipTf.position;
            while (t < 1f)
            {
                t += Time.deltaTime * _dropSpeed;
                chipTf.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            chipTf.position = targetPos;

            _slots[column, row] = new Connect4Chip { Team = team, Transform = chipTf };

            if (CheckWin(column, row, team))
            {
                OnWin(team);
                _gameOver = true;
                yield break;
            }
            else if (CheckDraw())
            {
                OnDraw();
                _gameOver = true;
                yield break;
            }

            if (triggerNextTurn)
            {
                _currentTurn = (_currentTurn == Connect4Teams.Red) ? Connect4Teams.Yellow : Connect4Teams.Red;

                if (_currentTurn != _playerTeam && !_gameOver)
                {
                    yield return new WaitForSeconds(0.5f);
                    int aiCol = GetAIColumn(_currentTurn);
                    if (aiCol != -1) StartCoroutine(SpawnChip(aiCol, _currentTurn, true));
                }
            }
            
            _moving = false;
        }

        private int GetNextEmptyRow(int col)
        {
            for (int r = 0; r < _rows; r++)
            {
                if (_slots[col, r] == null) return r;
            }
            return -1;
        }

        private int GetRandomValidColumn()
        {
            List<int> validCols = new List<int>();
            for (int c = 0; c < _columns; c++)
            {
                if (_slots[c, _rows - 1] == null)
                    validCols.Add(c);
            }

            if (validCols.Count == 0)
                return -1;

            return validCols[Random.Range(0, validCols.Count)];
        }

        private int GetAIColumn(Connect4Teams aiTeam)
        {
            if (Random.value > _aiSmartness)
                return GetRandomValidColumn();

            for (int c = 0; c < _columns; c++)
            {
                int r = GetNextEmptyRow(c);
                if (r == -1) continue;
                _slots[c, r] = new Connect4Chip { Team = aiTeam };
                if (CheckWin(c, r, aiTeam))
                {
                    _slots[c, r] = null;
                    return c;
                }
                _slots[c, r] = null;
            }

            Connect4Teams playerTeam = (aiTeam == Connect4Teams.Red) ? Connect4Teams.Yellow : Connect4Teams.Red;
            for (int c = 0; c < _columns; c++)
            {
                int r = GetNextEmptyRow(c);
                if (r == -1) continue;
                _slots[c, r] = new Connect4Chip { Team = playerTeam };
                if (CheckWin(c, r, playerTeam))
                {
                    _slots[c, r] = null;
                    return c;
                }
                _slots[c, r] = null;
            }

            int depth = Mathf.CeilToInt(1 + _aiSmartness * 3);
            int bestCol = -1;
            int bestScore = int.MinValue;

            for (int c = 0; c < _columns; c++)
            {
                int r = GetNextEmptyRow(c);
                if (r == -1) continue;
                _slots[c, r] = new Connect4Chip { Team = aiTeam };
                int score = Minimax(depth - 1, false, aiTeam, int.MinValue, int.MaxValue);
                _slots[c, r] = null;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCol = c;
                }
            }

            return bestCol != -1 ? bestCol : GetRandomValidColumn();
        }

        private int Minimax(int depth, bool maximizing, Connect4Teams aiTeam, int alpha, int beta)
        {
            Connect4Teams playerTeam = (aiTeam == Connect4Teams.Red) ? Connect4Teams.Yellow : Connect4Teams.Red;

            for (int c = 0; c < _columns; c++)
            {
                int r = GetNextEmptyRow(c);
                if (r == -1) continue;
                Connect4Teams team = maximizing ? aiTeam : playerTeam;
                _slots[c, r] = new Connect4Chip { Team = team };
                if (CheckWin(c, r, team))
                {
                    _slots[c, r] = null;
                    return maximizing ? 1000 : -1000; 
                }
                _slots[c, r] = null;
            }

            if (depth == 0) return EvaluateBoard(aiTeam);

            if (maximizing)
            {
                int maxEval = int.MinValue;
                for (int c = 0; c < _columns; c++)
                {
                    int r = GetNextEmptyRow(c);
                    if (r == -1) continue;

                    _slots[c, r] = new Connect4Chip { Team = aiTeam };
                    int eval = Minimax(depth - 1, false, aiTeam, alpha, beta);
                    _slots[c, r] = null;

                    maxEval = Mathf.Max(maxEval, eval);
                    alpha = Mathf.Max(alpha, eval);
                    if (beta <= alpha) break; 
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                for (int c = 0; c < _columns; c++)
                {
                    int r = GetNextEmptyRow(c);
                    if (r == -1) continue;

                    _slots[c, r] = new Connect4Chip { Team = playerTeam };
                    int eval = Minimax(depth - 1, true, aiTeam, alpha, beta);
                    _slots[c, r] = null;

                    minEval = Mathf.Min(minEval, eval);
                    beta = Mathf.Min(beta, eval);
                    if (beta <= alpha) break; 
                }
                return minEval;
            }
        }

        private int EvaluateBoard(Connect4Teams aiTeam)
        {
            Connect4Teams playerTeam = (aiTeam == Connect4Teams.Red) ? Connect4Teams.Yellow : Connect4Teams.Red;
            int score = 0;

            int centerCol = _columns / 2;

            for (int c = 0; c < _columns; c++)
            {
                for (int r = 0; r < _rows; r++)
                {
                    if (!_slots[c, r].HasValue) continue;

                    int potential = CountPotential(c, r, _slots[c, r].Value.Team);

                    if (_slots[c, r].Value.Team == aiTeam)
                        score += potential;
                    else
                        score -= potential;

                    int distanceFromCenter = Mathf.Abs(c - centerCol);
                    if (_slots[c, r].Value.Team == aiTeam)
                        score += (3 - distanceFromCenter);
                    else
                        score -= (3 - distanceFromCenter);
                }
            }

            return score;
        }

        private int CountPotential(int col, int row, Connect4Teams team)
        {
            int total = 0;
            total += CountDir(col, row, 1, 0, team) + CountDir(col, row, -1, 0, team);
            total += CountDir(col, row, 0, 1, team) + CountDir(col, row, 0, -1, team);
            total += CountDir(col, row, 1, 1, team) + CountDir(col, row, -1, -1, team);
            total += CountDir(col, row, 1, -1, team) + CountDir(col, row, -1, 1, team);
            return total;
        }

        private bool CheckWin(int col, int row, Connect4Teams team)
        {
            if (CountDir(col, row, 1, 0, team) + CountDir(col, row, -1, 0, team) >= 3) return true;
            if (CountDir(col, row, 0, 1, team) + CountDir(col, row, 0, -1, team) >= 3) return true;
            if (CountDir(col, row, 1, 1, team) + CountDir(col, row, -1, -1, team) >= 3) return true;
            if (CountDir(col, row, 1, -1, team) + CountDir(col, row, -1, 1, team) >= 3) return true;
            return false;
        }

        private int CountDir(int col, int row, int dCol, int dRow, Connect4Teams team)
        {
            int count = 0;
            int c = col + dCol, r = row + dRow;
            while (c >= 0 && c < _columns && r >= 0 && r < _rows)
            {
                if (_slots[c, r].HasValue && _slots[c, r].Value.Team == team)
                {
                    count++;
                    c += dCol; r += dRow;
                }
                else break;
            }
            return count;
        }

        private void Awake()
        {
            BuildSlotGrid();
            Restart();
        }

        private void Update()
        {
        }
    }
}
