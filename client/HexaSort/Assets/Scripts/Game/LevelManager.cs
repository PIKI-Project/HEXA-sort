using System.Collections.Generic;
using Controller;
using Core;
using HexaSort.Services;
using Progress;
using UnityEngine;
using LevelData = Progress.LevelData;

namespace Game
{
    public class LevelManager : MonoBehaviour
    {
        public GameController gameController;

        private async void Start()
        {
            int levelId = PlayerPrefs.GetInt("SelectedLevel", 1);
            Debug.Log($"[LevelManager] Loading level {levelId}");

            HexaSort.Services.LevelData firebaseLevel = await LevelDatabaseService.Instance.GetLevelAsync(levelId);

            LevelData data;

            if (firebaseLevel != null)
            {
                Debug.Log($"[LevelManager] Level {levelId} loaded from Firebase");
                data = ConvertToLevelData(firebaseLevel);
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Level {levelId} not found in Firebase, using local");
                data = LevelDatabase.GetLevel(levelId);
            }

            Debug.Log("Level data loaded!");
            gameController.Build(data);
        }

        private LevelData ConvertToLevelData(HexaSort.Services.LevelData firebaseLevel)
        {
            bool[,] mask = firebaseLevel.Mask;

            var startCells = new List<(int x, int y, Cell cell)>();

            if (firebaseLevel.StartCells == null) return new LevelData(mask, startCells);

            foreach (StartCellData cellData in firebaseLevel.StartCells)
            {
                var stack = new Stack<Hex>();

                for (int i = cellData.Stack.Count - 1; i >= 0; i--)
                {
                    stack.Push(new Hex(cellData.Stack[i]));
                }

                startCells.Add((cellData.X, cellData.Y, new Cell(stack)));
            }

            return new LevelData(mask, startCells);
        }
    }
}
