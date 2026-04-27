using Core;
using prefabs;
using Progress;
using UnityEngine;

namespace Controller
{
    public class GameController : MonoBehaviour
    {
        private const int _movesCount = 3;

        public GridView gridView;
        private readonly Cell[] _moves = new Cell[_movesCount];
        private GridManager _gridMgr;
        private int? _selectedCell;

        public void Build(LevelData data)
        {
            _gridMgr = new GridManager(data.Mask, data.StartCells);
            gridView.CreateGrid(_gridMgr);
        }

        /*private void RefreshView()
        {
            for (int i = 0; i < _grid.Cells.Count; i++)
            {
                Hex top = _grid.Cells[i].Top();
                gridView.UpdateCell(i, top);
            }
        }*/

        public void OnCellClicked(int index)
        {
            /*if (_selectedCell == null)
            {
                _selectedCell = index;
            }
            else
            {
                bool moved = _grid.TryMove(_selectedCell.Value, index);

                if (moved)
                {
                    RefreshView();
                    if (_grid.CheckWin())
                    {
                        Debug.Log("You win!");
                    }
                }

                _selectedCell = null;
            }*/
        }
    }
}
