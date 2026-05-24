using System.Collections.Generic;
using Core;
using UnityEngine;
using Utilities;

namespace Progress
{
    [CreateAssetMenu(menuName = "Game/LevelDatabase")]
    public class LevelDatabase : ScriptableObject
    {
        // TODO: Change to level loading from cloud database
        private static readonly bool[,] _mask =
        {
            { false, true, true, true, true },
            { true, true, true, true, true },
            { false, true, true, true, false },
            { true, true, true, true, true }
        };

        private static readonly List<(int x, int y, Cell cell)> _startCells = new()
        {
            (1, 0, new Cell(Creator.CreateStack(1, 1, 1))),
            (2, 0, new Cell(Creator.CreateStack(3, 2, 2, 2))),
            (2, 2, new Cell(Creator.CreateStack(3, 2, 1)))
        };

        private static readonly LevelData[] _levels =
        {
            new(_mask, _startCells),
            new(_mask, _startCells),
            new(_mask, _startCells),
            new(_mask, _startCells)
        };

        public static LevelData GetLevel(int id) => _levels[id];
    }
}
