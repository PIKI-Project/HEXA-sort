using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Core
{
    public class Cell
    {
        public readonly Stack<Hex> Items;

        public Cell()
        {
            Items = new Stack<Hex>();
        }

        public Cell(Stack<Hex> startData)
        {
            Items = new Stack<Hex>(startData.Reverse());
        }

        public bool IsEmpty => Items.Count == 0;
        public int Size => Items.Count;

        public Stack<Hex> PopTopIdentical()
        {
            var result = new Stack<Hex>();

            if (Items.Count == 0)
                return result;

            int topValue = Items.Peek().Type;

            foreach (Hex hex in Items.TakeWhile(hex => hex.Type == topValue))
            {
                result.Push(hex);
            }

            for (int i = 0; i < result.Count; i++)
            {
                Pop();
            }

            return new Stack<Hex>(result);
        }

        [CanBeNull]
        public Hex Peek() => IsEmpty ? null : Items.Peek();

        public void PushToTop(Stack<Hex> stack)
        {
            foreach (Hex item in stack.Reverse())
            {
                Items.Push(item);
            }
        }

        private Hex Pop() => Items.Pop();

        public void Free() => Items.Clear();

        public bool IsUniform()
        {
            if (IsEmpty) return false;

            int firstType = Items.Peek().Type;

            return Items.All(item => item.Type == firstType);
        }

        public int GetTopColorCount()
        {
            if (IsEmpty) return 0;

            int topType = Items.Peek().Type;

            return Items.TakeWhile(x => x.Type == topType).Count();
        }
    }
}
