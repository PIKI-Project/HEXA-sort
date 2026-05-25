using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using prefabs;
using Progress;
using UnityEngine;
using Utilities;
using HexaSort.Services;
using Random = UnityEngine.Random;

namespace Controller
{
    internal enum GameState
    {
        Animating,
        Select,
        Move
    }

    public class GameController : MonoBehaviour
    {
        private const int _movesCount = 3;
        public GridView gridView;

        private readonly Cell[] _moves = new Cell[_movesCount];

        private readonly Cell[] _someReadyMoves =
        {
            new(Creator.CreateStack(3, 2, 2)),
            new(Creator.CreateStack(3, 2, 1)),
            new(Creator.CreateStack(3, 3, 3)),
            new(Creator.CreateStack(2, 2, 2)),
            new(Creator.CreateStack(2, 2, 1)),
            new(Creator.CreateStack(1, 3, 3)),
            new(Creator.CreateStack(2, 2, 3))
        };

        private GameState _gameState = GameState.Select;
        private GridManager _gridMgr;
        private int _moveIndex = -1;
        private int _currentScore = 0;

        public ClusterFinder Finder;

        public void Build(Progress.LevelData data)
        {
            _gridMgr = new GridManager(data.Mask, data.StartCells);
            gridView.CreateGrid(_gridMgr, _movesCount);

            int levelId = PlayerPrefs.GetInt("SelectedLevel", 1);

            if (PlayerProgressService.Instance != null && 
                PlayerProgressService.Instance.HasActiveGame(levelId))
            {
                Debug.Log("[GameController] Restoring saved game...");
                RestoreFromSave(PlayerProgressService.Instance.GetActiveGameState());
            }
            else
            {
                Debug.Log("[GameController] Starting new game");
                StartNewGame();
            }

            Finder = new ClusterFinder(_gridMgr);
        }

        private void StartNewGame()
        {
            for (int i = 0; i < _movesCount; i++)
            {
                _moves[i] = new Cell(Creator.CreateStack(3, 2, 1));
            }
            gridView.UpdateMoves(_moves);
            _currentScore = 0;
        }

        private void RestoreFromSave(ActiveGameState saved)
        {
            _currentScore = saved.score;

            for (int i = 0; i < _movesCount; i++)
            {
                _moves[i] = new Cell();
            }

            foreach (CellState moveState in saved.moveSlots)
            {
                int slotIndex = moveState.x;
                if (slotIndex >= 0 && slotIndex < _movesCount)
                {
                    var stack = new Stack<Hex>();
                    for (int i = moveState.hexTypes.Count - 1; i >= 0; i--)
                    {
                        stack.Push(new Hex(moveState.hexTypes[i]));
                    }
                    _moves[slotIndex] = new Cell(stack);
                }
            }
            gridView.UpdateMoves(_moves);

            for (int y = 0; y < _gridMgr.Height; y++)
            {
                for (int x = 0; x < _gridMgr.Width; x++)
                {
                    Cell cell = _gridMgr.GetCell(x, y);
                    if (cell != null)
                    {
                        cell.Free();
                        gridView.UpdateCell(x, y, cell);
                    }
                }
            }

            foreach (CellState cellState in saved.gridCells)
            {
                Cell cell = _gridMgr.GetCell(cellState.x, cellState.y);
                if (cell != null)
                {
                    for (int i = cellState.hexTypes.Count - 1; i >= 0; i--)
                    {
                        cell.Items.Push(new Hex(cellState.hexTypes[i]));
                    }
                    gridView.UpdateCell(cellState.x, cellState.y, cell);
                }
            }

            Debug.Log($"[GameController] Restored: {saved.gridCells.Count} cells, {saved.moveSlots.Count} moves");
        }

        private void UpdateMoves()
        {
            if (_moves.Any(mv => !mv.IsEmpty))
            {
                return;
            }

            for (int i = 0; i < _movesCount; i++)
            {
                _moves[i] = new Cell(_someReadyMoves[Random.Range(0, _someReadyMoves.Length - 1)].Items);
            }

            gridView.UpdateMoves(_moves);
        }

        public void OnMoveCellClicked(int index)
        {
            if (_gameState == GameState.Select)
            {
                if (_moves[index].IsEmpty)
                {
                    Debug.Log("This cell is empty!");
                    return;
                }

                Debug.Log("You chosen move: " + index);
                _moveIndex = index;
                _gameState = GameState.Move;
            }
            else
            {
                Debug.Log("You are already in move state!");
            }
        }

        private HexCoord SelectTargetCell(List<HexCoord> cluster, HexCoord lastMove)
        {
            HexCoord chosen = lastMove;
            foreach (HexCoord c in cluster)
            {
                Cell cell = _gridMgr.GetCell(c.X, c.Y);

                if (cell == null)
                    throw new ArgumentNullException(nameof(cell), "Cell not found!");

                if (cell.IsUniform())
                    chosen = new HexCoord(c.X, c.Y);
            }

            return chosen;
        }

        private IEnumerator RebuildField(int lastMoveX, int lastMoveY)
        {
            List<List<HexCoord>> clusters = Finder.FindAllClusters();
            foreach (List<HexCoord> cluster in clusters)
            {
                HexCoord target = SelectTargetCell(cluster, new HexCoord(lastMoveX, lastMoveY));

                List<ClusterFinder.PullStep> steps = Finder.PullCluster(cluster, target);
                foreach (ClusterFinder.PullStep step in steps)
                {
                    Debug.Log($"MOVE {step.From.X},{step.From.Y} -> {step.To.X},{step.To.Y}");

                    Cell fromCell = _gridMgr.GetCell(step.From.X, step.From.Y);
                    Cell toCell = _gridMgr.GetCell(step.To.X, step.To.Y);

                    if (fromCell == null || toCell == null)
                        throw new ArgumentNullException(nameof(toCell), "from/to Cell not found!");

                    toCell.PushToTop(fromCell.PopTopIdentical());
                    gridView.UpdateCell(step.From.X, step.From.Y, fromCell);
                    gridView.UpdateCell(step.To.X, step.To.Y, toCell);

                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        private IEnumerator ProcessMove(int x, int y)
        {
            var moveCopied = new Cell(_moves[_moveIndex].Items);
            _moves[_moveIndex].Free();
            _moveIndex = -1;
            gridView.UpdateMoves(_moves);

            _gameState = GameState.Animating;
            bool moved = _gridMgr.TryMove(new Cell(moveCopied.Items), x, y);

            if (!moved)
            {
                _gameState = GameState.Select;
                yield break;
            }

            gridView.UpdateCell(x, y, new Cell(moveCopied.Items));

            List<List<HexCoord>> clusters = Finder.FindAllClusters();
            while (clusters.Count > 0)
            {
                yield return StartCoroutine(RebuildField(x, y));
                clusters = Finder.FindAllClusters();
            }

            UpdateMoves();
            SaveCurrentState();
            _gameState = GameState.Select;

            // TODO: Check if win
        }

        private async void SaveCurrentState()
        {
            if (PlayerProgressService.Instance == null) return;

            int levelNumber = PlayerPrefs.GetInt("SelectedLevel", 1);

            var gridCells = new List<CellState>();
            for (int y = 0; y < _gridMgr.Height; y++)
            {
                for (int x = 0; x < _gridMgr.Width; x++)
                {
                    Cell cell = _gridMgr.GetCell(x, y);
                    if (cell != null && !cell.IsEmpty)
                    {
                        var state = new CellState
                        {
                            x = x,
                            y = y,
                            hexTypes = new List<int>()
                        };

                        foreach (Hex hex in cell.Items)
                        {
                            state.hexTypes.Add(hex.Type);
                        }

                        gridCells.Add(state);
                    }
                }
            }

            var moveSlots = new List<CellState>();
            for (int i = 0; i < _moves.Length; i++)
            {
                if (!_moves[i].IsEmpty)
                {
                    var state = new CellState
                    {
                        x = i,
                        y = 0,
                        hexTypes = new List<int>()
                    };

                    foreach (Hex hex in _moves[i].Items)
                    {
                        state.hexTypes.Add(hex.Type);
                    }

                    moveSlots.Add(state);
                }
            }

            var gameState = new ActiveGameState
            {
                levelNumber = levelNumber,
                gridCells = gridCells,
                moveSlots = moveSlots,
                score = _currentScore
            };

            bool saved = await PlayerProgressService.Instance.SaveGameStateAsync(gameState);
            Debug.Log($"[GameController] Game state saved: {saved}");
        }

        public void OnCellClicked(int x, int y)
        {
            if (_gameState != GameState.Move)
            {
                Debug.Log("You need to choose move!");
            }
            else
            {
                Debug.Log("You chosen cell: {" + x + ", " + y + "}");
                StartCoroutine(ProcessMove(x, y));
            }
        }
    }
}