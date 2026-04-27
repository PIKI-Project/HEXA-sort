using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneLoader : MonoBehaviour
    {
        // TODO: Remove when UI is ready. Level choosing
        private void Update()
        {
            // TODO: IMPORTANT: Change project input system from 'both' to 'new'
            // when delete this line, it necessary for KeyCode.E usage
            if (Input.GetKeyDown(KeyCode.E))
            {
                LoadLevel(2);
            }
        }

        private static void LoadLevel(int levelId)
        {
            GameManager.Instance.selectedLevel = levelId;
            SceneManager.LoadScene("GameLevelScene");
        }

        public void LoadMenu() => SceneManager.LoadScene("MainMenuScene");
    }
}
