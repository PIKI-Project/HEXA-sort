using System;
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

        private const float _hexHeight = 0.18f;

        private const float _labelHeight = 0.3f;

        // Distance between (rad=1) centers of neighbor hexagons = sqrt(3)
        private static readonly double _centerDistance = Math.Sqrt(3) + 0.04;
        private static readonly double _sin60 = Math.Sin(Math.PI / 3);
        private static readonly double _centerDistanceSin60 = _centerDistance * _sin60;

        public GameObject hexPrefab;
        public GameObject hexPlatformPrefab;
        public GameObject hexCountLabelPrefab;
        public GameController controller;
        private GameObject[,] _cellLabelObjects;

        private TextMeshProUGUI[,] _cellLabels;

        private GameObject[] _moveLabelObjects;
        private TextMeshProUGUI[] _moveLabels;
        private GameObject[] _movePlatformObjects;

        private GameObject[,] _platformObjects;

        private double _upperLeftX, _upperLeftY;

        public void UpdateMoves(Cell[] moves)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                Vector3 pos = _movePlatformObjects[i].transform.position;

                int level = moves[i].Items.Count;
                foreach (Hex hex in moves[i].Items)
                {
                    if (hex.Obj is not null) continue;


                    hex.Obj = Instantiate(hexPrefab,
                        new Vector3(pos.x, pos.y + level * (_hexHeight * _layerThreshold), pos.z),
                        Quaternion.identity);
                    level--;

                    HexView view = hex.Obj.GetComponent<HexView>();
                    view.SetColor(hex.Type);
                }
            }
        }

        private Vector3 ComposePosition(Vector3 pos, int level) =>
            new(pos.x, pos.y + level * (_hexHeight * _layerThreshold), pos.z);

        private GameObject InstantiateHex(Vector3 pos, int hexType)
        {
            GameObject obj = Instantiate(hexPrefab, pos, Quaternion.identity);
            HexView view = obj.GetComponent<HexView>();
            view.SetColor(hexType);

            return obj;
        }

        private GameObject InstantiatePlatform(Vector3 pos)
        {
            GameObject obj = Instantiate(hexPlatformPrefab, pos, Quaternion.identity);
            HexView view = obj.GetComponent<HexView>();
            view.SetColor(-1);

            return obj;
        }

        public void UpdateCell(int x, int y, Cell cell)
        {
            int level = cell.Items.Count;
            GameObject platformObject = _platformObjects[y, x];

            if (platformObject is null) return;

            Vector3 pos = platformObject.transform.position;
            foreach (Hex hex in cell.Items)
            {
                if (hex.Obj is not null)
                {
                    hex.Obj.transform.position = ComposePosition(pos, level);
                    HexView view = hex.Obj.GetComponent<HexView>();
                    view.SetColor(hex.Type);
                }
                else
                {
                    hex.Obj = InstantiateHex(ComposePosition(pos, level), hex.Type);
                }

                level--;
            }

            UpdateCellLabel(x, y, cell);
        }

        public void UpdateCellLabel(int x, int y, Cell cell)
        {
            if (_cellLabels?[y, x] is null) return;

            int count = cell.GetTopColorCount();
            _cellLabels[y, x].text = count > 0 ? count.ToString() : "";

            Vector3 pos = _platformObjects[y, x].transform.position;
            float stackHeight = cell.Size * _hexHeight * _layerThreshold;
            _cellLabelObjects[y, x].transform.position = new Vector3(pos.x, stackHeight + _labelHeight, pos.z);
        }

        public void CreateGrid(GridManager gridMgr, int movesCount)
        {
            _platformObjects = new GameObject[gridMgr.Height, gridMgr.Width];
            _cellLabels = new TextMeshProUGUI[gridMgr.Height, gridMgr.Width];
            _cellLabelObjects = new GameObject[gridMgr.Height, gridMgr.Width];

            _upperLeftY = (gridMgr.Height - 1) * _centerDistanceSin60 / 2;
            _upperLeftX = -gridMgr.Width * _centerDistance / 2;

            // Poses for player moves
            _movePlatformObjects = new GameObject[movesCount];
            for (int i = 0; i < movesCount; i++)
            {
                var vec = new Vector3((float)(_upperLeftX - 2 * _centerDistance - 0.3f * i), -_hexHeight / 2,
                    (float)_upperLeftY - 2.3f * i);
                GameObject move = InstantiatePlatform(vec);
                move.AddComponent<MoveCellClickHandler>().Init(controller, i);
                _movePlatformObjects[i] = move;
            }

            // Game cells
            for (int y = 0; y < gridMgr.Height; y++)
            {
                for (int x = 0; x < gridMgr.Width; x++)
                {
                    float globalY = (float)(_upperLeftY - y * _centerDistanceSin60);
                    float globalX = (float)(_upperLeftX + x * _centerDistance + _centerDistance * (y % 2) / 2);

                    if (!gridMgr.GetMask(x, y))
                        continue;

                    Cell cell = gridMgr.GetCell(x, y);

                    // Create platform
                    var pos = new Vector3(globalX, -_hexHeight / 2, globalY);
                    GameObject platform = InstantiatePlatform(pos);
                    platform.AddComponent<CellClickHandler>().Init(controller, x, y);
                    _platformObjects[y, x] = platform;

                    if (hexCountLabelPrefab != null)
                    {
                        GameObject label = Instantiate(hexCountLabelPrefab, new Vector3(globalX, _labelHeight, globalY),
                            Quaternion.identity);
                        _cellLabelObjects[y, x] = label;
                        _cellLabels[y, x] = label.GetComponentInChildren<TextMeshProUGUI>();
                    }

                    // Create hexes on platform
                    int level = cell.Items.Count;
                    foreach (Hex hex in cell.Items)
                    {
                        hex.Obj = InstantiateHex(ComposePosition(pos, level), hex.Type);
                        level--;
                    }

                    UpdateCellLabel(x, y, cell);
                }
            }
        }
    }
}
