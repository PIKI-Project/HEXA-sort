using System;
using prefabs;
using UnityEngine;
using DG.Tweening;
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

        public Tween AnimateMoveTo(Vector3 targetPos, float duration = 0.3f)
        {
            if (Obj == null) return null;

            return Obj.transform
                .DOMove(targetPos, duration)
                .SetEase(Ease.OutQuad);
        }

        public Tween AnimateJumpTo(Vector3 targetPos, float jumpPower = 0.6f, float duration = 0.4f)
        {
            if (Obj == null) return null;

            Vector3 direction = targetPos - Obj.transform.position;
            direction.y = 0;
            direction.Normalize();

            Vector3 rotateAxis = Vector3.Cross(direction, Vector3.up) * -360;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(Obj.transform
                .DOJump(targetPos, jumpPower, 1, duration)
                .SetEase(Ease.OutQuad));

            sequence.Join(Obj.transform
                .DORotate(rotateAxis, duration, RotateMode.WorldAxisAdd)
                .SetEase(Ease.Linear));

            sequence.Append(Obj.transform
                .DORotateQuaternion(Quaternion.identity, 0.1f)
                .SetEase(Ease.OutQuad));

            sequence.Append(Obj.transform
                .DOScale(1.1f, 0.08f)
                .SetEase(Ease.OutQuad));
            
            sequence.Append(Obj.transform
                .DOScale(1.0f, 0.08f)
                .SetEase(Ease.InQuad));

            return sequence;
        }

        public Tween AnimateDisappear(float duration = 0.2f)
        {
            if (Obj == null) return null;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(Obj.transform
                .DOMove(Obj.transform.position + Vector3.up * 0.3f, duration * 0.5f)
                .SetEase(Ease.OutQuad));

            sequence.Join(Obj.transform
                .DOScale(0f, duration)
                .SetEase(Ease.InBack));

            return sequence;
        }
    }
}