using System;
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

        private void OnMouseDown() => _controller.OnCellClicked(_index);

        public void Init(GameController controller, int index)
        {
            _controller = controller;
            _index = index;
        }
    }

    public class GridView : MonoBehaviour
    {
        // Distance between (rad=1) centers of neighbor hexagons = sqrt(3)
        private static readonly double _centerDistance = Math.Sqrt(3) + 0.04;
        private static readonly double _sin60 = Math.Sin(Math.PI / 3);
        private static readonly double _centerDistanceSin60 = _centerDistance * _sin60;

        private static readonly float _layerThreshold = 1.2f;
        private static readonly float _hexaHeight = 0.18f;

        public GameObject hexPrefab;
        public GameController controller;
        private HexStackPos[,] _hexStackPoses;
        private Stack<HexView>[,] _hexStacks;

        public void CreateGrid(GridManager gridMgr)
        {
            _hexStacks = new Stack<HexView>[gridMgr.Height, gridMgr.Width];
            for (int y = 0; y < gridMgr.Height; y++)
                for (int x = 0; x < gridMgr.Width; x++)
                {
                    _hexStacks[y, x] = new Stack<HexView>();
                }

            _hexStackPoses = new HexStackPos[gridMgr.Height, gridMgr.Width];

            double upperLeftY = (gridMgr.Height - 1) * _centerDistanceSin60 / 2;
            double upperLeftX = -gridMgr.Width * _centerDistance / 2;

            int cnt = 0;
            for (int i = 0; i < gridMgr.Height; i++)
            {
                for (int j = 0; j < gridMgr.Width; j++)
                {
                    float y = (float)(upperLeftY - i * _centerDistanceSin60);
                    float x = (float)(upperLeftX + j * _centerDistance + _centerDistance * (i % 2) / 2);

                    Cell cell = gridMgr.GetCell(j, i);

                    if (cell == null)
                        continue;

                    GameObject platform =
                        Instantiate(hexPrefab, new Vector3(x, -_hexaHeight / 2, y), Quaternion.identity);
                    platform.AddComponent<CellClickHandler>().Init(controller, cnt++);
                    HexView platformView = platform.GetComponent<HexView>();
                    platformView.SetColor(0);
                    _hexStacks[i, j].Push(platformView);

                    int level = 0;
                    foreach (Hex hex in cell.Items)
                    {
                        GameObject obj = Instantiate(hexPrefab,
                            new Vector3(x, _hexaHeight * _layerThreshold / 2 +
                                           level * (_hexaHeight * _layerThreshold), y),
                            Quaternion.identity);
                        level++;
                        HexView view = obj.GetComponent<HexView>();
                        view.SetColor(hex.Type);

                        _hexStacks[i, j].Push(view);
                    }
                }
            }
        }

        /*public void UpdateCell(int index, Hex topValue)
        {
            if (topValue == null)
                _hexStacks[index][0].SetColor(-1);
            else
                _hexStacks[index][0].SetColor(topValue.Type);
        }*/

        private struct HexStackPos
        {
            private float _x;
            private float _y;
        }
    }
}
