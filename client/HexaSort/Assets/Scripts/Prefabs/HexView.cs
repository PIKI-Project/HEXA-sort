using UnityEngine;

namespace prefabs
{
    public class HexView : MonoBehaviour
    {
        private static readonly int _baseColor = Shader.PropertyToID("_BaseColor");
        private Renderer _renderer;
        public int Index { get; set; }

        private void Awake() => _renderer = GetComponent<Renderer>();

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
