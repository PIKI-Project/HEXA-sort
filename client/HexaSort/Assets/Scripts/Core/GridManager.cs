using System.Collections.Generic;

namespace Core
{
    public class GridManager
    {
        public List<Cell> Cells { get; }

        private readonly MoveValidator _moveValidator;

        public GridManager(int cellCount, int cellCapacity)
        {
            Cells = new List<Cell>();

            for (int i = 0; i < cellCount; i++)
            {
                Cells.Add(new Cell(cellCapacity));
            }

            _moveValidator = new MoveValidator();
        }

        public bool TryMove(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return false;

            Cell from = Cells[fromIndex];
            Cell to = Cells[toIndex];

            if (!_moveValidator.IsValidMove(from, to))
                return false;

            Hex value = from.Pop();
            to.Push(value);

            return true;
        }

        public bool CheckWin()
        {
            foreach (Cell cell in Cells)
            {
                if (!cell.IsEmpty && !cell.IsUniform())
                    return false;
            }

            return true;
        }
    }
}
