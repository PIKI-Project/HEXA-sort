using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using prefabs;
using Progress;
using UnityEngine;
using Utilities;
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

        // TODO: write system for giving good moves
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

        public ClusterFinder Finder;

        public void Build(LevelData data)
        {
            _gridMgr = new GridManager(data.Mask, data.StartCells);
            gridView.CreateGrid(_gridMgr, _movesCount);

            for (int i = 0; i < _movesCount; i++)
            {
                _moves[i] = new Cell(Creator.CreateStack(3, 2, 1));
            }

            gridView.UpdateMoves(_moves);
            Finder = new ClusterFinder(_gridMgr);
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
            List<List<HexCoord>> clusters =
                Finder.FindAllClusters();
            foreach (List<HexCoord> cluster in clusters)
            {
                HexCoord target =
                    SelectTargetCell(cluster, new HexCoord(lastMoveX, lastMoveY));

                List<ClusterFinder.PullStep> steps = Finder.PullCluster(cluster, target);
                foreach (ClusterFinder.PullStep step in steps)
                {
                    Debug.Log(
                        $"MOVE {step.From.X},{step.From.Y} " +
                        $"-> {step.To.X},{step.To.Y}");

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

            // Rebuild field if needed
            List<List<HexCoord>> clusters =
                Finder.FindAllClusters();
            while (clusters.Count > 0)
            {
                yield return StartCoroutine(RebuildField(x, y));

                clusters = Finder.FindAllClusters();
            }

            // Change game state
            UpdateMoves();
            _gameState = GameState.Select;

            // TODO: Check if win
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
