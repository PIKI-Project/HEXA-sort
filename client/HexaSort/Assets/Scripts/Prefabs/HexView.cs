using UnityEngine;

namespace prefabs
{
    public class HexView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        public int Index { get; set; }

        public void SetColor(int value)
        {
            Color color = value switch
            {
                0 => new Color32(100, 100, 100, 255),
                1 => Color.red,
                2 => Color.blue,
                3 => Color.green,
                4 => Color.yellow,
                _ => Color.white
            };

            _renderer.material.SetColor(_baseColor, color);
        }
    }
}
