using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Core
{
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        public readonly int X;
        public readonly int Y;

        public HexCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(HexCoord other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public class ClusterFinder
    {
        // odd-r offset
        private static readonly HexCoord[] _evenRowDirs =
        {
            new(+1, 0),
            new(-1, 0),

            new(0, -1),
            new(-1, -1),

            new(0, +1),
            new(-1, +1)
        };

        private static readonly HexCoord[] _oddRowDirs =
        {
            new(+1, 0),
            new(-1, 0),

            new(+1, -1),
            new(0, -1),

            new(+1, +1),
            new(0, +1)
        };

        private readonly GridManager _grid;

        public ClusterFinder(GridManager grid)
        {
            _grid = grid;
        }

        public List<List<HexCoord>> FindAllClusters()
        {
            bool[,] visited = new bool[_grid.Height, _grid.Width];

            List<List<HexCoord>> result = new();

            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    if (visited[y, x])
                        continue;

                    Cell cell = _grid.GetCell(x, y);

                    if (!IsValid(cell))
                        continue;

                    List<HexCoord> cluster =
                        FindClusterInternal(x, y, visited);

                    if (cluster.Count > 1)
                        result.Add(cluster);
                }
            }

            return result;
        }

        private List<HexCoord> FindClusterInternal(
            int startX,
            int startY,
            bool[,] visited)
        {
            Cell startCell = _grid.GetCell(startX, startY);

            int targetColor = GetTopColor(startCell);

            Queue<HexCoord> queue = new();

            List<HexCoord> cluster = new();

            queue.Enqueue(new HexCoord(startX, startY));

            visited[startY, startX] = true;

            while (queue.Count > 0)
            {
                HexCoord current = queue.Dequeue();

                cluster.Add(current);

                foreach (HexCoord n in GetNeighbors(current))
                {
                    if (visited[n.Y, n.X])
                        continue;

                    Cell neighbor = _grid.GetCell(n.X, n.Y);

                    if (!IsValid(neighbor))
                        continue;

                    if (GetTopColor(neighbor) != targetColor)
                        continue;

                    visited[n.Y, n.X] = true;

                    queue.Enqueue(n);
                }
            }

            return cluster;
        }

        private IEnumerable<HexCoord> GetNeighbors(HexCoord pos)
        {
            HexCoord[] dirs =
                (pos.Y & 1) == 0
                    ? _evenRowDirs
                    : _oddRowDirs;

            foreach (HexCoord d in dirs)
            {
                int nx = pos.X + d.X;
                int ny = pos.Y + d.Y;

                if (nx < 0 ||
                    ny < 0 ||
                    nx >= _grid.Width ||
                    ny >= _grid.Height)
                    continue;

                if (_grid.GetCell(nx, ny) == null)
                    continue;

                yield return new HexCoord(nx, ny);
            }
        }

        // Cluster pulling
        public List<PullStep> PullCluster(
            List<HexCoord> cluster,
            HexCoord target)
        {
            HashSet<HexCoord> clusterSet = new(cluster);

            Queue<HexCoord> queue = new();

            Dictionary<HexCoord, HexCoord> parent = new();

            Dictionary<HexCoord, int> distance = new();

            queue.Enqueue(target);

            distance[target] = 0;

            // BFS tree from target
            while (queue.Count > 0)
            {
                HexCoord current = queue.Dequeue();

                foreach (HexCoord n in GetNeighbors(current))
                {
                    if (!clusterSet.Contains(n))
                        continue;

                    if (distance.ContainsKey(n))
                        continue;

                    distance[n] = distance[current] + 1;

                    parent[n] = current;

                    queue.Enqueue(n);
                }
            }

            // far -> near
            var ordered =
                cluster
                    .Where(c => !c.Equals(target))
                    .OrderByDescending(c => distance[c])
                    .ToList();

            List<PullStep> result = new();

            foreach (HexCoord cell in ordered)
            {
                result.Add(new PullStep(
                    cell,
                    parent[cell]));
            }

            return result;
        }

        private bool IsValid(Cell cell) => cell is { Size: > 0 };

        private int GetTopColor(Cell cell)
        {
            Hex top = cell.Peek();

            return top?.Type ?? -1;
        }

        public struct PullStep
        {
            public HexCoord From;
            public HexCoord To;

            public PullStep(HexCoord from, HexCoord to)
            {
                From = from;
                To = to;
            }
        }
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
