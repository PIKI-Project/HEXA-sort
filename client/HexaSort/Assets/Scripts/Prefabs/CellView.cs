using UnityEngine;

namespace prefabs
{
    public class CellView : MonoBehaviour
    {
        public int Index { get; set; }
        private Renderer _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void SetColor(int value)
        {
            Color color = value switch
            {
                0 => Color.red,
                1 => Color.blue,
                2 => Color.green,
                3 => Color.yellow,
                _ => Color.white
            };
            _renderer.material.color = color;
        }
    }
}
