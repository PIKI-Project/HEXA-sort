using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core
{
    public class CellGrid
    {
        public readonly bool[,] _mask;

        public readonly int _width, _height;

        // Cells grid is strings with delta (hex grid)
        // # # # #
        //  # # # #
        // # # # #
        public readonly Cell[,] Cells;

        public CellGrid(bool[,] mask)
        {
            _height = mask.GetLength(0);
            _width = mask.GetLength(1);

            _mask = new bool[_height, _width];
            Array.Copy(mask, _mask, mask.Length);

            Cells = new Cell[_height, _width];
            for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                Cells[y, x] = new Cell();
            }
        }

        public bool TryPushCell(int posX, int posY, Cell cell)
        {
            if (!IsValidMove(cell, posX, posY))
                return false;

            Cells[posX, posY] = cell;

            return true;
        }

        private bool IsValidMove(Cell from, int toPosX, int toPosY)
        {
            if (!_mask[toPosX, toPosY] || !Cells[toPosX, toPosY].IsEmpty)
                return false;

            return !from.IsEmpty;
        }
    }

    public class GridManager
    {
        private readonly CellGrid _grid;

        public GridManager(bool[,] mask, List<(int x, int y, Cell cell)> startCells = null)
        {
            _grid = new CellGrid(mask);

            if (startCells == null) return;

            foreach ((int x, int y, Cell cell) in startCells)
            {
                _grid.TryPushCell(x, y, cell);
            }
        }

        public int Width => _grid._width;

        public int Height => _grid._height;

        public bool TryMove(Cell from, int toPosX, int toPosY) => _grid.TryPushCell(toPosX, toPosY, from);

        [CanBeNull]
        public Cell GetCell(int x, int y) => _grid._mask[y, x] ? _grid.Cells[y, x] : null;
    }
}
