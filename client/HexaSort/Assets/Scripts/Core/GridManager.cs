using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core
{
    public class CellGrid
    {
        // Cells grid is strings with delta (hex grid)
        // # # # #
        //  # # # #
        // # # # #
        public readonly Cell[,] Cells;

        public CellGrid(bool[,] mask)
        {
            Height = mask.GetLength(0);
            Width = mask.GetLength(1);

            Mask = new bool[Height, Width];
            Array.Copy(mask, Mask, mask.Length);

            Cells = new Cell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Cells[y, x] = new Cell();
                }
        }

        public int Width { get; }

        public int Height { get; }

        public bool[,] Mask { get; }

        public bool TryPushCell(int posX, int posY, Cell cell)
        {
            if (!IsValidMove(cell, posX, posY))
                return false;

            Cells[posY, posX] = cell;

            return true;
        }

        private bool IsValidMove(Cell from, int toPosX, int toPosY)
        {
            if (!Mask[toPosY, toPosX] || !Cells[toPosY, toPosX].IsEmpty)
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

        public int Width => _grid.Width;

        public int Height => _grid.Height;

        public bool TryMove(Cell from, int toPosX, int toPosY) => _grid.TryPushCell(toPosX, toPosY, from);

        [CanBeNull]
        public Cell GetCell(int x, int y) => _grid.Mask[y, x] ? _grid.Cells[y, x] : null;
    }
}
