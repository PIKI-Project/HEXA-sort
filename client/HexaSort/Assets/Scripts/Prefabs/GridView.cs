using System;
using System.Collections.Generic;
using Controller;
using Core;
using TMPro;
using UnityEngine;

namespace prefabs
{
    public class MoveCellClickHandler : MonoBehaviour
    {
        private GameController _controller;
        private int _index;

        private void OnMouseDown() => _controller.OnMoveCellClicked(_index);

        public void Init(GameController controller, int index)
        {
            _controller = controller;
            _index = index;
        }
    }

    public class CellClickHandler : MonoBehaviour
    {
        private GameController _controller;
        private int _posX, _posY;

        private void OnMouseDown() => _controller.OnCellClicked(_posX, _posY);

        public void Init(GameController controller, int x, int y)
        {
            _controller = controller;
            _posX = x;
            _posY = y;
        }
    }

    public class GridView : MonoBehaviour
    {
        private const float _layerThreshold = 1.2f;

        private const float _hexaHeight = 0.18f;

        private const float _labelHeight = 0.3f;

        // Distance between (rad=1) centers of neighbor hexagons = sqrt(3)
        private static readonly double _centerDistance = Math.Sqrt(3) + 0.04;
        private static readonly double _sin60 = Math.Sin(Math.PI / 3);
        private static readonly double _centerDistanceSin60 = _centerDistance * _sin60;

        public GameObject hexPrefab;
        public GameObject hexCountLabelPrefab;
        public GameController controller;
        private GameObject[,] _cellLabelObjects;

        private TextMeshProUGUI[,] _cellLabels;

        private List<GameObject>[,] _cellObjects;
        private List<GameObject>[] _moveCellObjects;
        private GameObject[] _moveLabelObjects;
        private TextMeshProUGUI[] _moveLabels;
        private GameObject[] _movePlatformObjects;

        private GameObject[,] _platformObjects;

        private double _upperLeftX, _upperLeftY;

        public void UpdateMoves(Cell[] moves)
        {
            if (_moveCellObjects == null)
            {
                _moveCellObjects = new List<GameObject>[moves.Length];
                for (int i = 0; i < _moveCellObjects.Length; i++)
                {
                    _moveCellObjects[i] = new List<GameObject>();
                }
            }

            foreach (List<GameObject> cell in _moveCellObjects)
            {
                foreach (GameObject obj in cell)
                {
                    Destroy(obj);
                }
            }

            for (int i = 0; i < moves.Length; i++)
            {
                Vector3 pos = _movePlatformObjects[i].transform.position;

                int level = moves[i].Items.Count;
                foreach (Hex hex in moves[i].Items)
                {
                    GameObject obj = Instantiate(hexPrefab,
                        new Vector3(pos.x, pos.y + level * (_hexaHeight * _layerThreshold), pos.z),
                        Quaternion.identity);
                    level--;

                    _moveCellObjects[i].Add(obj);
                    HexView view = obj.GetComponent<HexView>();
                    view.SetColor(hex.Type);
                }
            }
        }

        public void UpdateCell(int x, int y, Cell cell)
        {
            if (_cellObjects[y, x].Count != 0)
            {
                Debug.Log("Tha hell?! Impossible");

                return;
            }

            int level = cell.Items.Count - 1;
            Vector3 pos = _platformObjects[y, x].transform.position;
            foreach (Hex hex in cell.Items)
            {
                GameObject obj = Instantiate(hexPrefab,
                    new Vector3(pos.x, _hexaHeight * _layerThreshold / 2 +
                                       level * (_hexaHeight * _layerThreshold), pos.z),
                    Quaternion.identity);
                _cellObjects[y, x].Add(obj);

                level--;
                HexView view = obj.GetComponent<HexView>();
                view.SetColor(hex.Type);
            }

            UpdateCellLabel(x, y, cell);
        }

        public void UpdateCellLabel(int x, int y, Cell cell)
        {
            if (_cellLabels == null || _cellLabels[y, x] == null) return;

            int count = cell.GetTopColorCount();
            _cellLabels[y, x].text = count > 0 ? count.ToString() : "";

            Vector3 pos = _platformObjects[y, x].transform.position;
            float stackHeight = cell.Size * _hexaHeight * _layerThreshold;
            _cellLabelObjects[y, x].transform.position = new Vector3(pos.x, stackHeight + _labelHeight, pos.z);
        }

        public void CreateGrid(GridManager gridMgr, int movesCount)
        {
            _platformObjects = new GameObject[gridMgr.Height, gridMgr.Width];
            _cellObjects = new List<GameObject>[gridMgr.Height, gridMgr.Width];
            _cellLabels = new TextMeshProUGUI[gridMgr.Height, gridMgr.Width];
            _cellLabelObjects = new GameObject[gridMgr.Height, gridMgr.Width];
            for (int y = 0; y < gridMgr.Height; y++)
                for (int x = 0; x < gridMgr.Width; x++)
                {
                    _cellObjects[y, x] = new List<GameObject>();
                }

            _upperLeftY = (gridMgr.Height - 1) * _centerDistanceSin60 / 2;
            _upperLeftX = -gridMgr.Width * _centerDistance / 2;

            // Poses for player moves
            _movePlatformObjects = new GameObject[movesCount];
            for (int i = 0; i < movesCount; i++)
            {
                var vec = new Vector3((float)(_upperLeftX - 2 * _centerDistance - 0.3f * i), -_hexaHeight / 2,
                    (float)_upperLeftY - 2.3f * i);
                GameObject move = Instantiate(hexPrefab, vec, Quaternion.identity);
                move.AddComponent<MoveCellClickHandler>().Init(controller, i);
                _movePlatformObjects[i] = move;
            }

            // Game cells
            for (int i = 0; i < gridMgr.Height; i++)
            {
                for (int j = 0; j < gridMgr.Width; j++)
                {
                    float y = (float)(_upperLeftY - i * _centerDistanceSin60);
                    float x = (float)(_upperLeftX + j * _centerDistance + _centerDistance * (i % 2) / 2);

                    Cell cell = gridMgr.GetCell(j, i);

                    if (cell == null)
                        continue;

                    // Create platform
                    GameObject platform =
                        Instantiate(hexPrefab, new Vector3(x, -_hexaHeight / 2, y), Quaternion.identity);
                    platform.AddComponent<CellClickHandler>().Init(controller, j, i);
                    _platformObjects[i, j] = platform;

                    HexView platformView = platform.GetComponent<HexView>();
                    platformView.SetColor(0);

                    if (hexCountLabelPrefab != null)
                    {
                        GameObject label = Instantiate(hexCountLabelPrefab, new Vector3(x, _labelHeight, y),
                            Quaternion.identity);
                        _cellLabelObjects[i, j] = label;
                        _cellLabels[i, j] = label.GetComponentInChildren<TextMeshProUGUI>();
                    }

                    // Create hexes on platform
                    int level = cell.Items.Count - 1;
                    foreach (Hex hex in cell.Items)
                    {
                        GameObject obj = Instantiate(hexPrefab,
                            new Vector3(x, _hexaHeight * _layerThreshold / 2 +
                                           level * (_hexaHeight * _layerThreshold), y),
                            Quaternion.identity);
                        _cellObjects[i, j].Add(obj);

                        level--;
                        HexView view = obj.GetComponent<HexView>();
                        view.SetColor(hex.Type);
                    }

                    UpdateCellLabel(j, i, cell);
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
