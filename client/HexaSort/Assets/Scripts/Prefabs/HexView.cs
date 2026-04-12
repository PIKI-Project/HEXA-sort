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
                0 => new Color32(173, 168, 174, 255),
                1 => new Color32(255, 35, 47, 255),
                2 => new Color32(35, 107, 254, 255),
                3 => new Color32(62, 227, 75, 255),
                4 => Color.yellow,
                _ => Color.white
            };

            _renderer.material.SetColor(_baseColor, color);
        }
    }
}
