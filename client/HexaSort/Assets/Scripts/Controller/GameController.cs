using Core;
using prefabs;
using UnityEngine;

namespace Controller
{
    public class GameController : MonoBehaviour
    {
        public GridView gridView;
        private GridManager _grid;

        private int? _selectedCell;

        private void Start()
        {
            _grid = new GridManager(7, 4);
            gridView.CreateGrid(_grid.Cells.Count);

            _grid.Cells[0].Push(new Hex(0));
            _grid.Cells[0].Push(new Hex(1));
            _grid.Cells[1].Push(new Hex(1));
            _grid.Cells[2].Push(new Hex(0));

            RefreshView();
        }

        private void RefreshView()
        {
            for (int i = 0; i < _grid.Cells.Count; i++)
            {
                Hex top = _grid.Cells[i].Top();
                gridView.UpdateCell(i, top);
            }
        }

        public void OnCellClicked(int index)
        {
            if (_selectedCell == null)
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
            }
        }
    }
}
