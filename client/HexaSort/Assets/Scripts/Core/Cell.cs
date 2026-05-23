using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;

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
            Items = new Stack<Hex>(startData);
        }

        public bool IsEmpty => Items.Count == 0;
        public int Size => Items.Count;

        [CanBeNull]
        public Hex Top() => IsEmpty ? null : Items.Peek();

        public void Push(Cell stack)
        {
            foreach (Hex item in stack.Items)
            {
                Items.Push(item);
            }
        }

        public Hex Pop() => Items.Pop();

        public void Free() => Items.Clear();

        public bool IsUniform()
        {
            if (IsEmpty) return false;

            Hex first = Items.Peek();

            return Items.All(item => item == first);
        }

        public int GetTopColorCount()
        {
            if (IsEmpty) return 0;

            Hex[] items = Items.ToArray();
            int topType = items[items.Length - 1].Type;
            int count = 0;

            for (int i = items.Length - 1; i >= 0; i--)
            {
                if (items[i].Type == topType)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }
    }
}
