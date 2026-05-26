using UnityEngine;

namespace prefabs
{
    public class HexView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        [SerializeField] private Renderer _renderer;
        public int Index { get; set; }

        public void SetColor(int value)
        {
            Color color = value switch
            {
                0 => new Color32(255, 255, 255, 255),
                1 => new Color32(255, 35, 47, 255),
                2 => new Color32(35, 107, 254, 255),
                3 => new Color32(62, 227, 75, 255),
                4 => new Color32(20, 20, 20, 255),
                5 => new Color32(255, 255, 51, 255),
                100 => new Color32(255, 191, 0, 255),
                _ => new Color32(173, 168, 174, 255)

                // HexColor.White => new Color32(255, 255, 255, 255),
                // HexColor.Red => new Color32(255, 35, 47, 255),
                // HexColor.Blue => new Color32(35, 107, 254, 255),
                // HexColor.Green => new Color32(62, 227, 75, 255),
                // HexColor.Black => new Color32(20, 20, 20, 255),
                // HexColor.Yellow => new Color32(255, 255, 51, 255),
                // Special colors
                // HexColor.Platform => new Color32(173, 168, 174, 255),
                // HexColor.Scored => new Color32(173, 168, 174, 255),
                // _ => throw new ArgumentOutOfRangeException(nameof(val), val, null)
            };

            _renderer.material.SetColor(_baseColor, color);
        }
    }
}
