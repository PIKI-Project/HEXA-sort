using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneLoader : MonoBehaviour
    {
        private static void LoadLevel(int levelId)
        {
            GameManager.Instance.selectedLevel = levelId;
            SceneManager.LoadScene("GameLevelScene");
        }

        public void LoadMenu() => SceneManager.LoadScene("MainMenuScene");
    }
}
