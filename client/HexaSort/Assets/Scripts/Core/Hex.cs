using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
    public struct HexCoord : IEquatable<HexCoord>
    {
        public int X;
        public int Y;

        public HexCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(HexCoord other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is HexCoord other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public class Hex
    {
        public readonly int Type;

        private HexCoord _pos = new(0, 0);
        public bool IsMovable = true;
        public GameObject Obj;

        public Hex(int type)
        {
            Type = type;
        }

        public Hex(Hex other)
        {
            Type = other.Type;
            _pos = other._pos;
            IsMovable = other.IsMovable;

            Obj = null;
        }

        public void Free() => Object.Destroy(Obj);

        public void SetPos(int posX, int posY)
        {
            _pos.X = posX;
            _pos.Y = posY;
        }
    }
}
