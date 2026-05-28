using System;
using System.Collections.Generic;
using System.Linq;
using Controller;

namespace Core
{
    // odd-r offset
    internal static class Offsets
    {
        public static readonly HexCoord[] EvenRowDirs =
        {
            new(+1, 0),
            new(-1, 0),

            new(0, -1),
            new(-1, -1),

            new(0, +1),
            new(-1, +1)
        };

        public static readonly HexCoord[] OddRowDirs =
        {
            new(+1, 0),
            new(-1, 0),

            new(+1, -1),
            new(0, -1),

            new(+1, +1),
            new(0, +1)
        };
    }

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

        private readonly int _totalCells;

        public GridManager(bool[,] mask, List<(int x, int y, Cell cell)> startCells = null)
        {
            _grid = new CellGrid(mask);

            int cellsCounter = 0;
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (_grid.Mask[y, x])
                        ++cellsCounter;
                }

            _totalCells = cellsCounter;

            if (startCells == null) return;

            foreach ((int x, int y, Cell cell) in startCells)
            {
                _grid.TryPushCell(x, y, cell);
            }
        }

        public int Width => _grid.Width;

        public int Height => _grid.Height;

        public int GetFreeCells()
        {
            int counter = 0;
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (_grid.Mask[y, x] && _grid.Cells[y, x].IsEmpty)
                        ++counter;
                }

            return counter;
        }

        public HashSet<int> GetAvailableColors()
        {
            HashSet<int> colors = new();

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Cell c = GetCell(x, y);

                    if (c == null || c.IsEmpty)
                        continue;

                    Hex top = c.Peek();
                    if (top != null)
                        colors.Add(top.Type);
                }

            return colors;
        }

        public int GetDeadCells()
        {
            int counter = 0;
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (!_grid.Mask[y, x]) continue;

                    Cell c = _grid.Cells[y, x];

                    if ((double)c.GetMostColorRow() / c.Size < 0.4)
                        ++counter;
                }

            return counter;
        }

        public List<ColorStats> GetColorStats()
        {
            Dictionary<int, ColorStats> statsMap = new();

            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (!_grid.Mask[y, x])
                        continue;

                    Cell cell = _grid.Cells[y, x];

                    if (cell.IsEmpty)
                        continue;

                    // =========
                    // TOTAL COUNT
                    // =========

                    foreach (Hex hex in cell.Items)
                    {
                        if (!statsMap.TryGetValue(hex.Type, out ColorStats totalStats))
                        {
                            totalStats = new ColorStats
                            {
                                Color = hex.Type
                            };

                            statsMap[hex.Type] = totalStats;
                        }

                        totalStats.TotalCount++;
                    }

                    // =========
                    // TOP COUNT
                    // =========

                    Hex top = cell.Peek();

                    if (top == null)
                        continue;

                    int topColor = top.Type;

                    ColorStats stats = statsMap[topColor];

                    stats.TopCount++;

                    // =========
                    // NEAR COMPLETE
                    // =========

                    int topRow = cell.GetTopColorCount();
                    if (topRow >= 7)
                    {
                        stats.NearCompleteStacks++;
                    }

                    // =========
                    // DEAD STACKS
                    // =========

                    double uniformity =
                        (double)cell.GetMostColorRow() / cell.Size;

                    if (uniformity < 0.4)
                    {
                        stats.DeadStacks++;
                    }

                    // =========
                    // NEIGHBOR POTENTIAL
                    // =========

                    HexCoord[] dirs =
                        (y & 1) == 0
                            ? Offsets.EvenRowDirs
                            : Offsets.OddRowDirs;

                    foreach (HexCoord d in dirs)
                    {
                        int nx = x + d.X;
                        int ny = y + d.Y;

                        if (nx < 0 ||
                            ny < 0 ||
                            nx >= _grid.Width ||
                            ny >= _grid.Height)
                            continue;

                        if (!_grid.Mask[ny, nx])
                            continue;

                        Cell neighbor = _grid.Cells[ny, nx];

                        if (neighbor.IsEmpty)
                            continue;

                        Hex neighborTop = neighbor.Peek();

                        if (neighborTop == null)
                            continue;

                        if (neighborTop.Type == topColor)
                        {
                            stats.NeighborPotential++;
                        }
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                if (statsMap.ContainsKey(i)) continue;

                var totalStats = new ColorStats
                {
                    Color = i,
                    TotalCount = 0,
                    DeadStacks = 0,
                    NearCompleteStacks = 0,
                    NeighborPotential = 0,
                    TopCount = 0
                };

                statsMap[i] = totalStats;
            }

            // =========
            // NORMALIZATION
            // =========
            foreach (ColorStats s in statsMap.Values)
            {
                s.NeighborPotential /= 2;
            }

            return statsMap.Values.ToList();
        }

        public double GetCloseDoneCells()
        {
            double counter = 0;
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    // 7 is good result, just done
                    if (_grid.Mask[y, x])
                        counter += _grid.Cells[y, x].GetTopColorCount() / 7.0;
                }

            return counter;
        }

        public double GetEntropyCells()
        {
            double counter = 0;
            for (int y = 0; y < _grid.Height; y++)
                for (int x = 0; x < _grid.Width; x++)
                {
                    double currentCounter = 0;
                    double currentRealCounter = 0;
                    Hex current = GetCell(x, y)?.Peek();

                    if (current is null) continue;

                    int type = current.Type;

                    HexCoord[] dirs =
                        (y & 1) == 0
                            ? Offsets.EvenRowDirs
                            : Offsets.OddRowDirs;

                    foreach (HexCoord d in dirs)
                    {
                        int nx = x + d.X;
                        int ny = y + d.Y;

                        if (nx < 0 ||
                            ny < 0 ||
                            nx >= _grid.Width ||
                            ny >= _grid.Height)
                            continue;

                        Hex neighbor = GetCell(nx, ny)?.Peek();

                        if (neighbor == null)
                            continue;

                        ++currentRealCounter;
                        if (neighbor.Type != type)
                            currentCounter++;
                    }

                    if (currentRealCounter > 0)
                        counter += currentCounter / currentRealCounter;
                }

            return counter;
        }

        public int GetTotalCells() => _totalCells;

        public bool TryMove(Cell from, int toPosX, int toPosY) => _grid.TryPushCell(toPosX, toPosY, from);

        public Cell GetCell(int x, int y) => _grid.Cells[y, x];

        public bool GetMask(int x, int y) => _grid.Mask[y, x];
    }
}
