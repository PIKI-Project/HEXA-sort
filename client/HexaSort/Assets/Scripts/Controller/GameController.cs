using System.Collections.Generic;
using System.Linq;
using Core;
using prefabs;
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

        private void Start()
        {
            // TODO: replace with map loader
            bool[,] mask =
            {
                { false, true, true, false, false },
                { true, true, true, false, false },
                { false, false, true, false, false },
                { false, false, false, false, false }
            };

            var startCells = new List<(int x, int y, Cell cell)>
            {
                (0, 1, new Cell(CreateStack(1, 1, 1))),
                (0, 2, new Cell(CreateStack(2, 2, 2, 3))),
                (2, 2, new Cell(CreateStack(1, 2, 3)))
            };

            _gridMgr = new GridManager(mask, startCells);
            gridView.CreateGrid(_gridMgr);
        }

        private Stack<Hex> CreateStack(params int[] values) => new(values.Select(v => new Hex(v)));

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
