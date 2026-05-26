using System;
using prefabs;
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
        private HexCoord _pos = new(0, 0);
        public GameObject Obj;
        public int Type;

        public Hex(int type)
        {
            Type = type;
        }

        public Hex(Hex other)
        {
            Type = other.Type;
            _pos = other._pos;

            Obj = null;
        }

        public void UpdateColor(int type)
        {
            Type = type;
            HexView view = Obj.GetComponent<HexView>();
            view.SetColor(Type);
        }

        public void Free() => Object.Destroy(Obj);

        public void SetPos(int posX, int posY)
        {
            _pos.X = posX;
            _pos.Y = posY;
        }
    }
}
