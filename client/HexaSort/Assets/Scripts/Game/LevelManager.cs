using System.Collections.Generic;
using Controller;
using Core;
using UnityEngine;
using HexaSort.Services;
using Utilities;

namespace Game
{
    public class LevelManager : MonoBehaviour
    {
        public GameController gameController;

        private async void Start()
        {
            int levelId = PlayerPrefs.GetInt("SelectedLevel", 1);
            Debug.Log($"[LevelManager] Loading level {levelId}");

            var firebaseLevel = await LevelDatabaseService.Instance.GetLevelAsync(levelId);

            Progress.LevelData data;

            if (firebaseLevel != null)
            {
                Debug.Log($"[LevelManager] Level {levelId} loaded from Firebase");
                data = ConvertToLevelData(firebaseLevel);
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Level {levelId} not found in Firebase, using local");
                data = Progress.LevelDatabase.GetLevel(levelId);
            }

            Debug.Log("Level data loaded!");
            gameController.Build(data);
        }

        private Progress.LevelData ConvertToLevelData(HexaSort.Services.LevelData firebaseLevel)
        {
            bool[,] mask = firebaseLevel.Mask;

            var startCells = new List<(int x, int y, Cell cell)>();

            if (firebaseLevel.StartCells != null)
            {
                foreach (var cellData in firebaseLevel.StartCells)
                {
                    var stack = new Stack<Hex>();

                    for (int i = cellData.Stack.Count - 1; i >= 0; i--)
                    {
                        stack.Push(new Hex(cellData.Stack[i]));
                    }

                    startCells.Add((cellData.X, cellData.Y, new Cell(stack)));
                }
            }

            return new Progress.LevelData(mask, startCells);
        }
    }
}