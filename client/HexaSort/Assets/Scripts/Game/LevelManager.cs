using Controller;
using Progress;
using UnityEngine;

namespace Game
{
    public class LevelManager : MonoBehaviour
    {
        public GameController gameController;

        private void Start()
        {
            int levelId = GameManager.Instance.selectedLevel;

            LevelData data = LevelDatabase.GetLevel(levelId);
            Debug.Log("Level data loaded!");
            gameController.Build(data);
        }
    }
}
