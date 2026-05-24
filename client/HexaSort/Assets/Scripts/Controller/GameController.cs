using System;
using System.Collections.Generic;
using Core;
using prefabs;
using Progress;
using UnityEngine;
using Utilities;

namespace Controller
{
    internal enum GameState
    {
        ANIMATING,
        SELECT,
        MOVE
    }

    public class GameController : MonoBehaviour
    {
        private const int _movesCount = 3;
        public GridView gridView;

        private readonly Cell[] _moves = new Cell[_movesCount];
        private GameState _gameState = GameState.SELECT;
        private GridManager _gridMgr;
        private int _moveIndex = -1;

        public ClusterFinder Finder;

        public void Build(LevelData data)
        {
            _gridMgr = new GridManager(data.Mask, data.StartCells);
            gridView.CreateGrid(_gridMgr, _movesCount);

            for (int i = 0; i < _movesCount; i++)
            {
                _moves[i] = new Cell(Creator.CreateStack(1, 2, 3));
            }

            gridView.UpdateMoves(_moves);
            Finder = new ClusterFinder(_gridMgr);
        }

        public void OnMoveCellClicked(int index)
        {
            if (_gameState == GameState.SELECT)
            {
                if (_moves[index].IsEmpty)
                {
                    Debug.Log("This cell is empty!");

                    return;
                }

                Debug.Log("You chosen move: " + index);
                _moveIndex = index;
                _gameState = GameState.MOVE;
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

        public void OnCellClicked(int x, int y)
        {
            if (_gameState != GameState.MOVE)
            {
                Debug.Log("You need to choose move!");
            }
            else
            {
                Debug.Log("You chosen cell: {" + x + ", " + y + "}");
                bool moved = _gridMgr.TryMove(new Cell(_moves[_moveIndex].Items), x, y);

                if (moved)
                {
                    Debug.Log("Move success!!!");
                    gridView.UpdateCell(x, y, new Cell(_moves[_moveIndex].Items));

                    // Rebuild field if needed
                    List<List<HexCoord>> clusters =
                        Finder.FindAllClusters();
                    foreach (List<HexCoord> cluster in clusters)
                    {
                        HexCoord target =
                            SelectTargetCell(cluster, new HexCoord(x, y));

                        List<ClusterFinder.PullStep> steps = Finder.PullCluster(cluster, target);
                        foreach (ClusterFinder.PullStep step in steps)
                        {
                            Debug.Log(
                                $"MOVE {step.From.X},{step.From.Y} " +
                                $"-> {step.To.X},{step.To.Y}");
                        }
                    }

                    // Free move that is used
                    _moves[_moveIndex].Free();
                    _moveIndex = -1;
                    gridView.UpdateMoves(_moves);
                    _gameState = GameState.SELECT;

                    // TODO: Check if win
                }
            }
        }
    }
}
