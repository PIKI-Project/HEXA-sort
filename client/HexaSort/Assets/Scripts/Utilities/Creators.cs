using System.Collections.Generic;
using System.Linq;
using Core;

namespace Utilities
{
    public class Creator
    {
        public static Stack<Hex> CreateStack(params int[] values) => new(values.Select(v => new Hex(v)));
    }
}
