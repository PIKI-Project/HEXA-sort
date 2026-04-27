using System;
using System.Collections.Generic;
using Core;

namespace Progress
{
    [Serializable]
    public class LevelData
    {
        public readonly bool[,] Mask;
        public readonly List<(int x, int y, Cell cell)> StartCells;

        public LevelData(bool[,] mask, List<(int x, int y, Cell cell)> startCells)
        {
            Mask = mask;
            StartCells = startCells;
        }
    }
}
