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

        public void Build(LevelData data)
        {
            _gridMgr = new GridManager(data.Mask, data.StartCells);
            gridView.CreateGrid(_gridMgr, _movesCount);

            for (int i = 0; i < _movesCount; i++)
            {
                _moves[i] = new Cell(Creator.CreateStack(1, 2, 3));
            }

            gridView.UpdateMoves(_moves);
        }

        private void RefreshView()
        {
            /*for (int i = 0; i < _gridMgr.; i++)
            {
                Hex top = _grid.Cells[i].Top();
                gridView.UpdateCell(i, top);
            }*/
        }

        public void OnMoveCellClicked(int index)
        {
            if (_gameState == GameState.SELECT)
            {
                Debug.Log("You chosen move: " + index);
                _moveIndex = index;
                _gameState = GameState.MOVE;
            }
            else
            {
                Debug.Log("You are already in move state!");
            }
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
                bool moved = _gridMgr.TryMove(_moves[_moveIndex], x, y);

                if (moved)
                {
                    _moves[_moveIndex].Free();
                    _moveIndex = -1;
                    _gameState = GameState.SELECT;

                    Debug.Log("Move success!!!");
                    gridView.UpdateMoves(_moves);
                    RefreshView();
                    // TODO: Check if win
                }
            }
        }
    }
}
