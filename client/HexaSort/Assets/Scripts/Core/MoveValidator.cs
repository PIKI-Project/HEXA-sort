namespace Core
{
    public class MoveValidator
    {
        public bool IsValidMove(Cell from, Cell to)
        {
            if (from.IsEmpty) return false;
            if (to.IsFull) return false;

            Hex movingValue = from.Top();

            if (to.IsEmpty) return true;

            return to.Top() == movingValue;
        }
    }
}
