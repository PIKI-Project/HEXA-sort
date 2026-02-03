using System.Collections.Generic;
using Controller;
using Core;
using UnityEngine;

namespace prefabs
{
    public class CellClickHandler : MonoBehaviour
    {
        private GameController _controller;
        private int _index;

        public void Init(GameController controller, int index)
        {
            _controller = controller;
            _index = index;
        }

        private void OnMouseDown() => _controller.OnCellClicked(_index);
    }

    public class GridView : MonoBehaviour
    {
        public GameObject cellPrefab;
        public GameController controller;
        private readonly List<CellView> _cellViews = new();

        public void CreateGrid(int cellCount)
        {
            for (int i = 0; i < cellCount; i++)
            {
                GameObject obj = Instantiate(cellPrefab, new Vector3(i * 2f, 0.6f, i * 2f), Quaternion.identity);
                var view = obj.GetComponent<CellView>();
                view.Index = i;

                obj.AddComponent<CellClickHandler>().Init(controller, i);

                _cellViews.Add(view);
            }
        }

        public void UpdateCell(int index, Hex topValue)
        {
            if (topValue == null)
                _cellViews[index].SetColor(-1);
            else
                _cellViews[index].SetColor(topValue.Type);
        }
    }
}
