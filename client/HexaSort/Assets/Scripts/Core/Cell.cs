using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core
{
    public class Cell
    {
        private readonly Stack<Hex> _items;
        private int Capacity { get; }

        public Cell(int capacity)
        {
            Capacity = capacity;
            _items = new Stack<Hex>(capacity);
        }

        public bool IsEmpty => _items.Count == 0;
        public bool IsFull => _items.Count >= Capacity;

        [CanBeNull]
        public Hex Top() => IsEmpty ? null : _items.Peek();

        public bool CanPush(Hex value)
        {
            if (IsFull) return false;
            if (IsEmpty) return true;

            return Top() == value;
        }

        public void Push(Hex value) => _items.Push(value);

        public Hex Pop() => _items.Pop();

        public bool IsUniform()
        {
            if (IsEmpty) return false;

            Hex first = _items.Peek();
            foreach (Hex item in _items)
            {
                if (item != first) return false;
            }

            return true;
        }
    }
}
